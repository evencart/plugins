using System;
using Genesis.Infrastructure.Mvc;
using Genesis.Infrastructure.Mvc.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Payments.TwoCheckout.Models;

namespace Payments.TwoCheckout.Controllers
{
    [Route("twoCheckout")]
    [PluginType(PluginType = typeof(TwoCheckoutPlugin))]
    public class TwoCheckoutController : GenesisPluginController
    {
        private readonly TwoCheckoutSettings _twoCheckoutSettings;

        public TwoCheckoutController(TwoCheckoutSettings twoCheckoutSettings)
        {
            _twoCheckoutSettings = twoCheckoutSettings;
        }

        [HttpGet("payment-info", Name = TwoCheckoutConfig.PaymentHandlerComponentRouteName)]
        public IActionResult PaymentInfoDisplayPage()
        {
            var model = new PaymentInfoModel();
            var baseYear = DateTime.UtcNow.Year;
            for (var i = 1; i < 13; i++)
            {
                var value = i.ToString();
                model.AvailableMonths.Add(new SelectListItem(value, value));
            }
            //50 years from now
            for (var i = 0; i < 51; i++)
            {
                var value = (baseYear + i).ToString();
                model.AvailableYears.Add(new SelectListItem(value, value));
            }
            model.Month = DateTime.UtcNow.Month;
            model.Year = DateTime.UtcNow.Year;
            model.EnableTestMode = _twoCheckoutSettings.EnableTestMode;
            model.SellerId = _twoCheckoutSettings.SellerId;
            model.PublishableKey = _twoCheckoutSettings.PublishableKey;
            return R.With("paymentInfo", model).Result;
        }
    }
}