#if !NET8_0
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using ParadeDB.EntityFrameworkCore.Internal.Storage;

namespace ParadeDB.EntityFrameworkCore.Internal;

internal sealed class ParadeDbDataSourceConfigurationPlugin : INpgsqlDataSourceConfigurationPlugin
{
    public void Configure(NpgsqlDataSourceBuilder npgsqlDataSourceBuilder) =>
        npgsqlDataSourceBuilder.AddTypeInfoResolverFactory(new PdbVectorTypeInfoResolverFactory());
}
#endif
