using System;
using System.Collections;
using System.Collections.Generic;
using EvenCart.Core.Plugins;
using EvenCart.Data.Entity.Purchases;
using EvenCart.Data.Entity.Shop;
using EvenCart.Infrastructure;
using EvenCart.Services.Plugins;
using Address = EvenCart.Data.Entity.Addresses.Address;
using Shippo;
using Shippo.Models;
using EvenCart.Services.Converters;
using System.Linq;
using EvenCart.Core.Exception;
using Newtonsoft.Json;
using static Shippo.ShippoEnums;
using EvenCart.Services.Addresses;

namespace Shipping.Shippo
{
    public class ProviderPlugin : FoundationPlugin, IShipmentHandlerPlugin
    {
        private readonly ShippoSettings _shippoSettings;
        private readonly IConverterService _converterService;
        private readonly IStateOrProvinceService _stateProvinceService;
        public ProviderPlugin(ShippoSettings ShippoSettings, IConverterService converterService,
            IStateOrProvinceService stateProvinceService)
        {
            _shippoSettings = ShippoSettings;
            _converterService = converterService;
            _stateProvinceService = stateProvinceService;
        }
        private APIResource InitResource()
        {
            var API_KEY = _shippoSettings.DebugMode ? _shippoSettings.TestApiKey : _shippoSettings.LiveApiKey;
            return new APIResource(API_KEY);
        }
        public bool SupportsLabelPurchase => true;

        public bool IsMethodAvailable(Cart cart)
        {
            return true;
        }

        public IList<ShippingOption> GetAvailableOptions(IList<Product> products, Address shipperInfo, Address receiverInfo)
        {
            var shippingOptions = new List<ShippingOption>();

            var shipmentTable = CreateShipmentConfig(products, shipperInfo, receiverInfo);

            var resource = InitResource();
            var shipment = resource.CreateShipment(shipmentTable);

            if (shipment.Rates.Length == 0)
            {
                var error = JsonConvert.SerializeObject(shipment.Messages);
                throw new EvenCartException(error);
            }

            foreach (Rate rate in shipment.Rates)
            {
                var name = $"{rate.Provider + " - " + rate.Servicelevel.Name?.ToString()}";
                shippingOptions.Add(new ShippingOption
                {
                    Rate = Convert.ToDecimal(rate.Amount),
                    DeliveryTime = rate.EstimatedDays?.ToString(),
                    Description = rate.DurationTerms?.ToString(),
                    Name = name,
                    Remarks = rate.Servicelevel.Terms?.ToString(),
                    Id = rate.ObjectId,
                });
            }

            return shippingOptions;
        }

        public IList<ShippingOption> GetAvailableOptions(IList<(Product, int)> products, Address shipperInfo, Address receiverInfo)
        {
            var productList = new List<Product>();
            foreach (var item in products)
                for (var i = 0; i < item.Item2; i++)
                    productList.Add(item.Item1);

            return GetAvailableOptions(productList, shipperInfo, receiverInfo);
        }

        public ShipmentInfo GetShipmentInfo(ShippingOption selectedShippingOption, IList<(Product, int)> products)
        {
            return GetLabel(selectedShippingOption);
        }
        private ShipmentInfo GetLabel(ShippingOption shippingOption)
        {
            var resource = InitResource();

            Hashtable transactionParameters = new Hashtable();
            transactionParameters.Add("rate", shippingOption.Id);
            transactionParameters.Add("async", false);
            Transaction transaction = resource.CreateTransaction(transactionParameters);

            if (transaction.Status != TransactionStatuses.SUCCESS)
            {
                var error = JsonConvert.SerializeObject(transaction.Messages);
                throw new EvenCartException(error);
            }
            return new ShipmentInfo
            {
                ShippingLabelUrl = transaction.LabelURL,
                TrackingNumber = transaction.TrackingNumber,
                TrackingUrl = transaction.TrackingUrlProvider,
            };
        }

        public override string ConfigurationUrl =>
            ApplicationEngine.RouteUrl(ProviderConfig.ShippoProviderSettingsRouteName);

