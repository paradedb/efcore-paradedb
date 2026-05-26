using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ParadeDB.EntityFrameworkCore;

public static partial class Pdb
{
    public static PdbMoreLikeThisQuery DocumentId(long documentId) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DocumentId)));

    public static PdbMoreLikeThisQuery Document(string documentJson) =>
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DocumentId)));
}
