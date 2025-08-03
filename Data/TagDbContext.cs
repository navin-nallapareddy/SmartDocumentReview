using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartDocumentReview.Models;

namespace SmartDocumentReview.Data
{
    public class TagDbContext : DbContext
    {
        public TagDbContext(DbContextOptions<TagDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<TagMatch> TagMatches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Convert table and column names to lower case to make comparisons case-insensitive
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entity.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                {
                    entity.SetTableName(tableName.ToLowerInvariant());
                }

                foreach (var property in entity.GetProperties())
                {
                    var columnName = property.GetColumnBaseName();
                    property.SetColumnName(columnName.ToLowerInvariant());
                }
            }
        }
    }
}