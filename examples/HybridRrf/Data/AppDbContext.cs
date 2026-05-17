using Microsoft.EntityFrameworkCore;

namespace HybridRrf.Data;

public class AppDbContext : DbContext
{
    public DbSet<MockItemWithEmbedding> MockItems { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MockItemWithEmbedding>().Property(e => e.Id).ValueGeneratedNever();

        modelBuilder
            .Entity<MockItemWithEmbedding>()
            .Property(x => x.Embedding)
            .HasColumnType("vector(384)")
            .HasConversion(
                v => string.Join(",", v!),
                v =>
                    v.Trim('[', ']')
                        .Split(',', StringSplitOptions.TrimEntries)
                        .Select(float.Parse)
                        .ToArray()
            );
    }
}
