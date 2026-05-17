using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace ParadeDB.EFCore.Internal.Storage;

internal static class PdbTypeMappings
{
    public static readonly RelationalTypeMapping Boolean = new BoolTypeMapping("boolean");
    public static readonly RelationalTypeMapping Text = new NpgsqlStringTypeMapping(
        "text",
        NpgsqlDbType.Text
    );
}
