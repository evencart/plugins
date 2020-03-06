using Authentication.Twitter.Components;
using EvenCart.Core.Infrastructure;
using EvenCart.Core.Plugins;
using EvenCart.Infrastructure;
using EvenCart.Infrastructure.Plugins;
using EvenCart.Services.Settings;

namespace Authentication.Twitter
{
    public class TwitterAuthPlugin : FoundationPlugin
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
            var widgetId = DependencyResolver.Resolve<IPluginAccountant>().AddWidget(LoginButtonWidget.WidgetSystemName, "EvenCart.Authentication.Twitter", "login");
            _settingService.Save(new TwitterSettings()
            {
                WidgetId = widgetId,
                ClientId = "xxxx",
                ClientSecret = "xxxx"
            }, ApplicationEngine.CurrentStore.Id);
        }

        public override void Uninstall()
        {
            base.Uninstall();
            var settings = DependencyResolver.Resolve<TwitterSettings>();
            DependencyResolver.Resolve<IPluginAccountant>().DeleteWidget(settings.WidgetId);
            _settingService.DeleteSettings<TwitterSettings>(ApplicationEngine.CurrentStore.Id);
        }

        public override string ConfigurationUrl => ApplicationEngine.RouteUrl(TwitterConfig.TwitterSettingsRouteName);
    }
}
