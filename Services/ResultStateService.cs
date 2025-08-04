using SmartDocumentReview.Models;

namespace SmartDocumentReview.Services
{
    public class ResultStateService
    {
        public List<TagMatch> Matches { get; set; } = new();
        public List<Keyword> Keywords { get; set; } = new();
    }
}
