using FluentValidation;
using Genesis.Infrastructure.Mvc.Models;
using Genesis.Infrastructure.Mvc.Validator;

namespace Payments.TwoCheckout.Models
{
    public class SettingsModel : GenesisModel, IRequiresValidations<SettingsModel>
    {
        public string SellerId { get; set; }

        public string PublishableKey { get; set; }

        public string PrivateKey { get; set; }

        public bool EnableTestMode { get; set; }

        public bool UsePercentageForAdditionalFee { get; set; }

        public decimal AdditionalFee { get; set; }

        public string Description { get; set; }

        public void SetupValidationRules(ModelValidator<SettingsModel> v)
        {
            v.RuleFor(x => x.SellerId).NotEmpty();
            v.RuleFor(x => x.PrivateKey).NotEmpty();
        }
    }
}