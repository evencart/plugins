using EvenCart.Core.Config;

namespace Authentication.Google
{
    public class GoogleSettings : ISettingGroup
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string WidgetId { get; set; }
    }
}