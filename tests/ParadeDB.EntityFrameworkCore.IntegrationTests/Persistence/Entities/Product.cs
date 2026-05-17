namespace ParadeDB.EntityFrameworkCore.IntegrationTests.Persistence.Entities;

public sealed class Product
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
