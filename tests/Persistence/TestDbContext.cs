using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Tests.Persistence.Entities;

namespace ParadeDB.EntityFrameworkCore.Tests.Persistence;

public sealed class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options) { }

    public DbSet<MockItem> MockItems => Set<MockItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MockItem>().HasKey(p => p.Id);
    }
}
