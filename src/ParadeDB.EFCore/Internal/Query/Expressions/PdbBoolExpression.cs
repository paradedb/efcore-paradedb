using System.Collections.Frozen;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using ParadeDB.EFCore.Internal.Storage;

namespace ParadeDB.EFCore.Internal.Query.Expressions;

internal sealed class PdbBoolExpression : PgUnknownBinaryExpression
{
    private static readonly FrozenDictionary<PdbOperatorType, string> OperatorMap = new Dictionary<
        PdbOperatorType,
        string
    >
    {
        { PdbOperatorType.Disjunction, "|||" },
        { PdbOperatorType.Conjunction, "&&&" },
        { PdbOperatorType.Phrase, "###" },
        { PdbOperatorType.Term, "===" },
        { PdbOperatorType.Function, "@@@" },
    }.ToFrozenDictionary();

    public PdbBoolExpression(SqlExpression left, SqlExpression right, PdbOperatorType operatorType)
        : base(left, right, OperatorMap[operatorType], typeof(bool), PdbTypeMappings.Boolean) { }
}
