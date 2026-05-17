using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using ParadeDB.EFCore.Modifiers;

namespace ParadeDB.EFCore.Extensions;

[ExcludeFromCodeCoverage]
public static class ParadeDbFunctionsExtensions
{
    [DbFunction]
    public static bool MatchDisjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchDisjunction)));

    [DbFunction]
    public static bool MatchDisjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchDisjunction)));

    [DbFunction]
    public static bool MatchDisjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Fuzzy fuzzy
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchDisjunction)));

    [DbFunction]
    public static bool MatchDisjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values,
        [NotParameterized] Fuzzy fuzzy
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchDisjunction)));

    [DbFunction]
    public static bool MatchDisjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Boost boost
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchDisjunction)));

    [DbFunction]
    public static bool MatchDisjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values,
        [NotParameterized] Boost boost
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchDisjunction)));

    [DbFunction]
    public static bool MatchDisjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Const @const
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchDisjunction)));

    [DbFunction]
    public static bool MatchDisjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values,
        [NotParameterized] Const @const
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchDisjunction)));

    [DbFunction]
    public static bool MatchDisjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Fuzzy fuzzy,
        [NotParameterized] Boost boost
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchDisjunction)));

    [DbFunction]
    public static bool MatchConjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchConjunction)));

    [DbFunction]
    public static bool MatchConjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchConjunction)));

    [DbFunction]
    public static bool MatchConjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Fuzzy fuzzy
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchConjunction)));

    [DbFunction]
    public static bool MatchConjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values,
        [NotParameterized] Fuzzy fuzzy
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchConjunction)));

    [DbFunction]
    public static bool MatchConjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Boost boost
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchConjunction)));

    [DbFunction]
    public static bool MatchConjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values,
        [NotParameterized] Boost boost
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchConjunction)));

    [DbFunction]
    public static bool MatchConjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Const @const
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchConjunction)));

    [DbFunction]
    public static bool MatchConjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values,
        [NotParameterized] Const @const
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchConjunction)));

    [DbFunction]
    public static bool MatchConjunction<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Fuzzy fuzzy,
        [NotParameterized] Boost boost
    ) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchConjunction)));

    [DbFunction]
    public static bool Phrase<TProperty>(this DbFunctions _, TProperty property, string value) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Phrase)));

    [DbFunction]
    public static bool Phrase<TProperty>(this DbFunctions _, TProperty property, string[] values) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Phrase)));

    [DbFunction]
    public static bool Phrase<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Boost boost
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Phrase)));

    [DbFunction]
    public static bool Phrase<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values,
        [NotParameterized] Boost boost
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Phrase)));

    [DbFunction]
    public static bool Phrase<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Const @const
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Phrase)));

    [DbFunction]
    public static bool Phrase<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values,
        [NotParameterized] Const @const
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Phrase)));

    [DbFunction]
    public static bool Phrase<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Slop slop
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Phrase)));

    [DbFunction]
    public static bool Phrase<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values,
        [NotParameterized] Slop slop
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Phrase)));

    [DbFunction]
    public static bool Term<TProperty>(this DbFunctions _, TProperty property, string value) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Term)));

    [DbFunction]
    public static bool Term<TProperty>(this DbFunctions _, TProperty property, string[] values) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Term)));

    [DbFunction]
    public static bool Term<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Fuzzy fuzzy
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Term)));

    [DbFunction]
    public static bool Term<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values,
        [NotParameterized] Fuzzy fuzzy
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Term)));

    [DbFunction]
    public static bool Term<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Boost boost
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Term)));

    [DbFunction]
    public static bool Term<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values,
        [NotParameterized] Boost boost
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Term)));

    [DbFunction]
    public static bool Term<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Const @const
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Term)));

    [DbFunction]
    public static bool Term<TProperty>(
        this DbFunctions _,
        TProperty property,
        string[] values,
        [NotParameterized] Const @const
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Term)));

    [DbFunction]
    public static bool Term<TProperty>(
        this DbFunctions _,
        TProperty property,
        string value,
        [NotParameterized] Fuzzy fuzzy,
        [NotParameterized] Boost boost
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Term)));

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
        int maxNumChars
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Snippet)));

    [DbFunction]
    public static string? Snippet<TProperty>(
        this DbFunctions _,
        TProperty property,
        string startTag,
        string endTag
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Snippet)));

    [DbFunction]
    public static string? Snippet<TProperty>(
        this DbFunctions _,
        TProperty property,
        string startTag,
        string endTag,
        int maxNumChars
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Snippet)));

    [DbFunction]
    public static bool Match<TProperty>(this DbFunctions _, TProperty property, PdbQuery query) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Match)));

    /*
     TODO https://docs.paradedb.com/documentation/full-text/phrase#using-a-custom-tokenizer
     If the tokenizer produces an empty result, the query throws an exception.
     TODO Keep this internal for now; expose publicly only if that behavior changes.
   */
    /*[DbFunction]
    public static string Tokenize<TProperty>(
        this DbFunctions _,
        TProperty property,
        [NotParameterized] Tokenizer tokenizer
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Tokenize)));*/

    [DbFunction]
    public static IEnumerable<string> TokenizeAsArray<TProperty>(
        this DbFunctions _,
        TProperty property,
        [NotParameterized] Tokenizer tokenizer
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TokenizeAsArray)));

    [DbFunction]
    public static string Alias<TProperty>(
        this DbFunctions _,
        TProperty property,
        [NotParameterized] string aliasName
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Alias)));
}
