using Microsoft.EntityFrameworkCore;
public class TagDbContext : DbContext
{
    public DbSet<TagMatch> TagMatches { get; set; }
    public TagDbContext(DbContextOptions<TagDbContext> options) : base(options) { }
}