using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace ParadeDB.EntityFrameworkCore.Internal.Metadata;

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
            i.FindAnnotation(ParadeDbAnnotationNames.IndexKeyProperty)?.Value is string
        );
        if (
            mappedIndex?.FindAnnotation(ParadeDbAnnotationNames.IndexKeyProperty)?.Value
            is not string keyPropertyName
        )
        {
            yield break;
        }

        var fieldProperties = (string[])
            mappedIndex.FindAnnotation(ParadeDbAnnotationNames.IndexFieldProperties)!.Value!;
        var fieldKinds = (string[])
            mappedIndex.FindAnnotation(ParadeDbAnnotationNames.IndexFieldKinds)!.Value!;
        var fieldTokenizers = (string[])
            mappedIndex.FindAnnotation(ParadeDbAnnotationNames.IndexFieldTokenizers)!.Value!;
        var fieldAliases = (string[])
            mappedIndex.FindAnnotation(ParadeDbAnnotationNames.IndexFieldAliases)!.Value!;

        if (
            fieldProperties.Length != fieldKinds.Length
            || fieldProperties.Length != fieldTokenizers.Length
            || fieldProperties.Length != fieldAliases.Length
        )
        {
            throw new InvalidOperationException(
                "A ParadeDB index must have one kind, tokenizer, and alias entry for each field."
            );
        }

        var storeObject = StoreObjectIdentifier.Table(index.Table.Name, index.Table.Schema);

        yield return new Annotation(
            ParadeDbAnnotationNames.IndexKeyField,
            mappedIndex
                .DeclaringEntityType.FindProperty(keyPropertyName)!
                .GetColumnName(storeObject)!
        );

        yield return new Annotation(
            ParadeDbAnnotationNames.IndexFields,
            fieldProperties
                .Select(
                    (field, i) =>
                        RenderField(
                            mappedIndex.DeclaringEntityType,
                            field,
                            fieldKinds[i],
                            storeObject,
                            string.IsNullOrEmpty(fieldTokenizers[i]) ? null : fieldTokenizers[i],
                            string.IsNullOrEmpty(fieldAliases[i]) ? null : fieldAliases[i]
                        )
                )
                .ToArray()
        );

        if (
            mappedIndex.FindAnnotation(ParadeDbAnnotationNames.IndexSearchTokenizer)?.Value
            is string searchTokenizer
        )
        {
            yield return new Annotation(
                ParadeDbAnnotationNames.IndexSearchTokenizer,
                searchTokenizer
            );
        }
    }

    private static string RenderField(
        IReadOnlyEntityType entityType,
        string field,
        string kind,
        StoreObjectIdentifier storeObject,
        string? tokenizer,
        string? alias
    )
    {
        var sql = kind switch
        {
            "property" => entityType.FindProperty(field)!.GetColumnName(storeObject)!,
            "sql" => field,
            _ => throw new InvalidOperationException($"Unknown field kind '{kind}'"),
        };

        if (tokenizer is null && alias is null)
        {
            return sql;
        }

        if (tokenizer is not null && alias is not null)
        {
            throw new InvalidOperationException(
                $"A field may not have a tokenizer and alias at the same time {tokenizer}, {alias}"
            );
        }

        if (tokenizer is not null)
        {
            return kind == "property" ? $"({sql}::{tokenizer})" : $"(({sql})::{tokenizer})";
        }

        return kind == "property"
            ? $"({sql}::pdb.alias('{alias}'))"
            : $"(({sql})::pdb.alias('{alias}'))";
    }
}
