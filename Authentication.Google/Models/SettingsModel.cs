using EvenCart.Infrastructure.Mvc.Models;
using EvenCart.Infrastructure.Mvc.Validator;
using FluentValidation;

namespace Authentication.Google.Models
{
    public class SettingsModel : FoundationModel, IRequiresValidations<SettingsModel>
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