using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ParadeDB.EntityFrameworkCore.Extensions;

namespace ParadeDB.EntityFrameworkCore.Internal.Query.Translators;

internal sealed class SnippetTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public SnippetTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (method.Name != nameof(ParadeDbFunctionsExtensions.Snippet))
        {
            return null;
        }

        List<SqlExpression> args = [arguments[1]];
        List<bool> argsNullability = [false];

        if (arguments.Count == 3)
        {
            args.AddRange([
                _sqlExpressionFactory.Constant("<b>"),
                _sqlExpressionFactory.Constant("</b>"),
                arguments[2],
            ]);

            argsNullability.AddRange([false, false, false]);
        }
        else if (arguments.Count == 4)
        {
            args.AddRange([arguments[2], arguments[3]]);
            argsNullability.AddRange([false, false]);
        }
        else if (arguments.Count == 5)
        {
            args.AddRange([arguments[2], arguments[3], arguments[4]]);
            argsNullability.AddRange([false, false, false]);
        }

        return _sqlExpressionFactory.Function(
            name: "snippet",
            schema: "pdb",
            nullable: true,
            arguments: args,
            argumentsPropagateNullability: argsNullability,
            returnType: typeof(string)
        );
    }
}
