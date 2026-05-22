using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ParadeDB.EntityFrameworkCore;

public sealed class VerifyIndexOptions
{
    public bool? HeapAllIndexed { get; set; }
    public double? SampleRate { get; set; }
    public bool? ReportProgress { get; set; }
    public bool? Verbose { get; set; }
    public bool? OnErrorStop { get; set; }
    public int[]? SegmentIds { get; set; }
}

public sealed class VerifyAllIndexesOptions
{
    public bool? HeapAllIndexed { get; set; }
    public double? SampleRate { get; set; }
    public bool? ReportProgress { get; set; }
    public bool? OnErrorStop { get; set; }
    public string? SchemaPattern { get; set; }
    public string? IndexPattern { get; set; }
}

public sealed class VerifyIndexResult
{
    [Column("check_name")]
    public string CheckName { get; set; } = null!;

    [Column("passed")]
    public bool Passed { get; set; }

    [Column("details")]
    public string Details { get; set; } = null!;
}

public sealed class IndexSegment
{
    [Column("partition_name")]
    public string PartitionName { get; set; } = null!;

    [Column("segment_idx")]
    public int SegmentIdx { get; set; }

    [Column("segment_id")]
    public string SegmentId { get; set; } = null!;

    [Column("num_docs")]
    public long NumDocs { get; set; }

    [Column("num_deleted")]
    public long NumDeleted { get; set; }

    [Column("max_doc")]
    public long MaxDoc { get; set; }
}

public sealed class IndexInfo
{
    [Column("schemaname")]
    public string SchemaName { get; set; } = null!;

    [Column("tablename")]
    public string TableName { get; set; } = null!;

    [Column("indexname")]
    public string IndexName { get; set; } = null!;

    [Column("indexrelid")]
    public long IndexRelId { get; set; }

    [Column("num_segments")]
    public long NumSegments { get; set; }

    [Column("total_docs")]
    public long TotalDocs { get; set; }
}

public static class ParadeDbDiagnosticsExtensions
{
    public static IQueryable<VerifyIndexResult> VerifyIndex(
        this DatabaseFacade database,
        string index,
        VerifyIndexOptions? options = null
    )
    {
        List<object> parameters = [index];
        var args = RenderVerifyIndexOptions(options, parameters);

        var sql = $"SELECT * FROM pdb.verify_index({{0}}{args})";

        return database.SqlQuery<VerifyIndexResult>(
            FormattableStringFactory.Create(sql, parameters.ToArray())
        );
    }

    private static string RenderVerifyIndexOptions(
        VerifyIndexOptions? options,
        List<object> parameters
    )
    {
        if (options is null)
        {
            return "";
        }

        List<string> args = [];
        if (options.HeapAllIndexed is not null)
        {
            args.Add($"heapallindexed => {{{parameters.Count}}}");
            parameters.Add(options.HeapAllIndexed);
        }

        if (options.SampleRate is not null)
        {
            args.Add($"sample_rate => {{{parameters.Count}}}");
            parameters.Add(options.SampleRate);
        }

        if (options.ReportProgress is not null)
        {
            args.Add($"report_progress => {{{parameters.Count}}}");
            parameters.Add(options.ReportProgress);
        }

        if (options.OnErrorStop is not null)
        {
            args.Add($"on_error_stop => {{{parameters.Count}}}");
            parameters.Add(options.OnErrorStop);
        }

        if (options.Verbose is not null)
        {
            args.Add($"verbose => {{{parameters.Count}}}");
            parameters.Add(options.Verbose);
        }

        if (options.SegmentIds is not null)
        {
            args.Add($"segment_ids => {{{parameters.Count}}}");
            parameters.Add(options.SegmentIds);
        }

        return args.Count == 0 ? "" : $", {string.Join(", ", args)}";
    }

    public static IQueryable<VerifyIndexResult> VerifyAllIndexes(
        this DatabaseFacade database,
        VerifyAllIndexesOptions? options = null
    )
    {
        options ??= new VerifyAllIndexesOptions();
        List<object> parameters = [];
        List<string> args = [];

        if (options.SchemaPattern is not null)
        {
            args.Add($"schema_pattern => {{{parameters.Count}}}");
            parameters.Add(options.SchemaPattern);
        }

        if (options.IndexPattern is not null)
        {
            args.Add($"index_pattern => {{{parameters.Count}}}");
            parameters.Add(options.IndexPattern);
        }

        if (options.HeapAllIndexed is not null)
        {
            args.Add($"heapallindexed => {{{parameters.Count}}}");
            parameters.Add(options.HeapAllIndexed);
        }

        if (options.SampleRate is not null)
        {
            args.Add($"sample_rate => {{{parameters.Count}}}");
            parameters.Add(options.SampleRate);
        }

        if (options.ReportProgress is not null)
        {
            args.Add($"report_progress => {{{parameters.Count}}}");
            parameters.Add(options.ReportProgress);
        }

        if (options.OnErrorStop is not null)
        {
            args.Add($"on_error_stop => {{{parameters.Count}}}");
            parameters.Add(options.OnErrorStop);
        }

        var sql = $"SELECT * FROM pdb.verify_all_indexes({string.Join(", ", args)})";

        return database.SqlQuery<VerifyIndexResult>(
            FormattableStringFactory.Create(sql, parameters.ToArray())
        );
    }

    public static IQueryable<IndexSegment> IndexSegments(
        this DatabaseFacade database,
        string index
    ) => database.SqlQuery<IndexSegment>($"SELECT * FROM pdb.index_segments({index})");

    public static IQueryable<IndexInfo> Indexes(this DatabaseFacade database) =>
        database.SqlQuery<IndexInfo>($"SELECT * FROM pdb.indexes()");
}
