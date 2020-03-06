using System;
using EvenCart.Data.Entity.Payments;
using EvenCart.Data.Enum;
using EvenCart.Data.Extensions;
using EvenCart.Infrastructure;
using EvenCart.Infrastructure.Mvc;
using EvenCart.Infrastructure.Mvc.Attributes;
using EvenCart.Infrastructure.Routing;
using EvenCart.Services.Logger;
using EvenCart.Services.Payments;
using EvenCart.Services.Purchases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Payments.Stripe.Helpers;
using Payments.Stripe.Models;

namespace Payments.Stripe.Controllers
{
    [Route("stripe")]
    [PluginType(PluginType = typeof(StripePlugin))]
    public class StripeController : FoundationPluginController
    {
        private readonly IOrderService _orderService;
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger _logger;
        public StripeController(IOrderService orderService, StripeSettings stripeSettings)
        {
            _orderService = orderService;
            _stripeSettings = stripeSettings;
        }

        [HttpGet("payment-info", Name = StripeConfig.PaymentHandlerComponentRouteName)]
        public IActionResult PaymentInfoDisplayPage()
        {
            var model = new PaymentInfoModel();
            if (_stripeSettings.UseRedirectionFlow)
                model.RedirectionEnabled = true;
            else
            {
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
            }

            return R.With("paymentInfo", model).Result;
        }

        [HttpPost("webhook", Name = StripeConfig.StripeWebhookUrl)]
        public IActionResult Webhook()
        {
            try
            {
                StripeHelper.ParseWebhookResponse(ApplicationEngine.CurrentHttpContext.Request);
            }
            catch (Exception ex)
            {
                _logger.Log<StripeController>(LogLevel.Fatal, ex.Message, ex);
            }
            return Ok();
        }

        [HttpGet("return/{orderGuid}", Name = StripeConfig.StripeReturnUrlRouteName)]
        public IActionResult Return(string orderGuid)
        {
            var order = _orderService.GetByGuid(orderGuid);
            if (order == null)
                return NotFound();
            return RedirectToRoute(RouteNames.CheckoutComplete, new { orderGuid = order.Guid });
        }

        [HttpGet("cancel/{orderGuid}", Name = StripeConfig.StripeCancelUrlRouteName)]
        public IActionResult Cancel(string orderGuid)
        {
            return RedirectToRoute(RouteNames.CheckoutPayment, new { orderGuid, error = true });
        }

        [HttpGet("redirect", Name = StripeConfig.StripeRedirectToUrlRouteName)]
        public IActionResult RedirectToStripe(string orderGuid, string sessionId)
        {
            var order = _orderService.GetByGuid(orderGuid);
            if (order == null || sessionId.IsNullEmptyOrWhiteSpace())
                return NotFound();
            var model = new RedirectToStripeModel()
            {
                PublishableKey = StripeHelper.GetPublishableKey(_stripeSettings),
                SessionId = sessionId,
                CancelUrl = ApplicationEngine.RouteUrl(RouteNames.CheckoutPayment, new {orderGuid, error = true})
            };
            return R.Success.With("redirectInfo", model).Result;
        }
    }
}