using Authentication.Google.Components;
using EvenCart.Core.Infrastructure;
using EvenCart.Core.Plugins;
using EvenCart.Infrastructure;
using EvenCart.Infrastructure.Plugins;
using EvenCart.Services.Settings;

namespace Authentication.Google
{
    public class GoogleAuthPlugin : FoundationPlugin
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
            var widgetId = DependencyResolver.Resolve<IPluginAccountant>().AddWidget(LoginButtonWidget.WidgetSystemName, "EvenCart.Authentication.Google", "login");
            _settingService.Save(new GoogleSettings()
            {
                WidgetId = widgetId,
                ClientId = "xxxx",
                ClientSecret = "xxxx"
            }, ApplicationEngine.CurrentStore.Id);
        }

        public override void Uninstall()
        {
            base.Uninstall();
            var settings = DependencyResolver.Resolve<GoogleSettings>();
            DependencyResolver.Resolve<IPluginAccountant>().DeleteWidget(settings.WidgetId);
            _settingService.DeleteSettings<GoogleSettings>(ApplicationEngine.CurrentStore.Id);
        }

        public override string ConfigurationUrl => ApplicationEngine.RouteUrl(GoogleConfig.GoogleSettingsRouteName);
    }
}
