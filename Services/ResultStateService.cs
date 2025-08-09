using SmartDocumentReview.Models;
using System.Linq;

namespace SmartDocumentReview.Services
{
    public class ResultStateService
    {
        public List<TagMatch> Matches { get; set; } = new();
        public List<Keyword> Keywords { get; set; } = new();

        public IEnumerable<IGrouping<int, TagMatch>> GroupByPage() => Matches.GroupBy(m => m.PageNumber);
    }
}
