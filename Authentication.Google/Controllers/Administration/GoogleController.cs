using Microsoft.AspNetCore.Mvc;
using Authentication.Google.Models;
using Genesis.Infrastructure.Mvc;
using Genesis.Infrastructure.Mvc.Attributes;
using Genesis.Modules.Settings;
using Genesis.Routing;

namespace Authentication.Google.Controllers.Administration
{
    public class GoogleController : GenesisPluginAdminController
    {
        private readonly GoogleSettings _googleSettings;
        private readonly ISettingService _settingService;
        public GoogleController(GoogleSettings googleSettings, ISettingService settingService)
        {
            _googleSettings = googleSettings;
            _settingService = settingService;
        }
        [HttpGet("settings", Name = GoogleConfig.GoogleSettingsRouteName)]
        public IActionResult Settings()
        {
            var settingsModel = new SettingsModel()
            {
                ClientId = _googleSettings.ClientId,
                ClientSecret = _googleSettings.ClientSecret,
            };
            return R.Success.With("settings", settingsModel).Result;
        }

        [DualPost("settings", Name = GoogleConfig.GoogleSettingsRouteName, OnlyApi = true)]
        [ValidateModelState(ModelType = typeof(SettingsModel))]
        public IActionResult SettingsSave(SettingsModel settingsModel)
        {
            _googleSettings.ClientId = settingsModel.ClientId;
            _googleSettings.ClientSecret = settingsModel.ClientSecret;
            _settingService.Save(_googleSettings, CurrentStore.Id);
            return R.Success.Result;
        }
    }
}