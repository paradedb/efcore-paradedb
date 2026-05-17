using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ParadeDB.EFCore.Extensions;
using ParadeDB.EFCore.MoreLikeThis.Data;

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
Console.WriteLine("MoreLikeThis Example");
Console.WriteLine("Find similar documents without vector embeddings");
Console.WriteLine(new string('=', 60));

var count = await dbContext.MockItems.CountAsync();
Console.WriteLine($"\nLoaded {count} items");

await DemoSimilarToSingleProduct(dbContext);
await DemoSimilarToMultipleProducts(dbContext);
await DemoSimilarByDocument(dbContext);
await DemoTuningParameters(dbContext);
await DemoCombinedWithFilters(dbContext);
await DemoMultifieldSimilarity(dbContext);

Console.WriteLine();
Console.WriteLine(new string('=', 60));
Console.WriteLine("Done!");
return;

static void PrintHeader(string title)
{
    Console.WriteLine();
    Console.WriteLine(new string('=', 60));
    Console.WriteLine(title);
    Console.WriteLine(new string('=', 60));
}

static async Task DemoSimilarToSingleProduct(AppDbContext db)
{
    PrintHeader("Demo 1: Similar to a single product");

    var sourceId = 3;

    var fields = new[] { "description" };

    var source = await db.MockItems.SingleAsync(x => x.Id == sourceId);

    Console.WriteLine();
    Console.WriteLine($"Source product (id={sourceId}):");
    Console.WriteLine($"  '{source.Description}' [{source.Category}]");

    var similar = await db
        .MockItems.FromSqlInterpolated(
            $"""
            SELECT *
            FROM mock_items
            WHERE id @@@ pdb.more_like_this(
                {sourceId},
                {fields}
            )
            """
        )
        .OrderByDescending(x => EF.Functions.Score(x.Id))
        .Take(5)
        .ToListAsync();

    Console.WriteLine();
    Console.WriteLine("Similar products (by description):");

    foreach (var item in similar)
    {
        var marker = item.Id == sourceId ? " (source)" : "";

        Console.WriteLine(
            $"  {item.Id}: {Truncate(item.Description)}... [{item.Category}]{marker}"
        );
    }
}

static async Task DemoSimilarToMultipleProducts(AppDbContext db)
{
    PrintHeader("Demo 2: Similar to multiple products (browsing history)");

    int[] browsedIds = [3, 12, 29];
    var fields = new[] { "description" };

    var browsed = await db
        .MockItems.Where(x => browsedIds.AsEnumerable().Contains(x.Id))
        .OrderBy(x => x.Id)
        .ToListAsync();

    Console.WriteLine();
    Console.WriteLine("User's browsing history:");

    foreach (var item in browsed)
    {
        Console.WriteLine($"  {item.Id}: {Truncate(item.Description)}... [{item.Category}]");
    }

    var similar = await db
        .MockItems.FromSqlInterpolated(
            $"""
            SELECT *
            FROM mock_items
            WHERE (
                id @@@ pdb.more_like_this({browsedIds[0]}, {fields})
                OR id @@@ pdb.more_like_this({browsedIds[1]}, {fields})
                OR id @@@ pdb.more_like_this({browsedIds[2]}, {fields})
            )
            AND NOT (id = ANY({browsedIds}))
            """
        )
        .OrderByDescending(x => EF.Functions.Score(x.Id))
        .Take(5)
        .ToListAsync();

    Console.WriteLine();
    Console.WriteLine("Recommended products (similar to any browsed item):");

    foreach (var item in similar)
    {
        Console.WriteLine($"  {item.Id}: {Truncate(item.Description)}... [{item.Category}]");
    }
}

static async Task DemoSimilarByDocument(AppDbContext db)
{
    PrintHeader("Demo 3: Similar to text description");

    var userDescription = "comfortable wireless audio for running";
    var document = """{"description":"comfortable wireless audio for running"}""";

    Console.WriteLine();
    Console.WriteLine($"User wants: '{userDescription}'");

    var similar = await db
        .MockItems.FromSqlInterpolated(
            $"""
            SELECT *
            FROM mock_items
            WHERE id @@@ pdb.more_like_this({document})
            """
        )
        .OrderByDescending(x => EF.Functions.Score(x.Id))
        .Take(5)
        .ToListAsync();

    Console.WriteLine();
    Console.WriteLine("Matching products:");

    foreach (var item in similar)
    {
        Console.WriteLine($"  {item.Id}: {Truncate(item.Description)}... [{item.Category}]");
    }
}

