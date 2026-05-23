using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ParadeDB.EntityFrameworkCore;

public static partial class Pdb
{
    public static PdbMoreLikeThisQuery Mlt(long documentId) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Mlt)));

    public static PdbMoreLikeThisQuery Mlt(string documentJson) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Mlt)));
}
