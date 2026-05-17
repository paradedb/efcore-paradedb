using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using ParadeDB.EFCore.Internal.Storage;

namespace ParadeDB.EFCore.Internal.Query.Expressions;

internal sealed class PdbProximityExpression : PgUnknownBinaryExpression
{
    public PdbProximityExpression(SqlExpression left, SqlExpression right, bool ordered = false)
        : base(left, right, ordered ? "##>" : "##", typeof(bool), PdbTypeMappings.Boolean) { }
}
