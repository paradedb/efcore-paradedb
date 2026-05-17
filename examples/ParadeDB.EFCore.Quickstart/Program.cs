using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ParadeDB.EFCore.Extensions;
using ParadeDB.EFCore.Quickstart.Data;

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
Console.WriteLine("Quickstart Example");
Console.WriteLine(new string('=', 60));

var count = await dbContext.MockItems.CountAsync();
Console.WriteLine($"Loaded {count} mock items");

await DemoBasicSearch(dbContext);
await DemoScoredSearch(dbContext);
await DemoPhraseSearch(dbContext);
await DemoSnippetHighlighting(dbContext);
await DemoFilteredSearch(dbContext);

Console.WriteLine();
Console.WriteLine(new string('=', 60));
Console.WriteLine("Done!");
return;

static async Task DemoBasicSearch(AppDbContext db)
{
    Console.WriteLine("\n--- Basic Search: 'shoes' ---");

    var results = await db
        .MockItems.Where(x => EF.Functions.MatchConjunction(x.Description, "shoes"))
        .Take(5)
        .ToListAsync();

    foreach (var item in results)
        Console.WriteLine($"  • {Truncate(item.Description, 60)}...");
}

static async Task DemoScoredSearch(AppDbContext db)
{
    Console.WriteLine("\n--- Scored Search: 'running' ---");

    var results = await db
        .MockItems.Where(x => EF.Functions.MatchConjunction(x.Description, "running"))
        .Select(x => new { Item = x, Score = EF.Functions.Score(x.Id) })
        .OrderByDescending(x => x.Score)
        .Take(5)
        .ToListAsync();

    foreach (var result in results)
        Console.WriteLine(
            $"  • {Truncate(result.Item.Description, 50)}... (score: {result.Score:F2})"
        );
}

static async Task DemoPhraseSearch(AppDbContext db)
{
    Console.WriteLine("\n--- Phrase Search: 'running shoes' ---");

    var results = await db
        .MockItems.Where(x => EF.Functions.Phrase(x.Description, "running shoes"))
        .Select(x => new { Item = x, Score = EF.Functions.Score(x.Id) })
        .OrderByDescending(x => x.Score)
        .Take(5)
        .ToListAsync();

    foreach (var result in results)
        Console.WriteLine(
            $"  • {Truncate(result.Item.Description, 50)}... (score: {result.Score:F2})"
        );
}

static async Task DemoSnippetHighlighting(AppDbContext db)
{
    Console.WriteLine("\n--- Snippet Highlighting: 'shoes' ---");

    var results = await db
        .MockItems.Where(x => EF.Functions.MatchConjunction(x.Description, "shoes"))
        .Select(x => new
        {
            Score = EF.Functions.Score(x.Id),
            Snippet = EF.Functions.Snippet(x.Description, "<b>", "</b>"),
        })
        .OrderByDescending(x => x.Score)
        .Take(3)
        .ToListAsync();

    foreach (var result in results)
        Console.WriteLine($"  • {result.Snippet}");
}

static async Task DemoFilteredSearch(AppDbContext db)
{
    Console.WriteLine("\n--- Filtered Search: 'shoes' + in_stock + rating >= 4 ---");

    var results = await db
        .MockItems.Where(x =>
            EF.Functions.MatchConjunction(x.Description, "shoes") && x.InStock && x.Rating >= 4
        )
        .Select(x => new { Item = x, Score = EF.Functions.Score(x.Id) })
        .OrderByDescending(x => x.Score)
        .Take(5)
        .ToListAsync();

    foreach (var result in results)
        Console.WriteLine(
            $"  • {Truncate(result.Item.Description, 40)}... (rating: {result.Item.Rating})"
        );
}

static string Truncate(string value, int length)
{
    return value.Length <= length ? value : value[..length];
}
