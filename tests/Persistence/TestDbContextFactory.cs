using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ParadeDB.EntityFrameworkCore.Tests.Persistence;

[ExcludeFromCodeCoverage]
public sealed class TestDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
{
    public TestDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql("Host=localhost;Database=factory;Username=factory;Password=factory")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new TestDbContext(options);
    }
}
