using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParadeDB.EntityFrameworkCore.Internal.Metadata;

namespace ParadeDB.EntityFrameworkCore.Extensions;

public static class ParadeDbIndexBuilderExtensions
{
    public static Bm25IndexBuilder<TEntity> HasBm25Index<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string name,
        Expression<Func<TEntity, object?>> keyExpression
    )
        where TEntity : class
    {
        var keyProperty = GetPropertyName(keyExpression);
        var indexBuilder = entityTypeBuilder.HasIndex(keyExpression).HasDatabaseName(name);

        indexBuilder.HasAnnotation(ParadeDbAnnotationNames.Bm25KeyProperty, keyProperty);
        indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.Bm25FieldProperties,
            new[] { keyProperty }
        );
        // BM25FieldKinds is used to track if each index field is an EF Core property or a SQL expression
        // so that it can be rendered appropriately
        indexBuilder.HasAnnotation(ParadeDbAnnotationNames.Bm25FieldKinds, new[] { "property" });
        indexBuilder.HasAnnotation(ParadeDbAnnotationNames.Bm25FieldTokenizers, new[] { "" });
        indexBuilder.HasAnnotation(ParadeDbAnnotationNames.Bm25FieldAliases, new[] { "" });

        return new Bm25IndexBuilder<TEntity>(indexBuilder);
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
            : throw new ArgumentException("The BM25 key expression must be a property access.");
    }
}

public record FieldAlias(string name) { }

// We use a custom index builder class instead of IndexBuilder directly to make it clear that not all
// normal index creation operations are supported here (e.g. `IsUnique`)
public sealed class Bm25IndexBuilder<TEntity>
    where TEntity : class
{
    private readonly IndexBuilder<TEntity> _indexBuilder;

    internal Bm25IndexBuilder(IndexBuilder<TEntity> indexBuilder)
    {
        _indexBuilder = indexBuilder;
    }

    public Bm25IndexBuilder<TEntity> HasField<TProperty>(
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

    public Bm25IndexBuilder<TEntity> HasField(string sql)
    {
        AddField(sql, "sql", null, null);

        return this;
    }

    public Bm25IndexBuilder<TEntity> HasField<TProperty>(
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

    public Bm25IndexBuilder<TEntity> HasField(string sql, Tokenizer tokenizer)
    {
        AddField(sql, "sql", tokenizer, null);

        return this;
    }

    public Bm25IndexBuilder<TEntity> HasField<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        FieldAlias @alias
    )
    {
        AddField(
            ParadeDbIndexBuilderExtensions.GetPropertyName(propertyExpression),
            "property",
            null,
            @alias.name
        );

        return this;
    }

    public Bm25IndexBuilder<TEntity> HasField(string sql, FieldAlias @alias)
    {
        AddField(sql, "sql", null, @alias.name);

        return this;
    }

    public Bm25IndexBuilder<TEntity> HasFilter(string? sql)
    {
        _indexBuilder.HasFilter(sql);

        return this;
    }

    public Bm25IndexBuilder<TEntity> IsCreatedConcurrently(bool createdConcurrently = true)
    {
        _indexBuilder.IsCreatedConcurrently(createdConcurrently);

        return this;
    }

    public Bm25IndexBuilder<TEntity> HasSearchTokenizer(Tokenizer tokenizer)
    {
        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.Bm25SearchTokenizer,
            tokenizer.ToSearchString()
        );

        return this;
    }

    private void AddField(string field, string kind, Tokenizer? tokenizer, string? @alias)
    {
        var properties = GetAnnotation(ParadeDbAnnotationNames.Bm25FieldProperties);
        var kinds = GetAnnotation(ParadeDbAnnotationNames.Bm25FieldKinds);
        var tokenizers = GetAnnotation(ParadeDbAnnotationNames.Bm25FieldTokenizers);
        var aliases = GetAnnotation(ParadeDbAnnotationNames.Bm25FieldAliases);

        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.Bm25FieldProperties,
            properties.Append(field).ToArray()
        );
        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.Bm25FieldKinds,
            kinds.Append(kind).ToArray()
        );
        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.Bm25FieldTokenizers,
            tokenizers.Append(tokenizer?.ToString() ?? "").ToArray()
        );

        _indexBuilder.HasAnnotation(
            ParadeDbAnnotationNames.Bm25FieldAliases,
            aliases.Append(@alias is null ? "" : @alias.Replace("'", "''")).ToArray()
        );
    }

    private string[] GetAnnotation(string name) =>
        (string[]?)_indexBuilder.Metadata.FindAnnotation(name)?.Value ?? [];
}
