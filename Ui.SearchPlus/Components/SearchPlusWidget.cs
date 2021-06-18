using System;
using System.Collections.Generic;
using Genesis.Extensions;
using Genesis.Infrastructure.Mvc;
using Genesis.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace Ui.SearchPlus.Components
{
    [ViewComponent(Name = WidgetSystemName)]
    public class SearchPlusWidget : GenesisComponent, IWidget
    {
        private readonly SearchPlusSettings _searchPlusSettings;

        public SearchPlusWidget(SearchPlusSettings searchPlusSettings)
        {
            _searchPlusSettings = searchPlusSettings;
        }

        public const string WidgetSystemName = "SearchPlusWidget";
        public override IViewComponentResult Invoke(object data = null)
        {
            var searchBoxId = _searchPlusSettings.SearchBoxId.IsNullEmptyOrWhiteSpace()
                ? UiSearchPlusPlugin.DefaultSearchBoxId
                : _searchPlusSettings.SearchBoxId;
            return R.Success.With("searchBoxId", searchBoxId).ComponentResult;
        }

        public string DisplayName => "Search Plus";

        public string SystemName => WidgetSystemName;

        public IList<string> WidgetZones { get;  }  = new List<string>() { "after_global_search" };

        public bool HasConfiguration { get; } = false;

        public bool SkipDragging { get; } = true;

        public string ConfigurationUrl { get; } = null;

        public Type SettingsType { get; } = null;

        public object GetViewObject(object settings)
        {
            return null;
        }
    }
}