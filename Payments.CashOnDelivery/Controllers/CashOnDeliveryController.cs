using System.Globalization;
using EvenCart.Infrastructure.Extensions;
using EvenCart.Infrastructure.Mvc;
using EvenCart.Infrastructure.Mvc.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Payments.CashOnDelivery.Controllers
{
    [Route("cash-on-delivery")]
    [PluginType(PluginType = typeof(CashOnDeliveryPlugin))]
    public class CashOnDeliveryController : FoundationPluginController
    {
        private readonly CashOnDeliverySettings _cashOnDeliverySettings;
        public CashOnDeliveryController(CashOnDeliverySettings cashOnDeliverySettings)
        {
            _cashOnDeliverySettings = cashOnDeliverySettings;
        }

        [HttpGet("payment-info", Name = CashOnDeliveryConfig.PaymentHandlerComponentRouteName)]
        public IActionResult PaymentInfoDisplayPage()
        {
            var feeValue = _cashOnDeliverySettings.UsePercentageForAdditionalFee
                ? _cashOnDeliverySettings.AdditionalFee.ToString(CultureInfo.InvariantCulture)
                : _cashOnDeliverySettings.AdditionalFee.ToCurrency();

            var additionalFeeString = feeValue + (_cashOnDeliverySettings.UsePercentageForAdditionalFee ? "%" : "");

            return R.With("feeString", additionalFeeString).With("fee", _cashOnDeliverySettings.AdditionalFee).Result;
        }

      
    }
}