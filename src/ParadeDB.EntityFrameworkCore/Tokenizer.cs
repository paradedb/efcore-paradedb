using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ParadeDB.EntityFrameworkCore;

public sealed class Tokenizer
{
    private readonly string _name;
    private readonly string[] _args;
    private readonly IReadOnlyDictionary<string, object> _options;

    private static readonly FrozenDictionary<LinderaLanguage, string> LinderaLanguageArgs =
        new Dictionary<LinderaLanguage, string>
        {
            { LinderaLanguage.Chinese, "chinese" },
            { LinderaLanguage.Japanese, "japanese" },
            { LinderaLanguage.Korean, "korean" },
        }.ToFrozenDictionary();

    private Tokenizer(string name, string[] args, Dictionary<string, object> options)
    {
        _name = name;
        _args = args;
        _options = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(options));
    }

    public override string ToString()
    {
        var args = _args
            .Concat(
                _options.Select(option =>
                    $"'{Quote(option.Key)}={FormatOptionValue(option.Value, option.Key)}'"
                )
            )
            .ToList();

        return args.Count == 0 ? $"pdb.{_name}" : $"pdb.{_name}({string.Join(",", args)})";
    }

    private static string FormatOptionValue(object value, string key) =>
        value switch
        {
            bool b => b.ToString().ToLowerInvariant(),
            string s => Quote(s),
            int n => n.ToString(CultureInfo.InvariantCulture),
            float n => n.ToString(CultureInfo.InvariantCulture),
            _ => throw new ArgumentException(
                $"Tokenizer option '{key}' has unsupported value type '{value?.GetType().Name}'. Each tokenizer option value must be a bool, string, int, or float.",
                "options"
            ),
        };

    private static string Quote(string value) => value.Replace("'", "''");

    public static Tokenizer Unicode(Dictionary<string, object> options) =>
        new("unicode_words", [], options);

    public static Tokenizer Literal(Dictionary<string, object> options) =>
        new("literal", [], options);

    public static Tokenizer LiteralNormalized(Dictionary<string, object> options) =>
        new("literal_normalized", [], options);

    public static Tokenizer Whitespace(Dictionary<string, object> options) =>
        new("whitespace", [], options);

    public static Tokenizer Ngram(int minGram, int maxGram, Dictionary<string, object> options) =>
        new("ngram", [$"{minGram},{maxGram}"], options);

    public static Tokenizer EdgeNgram(
        int minGram,
        int maxGram,
        Dictionary<string, object> options
    ) => new("edge_ngram", [$"{minGram},{maxGram}"], options);

    public static Tokenizer Simple(Dictionary<string, object> options) =>
        new("simple", [], options);

    public static Tokenizer RegexPattern(
        [StringSyntax(StringSyntaxAttribute.Regex)] string pattern,
        Dictionary<string, object> options
    ) => new("regex_pattern", [$"'{Quote(pattern)}'"], options);

    public static Tokenizer ChineseCompatible(Dictionary<string, object> options) =>
        new("chinese_compatible", [], options);

    public static Tokenizer Lindera(LinderaLanguage language, Dictionary<string, object> options) =>
        new("lindera", [$"'{LinderaLanguageArgs[language]}'"], options);

    public static Tokenizer Icu(Dictionary<string, object> options) => new("icu", [], options);

    public static Tokenizer Jieba(Dictionary<string, object> options) => new("jieba", [], options);

    public static Tokenizer SourceCode(Dictionary<string, object> options) =>
        new("source_code", [], options);
}
