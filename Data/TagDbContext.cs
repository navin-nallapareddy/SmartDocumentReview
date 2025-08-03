using Microsoft.EntityFrameworkCore;
using SmartDocumentReview.Models;

namespace SmartDocumentReview.Data
{
    public class TagDbContext : DbContext
    {
        public TagDbContext(DbContextOptions<TagDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<TagMatch> TagMatches { get; set; }
    }
}