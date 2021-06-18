using FluentValidation;
using Genesis.Infrastructure.Mvc.Models;
using Genesis.Infrastructure.Mvc.Validator;

namespace Payments.Square.Models
{
    public class SettingsModel : GenesisModel, IRequiresValidations<SettingsModel>
    {
        public string ApplicationId { get; set; }

        public string AccessToken { get; set; }

        public bool EnableSandbox { get; set; }

        public string SandboxApplicationId { get; set; }

        public string SandboxAccessToken { get; set; }

        public bool AuthorizeOnly { get; set; }

        public bool UsePercentageForAdditionalFee { get; set; }

        public decimal AdditionalFee { get; set; }

        public string Description { get; set; }

        public string LocationId { get; set; }

        public void SetupValidationRules(ModelValidator<SettingsModel> v)
        {
            v.RuleFor(x => x.ApplicationId).NotEmpty();
            v.RuleFor(x => x.AccessToken).NotEmpty();
            v.RuleFor(x => SandboxApplicationId).NotEmpty().When(x => x.EnableSandbox);
            v.RuleFor(x => SandboxAccessToken).NotEmpty().When(x => x.EnableSandbox);
        }
    }
}