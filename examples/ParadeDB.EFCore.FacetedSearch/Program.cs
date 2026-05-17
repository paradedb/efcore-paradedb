using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ParadeDB.EFCore.Extensions;
using ParadeDB.EFCore.FacetedSearch.Data;

var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var connectionString = config.GetConnectionString("Default");

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(connectionString, o => o.UseParadeDb())
    .UseSnakeCaseNamingConvention()
    .Options;

await using var dbContext = new AppDbContext(options);

await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.MigrateAsync();

Console.WriteLine(new string('=', 60));
Console.WriteLine("Faceted Search Example");
Console.WriteLine(new string('=', 60));

var count = await dbContext.MockItems.CountAsync();
Console.WriteLine($"\nLoaded {count} items");

var searchQuery = "shoes";
Console.WriteLine($"\nQuery: '{searchQuery}'");
Console.WriteLine("\n--- Facets + Rows (Top K) ---");

var results = await dbContext
    .MockItems.FromSqlInterpolated(
        $"""
        SELECT * FROM mock_items
        WHERE id @@@ paradedb.match('description', {searchQuery})
        """
    )
    .Select(x => new
    {
        x.Id,
        x.Description,
        x.Category,
        x.Rating,
        x.InStock,
        x.Metadata,
    })
    .OrderByDescending(x => x.Rating)
    .Take(5)
    .ToListAsync();

Console.WriteLine("Top results:");
foreach (var item in results)
{
    var color =
        item.Metadata?.RootElement.TryGetProperty("color", out var c) == true
            ? c.GetString()
            : "N/A";
    var stock = item.InStock ? "In Stock" : "Out of Stock";
    var desc = item.Description.Length > 50 ? item.Description[..50] : item.Description;
    Console.WriteLine(
        $"  - {desc}... [{item.Category}] (rating: {item.Rating}, {stock}, color: {color})"
    );
}

await dbContext.Database.OpenConnectionAsync();
await using var command = (NpgsqlCommand)dbContext.Database.GetDbConnection().CreateCommand();

command.CommandText = """
    SELECT
        pdb.agg('{"terms":{"field":"category"}}') OVER () as category_terms,
        pdb.agg('{"terms":{"field":"rating"}}') OVER () as rating_terms,
        pdb.agg('{"terms":{"field":"metadata.color"}}') OVER () as color_terms
    FROM mock_items
    WHERE id @@@ paradedb.match('description', @searchQuery)
    ORDER BY rating DESC
    LIMIT 5
    """;
command.Parameters.AddWithValue("@searchQuery", searchQuery);

await using var reader = await command.ExecuteReaderAsync();

Console.WriteLine("\nFacet buckets:");
if (await reader.ReadAsync())
{
    void PrintFacets(string label, int col)
    {
        if (reader.IsDBNull(col))
        {
            return;
        }

        var json = JsonDocument.Parse(reader.GetString(col));

        if (!json.RootElement.TryGetProperty("buckets", out var buckets))
        {
            return;
        }

        Console.WriteLine($"{label} ({buckets.GetArrayLength()} buckets)");

        foreach (var bucket in buckets.EnumerateArray())
        {
            Console.WriteLine(
                $"  - {bucket.GetProperty("key")}: {bucket.GetProperty("doc_count")}"
            );
        }
    }

    PrintFacets("category_terms", 0);
    PrintFacets("rating_terms", 1);
    PrintFacets("metadata.color_terms", 2);
}

Console.WriteLine();
Console.WriteLine(new string('=', 60));
Console.WriteLine("Done.");
