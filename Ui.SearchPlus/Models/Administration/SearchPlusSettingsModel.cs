using FluentValidation;
using Genesis.Infrastructure.Mvc.Models;
using Genesis.Infrastructure.Mvc.Validator;

namespace Ui.SearchPlus.Models.Administration
{
    public class SearchPlusSettingsModel : GenesisModel, IRequiresValidations<SearchPlusSettingsModel>
    {
        public int NumberOfAutoCompleteResults { get; set; }

        public bool ShowTermCategory { get; set; }

        public void SetupValidationRules(ModelValidator<SearchPlusSettingsModel> v)
        {
            v.RuleFor(x => x.NumberOfAutoCompleteResults).GreaterThan(0);
        }
    }
}