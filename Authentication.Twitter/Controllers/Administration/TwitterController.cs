using EvenCart.Services.Settings;
using EvenCart.Infrastructure.Mvc;
using EvenCart.Infrastructure.Mvc.Attributes;
using EvenCart.Infrastructure.Routing;
using Microsoft.AspNetCore.Mvc;
using Authentication.Twitter.Models;

namespace Authentication.Twitter.Controllers.Administration
{
    public class TwitterController : FoundationPluginAdminController
    {
        private readonly TwitterSettings _twitterSettings;
        private readonly ISettingService _settingService;
        public TwitterController(TwitterSettings twitterSettings, ISettingService settingService)
        {
            _twitterSettings = twitterSettings;
            _settingService = settingService;
        }
        [HttpGet("settings", Name = TwitterConfig.TwitterSettingsRouteName)]
        public IActionResult Settings()
        {
            var settingsModel = new SettingsModel()
            {
                ClientId = _twitterSettings.ClientId,
                ClientSecret = _twitterSettings.ClientSecret,
            };
            return R.Success.With("settings", settingsModel).Result;
        }

        [DualPost("settings", Name = TwitterConfig.TwitterSettingsRouteName, OnlyApi = true)]
        [ValidateModelState(ModelType = typeof(SettingsModel))]
        public IActionResult SettingsSave(SettingsModel settingsModel)
        {
            _twitterSettings.ClientId = settingsModel.ClientId;
            _twitterSettings.ClientSecret = settingsModel.ClientSecret;
            _settingService.Save(_twitterSettings);
            return R.Success.Result;
        }
    }
}