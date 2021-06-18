using System;
using System.Collections.Generic;
using EvenCart.Data.Entity.Payments;
using EvenCart.Data.Entity.Purchases;
using EvenCart.Data.Entity.Shop;
using Genesis;
using Genesis.Modules.Addresses;
using Genesis.Modules.Data;
using Genesis.Modules.Http;
using Genesis.Modules.Logging;

namespace Payments.TwoCheckout.Helpers
{
    public class TwoCheckoutHelper
    {
        private const string AuthorizeUrl = "https://{0}.2checkout.com/checkout/api/1/{1}/rs/authService";
        private const string RefundUrl = "https://{0}.2checkout.com/api/sales/refund_invoice";

        private const string TwoCheckoutCustomerIdKey = "TwoCheckoutCustomerId";
        private static string GetAuthorizeUrl(TwoCheckoutSettings twoCheckoutSettings)
        {
            return string.Format(AuthorizeUrl, twoCheckoutSettings.EnableTestMode ? "sandbox" : "www",
                twoCheckoutSettings.SellerId);
        }
        private static string GetRefundUrl(TwoCheckoutSettings twoCheckoutSettings)
        {
            return string.Format(RefundUrl, twoCheckoutSettings.EnableTestMode ? "sandbox" : "www");
        }

        public static TransactionResult ProcessPayment(TransactionRequest request, TwoCheckoutSettings twoCheckoutSettings, ILogger logger)
        {
            var authUrl = GetAuthorizeUrl(twoCheckoutSettings);
            var requestProvider = D.Resolve<IRequestProvider>();

            var parameters = request.Parameters;
            parameters.TryGetValue("requestToken", out var requestToken);

            var order = request.Order;
            //do we have a saved twoCheckout customer id?
            var billingAddress = order.BillingAddressSerialized.To<Address>();
            
            var authParameters = new
            {
                sellerId = twoCheckoutSettings.SellerId,
                privateKey = twoCheckoutSettings.PrivateKey,
                merchantOrderId = order.Id.ToString(),
                token = requestToken.ToString(),
                currency = order.CurrencyCode,
                lineItems = new List<object>(),
                billingAddr = new
                {
                    name = billingAddress.Name,
                    addrLine1 = billingAddress.Address1,
                    addrLine2 = billingAddress.Address2,
                    city = billingAddress.City,
                    state = billingAddress.StateProvinceName,
                    zipCode = billingAddress.ZipPostalCode,
                    country = billingAddress.Country.Name,
                    email = billingAddress.Email,
                    phoneNumber = billingAddress.Phone
                }
            };
            foreach (var orderItem in order.OrderItems)
            {
                authParameters.lineItems.Add(new
                {
                    type = "product",
                    name = orderItem.Product.Name,
                    quantity = orderItem.Quantity,
                    price = orderItem.Price.ToString("N"),
                    tangible = orderItem.Product.IsShippable ? "Y" : "N",
                    productId = orderItem.ProductId,
                    recurrence = orderItem.ProductSaleType == ProductSaleType.Subscription ? GetSubscriptionRecurrence(orderItem) : null
                });
            }

            var response = requestProvider.Post<dynamic>(authUrl, authParameters)?.response;
            if (response == null)
            {
                logger.Log<TransactionResult>(LogLevel.Warning, "The payment for Order#" + order.Id + " by TwoCheckout failed.");
                return new TransactionResult()
                {
                    Success = false,
                };
            }
            var processPaymentResult = new TransactionResult()
            {
                Success = true,
                ResponseParameters = new Dictionary<string, object>()
                {
                    {"invoiceId", response.transactionId },
                    {"responseMsg", response.responseMsg },
                    {"responseCode", response.responseCode},
                    {"currencyCode", response.currencyCode},
                    {"lineItems", response.lineItems}
                },
                TransactionGuid = request.TransactionGuid,
                TransactionAmount = (decimal)response.total,
                TransactionCurrencyCode = response.CurrencyCode,
                OrderGuid = order.Guid,
                NewStatus = PaymentStatus.Complete
            };
            return processPaymentResult;
        }

        public static TransactionResult ProcessRefund(TransactionRequest refundRequest, TwoCheckoutSettings twoCheckoutSettings, ILogger logger)
        {
            var refundUrl = GetRefundUrl(twoCheckoutSettings);
            var requestProvider = D.Resolve<IRequestProvider>();

            var parameters = refundRequest.Parameters;
            parameters.TryGetValue("invoiceId", out var invoiceId);

            var order = refundRequest.Order;
            //do we have a saved twoCheckout customer id?
            var amountToRefund = refundRequest.IsPartialRefund ? refundRequest.Amount : null;
            var refundParameters = new
            {
                invoice_id = invoiceId,
                amount = amountToRefund,
                currency = order.CurrencyCode,
                comment = "Admin Initiated Refund",
                category = 5 //https://www.2checkout.com/documentation/api/sales/refund-invoice
            };
            var response = requestProvider.Post<dynamic>(refundUrl, refundParameters);
            var refundResult = new TransactionResult()
            {
                TransactionGuid = Guid.NewGuid().ToString(),
                OrderGuid = refundRequest.Order.Guid,
                TransactionCurrencyCode = order.CurrencyCode,
                TransactionAmount = amountToRefund ?? refundRequest.Amount ?? order.OrderTotal
            };
            if (response == null || response.response_code != "OK")
            {
                logger.Log<TransactionResult>(LogLevel.Warning, "The refund for Order#" + refundRequest.Order.Id + " by 2Checkout failed." + response?.response_message);
                refundResult.Success = false;
                refundResult.Exception = new Exception("An error occurred while processing refund");
                return refundResult;
            }

            refundResult.NewStatus = refundRequest.IsPartialRefund ? PaymentStatus.RefundedPartially : PaymentStatus.Refunded;
            refundResult.Success = true;
            return refundResult;
        }

        public static TransactionResult ProcessVoid(TransactionRequest refundRequest,
            TwoCheckoutSettings twoCheckoutSettings, ILogger logger)
        {
            return ProcessRefund(refundRequest, twoCheckoutSettings, logger);
        }

        private static string GetSubscriptionRecurrence(OrderItem orderItem)
        {
            switch (orderItem.SubscriptionCycle)
            {
                case TimeCycle.Daily:
                    throw new Exception("Daily recurrence is not support by two checkout");
                    break;
                case TimeCycle.Monthly:
                    return $"{orderItem.CycleCount} Month";
                case TimeCycle.Yearly:
                    return $"{orderItem.CycleCount} Year";
                case TimeCycle.Weekly:
                default:
                    return $"{orderItem.CycleCount} Week";
            }
        }
    }
}