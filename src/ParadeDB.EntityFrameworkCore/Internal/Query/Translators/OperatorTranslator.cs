using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Internal.Query.Expressions;

namespace ParadeDB.EntityFrameworkCore.Internal.Query.Translators;

internal sealed class OperatorTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public OperatorTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        PdbOperatorType? operatorType = method.Name switch
        {
            nameof(ParadeDbFunctionsExtensions.MatchDisjunction) => PdbOperatorType.Disjunction,
            nameof(ParadeDbFunctionsExtensions.MatchConjunction) => PdbOperatorType.Conjunction,
            nameof(ParadeDbFunctionsExtensions.Phrase) => PdbOperatorType.Phrase,
            nameof(ParadeDbFunctionsExtensions.Term) => PdbOperatorType.Term,
            _ => null,
        };

        if (operatorType is null)
        {
            return null;
        }

        var left = arguments[1];
        var right = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]);

        return new PdbBoolExpression(left, right, operatorType.Value);
    }
}
