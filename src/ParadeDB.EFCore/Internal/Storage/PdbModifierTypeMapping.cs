using Microsoft.EntityFrameworkCore.Storage;

namespace ParadeDB.EFCore.Internal.Storage;

internal sealed class PdbModifierTypeMapping<T> : RelationalTypeMapping
    where T : struct
{
    public PdbModifierTypeMapping(T modifier)
        : base(modifier.ToString()!, typeof(T)) { }

    private PdbModifierTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters) =>
        new PdbModifierTypeMapping<T>(parameters);
}
