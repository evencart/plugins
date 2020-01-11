using Authentication.Facebook.Components;
using EvenCart.Core.Infrastructure;
using EvenCart.Core.Plugins;
using EvenCart.Infrastructure.Plugins;
using EvenCart.Services.Plugins;
using EvenCart.Services.Settings;

namespace Authentication.Facebook
{
    public class FacebookAuthPlugin : FoundationPlugin
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
            var widgetId = DependencyResolver.Resolve<IPluginAccountant>().AddWidget(LoginButtonWidget.WidgetSystemName, "EvenCart.Authentication.Facebook", "login");
            _settingService.Save(new FacebookSettings()
            {
                WidgetId = widgetId
            });
        }

        public override void Uninstall()
        {
            base.Uninstall();
            var settings = DependencyResolver.Resolve<FacebookSettings>();
            DependencyResolver.Resolve<IPluginAccountant>().DeleteWidget(settings.WidgetId);
            _settingService.DeleteSettings<FacebookSettings>();
        }
    }
}
