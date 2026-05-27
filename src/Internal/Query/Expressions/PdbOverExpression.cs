using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace ParadeDB.EntityFrameworkCore.Internal.Query.Expressions;

internal sealed class PdbOverExpression(SqlFunctionExpression function)
    : SqlFunctionExpression(
        function.Schema,
        function.Name,
        function.Arguments!,
        function.IsNullable,
        function.ArgumentsPropagateNullability!,
        function.Type,
        function.TypeMapping
    )
{
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var function = (SqlFunctionExpression)base.VisitChildren(visitor);
        return function == this ? this : new PdbOverExpression(function);
    }
}
