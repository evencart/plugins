using System;
using System.Collections.Generic;
using EvenCart.Data.Entity.Payments;
using EvenCart.Data.Entity.Purchases;
using EvenCart.Services.Payments;
using EvenCart.Services.Plugins;
using Genesis;
using Genesis.Extensions;
using Genesis.Modules.Localization;
using Genesis.Modules.Logging;
using Genesis.Plugins;
using Payments.TwoCheckout.Helpers;

namespace Payments.TwoCheckout
{
    public class TwoCheckoutPlugin : GenesisPlugin, IPaymentHandlerPlugin
    {
        private readonly TwoCheckoutSettings _twoCheckoutSettings;
        private readonly ILogger _logger;
        public TwoCheckoutPlugin(TwoCheckoutSettings twoCheckoutSettings, ILogger logger)
        {
            _twoCheckoutSettings = twoCheckoutSettings;
            _logger = logger;
        }

        public PaymentMethodType PaymentMethodType => PaymentMethodType.CreditCard;

        public string PaymentHandlerComponentRouteName => TwoCheckoutConfig.PaymentHandlerComponentRouteName;

        public PaymentOperation[] SupportedOperations => new[]
            {PaymentOperation.Authorize, PaymentOperation.Capture, PaymentOperation.Refund, PaymentOperation.Void};

        public bool SupportsSubscriptions => true;

        public TransactionResult ProcessTransaction(TransactionRequest request)
        {
            if (request.RequestType == TransactionRequestType.Payment || request.RequestType == TransactionRequestType.SubscriptionCreate)
                return TwoCheckoutHelper.ProcessPayment(request, _twoCheckoutSettings, _logger);
            if (request.RequestType == TransactionRequestType.Refund)
                return TwoCheckoutHelper.ProcessRefund(request, _twoCheckoutSettings, _logger);
            if (request.RequestType == TransactionRequestType.Void)
                return TwoCheckoutHelper.ProcessVoid(request, _twoCheckoutSettings, _logger);
            return null;
        }

        public decimal GetPaymentHandlerFee(Cart cart)
        {
            return _twoCheckoutSettings.UsePercentageForAdditionalFee
                ? _twoCheckoutSettings.AdditionalFee * cart.FinalAmount / 100
                : _twoCheckoutSettings.AdditionalFee;
        }

        public decimal GetPaymentHandlerFee(Order order)
        {
            return _twoCheckoutSettings.UsePercentageForAdditionalFee
                ? _twoCheckoutSettings.AdditionalFee * order.OrderTotal / 100
                : _twoCheckoutSettings.AdditionalFee;
        }

        public bool ValidatePaymentInfo(Dictionary<string, string> parameters, out string error)
        {
            error = null;
            parameters.TryGetValue("requestToken", out var requestToken);
            
            if (requestToken.IsNullEmptyOrWhiteSpace())
            {
                error = LocalizationHelper.Localize("The request token can not be empty");
                return false;
            }
            return true;
        }

        public override string ConfigurationUrl =>
            GenesisEngine.Instance.RouteUrl(TwoCheckoutConfig.TwoCheckoutSettingsRouteName);

        #region Helpers
        private static bool ExpiryCheck(string month, string year)
        {
            if (!int.TryParse(month, out var monthInt))
                return false;
            if (!int.TryParse(year, out var yearInt))
                return false;
            var now = DateTime.UtcNow;
            return monthInt >= now.Month && yearInt >= now.Year;
        }
        #endregion
    }
}
