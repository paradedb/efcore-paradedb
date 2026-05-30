using Microsoft.EntityFrameworkCore.Query;

namespace ParadeDB.EntityFrameworkCore.Internal.Query;

internal sealed class ParadeDbMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
{
    public ParadeDbMethodCallTranslatorPlugin(ISqlExpressionFactory sqlExpressionFactory)
    {
        Translators = [new Translator(sqlExpressionFactory)];
    }

    public IEnumerable<IMethodCallTranslator> Translators { get; }
}
