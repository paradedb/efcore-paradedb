using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using NpgsqlTypes;
using ParadeDB.EntityFrameworkCore.Modifiers;

namespace ParadeDB.EntityFrameworkCore;

[ExcludeFromCodeCoverage]
public static class Pdb
{
    public static Boost Boost(float factor) => new(factor);

    public static T Boost<T>(T value, [NotParameterized] float factor) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Boost)));

    public static Fuzzy Fuzzy(
        int distance,
        bool prefix = false,
        bool transpositionCostOne = false
    ) => new(distance, prefix, transpositionCostOne);

    public static T Fuzzy<T>(T value, [NotParameterized] int distance) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Fuzzy)));

    public static T Fuzzy<T>(
        T value,
        [NotParameterized] int distance,
        [NotParameterized] bool prefix
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Fuzzy)));

    public static T Fuzzy<T>(
        T value,
        [NotParameterized] int distance,
        [NotParameterized] bool prefix,
        [NotParameterized] bool transpositionCostOne
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Fuzzy)));

    public static Const Const(float value) => new(value);

    public static T Const<T>(T value, [NotParameterized] float score) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Const)));

    public static Slop Slop(int value) => new(value);

    public static T Slop<T>(T value, [NotParameterized] int slop) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Slop)));

    public static PdbMoreLikeThisQuery DocumentId(long documentId) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DocumentId)));

    public static PdbMoreLikeThisQuery Document(string documentJson) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DocumentId)));

    public static PdbProximityQuery Proximity(string token) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Proximity)));

    public static PdbProximityQuery ProximityRegex(
        [StringSyntax(StringSyntaxAttribute.Regex)] string pattern
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ProximityRegex)));

    public static PdbProximityQuery ProximityRegex(
        [StringSyntax(StringSyntaxAttribute.Regex)] string pattern,
        int maxExpansions
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ProximityRegex)));

    public static PdbProximityQuery ProximityArray(params string[] tokens) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ProximityArray)));

    public static PdbProximityQuery ProximityArray(params PdbProximityQuery[] operands) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ProximityArray)));

    public static PdbQuery Exists() =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Exists)));

    public static PdbQuery Parse(string pattern) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Parse)));

    public static PdbQuery Regex([StringSyntax(StringSyntaxAttribute.Regex)] string pattern) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Regex)));

    public static PdbQuery RangeTerm<T>(T value) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RangeTerm)));

    public static PdbQuery RangeTerm<T>(
        NpgsqlRange<T> range,
        [NotParameterized] RangeTermRelation relation
    )
        where T : IComparable<T> =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RangeTerm)));

    public static PdbQuery PhrasePrefix(string[] tokens, int? maxExpansions = null) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(PhrasePrefix)));

    public static PdbQuery RegexPhrase(
        [StringSyntax(StringSyntaxAttribute.Regex)] string[] phrases,
        int? slop = null,
        int? maxExpansions = null
    ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RegexPhrase)));
}
