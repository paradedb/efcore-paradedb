using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ParadeDB.EFCore.Extensions;
using ParadeDB.EFCore.Rag;
using ParadeDB.EFCore.Rag.Data;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .Build();

var connectionString = config.GetConnectionString("Default");
var openRouterApiKey = config["OpenRouter:ApiKey"];
var model = config["OpenRouter:Model"] ?? "anthropic/claude-3-haiku";

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(connectionString, o => o.UseParadeDb())
    .UseSnakeCaseNamingConvention()
    .Options;

await using var dbContext = new AppDbContext(options);

await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.MigrateAsync();

Console.WriteLine(new string('=', 60));
Console.WriteLine("RAG with ParadeDB + OpenRouter");
Console.WriteLine(new string('=', 60));
Console.WriteLine($"Using model: {model}");

if (string.IsNullOrWhiteSpace(openRouterApiKey))
{
    Console.WriteLine("OpenRouter:ApiKey is not set; generation responses will be skipped.");
}

var count = await dbContext.MockItems.CountAsync();
Console.WriteLine($"Loaded {count} products");

await Rag(dbContext, "What running shoes do you have?", openRouterApiKey, model);
await Rag(dbContext, "I need comfortable shoes for everyday use", openRouterApiKey, model);
await Rag(dbContext, "Do you have any wireless audio products?", openRouterApiKey, model);

Console.WriteLine();
Console.WriteLine(new string('=', 60));
Console.WriteLine("Done!");
return;

static async Task<List<ProductResult>> Retrieve(AppDbContext db, string query, int topK = 5)
{
    return await db
        .MockItems.FromSqlInterpolated(
            $"""
            SELECT *
            FROM mock_items
            WHERE id @@@ pdb.parse(
                {query},
                lenient => true
            )
            """
        )
        .Select(x => new ProductResult
        {
            Id = x.Id,
            Description = x.Description,
            Category = x.Category,
            Rating = x.Rating,
            InStock = x.InStock,
            Metadata = x.Metadata,
            Score = EF.Functions.Score(x.Id),
        })
        .OrderByDescending(x => x.Score)
        .ThenBy(x => x.Id)
        .Take(topK)
        .ToListAsync();
}

static string FormatContext(List<ProductResult> items)
{
    if (items.Count == 0)
    {
        return "No products found.";
    }

    var lines = new List<string>();

    foreach (var item in items)
    {
        var stock = item.InStock ? "In Stock" : "Out of Stock";

        var color = "N/A";

        if (
            item.Metadata is not null
            && item.Metadata.RootElement.TryGetProperty("color", out var colorElement)
        )
        {
            color = colorElement.GetString() ?? "N/A";
        }

        lines.Add(
            $"- {item.Description} | Category: {item.Category} | "
                + $"Rating: {item.Rating}/5 | {stock} | Color: {color}"
        );
    }

    return string.Join("\n", lines);
}

static async Task<string> Generate(string query, string context, string? apiKey, string model)
{
    if (string.IsNullOrWhiteSpace(apiKey))
        return "(Set OPENROUTER_API_KEY to enable generation.)";

    var prompt = $"""
        You are a helpful product assistant. Answer the customer's question based only on the product information provided below.

        Product Catalog:
        {context}

        Customer Question: {query}

        Provide a helpful, concise answer. If the products don't match what the customer is looking for, say so.
        """;

    using var http = new HttpClient();
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    var payload = new { model, messages = new[] { new { role = "user", content = prompt } } };

    try
    {
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await http.PostAsync(
            "https://openrouter.ai/api/v1/chat/completions",
            content
        );

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        return doc.RootElement.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
            ?? "";
    }
    catch (Exception ex)
        when (ex
                is HttpRequestException
                    or JsonException
                    or KeyNotFoundException
                    or InvalidOperationException
        )
    {
        return $"(OpenRouter error: {ex.Message}. Check your API key)";
    }
}

static async Task Rag(AppDbContext db, string query, string? apiKey, string model)
{
    Console.WriteLine($"\n{new string('=', 60)}");
    Console.WriteLine($"Question: {query}");
    Console.WriteLine(new string('=', 60));

    var items = await Retrieve(db, query);

    Console.WriteLine($"\nRetrieved {items.Count} products:");
    foreach (var item in items)
        Console.WriteLine($"  • {item.Description} (score: {item.Score:F2})");

    var context = FormatContext(items);

    Console.WriteLine("\nAnswer:");
    Console.WriteLine(new string('-', 40));

    var answer = await Generate(query, context, apiKey, model);
    Console.WriteLine(answer);
}

namespace ParadeDB.EFCore.Rag
{
    public sealed class ProductResult
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool InStock { get; set; }
        public JsonDocument? Metadata { get; set; }
        public float Score { get; set; }
    }
}
