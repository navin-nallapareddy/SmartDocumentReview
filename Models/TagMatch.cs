using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDocumentReview.Models
{
    public class TagMatch
    {
        public int Id { get; set; }
        public string Keyword { get; set; }
        public string SectionTitle { get; set; }
        public string MatchedText { get; set; }
        public string CreatedBy { get; set; }
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }
        public int DocumentId { get; set; }
        public int PageNumber { get; set; }
        public float PageX { get; set; }
        public float PageY { get; set; }
        public float Width { get; set; }
    }
}
