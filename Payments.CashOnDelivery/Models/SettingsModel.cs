using Genesis.Infrastructure.Mvc.Models;

namespace Payments.CashOnDelivery.Models
{
    public class SettingsModel : GenesisModel
    {
        public bool UsePercentageForAdditionalFee { get; set; }

        public decimal AdditionalFee { get; set; }
    }
}