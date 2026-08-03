using Microsoft.EntityFrameworkCore.Storage;

namespace ParadeDB.EntityFrameworkCore.Internal.Storage;

internal sealed class ParadeDbTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        if (
            !string.Equals(
                mappingInfo.StoreTypeNameBase,
                "vector",
                StringComparison.OrdinalIgnoreCase
            ) || (mappingInfo.ClrType is not null && mappingInfo.ClrType != typeof(float[]))
        )
        {
            return null;
        }

        return new PdbVectorTypeMapping(mappingInfo.StoreTypeName!);
    }
}
