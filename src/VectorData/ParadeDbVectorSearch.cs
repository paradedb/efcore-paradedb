using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.VectorData;
using ParadeDB.EntityFrameworkCore.Extensions;

namespace ParadeDB.VectorData;

/// <summary>Runs cosine similarity searches over an existing ParadeDB EF Core query.</summary>
/// <remarks>
/// Use a ParadeDB index with <c>vector_cosine_ops</c>. For index-backed filtering, include the
/// filter columns in the index. Enumerate results within the owning context’s scope and finish
/// enumeration before starting another operation on that context.
/// </remarks>
/// <param name="records">An EF query with a ParadeDB predicate, such as <c>EF.Functions.All</c> on the index key.</param>
/// <param name="embeddingProperty">The mapped <c>float[]</c> embedding property name.</param>
/// <param name="withoutVectors">The record projection used when embeddings are omitted.</param>
public sealed class ParadeDbVectorSearch<TRecord>(
    IQueryable<TRecord> records,
    string embeddingProperty,
    Expression<Func<TRecord, TRecord>> withoutVectors
) : IVectorSearchable<TRecord>
    where TRecord : class
{
    private readonly IQueryable<TRecord> _records =
        records ?? throw new ArgumentNullException(nameof(records));
    private readonly string _embeddingProperty = !string.IsNullOrWhiteSpace(embeddingProperty)
        ? embeddingProperty
        : throw new ArgumentException(
            "An embedding property is required.",
            nameof(embeddingProperty)
        );
    private readonly Expression<Func<TRecord, TRecord>> _withoutVectors =
        withoutVectors ?? throw new ArgumentNullException(nameof(withoutVectors));

    /// <summary>Returns up to <paramref name="top"/> records ordered by decreasing cosine similarity.</summary>
    /// <param name="searchValue">
    /// A precomputed <c>float[]</c> or <c>ReadOnlyMemory&lt;float&gt;</c> of the indexed dimension.
    /// </param>
    /// <param name="top">Maximum number of matches.</param>
    /// <param name="options">
    /// Supports <c>Filter</c> and <c>IncludeVectors</c>. Enabling <c>IncludeVectors</c>
    /// returns the full untracked entity. <c>Skip</c>, <c>ScoreThreshold</c>, and
    /// <c>VectorProperty</c> are unsupported.
    /// </param>
    /// <param name="cancellationToken">Cancels query execution and result enumeration.</param>
    /// <returns>Matches with cosine similarity scores computed as one minus cosine distance.</returns>
    public async IAsyncEnumerable<VectorSearchResult<TRecord>> SearchAsync<TInput>(
        TInput searchValue,
        int top,
        VectorSearchOptions<TRecord>? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
        where TInput : notnull
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(top);
        cancellationToken.ThrowIfCancellationRequested();

        var embedding = searchValue switch
        {
            float[] values => values,
            ReadOnlyMemory<float> values => values.ToArray(),
            _ => throw new NotSupportedException(
                "Supply a float[] or ReadOnlyMemory<float> embedding."
            ),
        };
        if (
            embedding.Length == 0
            || Array.Exists(embedding, value => !float.IsFinite(value))
            || Array.TrueForAll(embedding, value => value == 0)
        )
        {
            throw new ArgumentException(
                "A cosine query needs a finite, nonzero embedding.",
                nameof(searchValue)
            );
        }

        if (
            options is not null
            && (
                options.Skip != 0
                || options.ScoreThreshold is not null
                || options.VectorProperty is not null
            )
        )
        {
            throw new NotSupportedException(
                "Skip, ScoreThreshold, and VectorProperty are unsupported."
            );
        }

        var query = _records.AsNoTracking();
        if (options?.Filter is { } filter)
            query = query.Where(filter);

        Expression<Func<TRecord, double>> distance = record =>
            EF.Functions.CosineDistance(
                EF.Property<float[]?>(record, _embeddingProperty),
                embedding
            );
        // Exclude rows with no cosine distance, including when PostgreSQL scans the table.
        var ranked = query
            .Where(record => EF.Property<float[]?>(record, _embeddingProperty) != null)
            .OrderBy(distance)
            .Take(top);

        Expression<Func<TRecord, double, VectorSearchResult<TRecord>>> result = (record, value) =>
            new VectorSearchResult<TRecord>(record, 1 - value);
        var row = distance.Parameters[0];
        var projectedRecord =
            options?.IncludeVectors == true
                ? (Expression)row
                : ReplacingExpressionVisitor.Replace(
                    _withoutVectors.Parameters[0],
                    row,
                    _withoutVectors.Body
                );
        var selector = Expression.Lambda<Func<TRecord, VectorSearchResult<TRecord>>>(
            ReplacingExpressionVisitor.Replace(
                result.Parameters[0],
                projectedRecord,
                ReplacingExpressionVisitor.Replace(result.Parameters[1], distance.Body, result.Body)
            ),
            row
        );

        await foreach (
            var match in ranked
                .Select(selector)
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false)
        )
            yield return match;
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        return serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;
    }
}
