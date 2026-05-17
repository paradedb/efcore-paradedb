using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ParadeDB.EFCore.Extensions;

public static class PdbProximityQueryExtensions
{
    extension(PdbProximityQuery left)
    {
        public PdbProximityQuery Within(int distance, string right) =>
            throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Within)));

        public PdbProximityQuery Within(int distance, PdbProximityQuery right) =>
            throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Within)));

        public PdbProximityQuery WithinOrdered(int distance, string right) =>
            throw new InvalidOperationException(
                CoreStrings.FunctionOnClient(nameof(WithinOrdered))
            );

        public PdbProximityQuery WithinOrdered(int distance, PdbProximityQuery right) =>
            throw new InvalidOperationException(
                CoreStrings.FunctionOnClient(nameof(WithinOrdered))
            );
    }
}
