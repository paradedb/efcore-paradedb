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

    public ParadeDbIndexBuilder<TEntity> HasTrainingSampleRatio(double trainingSampleRatio)
    {
        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexTrainingSampleRatio,
            trainingSampleRatio
        );

        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasMaxLeafSize(int maxLeafSize)
    {
        _indexBuilder.HasAnnotation(ParadeDbAnnotationNames.IndexMaxLeafSize, maxLeafSize);

        return this;
    }

    /// <summary>Partition segments by single-valued columnar index field names.</summary>
    public ParadeDbIndexBuilder<TEntity> HasPartitionBy(string partitionBy)
    {
        if (
            string.IsNullOrWhiteSpace(partitionBy)
            || partitionBy.Split(',').Any(string.IsNullOrWhiteSpace)
        )
            throw new ArgumentException(
                "Partition keys must be non-empty index field names.",
                nameof(partitionBy)
            );
        _indexBuilder.HasAnnotation(ParadeDbAnnotationNames.IndexPartitionBy, partitionBy);
        return this;
    }

    public ParadeDbIndexBuilder<TEntity> HasTargetSegmentCount(int targetSegmentCount)
    {
        if (targetSegmentCount < 1)
            throw new ArgumentOutOfRangeException(nameof(targetSegmentCount));
        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexTargetSegmentCount,
            targetSegmentCount
        );
        return this;
    }

    /// <summary>Configure quantization for a vector index field at CREATE INDEX or REINDEX time.</summary>
    public ParadeDbIndexBuilder<TEntity> HasVectorQuantization(string field, bool quantization) =>
        SetVectorQuantization(field, quantization);

    /// <summary>Configure one to three quantization layers, with one to four bits per layer.</summary>
    public ParadeDbIndexBuilder<TEntity> HasVectorQuantization(string field, params int[] layers)
    {
        if (layers.Length is < 1 or > 3 || layers.Any(bits => bits is < 1 or > 4))
            throw new ArgumentException(
                "Use one to three layers with one to four bits per layer.",
                nameof(layers)
            );
        return SetVectorQuantization(field, new { layers });
    }

    private ParadeDbIndexBuilder<TEntity> SetVectorQuantization(string field, object quantization)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        var fields = _indexBuilder
            .Metadata.FindAnnotation(ParadeDbAnnotationNames.IndexVectorFields)
            ?.Value
            is string json
            ? System.Text.Json.JsonSerializer.Deserialize<
                Dictionary<string, System.Text.Json.JsonElement>
            >(json)!
            : new Dictionary<string, System.Text.Json.JsonElement>();
        fields[field] = System.Text.Json.JsonSerializer.SerializeToElement(new { quantization });
        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.IndexVectorFields,
            System.Text.Json.JsonSerializer.Serialize(fields)
        );
        return this;
    }

    /// <summary>Choose graph routing or the experimental stacked IVF router.</summary>
    public ParadeDbIndexBuilder<TEntity> HasVectorRouter(string vectorRouter)
    {
        if (vectorRouter is not ("graph" or "ivf"))
            throw new ArgumentException(
                "Vector router must be graph or ivf.",
                nameof(vectorRouter)
            );
        _indexBuilder.HasAnnotation(ParadeDbAnnotationNames.IndexVectorRouter, vectorRouter);
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
