using System.Text.Json;
using FacetedSearch.Data;
using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shared;

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(ExampleSetup.ConnectionString, o => o.UseParadeDb())
    .UseSnakeCaseNamingConvention()
    .Options;

await using var dbContext = new AppDbContext(options);

Console.WriteLine(new string('=', 60));
Console.WriteLine("Faceted Search Example");
Console.WriteLine(new string('=', 60));

await ExampleSetup.SetupMockItemsAsync(dbContext);

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
        CategoryTerms = EF.Functions.AggOver(
            new
            {
                terms = new
                {
                    field = "category",
                    size = 10,
                    order = new { _count = "desc" },
                },
            }
        ),
        RatingTerms = EF.Functions.AggOver(
            new
            {
                terms = new
                {
                    field = "rating",
                    size = 10,
                    order = new { _count = "desc" },
                },
            }
        ),
        ColorTerms = EF.Functions.AggOver(
            new
            {
                terms = new
                {
                    field = "metadata.color",
                    size = 10,
                    order = new { _count = "desc" },
                },
            }
        ),
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
    var desc = item.Description[..Math.Min(50, item.Description.Length)];
    Console.WriteLine(
        $"  - {desc}... [{item.Category}] (rating: {item.Rating}, {stock}, color: {color})"
    );
}

Console.WriteLine("\nFacet buckets:");
if (results.Count > 0)
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

    PrintFacets("category_terms", results[0].CategoryTerms);
    PrintFacets("rating_terms", results[0].RatingTerms);
    PrintFacets("metadata.color_terms", results[0].ColorTerms);
}

Console.WriteLine();
Console.WriteLine(new string('=', 60));
Console.WriteLine("Done.");
