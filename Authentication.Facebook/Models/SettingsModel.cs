using FluentValidation;
using Genesis.Infrastructure.Mvc.Models;
using Genesis.Infrastructure.Mvc.Validator;

namespace Authentication.Facebook.Models
{
    public class SettingsModel : GenesisModel, IRequiresValidations<SettingsModel>
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public void SetupValidationRules(ModelValidator<SettingsModel> v)
        {
            v.RuleFor(x => x.ClientSecret).NotEmpty();
            v.RuleFor(x => x.ClientId).NotEmpty();
        }
    }
}