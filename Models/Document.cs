using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDocumentReview.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string FilePath { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string CreatedBy { get; set; } = null!;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}