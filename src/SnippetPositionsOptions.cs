namespace ParadeDB.EntityFrameworkCore;

public sealed record SnippetPositionsOptions
{
    public int? Limit { get; init; }
    public int? Offset { get; init; }
}