static async Task DemoTuningParameters(AppDbContext db)
{
    PrintHeader("Demo 4: Tuning MoreLikeThis parameters");

    var sourceId = 5;

    var source = await db.MockItems.SingleAsync(x => x.Id == sourceId);

    Console.WriteLine();
    Console.WriteLine($"Source: '{source.Description}'");

    var fields = new[] { "description" };

    var defaultResults = await db
        .MockItems.FromSqlInterpolated(
            $"""
            SELECT *
            FROM mock_items
            WHERE id @@@ pdb.more_like_this({sourceId}, {fields})
            """
        )
        .OrderByDescending(x => EF.Functions.Score(x.Id))
        .Take(3)
        .ToListAsync();

    Console.WriteLine();
    Console.WriteLine("Default MLT (no tuning):");

    foreach (var item in defaultResults)
    {
        Console.WriteLine($"  {item.Id}: {Truncate(item.Description)}...");
    }

    var tunedResults = await db
        .MockItems.FromSqlInterpolated(
            $"""
            SELECT *
            FROM mock_items
            WHERE id @@@ pdb.more_like_this(
                {sourceId},
                {fields},
                min_doc_frequency => 2,
                max_query_terms => 5
            )
            """
        )
        .OrderByDescending(x => EF.Functions.Score(x.Id))
        .ThenBy(x => x.Id)
        .Take(3)
        .ToListAsync();

    Console.WriteLine();
    Console.WriteLine("Tuned MLT (min_doc_freq=2, max_query_terms=5):");

    foreach (var item in tunedResults)
    {
        Console.WriteLine($"  {item.Id}: {Truncate(item.Description)}...");
    }
}

static async Task DemoCombinedWithFilters(AppDbContext db)
{
    PrintHeader("Demo 5: MoreLikeThis + ORM filters");

    var sourceId = 15;
    var fields = new[] { "description" };

    var source = await db.MockItems.SingleAsync(x => x.Id == sourceId);

    Console.WriteLine();
    Console.WriteLine($"Source: '{source.Description}' (rating: {source.Rating})");

    var results = await db
        .MockItems.FromSqlInterpolated(
            $"""
            SELECT *
            FROM mock_items
            WHERE id @@@ pdb.more_like_this({sourceId}, {fields})
            """
        )
        .Where(x => x.InStock)
        .Where(x => x.Rating >= 4)
        .OrderByDescending(x => EF.Functions.Score(x.Id))
        .Take(5)
        .ToListAsync();

    Console.WriteLine();
    Console.WriteLine("Similar products (in_stock=True, rating >= 4):");

    foreach (var item in results)
    {
        var stock = item.InStock ? "In Stock" : "Out of Stock";

        Console.WriteLine(
            $"  {item.Id}: {Truncate(item.Description, 40)}... (rating: {item.Rating}, {stock})"
        );
    }
}

static async Task DemoMultifieldSimilarity(AppDbContext db)
{
    PrintHeader("Demo 6: Multi-field similarity");

    var sourceId = 3;

    var descriptionFields = new[] { "description" };
    var descriptionAndCategoryFields = new[] { "description", "category" };

    var source = await db.MockItems.SingleAsync(x => x.Id == sourceId);

    Console.WriteLine();
    Console.WriteLine($"Source: '{source.Description}' [{source.Category}]");

    var byDescription = await db
        .MockItems.FromSqlInterpolated(
            $"""
            SELECT *
            FROM mock_items
            WHERE id @@@ pdb.more_like_this({sourceId}, {descriptionFields})
            """
        )
        .Where(x => x.Id != sourceId)
        .OrderByDescending(x => EF.Functions.Score(x.Id))
        .Take(3)
        .ToListAsync();

    Console.WriteLine();
    Console.WriteLine("Similar by DESCRIPTION only:");

    foreach (var item in byDescription)
    {
        Console.WriteLine($"  {item.Id}: {Truncate(item.Description, 40)}... [{item.Category}]");
    }

    var byBoth = await db
        .MockItems.FromSqlInterpolated(
            $"""
            SELECT *
            FROM mock_items
            WHERE id @@@ pdb.more_like_this(
                {sourceId},
                {descriptionAndCategoryFields}
            )
            """
        )
        .Where(x => x.Id != sourceId)
        .OrderByDescending(x => EF.Functions.Score(x.Id))
        .Take(3)
        .ToListAsync();

    Console.WriteLine();
    Console.WriteLine("Similar by DESCRIPTION + CATEGORY (if both indexed):");

    foreach (var item in byBoth)
    {
        Console.WriteLine($"  {item.Id}: {Truncate(item.Description, 40)}... [{item.Category}]");
    }
}

static string Truncate(string value, int maxLength = 50)
{
    return value.Length <= maxLength ? value : value[..maxLength];
}
