namespace ParadeDB.EntityFrameworkCore;

public sealed record MoreLikeThisOptions
{
    public string[]? Fields { get; init; }
    public int? MinTermFrequency { get; init; }
    public int? MinDocFrequency { get; init; }
    public int? MaxDocFrequency { get; init; }
    public int? MaxQueryTerms { get; init; }
    public int? MinWordLength { get; init; }
    public int? MaxWordLength { get; init; }
    public string[]? Stopwords { get; init; }
}
