using Genesis.Infrastructure.Mvc.Models;

namespace Ui.SearchPlus.Data
{
    public class AutoCompleteResultModel : GenesisModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string ThumbnailUrl { get; set; }
    }
}