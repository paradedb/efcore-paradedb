using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace ParadeDB.EntityFrameworkCore.Internal.Query.Expressions;

internal sealed class PdbAccumulatorExpression : SqlExpression
{
    private static readonly ConstructorInfo QuotingConstructor = typeof(PdbAccumulatorExpression)
        .GetConstructors()
        .Single();

    public PdbAccumulatorExpression(
        IReadOnlyList<SqlExpression> arguments,
        IReadOnlyList<string?> argumentNames
    )
        : base(typeof(bool), null)
    {
        Arguments = arguments;
        ArgumentNames = argumentNames;
    }

    public IReadOnlyList<SqlExpression> Arguments { get; }
    public IReadOnlyList<string?> ArgumentNames { get; }

    protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

#if NET9_0_OR_GREATER
    public override Expression Quote() =>
        New(
            QuotingConstructor,
            NewArrayInit(typeof(SqlExpression), Arguments.Select(a => a.Quote())),
            NewArrayInit(
                typeof(string),
                ArgumentNames.Select(n =>
                    n is null ? Constant(null, typeof(string)) : Constant(n, typeof(string))
                )
            )
        );
#endif

    private PdbAccumulatorExpression With(SqlExpression argument, string? name)
    {
        var arguments = new List<SqlExpression>(Arguments) { argument };
        var argumentNames = new List<string?>(ArgumentNames) { name };
        return new PdbAccumulatorExpression(arguments, argumentNames);
    }

    public PdbAccumulatorExpression AppendPositional(SqlExpression argument) =>
        With(argument, null);

    public PdbAccumulatorExpression AppendNamed(string name, SqlExpression argument) =>
        With(argument, name);

    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("PdbAccumulator(");
        expressionPrinter.Visit(Arguments[0]);
        expressionPrinter.Append(")");
    }

    public override bool Equals(object? obj) =>
        obj is PdbAccumulatorExpression other
        && Arguments.SequenceEqual(other.Arguments)
        && ArgumentNames.SequenceEqual(other.ArgumentNames)
        && base.Equals(other);

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Arguments[0]);
}
