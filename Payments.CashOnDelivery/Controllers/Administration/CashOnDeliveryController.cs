﻿using Genesis.Infrastructure.Mvc;
using Genesis.Infrastructure.Mvc.Attributes;
using Genesis.Modules.Settings;
using Genesis.Routing;
using Microsoft.AspNetCore.Mvc;
using Payments.CashOnDelivery.Models;

namespace Payments.CashOnDelivery.Controllers.Administration
{
    public class CashOnDeliveryController : GenesisPluginAdminController
    {
        private readonly CashOnDeliverySettings _cashOnDeliverySettings;
        private readonly ISettingService _settingService;
        public CashOnDeliveryController(CashOnDeliverySettings cashOnDeliverySettings, ISettingService settingService)
        {
            _cashOnDeliverySettings = cashOnDeliverySettings;
            _settingService = settingService;
        }
        [HttpGet("settings", Name = CashOnDeliveryConfig.CashOnDeliverySettingsRouteName)]
        public IActionResult Settings()
        {
            var settingsModel = new SettingsModel()
            {
                AdditionalFee = _cashOnDeliverySettings.AdditionalFee,
                UsePercentageForAdditionalFee = _cashOnDeliverySettings.UsePercentageForAdditionalFee
            };
            return R.Success.With("settings", settingsModel).Result;
        }

        [DualPost("settings", Name = CashOnDeliveryConfig.CashOnDeliverySettingsRouteName, OnlyApi = true)]
        [ValidateModelState(ModelType = typeof(SettingsModel))]
        public IActionResult SettingsSave(SettingsModel settingsModel)
        {
            _cashOnDeliverySettings.AdditionalFee = settingsModel.AdditionalFee;
            _cashOnDeliverySettings.UsePercentageForAdditionalFee = settingsModel.UsePercentageForAdditionalFee;
            _settingService.Save(_cashOnDeliverySettings, CurrentStore.Id);
            return R.Success.Result;
        }
    }
}