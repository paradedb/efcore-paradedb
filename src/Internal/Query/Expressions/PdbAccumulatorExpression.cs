using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace ParadeDB.EntityFrameworkCore.Internal.Query.Expressions;

internal sealed class PdbAccumulatorExpression : SqlExpression
{
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

    private const string NotSupportedMessage =
        $"{nameof(PdbAccumulatorExpression)} is an intermediate carrier consumed during translation and must never appear in a final SQL expression tree.";

    protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

#if NET9_0_OR_GREATER
    public override Expression Quote() => throw new NotSupportedException(NotSupportedMessage);
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

    protected override void Print(ExpressionPrinter expressionPrinter) =>
        throw new NotSupportedException(NotSupportedMessage);

    public override bool Equals(object? obj) =>
        throw new NotSupportedException(NotSupportedMessage);

    public override int GetHashCode() => throw new NotSupportedException(NotSupportedMessage);
}
