using System;
using System.Collections.Generic;
using EvenCart.Core.Plugins;
using EvenCart.Infrastructure.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Google.Components
{
    [ViewComponent(Name = WidgetSystemName)]
    public class LoginButtonWidget : FoundationComponent, IWidget
    {
       
        public const string WidgetSystemName = "GoogleLoginButton";
        public override IViewComponentResult Invoke(object data = null)
        {
            return R.Success.ComponentResult;
        }

        public string DisplayName => "Login with Google button";

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