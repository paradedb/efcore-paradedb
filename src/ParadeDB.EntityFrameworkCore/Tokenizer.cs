using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace ParadeDB.EntityFrameworkCore;

public sealed class Tokenizer
{
    private readonly string _name;
    private readonly string[] _args;
    private readonly TokenFilter[] _filters;

    private static readonly FrozenDictionary<LinderaLanguage, string> LinderaLanguageArgs =
        new Dictionary<LinderaLanguage, string>
        {
            { LinderaLanguage.Chinese, "chinese" },
            { LinderaLanguage.Japanese, "japanese" },
            { LinderaLanguage.Korean, "korean" },
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<TokenChars, string> TokenCharsArgs = new Dictionary<
        TokenChars,
        string
    >
    {
        { TokenChars.Letter, "letter" },
        { TokenChars.Digit, "digit" },
        { TokenChars.Whitespace, "whitespace" },
        { TokenChars.Punctuation, "punctuation" },
        { TokenChars.Symbol, "symbol" },
    }.ToFrozenDictionary();

    private Tokenizer(string name, string[] args, params TokenFilter[] filters)
    {
        _name = name;
        _args = args;
        _filters = filters;
    }

    public override string ToString()
    {
        var args = _args.Concat(_filters.Select(f => f.ToString())).ToList();

        return args.Count == 0 ? $"pdb.{_name}" : $"pdb.{_name}({string.Join(", ", args)})";
    }

    public static Tokenizer Unicode(params TokenFilter[] filters) =>
        new("unicode_words", [], filters);

    public static Tokenizer Unicode(bool removeEmojis, params TokenFilter[] filters) =>
        removeEmojis
            ? new Tokenizer("unicode_words", ["'remove_emojis=true'"], filters)
            : Unicode(filters);

    public static Tokenizer Literal => new("literal", []);

    public static Tokenizer LiteralNormalized(params TokenFilter[] filters) =>
        new("literal_normalized", [], filters);

    public static Tokenizer Whitespace(params TokenFilter[] filters) =>
        new("whitespace", [], filters);

    public static Tokenizer Ngram(int minGram, int maxGram, params TokenFilter[] filters) =>
        new("ngram", [$"{minGram},{maxGram}"], filters);

    public static Tokenizer NgramPrefixOnly(
        int minGram,
        int maxGram,
        params TokenFilter[] filters
    ) => new("ngram", [$"{minGram}, {maxGram}, 'prefix_only=true'"], filters);

    public static Tokenizer NgramPositions(int gramSize, params TokenFilter[] filters) =>
        new("ngram", [$"{gramSize}, {gramSize}, 'positions=true'"], filters);

    public static Tokenizer EdgeNgram(int minGram, int maxGram, params TokenFilter[] filters) =>
        new("edge_ngram", [$"{minGram},{maxGram}"], filters);

    public static Tokenizer EdgeNgram(
        int minGram,
        int maxGram,
        TokenChars tokenChars,
        params TokenFilter[] filters
    ) =>
        new(
            "edge_ngram",
            [$"{minGram},{maxGram}", $"'token_chars={SerializeTokenChars(tokenChars)}'"],
            filters
        );

    public static Tokenizer Simple(params TokenFilter[] filters) => new("simple", [], filters);

    public static Tokenizer RegexPattern(
        [StringSyntax(StringSyntaxAttribute.Regex)] string pattern,
        params TokenFilter[] filters
    ) => new("regex_pattern", [$"'{pattern}'"], filters);

    public static Tokenizer ChineseCompatible(params TokenFilter[] filters) =>
        new("chinese_compatible", [], filters);

    public static Tokenizer Lindera(LinderaLanguage language, params TokenFilter[] filters) =>
        new("lindera", [LinderaLanguageArgs[language]], filters);

    public static Tokenizer Lindera(
        LinderaLanguage language,
        bool keepWhitespace,
        params TokenFilter[] filters
    ) =>
        keepWhitespace
            ? new Tokenizer(
                "lindera",
                [$"{LinderaLanguageArgs[language]}, 'keep_whitespace=true'"],
                filters
            )
            : Lindera(language, filters);

    public static Tokenizer Icu(params TokenFilter[] filters) => new("icu", [], filters);

    public static Tokenizer Jieba(params TokenFilter[] filters) => new("jieba", [], filters);

    public static Tokenizer SourceCode(params TokenFilter[] filters) =>
        new("source_code", [], filters);

    private static string SerializeTokenChars(TokenChars chars) =>
        string.Join(
            ",",
            Enum.GetValues<TokenChars>().Where(c => chars.HasFlag(c)).Select(c => TokenCharsArgs[c])
        );
}
