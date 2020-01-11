using EvenCart.Core.Config;

namespace Authentication.Facebook
{
    public class FacebookSettings : ISettingGroup
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string WidgetId { get; set; }
    }
}