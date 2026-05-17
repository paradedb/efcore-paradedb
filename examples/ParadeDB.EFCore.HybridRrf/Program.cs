using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ParadeDB.EFCore.Extensions;
using ParadeDB.EFCore.HybridRrf;
using ParadeDB.EFCore.HybridRrf.Data;

var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var connectionString = config.GetConnectionString("Default");

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(connectionString, o => o.UseParadeDb())
    .UseSnakeCaseNamingConvention()
    .Options;

await using var dbContext = new AppDbContext(options);

await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.MigrateAsync();

Console.WriteLine(new string('=', 70));
Console.WriteLine("Hybrid Search with Reciprocal Rank Fusion (RRF)");
Console.WriteLine(new string('=', 70));
Console.WriteLine("\nSingle-query CTE: BM25 (keyword) + Vector (semantic)");
Console.WriteLine("RRF formula: score = sum(1 / (k + rank)) across all rankings");

await LoadEmbeddingsAsync(dbContext);

await Demo(dbContext, "running shoes", QueryEmbeddings.Values);
await Demo(dbContext, "footwear for exercise", QueryEmbeddings.Values);
await Demo(dbContext, "wireless earbuds", QueryEmbeddings.Values);

Console.WriteLine("\n" + new string('=', 70));
Console.WriteLine("All results produced by a single SQL query per search.");
Console.WriteLine(new string('=', 70));
return;

static async Task Demo(AppDbContext db, string query, Dictionary<string, float[]> embeddings)
{
    var embedding = embeddings[query];
    var results = await HybridSearch(db, query, embedding);
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
    var embeddingStr = $"[{string.Join(",", queryEmbedding)}]";

    var conn = (NpgsqlConnection)db.Database.GetDbConnection();

    if (conn.State != ConnectionState.Open)
    {
        await conn.OpenAsync();
    }

    await using var cmd = conn.CreateCommand();

    cmd.CommandText = """
        WITH fulltext AS (
            SELECT id, ROW_NUMBER() OVER (ORDER BY paradedb.score(id) DESC) AS rank
            FROM mock_items
            WHERE id @@@ paradedb.match('description', @query)
            ORDER BY paradedb.score(id) DESC
            LIMIT @topK
        ),
        semantic AS (
            SELECT id, ROW_NUMBER() OVER (ORDER BY embedding <=> @embedding::vector ASC) AS rank
            FROM mock_items
            WHERE embedding IS NOT NULL
            ORDER BY embedding <=> @embedding::vector ASC
            LIMIT @topK
        ),
        rrf AS (
            SELECT id, 1.0 / (@rrfK + rank) AS score FROM fulltext
            UNION ALL
            SELECT id, 1.0 / (@rrfK + rank) AS score FROM semantic
        ),
        rrf_scores AS (
            SELECT id, SUM(score) AS rrf_score
            FROM rrf
            GROUP BY id
            ORDER BY rrf_score DESC
            LIMIT @limit
        )
        SELECT m.description, r.rrf_score
        FROM rrf_scores r
        JOIN mock_items m ON m.id = r.id
        ORDER BY r.rrf_score DESC
        """;

    cmd.Parameters.AddWithValue("@query", query);
    cmd.Parameters.AddWithValue("@embedding", embeddingStr);
    cmd.Parameters.AddWithValue("@topK", topK);
    cmd.Parameters.AddWithValue("@rrfK", rrfK);
    cmd.Parameters.AddWithValue("@limit", limit);

    await using var reader = await cmd.ExecuteReaderAsync();
    var results = new List<(string Description, double RrfScore)>();
    while (await reader.ReadAsync())
    {
        results.Add((reader.GetString(0), reader.GetDouble(1)));
    }

    return results;
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
    var csvPath = Path.Combine(AppContext.BaseDirectory, "mock_items_embeddings.csv");
    var lines = await File.ReadAllLinesAsync(csvPath);

    var conn = (NpgsqlConnection)db.Database.GetDbConnection();

    if (conn.State != ConnectionState.Open)
    {
        await conn.OpenAsync();
    }

    for (var i = 1; i < lines.Length; i++)
    {
        var parts = lines[i].Split(',', 3);
        var id = int.Parse(parts[0]);
        var embedding = parts[2].Trim('"');

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE mock_items SET embedding = @embedding::vector WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@embedding", embedding);
        await cmd.ExecuteNonQueryAsync();
    }

    Console.WriteLine($"Loaded {lines.Length - 1} embeddings");
}
