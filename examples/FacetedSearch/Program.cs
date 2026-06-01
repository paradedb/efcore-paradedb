using System.Text.Json;
using FacetedSearch.Data;
using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;

const string connectionString =
    "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=paradedb_faceted_search";

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
    .MockItems.Where(x => EF.Functions.MatchAll(x.Description, searchQuery))
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

var facets = await dbContext
    .MockItems.Where(x => EF.Functions.MatchAll(x.Description, searchQuery))
    .Select(x => new
    {
        Category = EF.Functions.AggOver(new { terms = new { field = "category" } }),
        Rating = EF.Functions.AggOver(new { terms = new { field = "rating" } }),
        Color = EF.Functions.AggOver(new { terms = new { field = "metadata.color" } }),
    })
    .FirstOrDefaultAsync();

Console.WriteLine("\nFacet buckets:");
if (facets is not null)
{
    static void PrintFacets(string label, JsonElement? json)
    {
        if (json is null)
        {
            return;
        }

        if (!json.Value.TryGetProperty("buckets", out var buckets))
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

    PrintFacets("category_terms", facets.Category);
    PrintFacets("rating_terms", facets.Rating);
    PrintFacets("metadata.color_terms", facets.Color);
}

Console.WriteLine();
Console.WriteLine(new string('=', 60));
Console.WriteLine("Done.");
