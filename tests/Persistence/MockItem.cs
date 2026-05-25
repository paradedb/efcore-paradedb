using System.Text.Json;
using NpgsqlTypes;

namespace ParadeDB.EntityFrameworkCore.Tests.Persistence;

public sealed class MockItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool InStock { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateOnly LastUpdatedDate { get; set; }
    public TimeOnly LatestAvailableTime { get; set; }
    public NpgsqlRange<int>? WeightRange { get; set; }
}
