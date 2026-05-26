using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ParadeDB.EntityFrameworkCore.Extensions;

namespace ParadeDB.EntityFrameworkCore.Internal.Query.Translators;

internal sealed class AggregateTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public AggregateTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (method.Name != nameof(ParadeDbFunctionsExtensions.Agg))
        {
            return null;
        }

        if (arguments[1] is not SqlConstantExpression { Value: { } aggregate })
        {
            return null;
        }
        List<SqlExpression> args =
        [
            _sqlExpressionFactory.Constant(JsonSerializer.Serialize(aggregate)),
            arguments[2],
        ];

        return _sqlExpressionFactory.Function(
            name: "agg",
            schema: "pdb",
            nullable: true,
            arguments: args,
            argumentsPropagateNullability: [false, false],
            returnType: typeof(JsonElement)
        );
    }
}
