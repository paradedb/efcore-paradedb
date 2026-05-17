using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Internal.Storage;

namespace ParadeDB.EntityFrameworkCore.Internal.Query.Translators;

internal sealed class AliasTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public AliasTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (method.Name != nameof(ParadeDbFunctionsExtensions.Alias))
        {
            return null;
        }

        if (arguments[2] is not SqlConstantExpression { Value: string alias })
        {
            return null;
        }

        var typeMapping = new AliasTypeMapping(alias);

        return _sqlExpressionFactory.MakeUnary(
            ExpressionType.Convert,
            arguments[1],
            typeMapping.ClrType,
            typeMapping
        );
    }
}
