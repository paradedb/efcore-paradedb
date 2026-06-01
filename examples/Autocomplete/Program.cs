using Autocomplete.Data;
using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shared;

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(ExampleSetup.ConnectionString, o => o.UseParadeDb())
    .UseSnakeCaseNamingConvention()
    .Options;

await using var dbContext = new AppDbContext(options);

Console.WriteLine(new string('=', 60));
Console.WriteLine("Autocomplete Example");
Console.WriteLine(new string('=', 60));

await ExampleSetup.SetupAutocompleteAsync(dbContext);

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

    var results = await dbContext
        .AutocompleteItems.Where(x => EF.Functions.Parse(x.Description, parseQuery))
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
        .ToListAsync();

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
