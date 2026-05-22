using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ParadeDB.EntityFrameworkCore.Extensions;

namespace ParadeDB.EntityFrameworkCore.Internal.Query.Translators;

internal sealed class ScoreTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public ScoreTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger
    )
    {
        if (method.Name != nameof(ParadeDbFunctionsExtensions.Score))
        {
            return null;
        }

        return _sqlExpressionFactory.Function(
            name: "score",
            schema: "pdb",
            nullable: false,
            arguments: [arguments[1]],
            argumentsPropagateNullability: [false],
            returnType: typeof(float)
        );
    }
}
