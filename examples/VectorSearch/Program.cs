using HybridRrf.Data;
using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shared;

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(ExampleSetup.ConnectionString, o => o.UseParadeDb())
    .UseSnakeCaseNamingConvention()
    .Options;

await using var dbContext = new AppDbContext(options);

Console.WriteLine(new string('=', 70));
Console.WriteLine("Vector Search");
Console.WriteLine(new string('=', 70));
Console.WriteLine("\nTop-K nearest-neighbor search over a pgvector column");

// The shared search_idx created here covers the embedding column with the
// cosine metric, so it serves the vector queries below
await ExampleSetup.SetupMockItemsAsync(dbContext);

await Demo(dbContext, "Sleek running shoes");
await Demo(dbContext, "Innovative wireless earbuds");
return;

static async Task Demo(AppDbContext db, string seedDescription)
{
    // The mock_items table ships with a pre-populated embedding column; use the
    // embedding of a known item as the query vector
    var queryEmbedding = await db
        .MockItems.Where(x => x.Description == seedDescription)
        .Select(x => x.Embedding!)
        .FirstAsync();

    // The @@@ predicate (EF.Functions.All) and the LIMIT are required for the
    // ParadeDB index to serve the query; the ORDER BY metric must match the
    // index opclass (cosine here)
    var results = await db
        .MockItems.Where(x => EF.Functions.All(x.Id))
        .Select(x => new
        {
            x.Description,
            Distance = EF.Functions.CosineDistance(x.Embedding, queryEmbedding),
        })
        .OrderBy(x => x.Distance)
        .Take(5)
        .ToListAsync();

    Console.WriteLine($"\n{new string('=', 70)}");
    Console.WriteLine($"Similar to: '{seedDescription}'");
    Console.WriteLine(new string('=', 70));

    for (var i = 0; i < results.Count; i++)
    {
        var desc = results[i].Description[..Math.Min(60, results[i].Description.Length)];
        Console.WriteLine($"  {i + 1}. {desc, -60} (distance: {results[i].Distance:F4})");
    }
}
