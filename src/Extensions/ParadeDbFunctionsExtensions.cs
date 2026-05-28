using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using NpgsqlTypes;

namespace ParadeDB.EntityFrameworkCore.Extensions;

[ExcludeFromCodeCoverage]
public static class ParadeDbFunctionsExtensions
{
    [DbFunction]
    public static bool MatchAny<TProperty>(this DbFunctions _, TProperty property, string value) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchAny)));

    [DbFunction]
    public static bool MatchAny<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchAny)));

    [DbFunction]
    public static bool MatchAll<TProperty>(this DbFunctions _, TProperty property, string value) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchAll)));

    [DbFunction]
    public static bool MatchAll<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchAll)));

    [DbFunction]
    public static bool Phrase<TProperty>(this DbFunctions _, TProperty property, string value) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Phrase)));

    [DbFunction]
    public static bool Phrase<TProperty>(this DbFunctions _, TProperty property, string[] values) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Phrase)));

    [DbFunction]
    public static bool Term<TProperty>(this DbFunctions _, TProperty property, string value) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Term)));

    [DbFunction]
    public static bool Term<TProperty>(this DbFunctions _, TProperty property, string[] values) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Term)));

    [DbFunction]
    public static float Score<TProperty>(this DbFunctions _, TProperty property) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Score)));

    [DbFunction]
    public static string? Snippet<TProperty>(this DbFunctions _, TProperty property) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Snippet)));

    [DbFunction]
    public static string? Snippet<TProperty>(
        this DbFunctions _,
        TProperty property,
        [NotParameterized] SnippetOptions? options
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Snippet)));

    [DbFunction]
    public static string[]? Snippets<TProperty>(this DbFunctions _, TProperty property) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Snippets)));

    [DbFunction]
    public static string[]? Snippets<TProperty>(
        this DbFunctions _,
        TProperty property,
        [NotParameterized] SnippetsOptions? options
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Snippets)));

    [DbFunction]
    public static int[,]? SnippetPositions<TProperty>(this DbFunctions _, TProperty property) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(SnippetPositions)));

    [DbFunction]
    public static bool Proximity<TProperty>(
        this DbFunctions _,
        TProperty property,
        PdbQuery query
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Proximity)));

    [DbFunction]
    public static string Tokenize<TProperty>(
        this DbFunctions _,
        TProperty property,
        [NotParameterized] Tokenizer tokenizer
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Tokenize)));

    [DbFunction]
    public static IEnumerable<string> TokenizeAsArray<TProperty>(
        this DbFunctions _,
        TProperty property,
        [NotParameterized] Tokenizer tokenizer
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TokenizeAsArray)));

    [DbFunction]
    public static bool MoreLikeThis<TProperty>(
        this DbFunctions _,
        TProperty property,
        PdbMoreLikeThisQuery query
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MoreLikeThis)));

    [DbFunction]
    public static string Alias<TProperty>(
        this DbFunctions _,
        TProperty property,
        [NotParameterized] string aliasName
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Alias)));

    [DbFunction]
    public static JsonElement? Agg(
        this DbFunctions _,
        [NotParameterized] object aggregate,
        [NotParameterized] bool exact = true
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Agg)));

    [DbFunction]
    public static JsonElement? AggFilter(
        this DbFunctions _,
        [NotParameterized] object aggregate,
        bool filter,
        [NotParameterized] bool exact = true
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(AggFilter)));

    [DbFunction]
    public static JsonElement? AggOver(
        this DbFunctions _,
        [NotParameterized] object aggregate,
        [NotParameterized] bool exact = true
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(AggOver)));

    [DbFunction]
    public static JsonElement? AggFilterOver(
        this DbFunctions _,
        [NotParameterized] object aggregate,
        bool filter,
        [NotParameterized] bool exact = true
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(AggFilterOver)));

    [DbFunction]
    public static bool All<TProperty>(this DbFunctions _, TProperty property) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(All)));

    [DbFunction]
    public static bool Exists<TProperty>(this DbFunctions _, TProperty property) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Exists)));

    [DbFunction]
    public static bool Parse<TProperty>(this DbFunctions _, TProperty property, string pattern) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Parse)));

    [DbFunction]
    public static bool Parse<TProperty>(
        this DbFunctions _,
        TProperty property,
        string pattern,
        bool? lenient = null,
        bool? conjunctionMode = null
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Parse)));

    [DbFunction]
    public static bool Regex<TProperty>(
        this DbFunctions _,
        TProperty property,
        [StringSyntax(StringSyntaxAttribute.Regex)] string pattern
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Regex)));

    [DbFunction]
    public static bool RangeTerm<TProperty, TValue>(
        this DbFunctions _,
        TProperty property,
        TValue value
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RangeTerm)));

    [DbFunction]
    public static bool RangeTerm<TProperty, TValue>(
        this DbFunctions _,
        TProperty property,
        NpgsqlRange<TValue> range,
        [NotParameterized] RangeTermRelation relation
    )
        where TValue : IComparable<TValue> =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RangeTerm)));

    [DbFunction]
    public static bool PhrasePrefix<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] tokens,
        int? maxExpansions = null
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(PhrasePrefix)));

    [DbFunction]
    public static bool RegexPhrase<TProperty>(
        this DbFunctions _,
        TProperty property,
        [StringSyntax(StringSyntaxAttribute.Regex)] string[] phrases,
        int? slop = null,
        int? maxExpansions = null
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RegexPhrase)));
}
