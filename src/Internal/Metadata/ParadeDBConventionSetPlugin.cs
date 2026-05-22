using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace ParadeDB.EntityFrameworkCore.Internal.Metadata;

internal sealed class ParadeDbConventionSetPlugin : IConventionSetPlugin
{
    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.ModelInitializedConventions.Add(new ParadeDbModelInitializedConvention());
        return conventionSet;
    }
}
