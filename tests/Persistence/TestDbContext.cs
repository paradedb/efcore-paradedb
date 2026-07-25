using Microsoft.EntityFrameworkCore;

namespace ParadeDB.EntityFrameworkCore.Tests.Persistence;

public sealed class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options) { }

    public DbSet<MockItem> MockItems => Set<MockItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MockItem>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Embedding).HasColumnType("vector(8)");
        });
    }
}
