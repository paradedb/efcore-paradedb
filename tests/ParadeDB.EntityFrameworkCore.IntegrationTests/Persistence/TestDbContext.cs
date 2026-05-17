using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.IntegrationTests.Persistence.Entities;

namespace ParadeDB.EntityFrameworkCore.IntegrationTests.Persistence;

public sealed class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>().HasKey(p => p.Id);
        modelBuilder.Entity<Item>().HasKey(p => p.Id);
    }
}
