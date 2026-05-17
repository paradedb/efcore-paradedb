using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ParadeDB.EFCore.Autocomplete.Data;
using ParadeDB.EFCore.Extensions;

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
Console.WriteLine("Autocomplete Example");
Console.WriteLine(new string('=', 60));

var count = await dbContext.AutocompleteItems.CountAsync();
Console.WriteLine($"\nLoaded {count} items");

string[] queries =
[
    "run",
    "runn",
    "running",
    "wire",
    "wirel",
    "wireles",
    "wireless",
    "blue",
    "blueto",
    "bluetooth",
];

foreach (var query in queries)
{
    Console.WriteLine($"\nUser types: '{query}'");

    var parseQuery = $"description_ngram:{query}";

    var results = dbContext
        .AutocompleteItems.FromSqlInterpolated(
            $"""
            SELECT * FROM autocomplete_items
            WHERE id @@@ pdb.parse({parseQuery})
            """
        )
        .Select(x => new
        {
            x.Id,
            x.Description,
            x.Category,
            x.Rating,
            x.InStock,
            SearchScore = EF.Functions.Score(x.Id),
        })
        .OrderByDescending(x => x.SearchScore)
        .Take(5)
        .ToList();

    if (results.Count == 0)
    {
        Console.WriteLine("  (no results)");
        continue;
    }

    foreach (var item in results)
    {
        var desc = item.Description.Length > 50 ? item.Description[..47] + "..." : item.Description;

        Console.WriteLine($"  - {desc} (score: {item.SearchScore:F2})");
    }
}

Console.WriteLine();
Console.WriteLine(new string('=', 60));
Console.WriteLine("Done.");
