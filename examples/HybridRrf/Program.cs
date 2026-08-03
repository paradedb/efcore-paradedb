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
Console.WriteLine("Hybrid Search with Reciprocal Rank Fusion (RRF)");
Console.WriteLine(new string('=', 70));
Console.WriteLine("\nBM25 (keyword) + Vector (semantic)");
Console.WriteLine("RRF formula: score = sum(1 / (k + rank)) across all rankings");

await ExampleSetup.SetupMockItemsAsync(dbContext);

await Demo(dbContext, "running shoes", "Sleek running shoes");
await Demo(dbContext, "wireless earbuds", "Innovative wireless earbuds");
await Demo(dbContext, "bluetooth speaker", "Bluetooth-enabled speaker");

Console.WriteLine("\n" + new string('=', 70));
Console.WriteLine("BM25 results use the ParadeDB EF query builder.");
Console.WriteLine(new string('=', 70));
return;

static async Task Demo(AppDbContext db, string query, string seedDescription)
{
    // The mock_items table ships with a pre-populated embedding column; use the
    // embedding of a known item as the semantic query vector
    var queryEmbedding = await db
        .MockItems.Where(x => x.Description == seedDescription)
        .Select(x => x.Embedding!)
        .FirstAsync();

    var results = await HybridSearch(db, query, queryEmbedding);
    DisplayResults(query, results);
}

static async Task<List<(string Description, double RrfScore)>> HybridSearch(
    AppDbContext db,
    string query,
    float[] queryEmbedding,
    int topK = 20,
    int rrfK = 60,
    int limit = 5
)
{
    var fulltext = await db
        .MockItems.Where(x => EF.Functions.MatchAll(x.Description, query))
        .Select(x => new
        {
            x.Id,
            x.Description,
            Score = EF.Functions.Score(x.Id),
        })
        .OrderByDescending(x => x.Score)
        .Take(topK)
        .ToListAsync();

    var semantic = await db
        .MockItems.Where(x => EF.Functions.All(x.Id))
        .Select(x => new
        {
            x.Id,
            x.Description,
            Distance = EF.Functions.CosineDistance(x.Embedding, queryEmbedding),
        })
        .OrderBy(x => x.Distance)
        .Take(topK)
        .ToListAsync();

    return fulltext
        .Select((x, index) => (x.Id, x.Description, Score: 1.0 / (rrfK + index + 1)))
        .Concat(
            semantic.Select((x, index) => (x.Id, x.Description, Score: 1.0 / (rrfK + index + 1)))
        )
        .GroupBy(x => x.Id)
        .Select(x => (x.First().Description, RrfScore: x.Sum(y => y.Score)))
        .OrderByDescending(x => x.RrfScore)
        .Take(limit)
        .ToList();
}

static void DisplayResults(string query, List<(string Description, double RrfScore)> results)
{
    Console.WriteLine($"\n{new string('=', 70)}");
    Console.WriteLine($"Query: '{query}'");
    Console.WriteLine(new string('=', 70));

    for (var i = 0; i < results.Count; i++)
    {
        var desc = results[i].Description[..Math.Min(60, results[i].Description.Length)];
        Console.WriteLine($"  {i + 1}. {desc, -60} (RRF: {results[i].RrfScore:F4})");
    }
}
