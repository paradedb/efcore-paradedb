using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParadeDB.EntityFrameworkCore.Internal.Metadata;

namespace ParadeDB.EntityFrameworkCore.Extensions;

public static class ParadeDbIndexBuilderExtensions
{
    public static ParadeDbIndexBuilder<TEntity> HasParadeDbIndex<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name,
        Expression<Func<TEntity, object?>> keyExpression
    )
        where TEntity : class
    {
        var fieldProperty = GetPropertyName(keyExpression);
        var indexBuilder = entityTypeBuilder.HasIndex(keyExpression).HasDatabaseName(name);

        indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexFieldProperties,
            new[] { fieldProperty }
        );
        // IndexFieldKinds is used to track if each index field is an EF Core property or a SQL expression
        // so that it can be rendered appropriately
        indexBuilder.HasAnnotation(ParadeDbAnnotationNames.IndexFieldKinds, new[] { "property" });
        indexBuilder.HasAnnotation(ParadeDbAnnotationNames.IndexFieldTokenizers, new[] { "" });
        indexBuilder.HasAnnotation(ParadeDbAnnotationNames.IndexFieldAliases, new[] { "" });
        indexBuilder.HasAnnotation(ParadeDbAnnotationNames.IndexFieldOpclasses, new[] { "" });

        return new ParadeDbIndexBuilder<TEntity>(indexBuilder);
    }

    internal static string GetPropertyName<TEntity, TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression
    )
    {
        var body = propertyExpression.Body is UnaryExpression unary
            ? unary.Operand
            : propertyExpression.Body;

        return body is MemberExpression member
            ? member.Member.Name
            : throw new ArgumentException(
                "The ParadeDB index field expression must be a property access."
            );
    }
}

public record FieldAlias(string Name);

