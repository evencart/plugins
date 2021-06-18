using Genesis.Config;

namespace Payments.CashOnDelivery
{
    public class CashOnDeliverySettings : ISettingGroup
    {
        public bool UsePercentageForAdditionalFee { get; set; }
       
        public decimal AdditionalFee { get; set; }
    }
}