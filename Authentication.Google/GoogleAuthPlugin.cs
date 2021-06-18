using Authentication.Google.Components;
using Genesis;
using Genesis.Modules.Pluggable;
using Genesis.Modules.Settings;
using Genesis.Plugins;

namespace Authentication.Google
{
    public class GoogleAuthPlugin : GenesisPlugin
    {
        private readonly ISettingService _settingService;
        public GoogleAuthPlugin(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public override void Install()
        {
            base.Install();
            //install the widget
            var widgetId = D.Resolve<IPluginAccountant>().AddWidget(LoginButtonWidget.WidgetSystemName, "EvenCart.Authentication.Google", "login");
            _settingService.Save(new GoogleSettings()
            {
                WidgetId = widgetId,
                ClientId = "xxxx",
                ClientSecret = "xxxx"
            }, GenesisEngine.Instance.CurrentStore.Id);
        }

        public override void Uninstall()
        {
            base.Uninstall();
            var settings = D.Resolve<GoogleSettings>();
            D.Resolve<IPluginAccountant>().DeleteWidget(settings.WidgetId);
            _settingService.DeleteSettings<GoogleSettings>(GenesisEngine.Instance.CurrentStore.Id);
        }

        public override string ConfigurationUrl => GenesisEngine.Instance.RouteUrl(GoogleConfig.GoogleSettingsRouteName);
    }
}
