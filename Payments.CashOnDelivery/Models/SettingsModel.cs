using EvenCart.Infrastructure.Mvc.Models;

namespace Payments.CashOnDelivery.Models
{
    public class SettingsModel : FoundationModel
    {
        public bool UsePercentageForAdditionalFee { get; set; }

        public decimal AdditionalFee { get; set; }
    }
}