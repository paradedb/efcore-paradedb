using Microsoft.EntityFrameworkCore.Query;
using ParadeDB.EntityFrameworkCore.Internal.Query.Translators;

namespace ParadeDB.EntityFrameworkCore.Internal.Query;

internal sealed class ParadeDbMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
{
    public ParadeDbMethodCallTranslatorPlugin(ISqlExpressionFactory sqlExpressionFactory)
    {
        Translators =
        [
            new ModifierTranslator(sqlExpressionFactory),
            new OperatorTranslator(sqlExpressionFactory),
            new ScoreTranslator(sqlExpressionFactory),
            new SnippetTranslator(sqlExpressionFactory),
            new ProximityTranslator(sqlExpressionFactory),
            new TokenizeTranslator(sqlExpressionFactory),
            new MoreLikeThisTranslator(sqlExpressionFactory),
            new AliasTranslator(sqlExpressionFactory),
            new AggregateTranslator(sqlExpressionFactory),
        ];
    }

    public IEnumerable<IMethodCallTranslator> Translators { get; }
}
