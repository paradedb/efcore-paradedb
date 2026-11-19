using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.VectorData;
using ParadeDB.EntityFrameworkCore.Extensions;

namespace ParadeDB.EntityFrameworkCore.VectorData;

/// <summary>Runs cosine similarity searches over an existing ParadeDB EF Core query.</summary>
/// <remarks>
/// <para>
/// Map the embedding property to a PostgreSQL vector column and include it in a ParadeDB index
/// with <c>vector_cosine_ops</c>. The source query needs a ParadeDB predicate for index-backed
/// ranking. For filtered searches, include the filter columns in the index and use
/// EF-translatable filters.
/// </para>
/// <para>
/// Register the adapter in the same DI scope as its context. Enumerate results within that
/// scope and complete or dispose the enumeration before using the context for another operation.
/// EF Core continues to own writes, migrations, and transactions.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// services.AddScoped&lt;IVectorSearchable&lt;Document&gt;&gt;(provider =>
/// {
///     var db = provider.GetRequiredService&lt;AppDbContext&gt;();
///     return new ParadeDbVectorSearch&lt;Document&gt;(
///         db.Documents.Where(document => EF.Functions.All(document.Id)),
///         nameof(Document.Embedding),
///         document => new Document { Id = document.Id, Title = document.Title }
///     );
/// });
///
/// await foreach (var hit in search.SearchAsync(
///     queryEmbedding,
///     top: 10,
///     options: new() { Filter = document => document.TenantId == tenantId },
///     cancellationToken: cancellationToken
/// ))
/// {
///     Console.WriteLine($"{hit.Record.Title}: {hit.Score}");
/// }
/// </code>
/// </example>
/// <param name="records">
/// An EF Core entity query with a ParadeDB search predicate, such as EF.Functions.All on the index key.
/// </param>
/// <param name="embeddingProperty">The EF property name of the indexed float array embedding.</param>
/// <param name="withoutVectors">
/// A projection that returns the desired record fields without its embeddings.
/// </param>
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
