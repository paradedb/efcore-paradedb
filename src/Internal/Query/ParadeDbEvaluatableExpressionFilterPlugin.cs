using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using ParadeDB.EntityFrameworkCore.Extensions;

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
                || (
                    call.Method.DeclaringType == typeof(ParadeDbFunctionsExtensions)
                    && call.Method.Name
                        is nameof(ParadeDbFunctionsExtensions.Tokenize)
                            or nameof(ParadeDbFunctionsExtensions.TokenizeAsArray)
                            or nameof(ParadeDbFunctionsExtensions.Agg)
                            or nameof(ParadeDbFunctionsExtensions.AggOver)
                )
            )
        )
        {
            return false;
        }

        return true;
    }
}
