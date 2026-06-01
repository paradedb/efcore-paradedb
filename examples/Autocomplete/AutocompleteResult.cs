namespace Autocomplete;

public class AutocompleteResult
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Rating { get; set; }
    public bool InStock { get; set; }
    public double SearchScore { get; set; }
}
