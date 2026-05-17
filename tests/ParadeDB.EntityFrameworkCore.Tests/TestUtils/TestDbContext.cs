using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Tests.TestModels;

namespace ParadeDB.EntityFrameworkCore.Tests.TestUtils;

public sealed class TestDbContext : DbContext
{
    public TestDbContext()
        : base(CreateDefaultOptions()) { }

    public DbSet<Product> Products => Set<Product>();

    private static DbContextOptions<TestDbContext> CreateDefaultOptions() =>
        new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(
                connectionString: "Host=localhost;Database=fake;Username=fake;Password=fake",
                npgsqlOptionsAction: o => o.UseParadeDb()
            )
            .UseSnakeCaseNamingConvention()
            .Options;
}
