namespace SmartDocumentReview.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}