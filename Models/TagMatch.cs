
namespace SmartDocumentReview.Models
{
    public class TagMatch
    {
        public int Id { get; set; }
        public string Keyword { get; set; } = default!;
        public string SectionTitle { get; set; } = default!;
        public string MatchedText { get; set; } = default!;
        public string CreatedBy { get; set; } = default!;
    }
}
