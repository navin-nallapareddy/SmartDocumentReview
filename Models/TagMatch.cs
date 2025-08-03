public class TagMatch
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string Keyword { get; set; }
    public string SectionTitle { get; set; }
    public string MatchedText { get; set; }
    public int PageNumber { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}