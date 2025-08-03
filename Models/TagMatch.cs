namespace SmartDocumentReview.Models
{
    public class TagMatch
    {
        public int Id { get; set; }
        public string Keyword { get; set; }
        public string SectionTitle { get; set; }
        public string MatchedText { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DocumentId { get; set; }
    }
}