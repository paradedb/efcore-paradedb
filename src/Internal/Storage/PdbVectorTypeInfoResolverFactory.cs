using Npgsql.Internal;
using Npgsql.Internal.Postgres;

namespace ParadeDB.EntityFrameworkCore.Internal.Storage;

internal sealed class PdbVectorTypeInfoResolverFactory : PgTypeInfoResolverFactory
{
    public override IPgTypeInfoResolver CreateResolver() => new Resolver();

    public override IPgTypeInfoResolver? CreateArrayResolver() => null;

    private sealed class Resolver : IPgTypeInfoResolver
    {
        private TypeInfoMappingCollection? _mappings;

        private TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new());

        public PgTypeInfo? GetTypeInfo(
            Type? type,
            DataTypeName? dataTypeName,
            PgSerializerOptions options
        ) => Mappings.Find(type, dataTypeName, options);

        private static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            mappings.AddType<float[]>(
                "vector",
                static (options, mapping, _) =>
                    mapping.CreateInfo(options, new PdbVectorConverter()),
                isDefault: true
            );

            return mappings;
        }
    }
}
