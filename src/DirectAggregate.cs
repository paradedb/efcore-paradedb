using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ParadeDB.EntityFrameworkCore;

public sealed record DirectAggregateOptions
{
    public bool? SolveMvcc { get; init; }
    public long MemoryLimit { get; init; } = 500000000;
    public long? BucketLimit { get; init; }
    public string? Visibility { get; init; }
}

public sealed class DirectAggregateResult
{
    [Column("result")]
    public JsonElement Result { get; set; }
}

public static class ParadeDbDirectAggregateExtensions
{
    public static IQueryable<DirectAggregateResult> Aggregate(
        this DatabaseFacade database,
        string index,
        SearchQuery query,
        object spec,
        DirectAggregateOptions? options = null
    )
    {
        ArgumentNullException.ThrowIfNull(query);
        options ??= new();
        if (options.MemoryLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.MemoryLimit));
        if (options.BucketLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.BucketLimit));
        if (
            options.Visibility is not null
            && options.Visibility is not ("transaction" or "raw" or "threshold")
        )
            throw new ArgumentException("Visibility must be transaction, raw, or threshold");
        if (options.SolveMvcc is not null && options.Visibility is not null)
            throw new ArgumentException("Specify SolveMvcc or Visibility, not both");
        object[] parameters =
        [
            index,
            query.ToJson(),
            JsonSerializer.Serialize(spec),
            options.SolveMvcc!,
            options.MemoryLimit,
            options.BucketLimit!,
            options.Visibility!,
        ];
        return database.SqlQuery<DirectAggregateResult>(
            FormattableStringFactory.Create(
                "SELECT paradedb.aggregate({0}::regclass, {1}::paradedb.searchqueryinput, {2}::json, {3}::boolean, {4}::bigint, {5}::bigint, {6}::text) AS result",
                parameters
            )
        );
    }
}
