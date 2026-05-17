using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace ParadeDB.EntityFrameworkCore.Internal.Metadata;

internal sealed class ParadeDbModelInitializedConvention : IModelInitializedConvention
{
    public void ProcessModelInitialized(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context
    )
    {
        modelBuilder.HasPostgresExtension("pg_search");
    }
}
