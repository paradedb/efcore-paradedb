using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using ParadeDB.EFCore.Internal.Metadata;
using ParadeDB.EFCore.Internal.Query;

namespace ParadeDB.EFCore.Internal;

internal sealed class ParadeDbOptionsExtension : IDbContextOptionsExtension
{
    public ParadeDbOptionsExtension()
    {
        Info = new ParadeDbOptionsExtensionInfo(this);
    }

    public void ApplyServices(IServiceCollection services)
    {
        services.AddScoped<IMethodCallTranslatorPlugin, ParadeDbMethodCallTranslatorPlugin>();
        services.AddScoped<IMemberTranslatorPlugin, ParadeDbMemberTranslatorPlugin>();
        services.AddSingleton<IConventionSetPlugin, ParadeDbConventionSetPlugin>();
        services.AddSingleton<
            IEvaluatableExpressionFilterPlugin,
            ParadeDbEvaluatableExpressionFilterPlugin
        >();
    }

    public void Validate(IDbContextOptions options) { }

    public DbContextOptionsExtensionInfo Info { get; }

    private sealed class ParadeDbOptionsExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ParadeDbOptionsExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension) { }

        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) =>
            true;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["ParadeDB"] = "Enabled";
        }

        public override bool IsDatabaseProvider => false;
        public override string LogFragment => "using ParadeDB ";
    }
}
