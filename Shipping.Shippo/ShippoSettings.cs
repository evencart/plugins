using Genesis.Config;

namespace Shipping.Shippo
{
    public class ShippoSettings : ISettingGroup
    {
        public bool DebugMode { get; set; }

        public string LiveApiKey { get; set; }

        public string TestApiKey { get; set; }

        public bool UseSinglePackageShipment { get; set; }
    }
}