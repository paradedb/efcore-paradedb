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
            .WithImage("paradedb/paradedb:latest")
            .WithDatabase("pg_search_test")
            .WithUsername("test")
            .WithPassword("Pass!w0rd1")
            .Build();

        await _container.StartAsync();

        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(_container.GetConnectionString(), o => o.UseParadeDb())
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var context = new TestDbContext(_options);
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
            CREATE INDEX IF NOT EXISTS search_idx ON mock_items
            USING bm25 (
              id,
              description,
              (description::pdb.simple('alias=description_simple')),
              category,
              rating,
              in_stock,
              created_at,
              metadata,
              weight_range
            )
            WITH (key_field='id');
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