        #region Private 
        private Hashtable CreateShipmentConfig(IList<Product> products, Address shipperInfo, Address receiverInfo)
        {
            //var toAddressStateName = receiverInfo.StateProvinceName;
            //if (toAddressStateName == null)
            //{
            //    var state = _stateProvinceService.Get(Convert.ToInt32(receiverInfo.StateProvinceId));
            //    if (state != null)
            //        toAddressStateName = state.Name;
            //}

            Hashtable toAddressTable = new Hashtable();
            toAddressTable.Add("name", receiverInfo.Name);
            toAddressTable.Add("street1", receiverInfo.Address1);
            toAddressTable.Add("city", receiverInfo.City);
            toAddressTable.Add("zip", receiverInfo.ZipPostalCode);
            toAddressTable.Add("country", receiverInfo.Country.Code);
            //toAddressTable.Add("state", toAddressStateName);
            toAddressTable.Add("phone", receiverInfo.Phone);
            toAddressTable.Add("email", receiverInfo.Email);

            //var fromAddressStateName = shipperInfo.StateProvinceName;
            //if (fromAddressStateName == null)
            //{
            //    var state = _stateProvinceService.Get(Convert.ToInt32(shipperInfo.StateProvinceId));
            //    if (state != null)
            //        fromAddressStateName = state.Name;
            //}
            // from address
            Hashtable fromAddressTable = new Hashtable();
            fromAddressTable.Add("name", shipperInfo.Name);
            fromAddressTable.Add("street1", shipperInfo.Address1);
            fromAddressTable.Add("city", shipperInfo.City);
            fromAddressTable.Add("zip", shipperInfo.ZipPostalCode);
            fromAddressTable.Add("country", shipperInfo.Country.Code);
            //fromAddressTable.Add("state", fromAddressStateName);
            fromAddressTable.Add("phone", shipperInfo.Phone);
            fromAddressTable.Add("email", shipperInfo.Email);
            fromAddressTable.Add("metadata", shipperInfo.Id);
            //fromAddressTable.Add("is_residential", receiverInfo.AddressType == AddressType.Home);

            // parcel
            List<Hashtable> parcels = new List<Hashtable>();
            var weightUnit = WeightUnit.Pound;
            var lengthUnit = LengthUnit.Inch;

            if (_shippoSettings.UseSinglePackageShipment)
            {
                var customProduct = GetCustomLargeProduct(products, weightUnit, lengthUnit);

                Hashtable parcelTable = new Hashtable();

                parcelTable.Add("name", customProduct.Name);
                parcelTable.Add("width", customProduct.PackageWidth);
                parcelTable.Add("height", customProduct.PackageHeight);
                parcelTable.Add("length", customProduct.PackageLength);
                parcelTable.Add("distance_unit", Helper.GetDistanceUnit(lengthUnit));
                parcelTable.Add("weight", customProduct.PackageWeight);
                parcelTable.Add("mass_unit", Helper.GetMassUnit(weightUnit));

                parcels.Add(parcelTable);
            }
            else
            {
                foreach (var product in products)
                {
                    Hashtable parcelTable = new Hashtable();
                    var weight = decimal.Round(_converterService.ConvertWeight(product.PackageWeightUnit, weightUnit, product.PackageWeight), 2, MidpointRounding.AwayFromZero);

                    var length = decimal.Round(_converterService.ConvertLength(product.PackageLengthUnit, lengthUnit, product.PackageLength), 2, MidpointRounding.AwayFromZero);

                    var width = decimal.Round(_converterService.ConvertLength(product.PackageWidthUnit, lengthUnit, product.PackageWidth), 2, MidpointRounding.AwayFromZero);

                    var height = decimal.Round(_converterService.ConvertLength(product.PackageHeightUnit, lengthUnit, product.PackageHeight), 2, MidpointRounding.AwayFromZero);

                    parcelTable.Add("name", product.Name);

                    parcelTable.Add("width", width);
                    parcelTable.Add("height", height);
                    parcelTable.Add("length", length);
                    parcelTable.Add("distance_unit", Helper.GetDistanceUnit(lengthUnit));

                    parcelTable.Add("weight", decimal.Round(weight, 2, MidpointRounding.AwayFromZero));
                    parcelTable.Add("mass_unit", Helper.GetMassUnit(weightUnit));

                    parcels.Add(parcelTable);
                }
            }
            // shipment
            Hashtable shipmentTable = new Hashtable();
            shipmentTable.Add("address_to", toAddressTable);
            shipmentTable.Add("address_from", fromAddressTable);
            shipmentTable.Add("parcels", parcels);
            shipmentTable.Add("object_purpose", "PURCHASE");
            shipmentTable.Add("async", false);

            return shipmentTable;
        }

        private Product GetCustomLargeProduct(IList<Product> products, WeightUnit weightUnit, LengthUnit lengthUnit)
        {
            var weights = products.Select(x => decimal.Round(_converterService.ConvertWeight(x.PackageWeightUnit, weightUnit, x.PackageWeight), 2, MidpointRounding.AwayFromZero)).ToList();
            var lengths = products.Select(x => decimal.Round(_converterService.ConvertLength(x.PackageLengthUnit, lengthUnit, x.PackageLength), 2, MidpointRounding.AwayFromZero)).ToList();
            var widths = products.Select(x => decimal.Round(_converterService.ConvertLength(x.PackageWidthUnit, lengthUnit, x.PackageWidth), 2, MidpointRounding.AwayFromZero)).ToList();
            var heights = products.Select(x => decimal.Round(_converterService.ConvertLength(x.PackageHeightUnit, lengthUnit, x.PackageHeight), 2, MidpointRounding.AwayFromZero)).ToList();

            var product = products.OrderByDescending(x => decimal.Round(_converterService.ConvertLength(x.PackageHeightUnit, lengthUnit, x.PackageHeight), 2, MidpointRounding.AwayFromZero)).FirstOrDefault();

            if (product == null)
                return null;

            product.PackageWeight = weights.Sum();
            product.PackageLength = lengths.OrderByDescending(x => x).FirstOrDefault();
            product.PackageWidth = widths.OrderByDescending(x => x).FirstOrDefault();
            product.PackageHeight = heights.OrderByDescending(x => x).FirstOrDefault();

            return product;
        }

        #endregion
    }
}
