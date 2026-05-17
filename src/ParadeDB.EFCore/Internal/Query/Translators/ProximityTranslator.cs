using System.Collections;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using ParadeDB.EFCore.Extensions;
using ParadeDB.EFCore.Internal.Query.Expressions;
using ParadeDB.EFCore.Internal.Storage;

namespace ParadeDB.EFCore.Internal.Query.Translators;

internal sealed class ProximityTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public ProximityTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        var declaring = method.DeclaringType;

        if (
            declaring == typeof(ParadeDbFunctionsExtensions)
            && method.Name == nameof(ParadeDbFunctionsExtensions.Match)
        )
        {
            var column = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]);
            var query = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]);
            return new PdbBoolExpression(column, query, PdbOperatorType.Function);
        }

        if (declaring == typeof(Pdb))
        {
            return method.Name switch
            {
                nameof(Pdb.Proximity) => _sqlExpressionFactory.ApplyDefaultTypeMapping(
                    arguments[0]
                ),
                nameof(Pdb.ProximityRegex) => BuildPdbFunction("prox_regex", arguments),
                nameof(Pdb.ProximityArray) => BuildPdbFunction("prox_array", arguments),
                _ => null,
            };
        }

        if (declaring == typeof(PdbProximityQueryExtensions))
        {
            return method.Name switch
            {
                nameof(PdbProximityQueryExtensions.Within) => BuildProximity(arguments, false),
                nameof(PdbProximityQueryExtensions.WithinOrdered) => BuildProximity(
                    arguments,
                    true
                ),
                _ => null,
            };
        }

        return null;
    }

    private PdbProximityExpression BuildProximity(
        IReadOnlyList<SqlExpression> arguments,
        bool ordered
    )
    {
        var left = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]);
        var distance = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]);
        var right = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]);

        var withDistance = new PdbProximityExpression(left, distance, ordered);
        return new PdbProximityExpression(withDistance, right, ordered);
    }

    private SqlExpression BuildPdbFunction(string name, IReadOnlyList<SqlExpression> arguments)
    {
        var flattened = Flatten(arguments);
        var mapped = flattened
            .Select(a => _sqlExpressionFactory.ApplyDefaultTypeMapping(a))
            .ToArray();

        return _sqlExpressionFactory.Function(
            schema: "pdb",
            name: name,
            arguments: mapped,
            nullable: true,
            argumentsPropagateNullability: new bool[mapped.Length],
            returnType: typeof(PdbQuery),
            typeMapping: PdbTypeMappings.Text
        );
    }

    private IReadOnlyList<SqlExpression> Flatten(IReadOnlyList<SqlExpression> arguments)
    {
        if (arguments.Count != 1)
        {
            return arguments;
        }

        var arg = arguments[0];

        if (arg is PgNewArrayExpression pgArray)
        {
            return pgArray.Expressions;
        }

        if (arg is SqlConstantExpression { Value: IEnumerable enumerable and not string })
        {
            var result = new List<SqlExpression>();

            foreach (var item in enumerable)
            {
                result.Add(_sqlExpressionFactory.Constant(item));
            }

            return result;
        }

        return arguments;
    }
}
