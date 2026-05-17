using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using ParadeDB.EFCore.Extensions;
using ParadeDB.EFCore.Internal.Query.Expressions;
using ParadeDB.EFCore.Internal.Storage;
using ParadeDB.EFCore.Modifiers;

namespace ParadeDB.EFCore.Internal.Query.Translators;

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

        for (int i = 3; i < arguments.Count; i++)
        {
            right = ApplyModifier(right, arguments[i]) ?? right;
        }

        return new PdbBoolExpression(left, right, operatorType.Value);
    }

    private SqlExpression? ApplyModifier(SqlExpression expression, SqlExpression modifierExpression)
    {
        if (modifierExpression is not SqlConstantExpression { Value: var value })
        {
            return expression;
        }

        RelationalTypeMapping? typeMapping = value switch
        {
            Fuzzy fuzzy => new PdbModifierTypeMapping<Fuzzy>(fuzzy),
            Boost boost => new PdbModifierTypeMapping<Boost>(boost),
            Slop slop => new PdbModifierTypeMapping<Slop>(slop),
            Const @const => new PdbModifierTypeMapping<Const>(@const),
            _ => null,
        };

        if (typeMapping is null)
        {
            return expression;
        }

        return _sqlExpressionFactory.MakeUnary(
            ExpressionType.Convert,
            expression,
            typeMapping.ClrType,
            typeMapping
        );
    }
}
