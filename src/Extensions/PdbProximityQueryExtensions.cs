using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

namespace ParadeDB.EntityFrameworkCore.Extensions;

public static class PdbProximityQueryExtensions
{
    extension(PdbProximityQuery left)
    {
        public PdbProximityQuery Within(
            int distance,
            string right,
            [NotParameterized] bool ordered = false
        ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Within)));

        public PdbProximityQuery Within(
            int distance,
            PdbProximityQuery right,
            [NotParameterized] bool ordered = false
        ) => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Within)));
    }
}
