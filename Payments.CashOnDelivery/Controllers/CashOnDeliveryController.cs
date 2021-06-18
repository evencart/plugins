using System.Globalization;
using Genesis.Infrastructure.Mvc;
using Genesis.Infrastructure.Mvc.Attributes;
using Genesis.Modules.Data;
using Microsoft.AspNetCore.Mvc;

namespace Payments.CashOnDelivery.Controllers
{
    [Route("cash-on-delivery")]
    [PluginType(PluginType = typeof(CashOnDeliveryPlugin))]
    public class CashOnDeliveryController : GenesisPluginController
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