using System.Collections.Generic;
using DotEntity.Versioning;
using Genesis;
using Genesis.Modules.Pluggable;
using Genesis.Modules.Settings;
using Genesis.Plugins;
using Ui.Slider.Components;
using Ui.Slider.Versions;

namespace Ui.Slider
{
    public class UiSliderPlugin : DatabasePlugin
    {
        private readonly ISettingService _settingService;
        public UiSliderPlugin(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public override IList<IDatabaseVersion> GetDatabaseVersions()
        {
            var versions = base.GetDatabaseVersions();
            versions.Add(new Version1());
            return versions;
        }

        public override string ConfigurationUrl =>
            GenesisEngine.Instance.RouteUrl(UiSliderRouteNames.SlidesList);

        public override void Install()
        {
            base.Install();
            //install the widget
            var widgetId = D.Resolve<IPluginAccountant>().AddWidget(SliderWidget.WidgetSystemName, "EvenCart.Ui.Slider", "slider");
            _settingService.Save(new UiSliderSettings()
            {
                WidgetId = widgetId
            }, GenesisEngine.Instance.CurrentStore.Id);
        }

        public override void Uninstall()
        {
            base.Uninstall();
            var settings = D.Resolve<UiSliderSettings>();
            D.Resolve<IPluginAccountant>().DeleteWidget(settings.WidgetId);
            _settingService.DeleteSettings<UiSliderSettings>(GenesisEngine.Instance.CurrentStore.Id);
        }
    }
}
