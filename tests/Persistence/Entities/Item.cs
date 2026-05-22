namespace ParadeDB.EntityFrameworkCore.Tests.Persistence.Entities;

public sealed class Item
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
