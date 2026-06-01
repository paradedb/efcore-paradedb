namespace Autocomplete.Models;

public class AutocompleteItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Rating { get; set; }
    public bool InStock { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
