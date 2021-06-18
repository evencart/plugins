using System;
using System.Collections.Generic;
using Genesis.Infrastructure.Mvc;
using Genesis.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Facebook.Components
{
    [ViewComponent(Name = WidgetSystemName)]
    public class LoginButtonWidget : GenesisComponent, IWidget
    {
       
        public const string WidgetSystemName = "FacebookLoginButton";
        public override IViewComponentResult Invoke(object data = null)
        {
            return R.Success.ComponentResult;
        }

        public string DisplayName => "Login with Facebook button";

        public string SystemName => WidgetSystemName;

        public IList<string> WidgetZones { get; } = new List<string>() { "login" };

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