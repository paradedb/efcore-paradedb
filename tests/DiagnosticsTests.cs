using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Tests.Persistence;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests;

public sealed class DiagnosticsTests
{
    private static readonly DbContextOptions<TestDbContext> Options =
        new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql("Host=localhost;Database=paradedb_tests")
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
}
