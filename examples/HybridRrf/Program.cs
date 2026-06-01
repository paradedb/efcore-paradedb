using HybridRrf;
using HybridRrf.Data;
using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Shared;

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(
        ExampleSetup.ConnectionString,
        o =>
        {
            o.UseParadeDb();
            o.UseVector();
        }
    )
    .UseSnakeCaseNamingConvention()
    .Options;

await using var dbContext = new AppDbContext(options);

Console.WriteLine(new string('=', 70));
Console.WriteLine("Hybrid Search with Reciprocal Rank Fusion (RRF)");
Console.WriteLine(new string('=', 70));
Console.WriteLine("\nBM25 (keyword) + Vector (semantic)");
Console.WriteLine("RRF formula: score = sum(1 / (k + rank)) across all rankings");

await ExampleSetup.SetupHybridAsync(dbContext);
await LoadEmbeddingsAsync(dbContext);

await Demo(dbContext, "running shoes", QueryEmbeddings.Values);
await Demo(dbContext, "footwear for exercise", QueryEmbeddings.Values);
await Demo(dbContext, "wireless earbuds", QueryEmbeddings.Values);

Console.WriteLine("\n" + new string('=', 70));
Console.WriteLine("BM25 results use the ParadeDB EF query builder.");
Console.WriteLine(new string('=', 70));
return;

static async Task Demo(AppDbContext db, string query, Dictionary<string, float[]> queryEmbeddings)
{
    var results = await HybridSearch(db, query, new Vector(queryEmbeddings[query]));
    DisplayResults(query, results);
}

static async Task<List<(string Description, double RrfScore)>> HybridSearch(
    AppDbContext db,
    string query,
    Vector queryEmbedding,
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
        .MockItems.Where(x => x.Embedding != null)
        .Select(x => new
        {
            x.Id,
            x.Description,
            Distance = x.Embedding!.CosineDistance(queryEmbedding),
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

static async Task LoadEmbeddingsAsync(AppDbContext db)
{
    var csvPath = Path.Combine(AppContext.BaseDirectory, "HybridRrf", "mock_items_embeddings.csv");
    var lines = await File.ReadAllLinesAsync(csvPath);

    var embeddings = new Dictionary<int, float[]>();

    for (var i = 1; i < lines.Length; i++)
    {
        var parts = lines[i].Split(',', 3);
        embeddings[int.Parse(parts[0])] = parts[2]
            .Trim('"', '[', ']')
            .Split(',', StringSplitOptions.TrimEntries)
            .Select(float.Parse)
            .ToArray();
    }

    var ids = embeddings.Keys.ToArray();
    var items = await db.MockItems.Where(x => ids.Contains(x.Id)).ToListAsync();
    foreach (var item in items)
    {
        item.Embedding = new Vector(embeddings[item.Id]);
    }

    await db.SaveChangesAsync();
    Console.WriteLine($"Loaded {items.Count} embeddings");
}
