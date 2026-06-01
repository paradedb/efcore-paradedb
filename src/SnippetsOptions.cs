namespace ParadeDB.EntityFrameworkCore;

public sealed record SnippetsOptions
{
    public string? StartTag { get; init; }
    public string? EndTag { get; init; }
    public int? MaxNumChars { get; init; }
    public int? Limit { get; init; }
    public int? Offset { get; init; }
    public string? SortBy { get; init; }
}
