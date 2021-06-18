using Authentication.Facebook.Components;
using Genesis;
using Genesis.Modules.Pluggable;
using Genesis.Modules.Settings;
using Genesis.Plugins;

namespace Authentication.Facebook
{
    public class FacebookAuthPlugin : GenesisPlugin
    {
        private readonly ISettingService _settingService;
        public FacebookAuthPlugin(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public override void Install()
        {
            base.Install();
            //install the widget
            var widgetId = D.Resolve<IPluginAccountant>().AddWidget(LoginButtonWidget.WidgetSystemName, "EvenCart.Authentication.Facebook", "login");
            _settingService.Save(new FacebookSettings()
            {
                WidgetId = widgetId,
                ClientId = "xxxx",
                ClientSecret = "xxxx"
            }, GenesisEngine.Instance.CurrentStore.Id);
        }

        public override void Uninstall()
        {
            base.Uninstall();
            var settings = D.Resolve<FacebookSettings>();
            D.Resolve<IPluginAccountant>().DeleteWidget(settings.WidgetId);
            _settingService.DeleteSettings<FacebookSettings>(GenesisEngine.Instance.CurrentStore.Id);
        }

        public override string ConfigurationUrl => GenesisEngine.Instance.RouteUrl(FacebookConfig.FacebookSettingsRouteName);
    }
}
