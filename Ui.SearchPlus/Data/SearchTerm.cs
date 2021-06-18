using Genesis.Data;

namespace Ui.SearchPlus.Data
{
    public class SearchTerm : GenesisEntity
    {
        public string Term { get; set; }

        public string TermCategory { get; set; }

        public int Score { get; set; }
    }
}