// We use a custom index builder class instead of IndexBuilder directly to make it clear that not all
// normal index creation operations are supported here (e.g. `IsUnique`)
public sealed class ParadeDbIndexBuilder<TEntity>
    where TEntity : class
{
    private readonly IndexBuilder<TEntity> _indexBuilder;

    internal ParadeDbIndexBuilder(IndexBuilder<TEntity> indexBuilder)
    {
        _indexBuilder = indexBuilder;
    }

    public ParadeDbIndexBuilder<TEntity> HasField<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression
    )
    {
        AddField(
            ParadeDbIndexBuilderExtensions.GetPropertyName(propertyExpression),
            "property",
            null,
            null
        );

        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasField<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        Tokenizer tokenizer
    )
    {
        AddField(
            ParadeDbIndexBuilderExtensions.GetPropertyName(propertyExpression),
            "property",
            tokenizer,
            null
        );

        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasField(string sql, Tokenizer tokenizer)
    {
        AddField(sql, "sql", tokenizer, null);

        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasField<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        FieldAlias alias
    )
    {
        AddField(
            ParadeDbIndexBuilderExtensions.GetPropertyName(propertyExpression),
            "property",
            null,
            alias.Name
        );

        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasField(string sql, FieldAlias alias)
    {
        AddField(sql, "sql", null, alias.Name);

        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasField<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        VectorMetric metric
    )
    {
        AddField(
            ParadeDbIndexBuilderExtensions.GetPropertyName(propertyExpression),
            "property",
            null,
            null,
            metric.ToOpclass()
        );

        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasField(string sql, VectorMetric metric)
    {
        AddField(sql, "sql", null, null, metric.ToOpclass());

        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasFilter(string? sql)
    {
        _indexBuilder.HasFilter(sql);

        return this;
    }

    public ParadeDbIndexBuilder<TEntity> IsCreatedConcurrently(bool createdConcurrently = true)
    {
        _indexBuilder.IsCreatedConcurrently(createdConcurrently);

        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasSearchTokenizer(Tokenizer tokenizer)
    {
        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexSearchTokenizer,
            tokenizer.ToSearchString()
        );

        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasLayerSizes(string layerSizes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(layerSizes);
        _indexBuilder.HasAnnotation(ParadeDbAnnotationNames.IndexLayerSizes, layerSizes);
        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasBackgroundLayerSizes(string backgroundLayerSizes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backgroundLayerSizes);
        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexBackgroundLayerSizes,
            backgroundLayerSizes
        );
        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasMutableSegmentRows(int mutableSegmentRows)
    {
        if (mutableSegmentRows is < 0 or > 10000)
            throw new ArgumentOutOfRangeException(nameof(mutableSegmentRows));
        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexMutableSegmentRows,
            mutableSegmentRows
        );
        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasTrainingSampleRatio(double trainingSampleRatio)
    {
        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexTrainingSampleRatio,
            trainingSampleRatio
        );

        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasTargetLeafSize(int targetLeafSize)
    {
        _indexBuilder.HasAnnotation(ParadeDbAnnotationNames.IndexTargetLeafSize, targetLeafSize);

        return this;
    }

    /// <summary>Partition segments by single-valued columnar index field names.</summary>
    public ParadeDbIndexBuilder<TEntity> HasPartitionBy(params string[] partitionBy)
    {
        _indexBuilder.HasAnnotation(ParadeDbAnnotationNames.IndexPartitionBy, partitionBy);
        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasTargetSegmentCount(int targetSegmentCount)
    {
        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexTargetSegmentCount,
            targetSegmentCount
        );
        return this;
    }

    /// <summary>Pass zero to disable quantization for the vector index.</summary>
    public ParadeDbIndexBuilder<TEntity> HasVectorQuantization(int quantization)
    {
        if (quantization != 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantization),
                "Use zero or an array of layer widths."
            );

        return SetVectorQuantization("0");
    }

    /// <summary>Configure one to three quantization layers, with one to four bits per layer.</summary>
    public ParadeDbIndexBuilder<TEntity> HasVectorQuantization(int[] layers)
    {
        ArgumentNullException.ThrowIfNull(layers);
        if (layers.Length is < 1 or > 3 || layers.Any(bits => bits is < 1 or > 4))
            throw new ArgumentException(
                "Use one to three layers with one to four bits per layer.",
                nameof(layers)
            );
        return SetVectorQuantization($"[{string.Join(",", layers)}]");
    }

    private ParadeDbIndexBuilder<TEntity> SetVectorQuantization(string quantization)
    {
        _indexBuilder.HasAnnotation(ParadeDbAnnotationNames.IndexQuantization, quantization);
        return this;
    }

    private void AddField(
        string field,
        string kind,
        Tokenizer? tokenizer,
        string? alias,
        string? opclass = null
    )
    {
        var properties = GetAnnotation(ParadeDbAnnotationNames.IndexFieldProperties);
        var kinds = GetAnnotation(ParadeDbAnnotationNames.IndexFieldKinds);
        var tokenizers = GetAnnotation(ParadeDbAnnotationNames.IndexFieldTokenizers);
        var aliases = GetAnnotation(ParadeDbAnnotationNames.IndexFieldAliases);
        var opclasses = GetAnnotation(ParadeDbAnnotationNames.IndexFieldOpclasses);

        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexFieldProperties,
            properties.Append(field).ToArray()
        );
        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexFieldKinds,
            kinds.Append(kind).ToArray()
        );
        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexFieldTokenizers,
            tokenizers.Append(tokenizer?.ToString() ?? "").ToArray()
        );

        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexFieldAliases,
            aliases.Append(alias is null ? "" : alias.Replace("'", "''")).ToArray()
        );

        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexFieldOpclasses,
            opclasses.Append(opclass ?? "").ToArray()
        );
    }

    private string[] GetAnnotation(string name) =>
        (string[]?)_indexBuilder.Metadata.FindAnnotation(name)?.Value ?? [];
}
