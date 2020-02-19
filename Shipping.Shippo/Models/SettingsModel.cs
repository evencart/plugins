using EvenCart.Infrastructure.Mvc.Models;
using EvenCart.Infrastructure.Mvc.Validator;

namespace Shipping.Shippo.Models
{
    public class SettingsModel : FoundationModel, IRequiresValidations<SettingsModel>
    {
        public bool DebugMode { get; set; }

        public string LiveApiKey { get; set; }

        public string TestApiKey { get; set; }

        public bool UseSinglePackageShipment { get; set; }

        public void SetupValidationRules(ModelValidator<SettingsModel> v)
        {
        }
    }
}