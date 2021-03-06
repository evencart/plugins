﻿using Genesis.Infrastructure.Mvc;
using Genesis.Infrastructure.Mvc.Attributes;
using Genesis.Modules.Settings;
using Genesis.Routing;
using Microsoft.AspNetCore.Mvc;
using Payments.PaypalWithRedirect.Models;

namespace Payments.PaypalWithRedirect.Controllers.Administration
{
    public class PaypalWithRedirectController : GenesisPluginAdminController
    {
        private readonly PaypalWithRedirectSettings _paypalWithRedirectSettings;
        private readonly ISettingService _settingService;
        public PaypalWithRedirectController(PaypalWithRedirectSettings paypalWithRedirectSettings, ISettingService settingService)
        {
            _paypalWithRedirectSettings = paypalWithRedirectSettings;
            _settingService = settingService;
        }
        [HttpGet("settings", Name = PaypalConfig.PaypalWithRedirectSettingsRouteName)]
        public IActionResult Settings()
        {
            var settingsModel = new SettingsModel()
            {
                ClientId = _paypalWithRedirectSettings.ClientId,
                ClientSecret = _paypalWithRedirectSettings.ClientSecret,
                EnableSandbox = _paypalWithRedirectSettings.EnableSandbox
            };
            return R.Success.With("settings", settingsModel).Result;
        }

        [DualPost("settings", Name = PaypalConfig.PaypalWithRedirectSettingsRouteName, OnlyApi = true)]
        [ValidateModelState(ModelType = typeof(SettingsModel))]
        public IActionResult SettingsSave(SettingsModel settingsModel)
        {
            _paypalWithRedirectSettings.ClientId = settingsModel.ClientId;
            _paypalWithRedirectSettings.ClientSecret = settingsModel.ClientSecret;
            _paypalWithRedirectSettings.EnableSandbox = settingsModel.EnableSandbox;
            _settingService.Save(_paypalWithRedirectSettings, CurrentStore.Id);
            return R.Success.Result;
        }
    }
}