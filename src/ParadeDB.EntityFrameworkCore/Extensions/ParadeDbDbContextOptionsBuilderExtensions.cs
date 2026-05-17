using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using ParadeDB.EntityFrameworkCore.Internal;

// ReSharper disable SuspiciousTypeConversion.Global

namespace ParadeDB.EntityFrameworkCore.Extensions;

public static class ParadeDbDbContextOptionsBuilderExtensions
{
    public static NpgsqlDbContextOptionsBuilder UseParadeDb(
        this NpgsqlDbContextOptionsBuilder optionsBuilder
    )
    {
        var coreOptionsBuilder = (
            (IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder
        ).OptionsBuilder;

        ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(
            new ParadeDbOptionsExtension()
        );

        return optionsBuilder;
    }
}
