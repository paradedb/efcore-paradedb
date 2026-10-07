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

public sealed class VectorConfig
{
    [Column("index_oid", TypeName = "oid")]
    public uint IndexOid { get; set; }

    [Column("quantized")]
    public bool Quantized { get; set; }

    [Column("layers")]
    public int[]? Layers { get; set; } = null!;

    [Column("bytes_per_row")]
    public int? BytesPerRow { get; set; }

    [Column("settings_version")]
    public int? SettingsVersion { get; set; }
}

public sealed class VectorEstimatorInfo
{
    [Column("depth")]
    public int Depth { get; set; }

    [Column("bias")]
    public float Bias { get; set; }

    [Column("spread")]
    public float Spread { get; set; }

    [Column("sample_rows")]
    public int SampleRows { get; set; }

    [Column("query_count")]
    public int QueryCount { get; set; }

    [Column("query_source")]
    public string QuerySource { get; set; } = null!;
}

public sealed class VectorInfo
{
    [Column("segno")]
    public string Segno { get; set; } = null!;

    [Column("vector_field")]
    public string VectorField { get; set; } = null!;

    [Column("vector_format")]
    public string VectorFormat { get; set; } = null!;

    [Column("vector_num_vectors")]
    public decimal VectorNumVectors { get; set; }

    [Column("vector_num_centroids")]
    public decimal? VectorNumCentroids { get; set; }

    [Column("vector_min_cluster_size")]
    public decimal? VectorMinClusterSize { get; set; }

    [Column("vector_max_cluster_size")]
    public decimal? VectorMaxClusterSize { get; set; }

    [Column("vector_avg_cluster_size")]
    public double? VectorAvgClusterSize { get; set; }

    [Column("vector_empty_clusters")]
    public decimal? VectorEmptyClusters { get; set; }

    [Column("vector_total_rows")]
    public decimal? VectorTotalRows { get; set; }

    [Column("quantized")]
    public bool Quantized { get; set; }

    [Column("layers")]
    public int[]? Layers { get; set; } = null!;

    [Column("quantizer_kinds")]
    public string[]? QuantizerKinds { get; set; } = null!;

    [Column("bytes_per_row")]
    public int? BytesPerRow { get; set; }
}

public static class ParadeDbDiagnosticsExtensions
{
    public static IQueryable<VectorInfo> VectorInfo(
        this DatabaseFacade database,
        string index,
        string field
    ) =>
        database.SqlQuery<VectorInfo>(
            $"SELECT * FROM paradedb.vector_info({index}::regclass, {field}::text)"
        );

    public static IQueryable<VectorConfig> VectorConfig(
        this DatabaseFacade database,
        string index,
        string field
    ) =>
        database.SqlQuery<VectorConfig>(
            $"SELECT * FROM paradedb.vector_config({index}::regclass, {field}::text)"
        );

    public static IQueryable<VectorEstimatorInfo> VectorEstimatorInfo(
        this DatabaseFacade database,
        string index,
        string field,
        IReadOnlyList<float[]>? queries = null
    )
    {
        List<object> parameters = [index, field];
        var queryArg = "";
        if (queries is not null)
        {
            var values = queries.Select(query =>
            {
                var placeholder = $"{{{parameters.Count}}}::vector";
                parameters.Add(
                    "["
                        + string.Join(
                            ",",
                            query.Select(value =>
                                value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                            )
                        )
                        + "]"
                );
                return placeholder;
            });
            queryArg = $", ARRAY[{string.Join(", ", values)}]::vector[]";
        }
        return database.SqlQuery<VectorEstimatorInfo>(
            FormattableStringFactory.Create(
                $"SELECT * FROM paradedb.vector_estimator_info({{0}}::regclass, {{1}}::text{queryArg})",
                parameters.ToArray()
            )
        );
    }

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
