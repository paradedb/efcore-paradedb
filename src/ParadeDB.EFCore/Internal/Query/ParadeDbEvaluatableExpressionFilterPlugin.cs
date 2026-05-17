using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace ParadeDB.EFCore.Internal.Query;

internal sealed class ParadeDbEvaluatableExpressionFilterPlugin : IEvaluatableExpressionFilterPlugin
{
    public bool IsEvaluatableExpression(Expression expression)
    {
        if (
            expression is MethodCallExpression call
            && typeof(PdbQuery).IsAssignableFrom(call.Method.ReturnType)
        )
        {
            return false;
        }

        return true;
    }
}
