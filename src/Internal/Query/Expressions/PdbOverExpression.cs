using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace ParadeDB.EntityFrameworkCore.Internal.Query.Expressions;

internal sealed class PdbOverExpression(SqlFunctionExpression function)
    : SqlFunctionExpression(
        function.Schema,
        function.Name,
        function.Arguments ?? [],
        function.IsNullable,
        function.ArgumentsPropagateNullability ?? [],
        function.Type,
        function.TypeMapping
    )
{
    public SqlExpression? Filter { get; } =
        function is PdbFilteredAggregateExpression filtered ? filtered.Filter : null;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var function = (SqlFunctionExpression)base.VisitChildren(visitor);
        var filter = Filter is null ? null : (SqlExpression)visitor.Visit(Filter);
        return ReferenceEquals(function, this) && ReferenceEquals(filter, Filter)
            ? this
            : new PdbOverExpression(
                filter is null ? function : new PdbFilteredAggregateExpression(function, filter)
            );
    }
}
