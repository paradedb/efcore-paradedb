using Microsoft.EntityFrameworkCore.Query;

namespace ParadeDB.EntityFrameworkCore.Internal.Query;

internal sealed class ParadeDbMemberTranslatorPlugin : IMemberTranslatorPlugin
{
    public IEnumerable<IMemberTranslator> Translators { get; } = [];
}
