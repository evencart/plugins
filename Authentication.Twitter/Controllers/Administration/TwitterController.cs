using Microsoft.AspNetCore.Mvc;
using Authentication.Twitter.Models;
using Genesis.Infrastructure.Mvc;
using Genesis.Infrastructure.Mvc.Attributes;
using Genesis.Modules.Settings;
using Genesis.Routing;

namespace Authentication.Twitter.Controllers.Administration
{
    public class TwitterController : GenesisPluginAdminController
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
            _settingService.Save(_twitterSettings, CurrentStore.Id);
            return R.Success.Result;
        }
    }
}