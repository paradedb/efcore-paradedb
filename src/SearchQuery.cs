using System.Text.Json;

namespace ParadeDB.EntityFrameworkCore;

/// <summary>A composable ParadeDB query input. Query strings may include field qualifiers.</summary>
public sealed class SearchQuery
{
    private readonly object _value;

    private SearchQuery(object value) => _value = value;

    public static SearchQuery Parse(
        string query,
        bool lenient = false,
        bool conjunctionMode = false
    )
    {
        ArgumentNullException.ThrowIfNull(query);
        return new(
            new
            {
                parse = new
                {
                    query_string = query,
                    lenient,
                    conjunction_mode = conjunctionMode,
                },
            }
        );
    }

    public static SearchQuery Boolean(
        IReadOnlyList<SearchQuery>? must = null,
        IReadOnlyList<SearchQuery>? should = null,
        IReadOnlyList<SearchQuery>? mustNot = null,
        int? minimumShouldMatch = null
    )
    {
        if (minimumShouldMatch < 0)
            throw new ArgumentOutOfRangeException(nameof(minimumShouldMatch));
        return new(
            new
            {
                boolean = new
                {
                    must = Values(must),
                    should = Values(should),
                    must_not = Values(mustNot),
                    minimum_should_match = minimumShouldMatch,
                },
            }
        );
    }

    public static SearchQuery DisjunctionMax(
        IReadOnlyList<SearchQuery> disjuncts,
        float? tieBreaker = null
    )
    {
        ArgumentNullException.ThrowIfNull(disjuncts);
        if (disjuncts.Count == 0)
            throw new ArgumentException("Disjuncts must not be empty", nameof(disjuncts));
        if (tieBreaker is { } tie && (!float.IsFinite(tie) || tie is < 0 or > 1))
            throw new ArgumentOutOfRangeException(nameof(tieBreaker));
        return new(
            new
            {
                disjunction_max = new { disjuncts = Values(disjuncts), tie_breaker = tieBreaker },
            }
        );
    }

    private static object[] Values(IReadOnlyList<SearchQuery>? values) =>
        values
            ?.Select(value =>
                value?._value ?? throw new ArgumentException("Query clauses must not be null")
            )
            .ToArray()
        ?? [];

    internal string ToJson() => JsonSerializer.Serialize(_value);
}
