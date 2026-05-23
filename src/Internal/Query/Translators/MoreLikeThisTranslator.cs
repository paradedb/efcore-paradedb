using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Internal.Query.Expressions;
using ParadeDB.EntityFrameworkCore.Internal.Storage;

namespace ParadeDB.EntityFrameworkCore.Internal.Query.Translators;

internal sealed class MoreLikeThisTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public MoreLikeThisTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        var declaring = method.DeclaringType;

        if (declaring == typeof(Pdb) && method.Name == nameof(Pdb.Mlt))
        {
            var seed = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]);
            return new PdbAccumulatorExpression([seed], [null]);
        }

        if (declaring == typeof(PdbMoreLikeThisQueryExtensions))
        {
            if (arguments[0] is not PdbAccumulatorExpression carrier)
            {
                return null;
            }

            var value = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]);

            return method.Name switch
            {
                nameof(PdbMoreLikeThisQueryExtensions.Fields) => carrier.AppendPositional(value),
                nameof(PdbMoreLikeThisQueryExtensions.MinTermFrequency) => carrier.AppendNamed(
                    "min_term_frequency",
                    value
                ),
                nameof(PdbMoreLikeThisQueryExtensions.MaxTermFrequency) => carrier.AppendNamed(
                    "max_term_frequency",
                    value
                ),
                nameof(PdbMoreLikeThisQueryExtensions.MinDocFrequency) => carrier.AppendNamed(
                    "min_doc_frequency",
                    value
                ),
                nameof(PdbMoreLikeThisQueryExtensions.MaxDocFrequency) => carrier.AppendNamed(
                    "max_doc_frequency",
                    value
                ),
                nameof(PdbMoreLikeThisQueryExtensions.MaxQueryTerms) => carrier.AppendNamed(
                    "max_query_terms",
                    value
                ),
                nameof(PdbMoreLikeThisQueryExtensions.MinWordLength) => carrier.AppendNamed(
                    "min_word_length",
                    value
                ),
                nameof(PdbMoreLikeThisQueryExtensions.MaxWordLength) => carrier.AppendNamed(
                    "max_word_length",
                    value
                ),
                nameof(PdbMoreLikeThisQueryExtensions.Stopwords) => carrier.AppendNamed(
                    "stopwords",
                    value
                ),
                _ => null,
            };
        }

        if (
            declaring == typeof(ParadeDbFunctionsExtensions)
            && method.Name == nameof(ParadeDbFunctionsExtensions.MoreLikeThis)
            && arguments[2] is PdbAccumulatorExpression matchCarrier
        )
        {
            var column = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]);

            var function = PgFunctionExpression.CreateWithNamedArguments(
                name: "pdb.more_like_this",
                arguments: matchCarrier.Arguments,
                argumentNames: matchCarrier.ArgumentNames,
                nullable: true,
                argumentsPropagateNullability: new bool[matchCarrier.Arguments.Count],
                builtIn: false,
                type: typeof(PdbMoreLikeThisQuery),
                typeMapping: PdbTypeMappings.Text
            );

            return new PdbBoolExpression(column, function, PdbOperatorType.Function);
        }

        return null;
    }
}
