using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace ParadeDB.EntityFrameworkCore.Internal.Storage;

internal static class PdbTypeMappings
{
    public static readonly RelationalTypeMapping Boolean = new NpgsqlBoolTypeMapping();
    public static readonly RelationalTypeMapping Text = new NpgsqlStringTypeMapping(
        "text",
        NpgsqlDbType.Text
    );
}
