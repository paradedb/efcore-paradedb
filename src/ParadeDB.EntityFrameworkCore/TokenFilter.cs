using System.Collections.Frozen;

namespace ParadeDB.EntityFrameworkCore;

public sealed class TokenFilter
{
    private readonly string _value;

    private static readonly FrozenDictionary<StopwordsLanguage, string> LanguageArgs =
        new Dictionary<StopwordsLanguage, string>
        {
            { StopwordsLanguage.Czech, "czech" },
            { StopwordsLanguage.Danish, "danish" },
            { StopwordsLanguage.Dutch, "dutch" },
            { StopwordsLanguage.English, "english" },
            { StopwordsLanguage.Finnish, "finnish" },
            { StopwordsLanguage.French, "french" },
            { StopwordsLanguage.German, "german" },
            { StopwordsLanguage.Hungarian, "hungarian" },
            { StopwordsLanguage.Italian, "italian" },
            { StopwordsLanguage.Norwegian, "norwegian" },
            { StopwordsLanguage.Polish, "polish" },
            { StopwordsLanguage.Portuguese, "portuguese" },
            { StopwordsLanguage.Russian, "russian" },
            { StopwordsLanguage.Spanish, "spanish" },
            { StopwordsLanguage.Swedish, "swedish" },
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<StemmerLanguage, string> StemmerLanguageArgs =
        new Dictionary<StemmerLanguage, string>
        {
            { StemmerLanguage.Arabic, "arabic" },
            { StemmerLanguage.Czech, "czech" },
            { StemmerLanguage.Danish, "danish" },
            { StemmerLanguage.Dutch, "dutch" },
            { StemmerLanguage.English, "english" },
            { StemmerLanguage.Finnish, "finnish" },
            { StemmerLanguage.French, "french" },
            { StemmerLanguage.German, "german" },
            { StemmerLanguage.Greek, "greek" },
            { StemmerLanguage.Hungarian, "hungarian" },
            { StemmerLanguage.Italian, "italian" },
            { StemmerLanguage.Norwegian, "norwegian" },
            { StemmerLanguage.Polish, "polish" },
            { StemmerLanguage.Portuguese, "portuguese" },
            { StemmerLanguage.Romanian, "romanian" },
            { StemmerLanguage.Russian, "russian" },
            { StemmerLanguage.Spanish, "spanish" },
            { StemmerLanguage.Swedish, "swedish" },
            { StemmerLanguage.Tamil, "tamil" },
            { StemmerLanguage.Turkish, "turkish" },
        }.ToFrozenDictionary();

    private TokenFilter(string value) => _value = value;

    public override string ToString() => $"'{_value}'";

    public static readonly TokenFilter AlphaNumericOnly = new("alpha_num_only=true");
    public static readonly TokenFilter AsciiFolding = new("ascii_folding=true");
    public static readonly TokenFilter PreserveCase = new("lowercase=false");

    public static TokenFilter RemoveStopwords(params StopwordsLanguage[] languages)
    {
        var joined = string.Join(",", languages.Select(l => LanguageArgs[l]));
        return new TokenFilter($"stopwords_language={joined}");
    }

    public static TokenFilter Stemmer(StemmerLanguage language) =>
        new($"stemmer={StemmerLanguageArgs[language]}");

    public static TokenFilter RemoveLong(int maxBytes) => new($"remove_long={maxBytes}");

    public static TokenFilter RemoveShort(int minBytes) => new($"remove_short={minBytes}");

    public static TokenFilter Trim => new("trim=true");
}
