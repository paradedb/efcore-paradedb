using System.Text.Json;

namespace Shared;

public class MockItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Rating { get; set; }
    public bool InStock { get; set; }
    public DateTime CreatedAt { get; set; }
    public JsonDocument? Metadata { get; set; }
}
