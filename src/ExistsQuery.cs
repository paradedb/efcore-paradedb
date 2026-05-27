using System.Diagnostics.CodeAnalysis;

namespace ParadeDB.EntityFrameworkCore;

[ExcludeFromCodeCoverage]
public abstract class ExistsQuery : PdbQuery
{
    private protected ExistsQuery() { }
}
