using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Genesis.Infrastructure.Mvc.Models;
using Genesis.Infrastructure.Mvc.Validator;

namespace Shipping.UPS.Models
{
    public class SettingsModel : GenesisModel, IRequiresValidations<SettingsModel>
    {
        public bool DebugMode { get; set; }

        public string ShipperNumber { get; set; }

        public string LicenseNumber { get; set; }

        public string UserId { get; set; }

        public string Password { get; set; }

        public decimal AdditionalFee { get; set; }

        public PickupType PickupType { get; set; }

        public PackagingType DefaultPackagingType { get; set; }

        public CustomerClassificationType CustomerClassificationType { get; set; }

        public IList<string> ActiveServices { get; set; }

        public void SetupValidationRules(ModelValidator<SettingsModel> v)
        {
            v.RuleFor(x => x.LicenseNumber).NotEmpty();
            v.RuleFor(x => x.UserId).NotEmpty();
            v.RuleFor(x => x.Password).NotEmpty();
            v.RuleFor(x => x.ActiveServices).NotNull().Must(x => x.Any());
        }
    }
}