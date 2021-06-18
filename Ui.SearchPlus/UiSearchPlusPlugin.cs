using System.Collections.Generic;
using DotEntity.Versioning;
using Genesis;
using Genesis.Modules.Pluggable;
using Genesis.Modules.Settings;
using Genesis.Plugins;
using Ui.SearchPlus.Components;
using Ui.SearchPlus.Versions;

namespace Ui.SearchPlus
{
    public class UiSearchPlusPlugin : DatabasePlugin
    {
        public const string DefaultSearchBoxId = "global-search";

        private readonly ISettingService _settingService;

        public UiSearchPlusPlugin(ISettingService settingService)
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
            GenesisEngine.Instance.RouteUrl(UiSearchPlusRouteNames.UiSearchPlusConfigure);

        public override void Install()
        {
            base.Install();
            //install the widget
            var widgetId = D.Resolve<IPluginAccountant>().AddWidget(SearchPlusWidget.WidgetSystemName, "EvenCart.Ui.SearchPlus", "after_global_search");
            _settingService.Save(new SearchPlusSettings()
            {
                WidgetId = widgetId,
                SearchBoxId = DefaultSearchBoxId,
                NumberOfAutoCompleteResults = 10,
                ShowTermCategory = false
            }, GenesisEngine.Instance.CurrentStore.Id);
        }

        public override void Uninstall()
        {
            base.Uninstall();
            var settings = D.Resolve<SearchPlusSettings>();
            D.Resolve<IPluginAccountant>().DeleteWidget(settings.WidgetId);
            _settingService.DeleteSettings<SearchPlusSettings>(GenesisEngine.Instance.CurrentStore.Id);
        }
    }
}
