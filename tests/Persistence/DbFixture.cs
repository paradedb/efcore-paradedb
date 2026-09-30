using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace ParadeDB.EntityFrameworkCore.Tests.Persistence;

public sealed class DbFixture : IAsyncInitializer, IAsyncDisposable
{
    private PostgreSqlContainer? _container;

    private DbContextOptions<TestDbContext> _options = null!;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:18")
            .WithImage("paradedb/paradedb:0.25.0-pg18")
            .WithDatabase("pg_search_test")
            .WithUsername("test")
            .WithPassword("Pass!w0rd1")
            .Build();

        await _container.StartAsync();

        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(_container.GetConnectionString(), o => o.UseParadeDb())
            .Options;

        await using var context = new TestDbContext(_options);
        await context.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS vector");
        await context.Database.ExecuteSqlRawAsync(
            """
            DO $$
            BEGIN
              IF to_regclass('public.mock_items') IS NULL THEN
                CALL paradedb.create_bm25_test_table(
                  schema_name => 'public',
                  table_name => 'mock_items'
                );
              END IF;
            END $$;
            """
        );

        await context.Database.ExecuteSqlRawAsync(
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

        await context.Database.ExecuteSqlRawAsync(
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
            )
            WITH (key_field='Id');
            """
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    public TestDbContext CreateContext() => new(_options);
}
