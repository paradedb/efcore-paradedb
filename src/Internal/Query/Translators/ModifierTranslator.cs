using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using ParadeDB.EntityFrameworkCore.Internal.Storage;
using ParadeDB.EntityFrameworkCore.Modifiers;

namespace ParadeDB.EntityFrameworkCore.Internal.Query.Translators;

internal sealed class ModifierTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public ModifierTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (method.DeclaringType != typeof(Pdb) || arguments.Count < 2)
        {
            return null;
        }

        var typeMapping = GetTypeMapping(method.Name, arguments);

        if (typeMapping is null)
        {
            return null;
        }

        return _sqlExpressionFactory.MakeUnary(
            ExpressionType.Convert,
            _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]),
            typeMapping.ClrType,
            typeMapping
        );
    }

    private static RelationalTypeMapping? GetTypeMapping(
        string methodName,
        IReadOnlyList<SqlExpression> arguments
    ) =>
        methodName switch
        {
            nameof(Pdb.Boost) when arguments[1] is SqlConstantExpression { Value: float factor } =>
                new PdbModifierTypeMapping<Boost>(new Boost(factor)),
            nameof(Pdb.Const) when arguments[1] is SqlConstantExpression { Value: float score } =>
                new PdbModifierTypeMapping<Const>(new Const(score)),
            nameof(Pdb.Fuzzy) when arguments[1] is SqlConstantExpression { Value: int distance } =>
                new PdbModifierTypeMapping<Fuzzy>(
                    new Fuzzy(
                        distance,
                        arguments.Count > 2
                            && arguments[2] is SqlConstantExpression { Value: true },
                        arguments.Count > 3 && arguments[3] is SqlConstantExpression { Value: true }
                    )
                ),
            nameof(Pdb.Slop) when arguments[1] is SqlConstantExpression { Value: int slop } =>
                new PdbModifierTypeMapping<Slop>(new Slop(slop)),
            _ => null,
        };
}
