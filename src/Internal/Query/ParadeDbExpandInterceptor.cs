using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

namespace ParadeDB.EntityFrameworkCore.Internal.Query;

// A method call translator only gets scalar arguments, so it never sees the rules source of
// Pdb.Expand. The rewrite turns the call into a scalar subquery over that source.
internal sealed class ParadeDbExpandInterceptor : ExpressionVisitor, IQueryExpressionInterceptor
{
    public Expression QueryCompilationStarting(
        Expression queryExpression,
        QueryExpressionEventData eventData
    ) => Visit(queryExpression);

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType != typeof(Pdb) || node.Method.Name != nameof(Pdb.Expand))
        {
            return base.VisitMethodCall(node);
        }

        var text = Visit(node.Arguments[0]);
        var rule = Expression.Parameter(typeof(string), "rule");
        var expandRule = Expression.Call(typeof(Pdb), nameof(Pdb.ExpandRule), null, text, rule);
        var select = Expression.Call(
            QueryableMethods.Select.MakeGenericMethod(typeof(string), typeof(string)),
            Visit(node.Arguments[1]),
            Expression.Quote(Expression.Lambda<Func<string, string>>(expandRule, rule))
        );

        // pdb.expand is an aggregate, so the subquery returns one row. SingleOrDefault adds the
        // LIMIT 1 of a scalar subquery without the warning that FirstOrDefault logs.
        return Expression.Call(
            QueryableMethods.SingleOrDefaultWithoutPredicate.MakeGenericMethod(typeof(string)),
            select
        );
    }
}
