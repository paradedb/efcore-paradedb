using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.IntegrationTests.Query;

public sealed class TokenizeTests : TestBase
{
    private static readonly TokenFilter[] AllFilters =
    [
        TokenFilter.AlphaNumericOnly,
        TokenFilter.AsciiFolding,
        TokenFilter.PreserveCase,
        TokenFilter.RemoveStopwords(StopwordsLanguage.English),
        TokenFilter.Stemmer(StemmerLanguage.English),
        TokenFilter.RemoveLong(100),
        TokenFilter.RemoveShort(2),
        TokenFilter.Trim,
    ];

    private static IEnumerable<Func<TokenFilter[], Tokenizer>> TokenizerBuilders()
    {
        yield return filters => Tokenizer.Unicode(filters);
        yield return filters => Tokenizer.Unicode(false, filters);
        yield return filters => Tokenizer.Unicode(true, filters);
        yield return filters => Tokenizer.LiteralNormalized(filters);
        yield return filters => Tokenizer.Whitespace(filters);
        yield return filters => Tokenizer.Ngram(2, 5, filters);
        yield return filters => Tokenizer.NgramPrefixOnly(2, 5, filters);
        yield return filters => Tokenizer.NgramPositions(3, filters);
        yield return filters => Tokenizer.EdgeNgram(2, 5, filters);
        yield return filters => Tokenizer.EdgeNgram(2, 5, TokenChars.Letter, filters);
        yield return filters =>
            Tokenizer.EdgeNgram(2, 5, TokenChars.Letter | TokenChars.Digit, filters);
        yield return filters =>
            Tokenizer.EdgeNgram(
                2,
                5,
                TokenChars.Letter | TokenChars.Digit | TokenChars.Punctuation,
                filters
            );
        yield return filters =>
            Tokenizer.EdgeNgram(
                2,
                5,
                TokenChars.Letter
                    | TokenChars.Digit
                    | TokenChars.Punctuation
                    | TokenChars.Whitespace,
                filters
            );
        yield return filters =>
            Tokenizer.EdgeNgram(
                2,
                5,
                TokenChars.Letter
                    | TokenChars.Digit
                    | TokenChars.Punctuation
                    | TokenChars.Whitespace
                    | TokenChars.Symbol,
                filters
            );
        yield return filters => Tokenizer.Simple(filters);
        yield return filters => Tokenizer.RegexPattern(@"\w+", filters);
        yield return filters => Tokenizer.ChineseCompatible(filters);
        yield return filters => Tokenizer.Lindera(LinderaLanguage.Japanese, filters);
        yield return filters => Tokenizer.Lindera(LinderaLanguage.Japanese, true, filters);
        yield return filters => Tokenizer.Lindera(LinderaLanguage.Japanese, false, filters);
        yield return filters => Tokenizer.Icu(filters);
        yield return filters => Tokenizer.Jieba(filters);
        yield return filters => Tokenizer.SourceCode(filters);
    }

    public static IEnumerable<Tokenizer> Tokenizers()
    {
        yield return Tokenizer.Literal;

        foreach (var build in TokenizerBuilders())
        {
            yield return build([]);
        }
    }

    public static IEnumerable<Tokenizer> TokenizersWithAllFilters()
    {
        return TokenizerBuilders().Select(build => build(AllFilters));
    }

    /*[Test]
    [MethodDataSource(nameof(Tokenizers))]
    public async Task Tokenize_Predicate_ExecutesSuccessfully(Tokenizer tokenizer)
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Phrase(p.Description, EF.Functions.Tokenize(p.Description, tokenizer))
            )
            .Select(p => p.Description)
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    [MethodDataSource(nameof(TokenizersWithAllFilters))]
    public async Task Tokenize_Predicate_WithAllFilters_ExecutesSuccessfully(Tokenizer tokenizer)
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Phrase(p.Description, EF.Functions.Tokenize(p.Description, tokenizer))
            )
            .Select(p => p.Description)
            .ToListAsync();

        results.ShouldNotBeNull();
    }*/

    [Test]
    [MethodDataSource(nameof(Tokenizers))]
    public async Task TokenizeAsArray_Projection_ExecutesSuccessfully(Tokenizer tokenizer)
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Select(p => EF.Functions.TokenizeAsArray(p.Description, tokenizer))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    [MethodDataSource(nameof(TokenizersWithAllFilters))]
    public async Task TokenizeAsArray_Projection_WithAllFilters_ExecutesSuccessfully(
        Tokenizer tokenizer
    )
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Select(p => EF.Functions.TokenizeAsArray(p.Description, tokenizer))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task TokenizeAsArray_WithRemoveStopwords_AllLanguages_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        foreach (var language in Enum.GetValues<StopwordsLanguage>())
        {
            var results = await context
                .Products.Select(p =>
                    EF.Functions.TokenizeAsArray(
                        p.Description,
                        Tokenizer.Unicode(TokenFilter.RemoveStopwords(language))
                    )
                )
                .ToListAsync();

            results.ShouldNotBeNull();
        }
    }

    [Test]
    public async Task TokenizeAsArray_WithStemmer_AllLanguages_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        foreach (var language in Enum.GetValues<StemmerLanguage>())
        {
            var results = await context
                .Products.Select(p =>
                    EF.Functions.TokenizeAsArray(
                        p.Description,
                        Tokenizer.Unicode(TokenFilter.Stemmer(language))
                    )
                )
                .ToListAsync();

            results.ShouldNotBeNull();
        }
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task TokenizeAsArray_WithLindera_AllLanguages_ExecutesSuccessfully(
        bool keepWhitespace
    )
    {
        await using var context = DbFixture.CreateContext();

        foreach (var language in Enum.GetValues<LinderaLanguage>())
        {
            var results = await context
                .Products.Select(p =>
                    EF.Functions.TokenizeAsArray(
                        p.Description,
                        Tokenizer.Lindera(language, keepWhitespace)
                    )
                )
                .ToListAsync();

            results.ShouldNotBeNull();
        }
    }
}
