using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Tests.Persistence;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests;

public sealed class DiagnosticsTests
{
    private static readonly DbContextOptions<TestDbContext> Options =
        new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql("Host=localhost;Database=paradedb_tests", o => o.UseParadeDb())
            .Options;

    private static void AssertSql(IQueryable query, string expected) =>
        NormalizeSql(query.ToQueryString()).ShouldBe(NormalizeSql(expected));

    private static string NormalizeSql(string sql) =>
        Regex
            .Replace(
                Regex.Replace(sql.ReplaceLineEndings("\n").TrimEnd(), @"@p\d+", "@p"),
                @"-- p\d+=",
                "-- @p="
            )
            .Replace(" (Nullable = true)", "");

    [Test]
    public void VerifyIndex()
    {
        using var context = new TestDbContext(Options);

        var query = context.Database.VerifyIndex(
            "search_idx",
            new VerifyIndexOptions
            {
                HeapAllIndexed = true,
                SampleRate = 0.1,
                ReportProgress = true,
                Verbose = true,
                OnErrorStop = true,
                SegmentIds = [0, 2],
            }
        );

        var sql = """
            -- @p='search_idx'
            -- @p='True'
            -- @p='0.1'
            -- @p='True'
            -- @p='True'
            -- @p='True'
            -- @p={ '0', '2' } (DbType = Object)
            SELECT * FROM pdb.verify_index(@p, heapallindexed => @p, sample_rate => @p, report_progress => @p, on_error_stop => @p, verbose => @p, segment_ids => @p)
            """;

        AssertSql(query, sql);
    }

    [Test]
    public void VerifyIndex_WithNoOptions()
    {
        using var context = new TestDbContext(Options);

        var query = context.Database.VerifyIndex("search_idx");

        var sql = """
            -- @p='search_idx'
            SELECT * FROM pdb.verify_index(@p)
            """;

        AssertSql(query, sql);
    }

    [Test]
    public void VerifyAllIndexes()
    {
        using var context = new TestDbContext(Options);

        var query = context.Database.VerifyAllIndexes(
            new VerifyAllIndexesOptions
            {
                SchemaPattern = "public",
                IndexPattern = "search_%",
                HeapAllIndexed = true,
                SampleRate = 0.5,
                ReportProgress = true,
                OnErrorStop = true,
            }
        );

        var sql = """
            -- @p='public'
            -- @p='search_%'
            -- @p='True'
            -- @p='0.5'
            -- @p='True'
            -- @p='True'
            SELECT * FROM pdb.verify_all_indexes(schema_pattern => @p, index_pattern => @p, heapallindexed => @p, sample_rate => @p, report_progress => @p, on_error_stop => @p)
            """;

        AssertSql(query, sql);
    }

    [Test]
    public void VerifyAllIndexes_WithNoOptions()
    {
        using var context = new TestDbContext(Options);

        var query = context.Database.VerifyAllIndexes();

        AssertSql(query, "SELECT * FROM pdb.verify_all_indexes()");
    }

    [Test]
    public void IndexSegments()
    {
        using var context = new TestDbContext(Options);

        AssertSql(
            context.Database.IndexSegments("search_idx"),
            """
            -- @p='search_idx'
            SELECT * FROM pdb.index_segments(@p)
            """
        );
        AssertSql(context.Database.Indexes(), "SELECT * FROM pdb.indexes()");
    }

    [Test]
    public void VectorDiagnostics()
    {
        using var context = new TestDbContext(Options);
        foreach (
            var (query, function) in new (IQueryable, string)[]
            {
                (context.Database.VectorInfo("search_idx", "embedding"), "vector_info"),
                (context.Database.VectorConfig("search_idx", "embedding"), "vector_config"),
                (
                    context.Database.VectorEstimatorInfo("search_idx", "embedding"),
                    "vector_estimator_info"
                ),
            }
        )
        {
            AssertSql(
                query,
                $"""
                -- @p='search_idx'
                -- @p='embedding'
                SELECT * FROM paradedb.{function}(@p::regclass, @p::text)
                """
            );
        }
        AssertSql(
            context.Database.VectorEstimatorInfo(
                "search_idx",
                "embedding",
                [
                    [0.1f, 0.2f],
                ]
            ),
            """
            -- @p='search_idx'
            -- @p='embedding'
            -- @p='[0.1,0.2]'
            SELECT * FROM paradedb.vector_estimator_info(@p::regclass, @p::text, ARRAY[@p::vector]::vector[])
            """
        );
    }
}

public sealed class DirectAggregateTests : TestBase
{
    [Test]
    public async Task DirectAggregateAndBucketLimit()
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
    }

    [Test]
    public void RejectsInvalidAggregateOptions()
    {
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
