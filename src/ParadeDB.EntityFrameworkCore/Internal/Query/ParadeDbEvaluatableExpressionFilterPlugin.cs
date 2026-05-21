using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace ParadeDB.EntityFrameworkCore.Internal.Query;

internal sealed class ParadeDbEvaluatableExpressionFilterPlugin : IEvaluatableExpressionFilterPlugin
{
    public bool IsEvaluatableExpression(Expression expression)
    {
        if (
            expression is MethodCallExpression call
            && (
                typeof(PdbQuery).IsAssignableFrom(call.Method.ReturnType)
                || (
                    call.Method.DeclaringType == typeof(Pdb)
                    && call.Method.Name
                        is nameof(Pdb.Boost)
                            or nameof(Pdb.Const)
                            or nameof(Pdb.Fuzzy)
                            or nameof(Pdb.Slop)
                    && call.Arguments.Count > 1
                )
            )
        )
        {
            return false;
        }

        return true;
    }
}
