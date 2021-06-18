using Genesis.Config;

namespace Authentication.Twitter
{
    public class TwitterSettings : ISettingGroup
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string WidgetId { get; set; }
    }
}