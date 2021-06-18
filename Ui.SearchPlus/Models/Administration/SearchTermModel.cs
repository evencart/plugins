using FluentValidation;
using Genesis.Infrastructure.Mvc.Models;
using Genesis.Infrastructure.Mvc.Validator;

namespace Ui.SearchPlus.Models.Administration
{
    public class SearchTermModel : GenesisEntityModel, IRequiresValidations<SearchTermModel>
    {
        public string Term { get; set; }

       public string TermCategory { get; set; }

       public int Score { get; set; }

        public void SetupValidationRules(ModelValidator<SearchTermModel> v)
        {
            v.RuleFor(x => x.Term).NotEmpty();
        }
    }
}