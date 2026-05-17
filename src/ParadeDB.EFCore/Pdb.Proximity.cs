using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ParadeDB.EFCore;

public static partial class Pdb
{
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
}
