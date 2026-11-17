using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
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

    /// <summary>
    /// Expands <paramref name="text" /> with the synonym rules in <paramref name="rules" />, for
    /// <c>MatchAny</c>, <c>MatchAll</c>, <c>Phrase</c> or <c>Parse</c>. Write the values of a
    /// <c>Where</c> on the rules as constants or with <c>EF.Constant</c>. A partial synonym index
    /// can't match a parameter.
    /// </summary>
    public static string Expand(string text, IQueryable<string> rules) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Expand)));

    internal static string ExpandRule(string text, string rule) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Expand)));
}
