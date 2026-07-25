using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using ParadeDB.EntityFrameworkCore.Internal;
#if NET8_0
using Npgsql;
using ParadeDB.EntityFrameworkCore.Internal.Storage;
#endif

// ReSharper disable SuspiciousTypeConversion.Global

namespace ParadeDB.EntityFrameworkCore.Extensions;

public static class ParadeDbDbContextOptionsBuilderExtensions
{
#if NET8_0
    private static int _vectorResolverRegistered;
#endif

    public static NpgsqlDbContextOptionsBuilder UseParadeDb(
        this NpgsqlDbContextOptionsBuilder optionsBuilder
    )
    {
#if NET8_0
        // Npgsql.EntityFrameworkCore.PostgreSQL 8.x has no INpgsqlDataSourceConfigurationPlugin,
        // so the vector resolver is registered globally instead
        if (Interlocked.Exchange(ref _vectorResolverRegistered, 1) == 0)
        {
#pragma warning disable CS0618
            NpgsqlConnection.GlobalTypeMapper.AddTypeInfoResolverFactory(
                new PdbVectorTypeInfoResolverFactory()
            );
#pragma warning restore CS0618
        }
#endif
        var coreOptionsBuilder = (
            (IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder
        ).OptionsBuilder;

        ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(
            new ParadeDbOptionsExtension()
        );

        return optionsBuilder;
    }
}
