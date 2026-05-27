using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Internal.Query.Expressions;

namespace ParadeDB.EntityFrameworkCore.Internal.Query.Translators;

internal sealed class AggregateTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _jsonbTypeMapping;

    public AggregateTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource
    )
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _jsonbTypeMapping = typeMappingSource.FindMapping(typeof(JsonElement), "jsonb")!;
    }

    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger
    )
    {
        return method.Name switch
        {
            nameof(ParadeDbFunctionsExtensions.Agg) => BuildExpression(arguments, false),
            nameof(ParadeDbFunctionsExtensions.AggOver) => BuildExpression(arguments, true),
            _ => null,
        };
    }

    private SqlExpression? BuildExpression(IReadOnlyList<SqlExpression> arguments, bool over)
    {
        if (arguments[1] is not SqlConstantExpression { Value: { } aggregate })
        {
            return null;
        }

        List<SqlExpression> args =
        [
            _sqlExpressionFactory.Constant(JsonSerializer.Serialize(aggregate)),
            arguments[2],
        ];

        var function = _sqlExpressionFactory.Function(
            name: "agg",
            schema: "pdb",
            nullable: true,
            arguments: args,
            argumentsPropagateNullability: [false, false],
            returnType: typeof(JsonElement),
            typeMapping: _jsonbTypeMapping
        );

        return over ? new PdbOverExpression((SqlFunctionExpression)function) : function;
    }
}
