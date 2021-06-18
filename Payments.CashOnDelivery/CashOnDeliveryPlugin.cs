using System;
using System.Collections.Generic;
using EvenCart.Data.Entity.Payments;
using EvenCart.Data.Entity.Purchases;
using EvenCart.Services.Payments;
using EvenCart.Services.Plugins;
using Genesis;
using Genesis.Modules.Logging;
using Genesis.Plugins;

namespace Payments.CashOnDelivery
{
    public class CashOnDeliveryPlugin : GenesisPlugin, IPaymentHandlerPlugin
    {
        private readonly CashOnDeliverySettings _cashOnDeliverySettings;
        private readonly ILogger _logger;
        public CashOnDeliveryPlugin(CashOnDeliverySettings cashOnDeliverySettings, ILogger logger)
        {
            _cashOnDeliverySettings = cashOnDeliverySettings;
            _logger = logger;
        }

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Misc;

        public string PaymentHandlerComponentRouteName => CashOnDeliveryConfig.PaymentHandlerComponentRouteName;

        public PaymentOperation[] SupportedOperations => null;

        public bool SupportsSubscriptions => false;

        public TransactionResult ProcessTransaction(TransactionRequest request)
        {
            return new TransactionResult()
            {
                Success = true,
                NewStatus = PaymentStatus.Pending,
                OrderGuid = request.Order.Guid,
                Order = request.Order,
                TransactionGuid = Guid.NewGuid().ToString(),
                TransactionAmount = request.Amount ?? request.Order.OrderTotal,
                TransactionCurrencyCode = request.Order.CurrencyCode
            };
        }

        public decimal GetPaymentHandlerFee(Cart cart)
        {
            return _cashOnDeliverySettings.UsePercentageForAdditionalFee
                ? _cashOnDeliverySettings.AdditionalFee * cart.FinalAmount / 100
                : _cashOnDeliverySettings.AdditionalFee;
        }

        public decimal GetPaymentHandlerFee(Order order)
        {
            return _cashOnDeliverySettings.UsePercentageForAdditionalFee
                ? _cashOnDeliverySettings.AdditionalFee * order.OrderTotal / 100
                : _cashOnDeliverySettings.AdditionalFee;
        }

        public bool ValidatePaymentInfo(Dictionary<string, string> parameters, out string error)
        {
            error = null;
            return true;
        }

        public override string ConfigurationUrl =>
            GenesisEngine.Instance.RouteUrl(CashOnDeliveryConfig.CashOnDeliverySettingsRouteName);
    }
}
