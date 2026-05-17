using System.Diagnostics.CodeAnalysis;

namespace ParadeDB.EntityFrameworkCore.Tests.TestModels;

[ExcludeFromCodeCoverage]
public sealed class Product
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
