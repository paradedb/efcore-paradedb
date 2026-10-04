using Microsoft.EntityFrameworkCore;
using Npgsql;
using ParadeDB.EntityFrameworkCore.Extensions;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace ParadeDB.EntityFrameworkCore.Tests.Persistence;

public sealed class DbFixture : IAsyncInitializer, IAsyncDisposable
{
    private PostgreSqlContainer? _container;
    private readonly string _schemaName = $"paradedb_tests_{Guid.NewGuid():N}";
    private bool _schemaCreated;

    private DbContextOptions<TestDbContext> _options = null!;

    public async Task InitializeAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("PARADEDB_TEST_DSN");
        if (string.IsNullOrEmpty(connectionString))
        {
            _container = new PostgreSqlBuilder("postgres:18")
                .WithImage(
                    Environment.GetEnvironmentVariable("PARADEDB_IMAGE")
                        ?? $"paradedb/paradedb:{Environment.GetEnvironmentVariable("PARADEDB_VERSION") ?? "0.26.0"}-pg{Environment.GetEnvironmentVariable("PARADEDB_POSTGRES_VERSION") ?? "18"}"
                )
                .WithDatabase("pg_search_test")
                .WithUsername("test")
                .WithPassword("Pass!w0rd1")
                .Build();

            await _container.StartAsync();
            connectionString = _container.GetConnectionString();
        }

        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(connectionString, o => o.UseParadeDb())
            .Options;

        await using var context = new TestDbContext(_options);
        await context.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS vector");
        // The schema identifier is generated from a GUID, never supplied by the caller.
        var createSchemaSql = $"CREATE SCHEMA \"{_schemaName}\"";
        await context.Database.ExecuteSqlRawAsync(createSchemaSql);
        _schemaCreated = true;
        var connectionBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            SearchPath = $"{_schemaName}, public",
        };
        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(connectionBuilder.ConnectionString, o => o.UseParadeDb())
            .Options;

        await using var isolatedContext = new TestDbContext(_options);
        await isolatedContext.Database.ExecuteSqlInterpolatedAsync(
            $"CALL paradedb.create_paradedb_test_table(schema_name => {_schemaName}, table_name => 'mock_items')"
        );

        await isolatedContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE "MockItems" AS
            SELECT
              id AS "Id",
              description AS "Description",
              rating AS "Rating",
              category AS "Category",
              in_stock AS "InStock",
              metadata AS "Metadata",
              created_at AS "CreatedAt",
              last_updated_date AS "LastUpdatedDate",
              latest_available_time AS "LatestAvailableTime",
              weight_range AS "WeightRange",
              embedding AS "Embedding"
            FROM mock_items;
            """
        );

        await isolatedContext.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS search_idx ON "MockItems"
            USING paradedb (
              "Id",
              "Description",
              ("Description"::pdb.simple('alias=description_simple')),
              "Category",
              "Rating",
              "InStock",
              "CreatedAt",
              "Metadata",
              "WeightRange",
              "Embedding" vector_cosine_ops
            );
            """
        );
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_schemaCreated)
            {
                await using var context = new TestDbContext(_options);
                var dropSchemaSql = $"DROP SCHEMA \"{_schemaName}\" CASCADE";
                await context.Database.ExecuteSqlRawAsync(dropSchemaSql);
                _schemaCreated = false;
            }
        }
        finally
        {
            if (_container is not null)
            {
                await _container.DisposeAsync();
            }
        }
    }

    public TestDbContext CreateContext() => new(_options);
}
