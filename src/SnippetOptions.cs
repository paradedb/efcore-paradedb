namespace ParadeDB.EntityFrameworkCore;

public sealed record SnippetOptions
{
    public string? StartTag { get; init; }
    public string? EndTag { get; init; }
    public int? MaxNumChars { get; init; }
    public int? Limit { get; init; }
    public int? Offset { get; init; }
}
