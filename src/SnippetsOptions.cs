namespace ParadeDB.EntityFrameworkCore;

public record SnippetsOptions(
    string? startTag,
    string? endTag,
    int? maxNumChars,
    int? limit,
    int? offset,
    string? sortBy
);
