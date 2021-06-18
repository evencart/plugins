using Genesis.Infrastructure.Mvc;
using Genesis.Infrastructure.Mvc.Attributes;
using Genesis.Modules.Settings;
using Genesis.Routing;
using Microsoft.AspNetCore.Mvc;
using Payments.TwoCheckout.Models;

namespace Payments.TwoCheckout.Controllers.Administration
{
    public class TwoCheckoutController : GenesisPluginAdminController
    {
        private readonly TwoCheckoutSettings _twoCheckoutSettings;
        private readonly ISettingService _settingService;
        public TwoCheckoutController(TwoCheckoutSettings twoCheckoutSettings, ISettingService settingService)
        {
            _twoCheckoutSettings = twoCheckoutSettings;
            _settingService = settingService;
        }
        [HttpGet("settings", Name = TwoCheckoutConfig.TwoCheckoutSettingsRouteName)]
        public IActionResult Settings()
        {
            var settingsModel = new SettingsModel()
            {
                Description = _twoCheckoutSettings.Description,
                AdditionalFee = _twoCheckoutSettings.AdditionalFee,
                EnableTestMode = _twoCheckoutSettings.EnableTestMode,
                SellerId = _twoCheckoutSettings.SellerId,
                PrivateKey = _twoCheckoutSettings.PrivateKey,
                PublishableKey = _twoCheckoutSettings.PublishableKey,
                UsePercentageForAdditionalFee = _twoCheckoutSettings.UsePercentageForAdditionalFee
            };
            return R.Success.With("settings", settingsModel).Result;
        }

        [DualPost("settings", Name = TwoCheckoutConfig.TwoCheckoutSettingsRouteName, OnlyApi = true)]
        [ValidateModelState(ModelType = typeof(SettingsModel))]
        public IActionResult SettingsSave(SettingsModel settingsModel)
        {
            _twoCheckoutSettings.Description = settingsModel.Description;
            _twoCheckoutSettings.AdditionalFee = settingsModel.AdditionalFee;
            _twoCheckoutSettings.EnableTestMode = settingsModel.EnableTestMode;
            _twoCheckoutSettings.SellerId = settingsModel.SellerId;
            _twoCheckoutSettings.PrivateKey = settingsModel.PrivateKey;
            _twoCheckoutSettings.PublishableKey = settingsModel.PublishableKey;
            _twoCheckoutSettings.UsePercentageForAdditionalFee = settingsModel.UsePercentageForAdditionalFee;
            _settingService.Save(_twoCheckoutSettings, CurrentStore.Id);
            return R.Success.Result;
        }
    }
}