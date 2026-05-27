using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace ParadeDB.EntityFrameworkCore.Internal.Query.Expressions;

internal sealed class PdbFilteredAggregateExpression(
    SqlFunctionExpression function,
    SqlExpression filter
)
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
    public SqlExpression Filter { get; } = filter;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var function = (SqlFunctionExpression)base.VisitChildren(visitor);
        var filter = (SqlExpression)visitor.Visit(Filter)!;
        return function == this && filter == Filter
            ? this
            : new PdbFilteredAggregateExpression(function, filter);
    }
}
