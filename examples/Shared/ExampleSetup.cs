using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Shared;

public static class ExampleSetup
{
    public static string ConnectionString
    {
        get
        {
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (
                !string.IsNullOrWhiteSpace(databaseUrl)
                && Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri)
            )
            {
                var userInfo = uri.UserInfo.Split(':', 2);
                var database = uri.AbsolutePath.TrimStart('/');

                return new NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port > 0 ? uri.Port : 5432,
                    Username = Uri.UnescapeDataString(userInfo[0]),
                    Password = userInfo.Length == 2 ? Uri.UnescapeDataString(userInfo[1]) : "",
                    Database = string.IsNullOrWhiteSpace(database) ? "postgres" : database,
                }.ConnectionString;
            }

            return new NpgsqlConnectionStringBuilder
            {
                Host = Environment.GetEnvironmentVariable("PGHOST") ?? "localhost",
                Port = int.Parse(Environment.GetEnvironmentVariable("PGPORT") ?? "5432"),
                Username = Environment.GetEnvironmentVariable("PGUSER") ?? "postgres",
                Password = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "postgres",
                Database = Environment.GetEnvironmentVariable("PGDATABASE") ?? "postgres",
            }.ConnectionString;
        }
    }

    public static async Task SetupMockItemsAsync(DbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS pg_search");
        await db.Database.ExecuteSqlRawAsync(
            "CALL paradedb.create_bm25_test_table(schema_name => 'public', table_name => 'mock_items')"
        );
        await db.Database.ExecuteSqlRawAsync("DROP INDEX IF EXISTS mock_items_bm25_idx");
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX mock_items_bm25_idx ON mock_items
            USING bm25 (
                id,
                description,
                rating,
                (category::pdb.literal('alias=category')),
                metadata
            )
            WITH (key_field='id', json_fields='{{"metadata":{{"fast":true}}}}');
            """
        );
    }

    public static async Task SetupAutocompleteAsync(DbContext db)
    {
        await SetupMockItemsAsync(db);
        await db.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS autocomplete_items CASCADE");
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE autocomplete_items (
                id integer PRIMARY KEY,
                description text NOT NULL,
                category varchar(100) NOT NULL,
                rating integer NOT NULL,
                in_stock boolean NOT NULL DEFAULT true,
                created_at timestamp DEFAULT CURRENT_TIMESTAMP
            );
            """
        );
        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO autocomplete_items (id, description, category, rating, in_stock, created_at)
            SELECT id, description, category, rating, in_stock, created_at
            FROM mock_items;
            """
        );
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX autocomplete_items_idx ON autocomplete_items
            USING bm25 (
                id,
                (description::pdb.unicode_words),
                (description::pdb.ngram(3,8,'alias=description_ngram')),
                (category::pdb.literal('alias=category'))
            )
            WITH (key_field='id');
            """
        );
    }

    public static async Task SetupHybridAsync(DbContext db)
    {
        await SetupMockItemsAsync(db);
        await db.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS vector");
        await db.Database.ExecuteSqlRawAsync(
            """
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_name = 'mock_items' AND column_name = 'embedding'
                ) THEN
                    ALTER TABLE mock_items ADD COLUMN embedding vector(384);
                END IF;
            END $$;
            """
        );
    }
}
