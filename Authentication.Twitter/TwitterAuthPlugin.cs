using Authentication.Twitter.Components;
using Genesis;
using Genesis.Modules.Pluggable;
using Genesis.Modules.Settings;
using Genesis.Plugins;

namespace Authentication.Twitter
{
    public class TwitterAuthPlugin : GenesisPlugin
    {
        private readonly ISettingService _settingService;
        public TwitterAuthPlugin(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public override void Install()
        {
            base.Install();
            //install the widget
            var widgetId = D.Resolve<IPluginAccountant>().AddWidget(LoginButtonWidget.WidgetSystemName, "EvenCart.Authentication.Twitter", "login");
            _settingService.Save(new TwitterSettings()
            {
                WidgetId = widgetId,
                ClientId = "xxxx",
                ClientSecret = "xxxx"
            }, GenesisEngine.Instance.CurrentStore.Id);
        }

        public override void Uninstall()
        {
            base.Uninstall();
            var settings = D.Resolve<TwitterSettings>();
            D.Resolve<IPluginAccountant>().DeleteWidget(settings.WidgetId);
            _settingService.DeleteSettings<TwitterSettings>(GenesisEngine.Instance.CurrentStore.Id);
        }

        public override string ConfigurationUrl => GenesisEngine.Instance.RouteUrl(TwitterConfig.TwitterSettingsRouteName);
    }
}
