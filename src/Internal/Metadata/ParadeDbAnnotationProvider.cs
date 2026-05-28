using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace ParadeDB.EntityFrameworkCore.Internal.Metadata;

// NpgsqlAnnotationProvider is not part of the public API they explicitly note that it
// is not subject to the same compatibility standards as the public API. So we may have to
// rework things here to support new releases. I don't know that there is a way for us to
// implement the indexing functionality in a way that only relies on the public API.
internal sealed class ParadeDbAnnotationProvider : NpgsqlAnnotationProvider
{
    public ParadeDbAnnotationProvider(RelationalAnnotationProviderDependencies dependencies)
        : base(dependencies) { }

    public override IEnumerable<IAnnotation> For(ITableIndex index, bool designTime)
    {
        foreach (var annotation in base.For(index, designTime))
        {
            yield return annotation;
        }

        var mappedIndex = index.MappedIndexes.FirstOrDefault(i =>
            i.FindAnnotation(ParadeDbAnnotationNames.Bm25KeyProperty)?.Value is string
        );
        if (
            mappedIndex?.FindAnnotation(ParadeDbAnnotationNames.Bm25KeyProperty)?.Value
            is not string keyPropertyName
        )
        {
            yield break;
        }

        var fieldProperties =
            (string[]?)
                mappedIndex.FindAnnotation(ParadeDbAnnotationNames.Bm25FieldProperties)?.Value
            ?? [];
        var fieldKinds =
            (string[]?)mappedIndex.FindAnnotation(ParadeDbAnnotationNames.Bm25FieldKinds)?.Value
            ?? [];
        var fieldTokenizers =
            (string[]?)
                mappedIndex.FindAnnotation(ParadeDbAnnotationNames.Bm25FieldTokenizers)?.Value
            ?? [];

        if (
            fieldProperties.Length != fieldKinds.Length
            || fieldProperties.Length != fieldTokenizers.Length
        )
        {
            throw new InvalidOperationException(
                "A BM25 index must have one kind and tokenizer entry for each field."
            );
        }

        var storeObject = StoreObjectIdentifier.Table(index.Table.Name, index.Table.Schema);

        yield return new Annotation(
            ParadeDbAnnotationNames.Bm25KeyField,
            GetColumnName(mappedIndex.DeclaringEntityType, keyPropertyName, storeObject)
        );
        yield return new Annotation(
            ParadeDbAnnotationNames.Bm25Fields,
            fieldProperties
                .Select(
                    (field, i) =>
                        RenderField(
                            mappedIndex.DeclaringEntityType,
                            field,
                            fieldKinds[i],
                            storeObject,
                            string.IsNullOrEmpty(fieldTokenizers[i]) ? null : fieldTokenizers[i]
                        )
                )
                .ToArray()
        );
    }

    private static string RenderField(
        IReadOnlyEntityType entityType,
        string field,
        string kind,
        StoreObjectIdentifier storeObject,
        string? tokenizer
    )
    {
        var sql =
            kind == "property" ? GetColumnName(entityType, field, storeObject)
            : kind == "sql" ? field
            : throw new InvalidOperationException(
                $"The BM25 field kind '{kind}' is not supported."
            );

        if (tokenizer is null)
        {
            return sql;
        }

        return kind == "property" ? $"({sql}::{tokenizer})" : $"(({sql})::{tokenizer})";
    }

    private static string GetColumnName(
        IReadOnlyEntityType entityType,
        string propertyName,
        StoreObjectIdentifier storeObject
    )
    {
        var property =
            entityType.FindProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"The BM25 field '{propertyName}' was not found."
            );

        return property.GetColumnName(storeObject)
            ?? throw new InvalidOperationException(
                $"The BM25 field '{propertyName}' is not mapped to a column."
            );
    }
}
