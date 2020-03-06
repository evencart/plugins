using EvenCart.Services.Settings;
using EvenCart.Infrastructure.Mvc;
using EvenCart.Infrastructure.Mvc.Attributes;
using EvenCart.Infrastructure.Routing;
using Microsoft.AspNetCore.Mvc;
using Authentication.Facebook.Models;

namespace Authentication.Facebook.Controllers.Administration
{
    public class FacebookController : FoundationPluginAdminController
    {
        private readonly FacebookSettings _facebookSettings;
        private readonly ISettingService _settingService;
        public FacebookController(FacebookSettings facebookSettings, ISettingService settingService)
        {
            _facebookSettings = facebookSettings;
            _settingService = settingService;
        }
        [HttpGet("settings", Name = FacebookConfig.FacebookSettingsRouteName)]
        public IActionResult Settings()
        {
            var settingsModel = new SettingsModel()
            {
                ClientId = _facebookSettings.ClientId,
                ClientSecret = _facebookSettings.ClientSecret,
            };
            return R.Success.With("settings", settingsModel).Result;
        }

        [DualPost("settings", Name = FacebookConfig.FacebookSettingsRouteName, OnlyApi = true)]
        [ValidateModelState(ModelType = typeof(SettingsModel))]
        public IActionResult SettingsSave(SettingsModel settingsModel)
        {
            _facebookSettings.ClientId = settingsModel.ClientId;
            _facebookSettings.ClientSecret = settingsModel.ClientSecret;
            _settingService.Save(_facebookSettings, CurrentStore.Id);
            return R.Success.Result;
        }
    }
}