namespace ParadeDB.EntityFrameworkCore;

public sealed record SnippetOptions
{
    public string? StartTag { get; init; }
    public string? EndTag { get; init; }
    public int? MaxNumChars { get; init; }
}
