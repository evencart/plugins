using System;
using System.Collections.Generic;
using EvenCart.Data.Entity.Payments;
using EvenCart.Data.Entity.Purchases;
using EvenCart.Services.Payments;
using EvenCart.Services.Plugins;
using Genesis;
using Genesis.Extensions;
using Genesis.Modules.Data;
using Genesis.Modules.Localization;
using Genesis.Modules.Logging;
using Genesis.Plugins;
using Payments.Stripe.Helpers;

namespace Payments.Stripe
{
    public class StripePlugin : GenesisPlugin, IPaymentHandlerPlugin
    {
        private readonly ILogger _logger;
        public StripePlugin(ILogger logger)
        {
            _logger = logger;
        }

        public PaymentMethodType PaymentMethodType => PaymentMethodType.CreditCard;

        public string PaymentHandlerComponentRouteName => StripeConfig.PaymentHandlerComponentRouteName;

        public PaymentOperation[] SupportedOperations => new[]
            {PaymentOperation.Authorize, PaymentOperation.Capture, PaymentOperation.Refund, PaymentOperation.Void};

        public bool SupportsSubscriptions => true;

        public TransactionResult ProcessTransaction(TransactionRequest request)
        {
            var stripeSettings = D.Resolve<StripeSettings>();
            if (request.RequestType == TransactionRequestType.Payment)
            {
                if (stripeSettings.UseRedirectionFlow)
                {
                    return StripeHelper.CreateSessionRedirect(request, stripeSettings, _logger, false);
                }
                return StripeHelper.ProcessPayment(request, stripeSettings, _logger);
            }
            if (request.RequestType == TransactionRequestType.Refund)
                return StripeHelper.ProcessRefund(request, stripeSettings, _logger);
            if (request.RequestType == TransactionRequestType.Void)
                return StripeHelper.ProcessVoid(request, stripeSettings, _logger);
            if (request.RequestType == TransactionRequestType.Capture)
                return StripeHelper.ProcessCapture(request, stripeSettings, _logger);
            if (request.RequestType == TransactionRequestType.SubscriptionCreate)
            {
                if (stripeSettings.UseRedirectionFlow)
                    return StripeHelper.CreateSessionRedirect(request, stripeSettings, _logger, true);
                return StripeHelper.CreateSubscription(request, stripeSettings, _logger);
            }
            if (request.RequestType == TransactionRequestType.SubscriptionCancel)
                return StripeHelper.StopSubscription(request, stripeSettings, _logger);
            return null;
        }

        public decimal GetPaymentHandlerFee(Cart cart)
        {
            var stripeSettings = D.Resolve<StripeSettings>();
            return stripeSettings.UsePercentageForAdditionalFee
                ? stripeSettings.AdditionalFee * cart.FinalAmount / 100
                : stripeSettings.AdditionalFee;
        }

        public decimal GetPaymentHandlerFee(Order order)
        {
            var stripeSettings = D.Resolve<StripeSettings>();
            return stripeSettings.UsePercentageForAdditionalFee
                ? stripeSettings.AdditionalFee * order.OrderTotal / 100
                : stripeSettings.AdditionalFee;
        }

        public bool ValidatePaymentInfo(Dictionary<string, string> parameters, out string error)
        {
            var stripeSettings = D.Resolve<StripeSettings>();
            error = null;
            if (stripeSettings.UseRedirectionFlow)
                return true; //we don't collect anything on our site in case of redirection flow
            parameters.TryGetValue("cardNumber", out var cardNumber);
            parameters.TryGetValue("cardName", out var cardName);
            parameters.TryGetValue("expireMonth", out var expireMonthStr);
            parameters.TryGetValue("expireYear", out var expireYearStr);
            parameters.TryGetValue("cvv", out var cvv);

            if (!PaymentCardHelper.IsCardNumberValid(cardNumber))
            {
                error = LocalizationHelper.Localize("The card number is incorrect");
                return false;
            }
            if (cardName.IsNullEmptyOrWhiteSpace())
            {
                error = LocalizationHelper.Localize("The card name can not be empty");
                return false;
            }
            if (cvv.IsNullEmptyOrWhiteSpace())
            {
                error = LocalizationHelper.Localize("The card security code can not be empty");
                return false;
            }

            try
            {
                if (ExpiryCheck(expireMonthStr, expireYearStr))
                {
                    error = LocalizationHelper.Localize("The card expiry is incorrect or card has expired");
                    return false;
                }
            }
            catch
            {
                error = LocalizationHelper.Localize("The card expiry is incorrect or card has expired");
                return false;
            }

            return true;
        }

        public override string ConfigurationUrl =>
            GenesisEngine.Instance.RouteUrl(StripeConfig.StripeSettingsRouteName);

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
