using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ParadeDB.EntityFrameworkCore.Extensions;

namespace Shared
{
    public abstract class ExampleDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
        where TContext : DbContext
    {
        protected abstract string ConnectionString { get; }

        protected abstract TContext Create(DbContextOptions<TContext> options);

        public TContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<TContext>()
                .UseNpgsql(ConnectionString, o => o.UseParadeDb())
                .UseSnakeCaseNamingConvention()
                .Options;

            return Create(options);
        }
    }
}

namespace Autocomplete.Data
{
    public class AppDbContextFactory : Shared.ExampleDbContextFactory<AppDbContext>
    {
        protected override string ConnectionString =>
            "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=paradedb_autocomplete";

        protected override AppDbContext Create(DbContextOptions<AppDbContext> options) =>
            new(options);
    }
}

namespace FacetedSearch.Data
{
    public class AppDbContextFactory : Shared.ExampleDbContextFactory<AppDbContext>
    {
        protected override string ConnectionString =>
            "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=paradedb_faceted_search";

        protected override AppDbContext Create(DbContextOptions<AppDbContext> options) =>
            new(options);
    }
}

namespace HybridRrf.Data
{
    public class AppDbContextFactory : Shared.ExampleDbContextFactory<AppDbContext>
    {
        protected override string ConnectionString =>
            "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=paradedb_hybrid_rrf";

        protected override AppDbContext Create(DbContextOptions<AppDbContext> options) =>
            new(options);
    }
}

namespace MoreLikeThis.Data
{
    public class AppDbContextFactory : Shared.ExampleDbContextFactory<AppDbContext>
    {
        protected override string ConnectionString =>
            "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=paradedb_more_like_this";

        protected override AppDbContext Create(DbContextOptions<AppDbContext> options) =>
            new(options);
    }
}

namespace Quickstart.Data
{
    public class AppDbContextFactory : Shared.ExampleDbContextFactory<AppDbContext>
    {
        protected override string ConnectionString =>
            "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=paradedb_quickstart";

        protected override AppDbContext Create(DbContextOptions<AppDbContext> options) =>
            new(options);
    }
}

namespace Rag.Data
{
    public class AppDbContextFactory : Shared.ExampleDbContextFactory<AppDbContext>
    {
        protected override string ConnectionString =>
            "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=paradedb_rag";

        protected override AppDbContext Create(DbContextOptions<AppDbContext> options) =>
            new(options);
    }
}
