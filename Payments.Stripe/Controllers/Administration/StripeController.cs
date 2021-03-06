﻿using Genesis.Infrastructure.Mvc;
using Genesis.Infrastructure.Mvc.Attributes;
using Genesis.Modules.Settings;
using Genesis.Routing;
using Microsoft.AspNetCore.Mvc;
using Payments.Stripe.Models;

namespace Payments.Stripe.Controllers.Administration
{
    public class StripeController : GenesisPluginAdminController
    {
        private readonly StripeSettings _stripeSettings;
        private readonly ISettingService _settingService;
        public StripeController(StripeSettings stripeSettings, ISettingService settingService)
        {
            _stripeSettings = stripeSettings;
            _settingService = settingService;
        }
        [HttpGet("settings", Name = StripeConfig.StripeSettingsRouteName)]
        public IActionResult Settings()
        {
            var settingsModel = new SettingsModel()
            {
                Description = _stripeSettings.Description,
                TestPublishableKey = _stripeSettings.TestPublishableKey,
                TestSecretKey = _stripeSettings.TestSecretKey,
                AdditionalFee = _stripeSettings.AdditionalFee,
                AuthorizeOnly = _stripeSettings.AuthorizeOnly,
                EnableTestMode = _stripeSettings.EnableTestMode,
                PublishableKey = _stripeSettings.PublishableKey,
                SecretKey = _stripeSettings.SecretKey,
                UsePercentageForAdditionalFee = _stripeSettings.UsePercentageForAdditionalFee,
                UseRedirectionFlow = _stripeSettings.UseRedirectionFlow
            };
            return R.Success.With("settings", settingsModel).Result;
        }

        [DualPost("settings", Name = StripeConfig.StripeSettingsRouteName, OnlyApi = true)]
        [ValidateModelState(ModelType = typeof(SettingsModel))]
        public IActionResult SettingsSave(SettingsModel settingsModel)
        {
            _stripeSettings.Description = settingsModel.Description;
            _stripeSettings.TestPublishableKey = settingsModel.TestPublishableKey;
            _stripeSettings.TestSecretKey = settingsModel.TestSecretKey;
            _stripeSettings.AdditionalFee = settingsModel.AdditionalFee;
            _stripeSettings.AuthorizeOnly = settingsModel.AuthorizeOnly;
            _stripeSettings.EnableTestMode = settingsModel.EnableTestMode;
            _stripeSettings.PublishableKey = settingsModel.PublishableKey;
            _stripeSettings.SecretKey = settingsModel.SecretKey;
            _stripeSettings.UsePercentageForAdditionalFee = settingsModel.UsePercentageForAdditionalFee;
            _stripeSettings.UseRedirectionFlow = settingsModel.UseRedirectionFlow;
            _settingService.Save(_stripeSettings, CurrentStore.Id);
            return R.Success.Result;
        }
    }
}