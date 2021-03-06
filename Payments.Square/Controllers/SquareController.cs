﻿using EvenCart.Genesis;
using EvenCart.Services.Orders;
using Genesis;
using Genesis.Extensions;
using Genesis.Infrastructure.Mvc;
using Genesis.Infrastructure.Mvc.Attributes;
using Genesis.Modules.Addresses;
using Genesis.Modules.Data;
using Microsoft.AspNetCore.Mvc;
using Payments.Square.Helpers;
using Payments.Square.Models;

namespace Payments.Square.Controllers
{
    [Route("square")]
    [PluginType(PluginType = typeof(SquarePlugin))]
    public class SquareController : GenesisPluginController
    {
        private SquareSettings _squareSettings;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        public SquareController(SquareSettings squareSettings, ICartService cartService, IOrderService orderService)
        {
            _squareSettings = squareSettings;
            _cartService = cartService;
            _orderService = orderService;
        }

        [HttpGet("payment-info", Name = SquareConfig.PaymentHandlerComponentRouteName)]
        public IActionResult PaymentInfoDisplayPage(string orderGuid)
        {
            var postalCode = "";
            if (orderGuid.IsNullEmptyOrWhiteSpace())
            {
                var cart = _cartService.GetCart(CurrentUser.Id);
                postalCode = cart?.BillingAddress?.ZipPostalCode;
            }
            else
            {
                var order = _orderService.GetByGuid(orderGuid);
                postalCode = order?.BillingAddressSerialized?.To<Address>()?.ZipPostalCode;
            }
            var model = new PaymentInfoModel()
            {
                ApplicationId = _squareSettings.EnableSandbox ? _squareSettings.SandboxApplicationId : _squareSettings.ApplicationId,
                PostalCode = postalCode,
                ScriptUrl = _squareSettings.EnableSandbox ? SquareConfig.SandboxScriptUrl : SquareConfig.ScriptUrl
            };
            return R.With("paymentInfo", model).Result;
        }

        [HttpPost("webhook", Name = SquareConfig.SquareWebhookUrl)]
        public IActionResult Webhook()
        {
            SquareHelper.ParseWebhookResponse(GenesisEngine.Instance.CurrentHttpContext.Request);
            return Ok();
        }
    }
}