namespace ParadeDB.EntityFrameworkCore;

[Flags]
public enum TokenChars
{
    Letter = 1 << 0,
    Digit = 1 << 1,
    Whitespace = 1 << 2,
    Punctuation = 1 << 3,
    Symbol = 1 << 4,
}
