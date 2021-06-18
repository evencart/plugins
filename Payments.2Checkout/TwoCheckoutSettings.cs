using Genesis.Config;

namespace Payments.TwoCheckout
{
    public class TwoCheckoutSettings : ISettingGroup
    {
        public string SellerId { get; set; }

        public string PublishableKey { get; set; }

        public string PrivateKey { get; set; }

        public bool EnableTestMode { get; set; }

        public bool UsePercentageForAdditionalFee { get; set; }
       
        public decimal AdditionalFee { get; set; }

        public string Description { get; set; }
    }
}