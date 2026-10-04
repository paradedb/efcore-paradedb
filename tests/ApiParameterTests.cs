using Microsoft.EntityFrameworkCore;
using Npgsql;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests;

public sealed class ApiParameterTests : TestBase
{
    [Test]
    public async Task NestedQueriesAndDirectAggregate()
    {
        await using var context = DbFixture.CreateContext();
        var conjunction = SearchQuery.Boolean(
            should:
            [
                SearchQuery.Parse("Description:running"),
                SearchQuery.Parse("Description:shoes"),
            ],
            minimumShouldMatch: 2
        );
        var expected = await context
            .MockItems.Where(item => EF.Functions.MatchAll(item.Description, "running shoes"))
            .CountAsync();
        expected.ShouldBeGreaterThan(0);
        (
            await context
                .MockItems.Where(item => EF.Functions.Search(item.Id, conjunction))
                .CountAsync()
        ).ShouldBe(expected);
        var query = SearchQuery.Boolean(
            must:
            [
                SearchQuery.DisjunctionMax(
                    [conjunction, SearchQuery.Parse("Description:boots")],
                    tieBreaker: 0.5f
                ),
            ],
            mustNot: [SearchQuery.Parse("Description:sandals")]
        );
        var count = await context
            .MockItems.Where(item => EF.Functions.Search(item.Id, query))
            .CountAsync();
        count.ShouldBeGreaterThan(0);
        var result = await context
            .Database.Aggregate(
                "search_idx",
                query,
                new { count = new { value_count = new { field = "Id" } } },
                new()
                {
                    MemoryLimit = 10000000,
                    BucketLimit = 100,
                    Visibility = "transaction",
                }
            )
            .SingleAsync();
        result.Result.GetProperty("count").GetProperty("value").GetDouble().ShouldBe(count);
        var error = await Should.ThrowAsync<PostgresException>(() =>
            context
                .Database.Aggregate(
                    "search_idx",
                    SearchQuery.Parse("Description:shoes"),
                    new { ids = new { terms = new { field = "Id", size = 10 } } },
                    new() { MemoryLimit = 10000000, BucketLimit = 1 }
                )
                .SingleAsync()
        );
        error.MessageText.ShouldContain("bucket limit was exceeded");
        var escaped = SearchQuery.Parse("Description:\"O'Reilly\"");
        (
            await context
                .MockItems.Where(item => EF.Functions.Search(item.Id, escaped))
                .CountAsync()
        ).ShouldBe(0);
    }

    [Test]
    public async Task SparseSnippetOptionsAndPagination()
    {
        await using var context = DbFixture.CreateContext();
        var options = new SnippetOptions
        {
            MaxNumChars = 20,
            Limit = 1,
            Offset = 0,
        };
        var positions = new SnippetPositionsOptions { Limit = 1, Offset = 1 };
        var query = context.MockItems.Where(item =>
            EF.Functions.MatchAny(item.Description, "shoes")
        );
        var rows = await query
            .Select(item => new
            {
                Snippet = EF.Functions.Snippet(item.Description, options),
                Positions = EF.Functions.SnippetPositions(item.Description, positions),
            })
            .ToListAsync();
        rows.Count.ShouldBeGreaterThan(0);
        rows.All(row => row.Snippet is not null && row.Snippet.Contains("<b>")).ShouldBeTrue();
        var sql = query
            .Select(item => EF.Functions.SnippetPositions(item.Description, positions))
            .ToQueryString();
        sql.ShouldContain("\"limit\" => 1");
        sql.ShouldContain("\"offset\" => 1");
        var sparse = new SnippetOptions { EndTag = "</mark>" };
        var snippets = await query
            .Select(item => EF.Functions.Snippet(item.Description, sparse))
            .ToListAsync();
        snippets.All(snippet => snippet is not null && snippet.Contains("</mark>")).ShouldBeTrue();
    }

    [Test]
    public void RejectsInvalidOptions()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            SearchQuery.Boolean(minimumShouldMatch: -1)
        );
        Should.Throw<ArgumentOutOfRangeException>(() =>
            SearchQuery.DisjunctionMax([SearchQuery.Parse("Description:shoes")], float.NaN)
        );
        Should.Throw<ArgumentException>(() => SearchQuery.DisjunctionMax([]));
        using var context = DbFixture.CreateContext();
        Should.Throw<ArgumentException>(() =>
            context.Database.Aggregate(
                "idx",
                SearchQuery.Parse("*"),
                new { },
                new() { SolveMvcc = true, Visibility = "raw" }
            )
        );
        Should.Throw<ArgumentOutOfRangeException>(() =>
            context.Database.Aggregate(
                "idx",
                SearchQuery.Parse("*"),
                new { },
                new() { MemoryLimit = 0 }
            )
        );
    }
}
