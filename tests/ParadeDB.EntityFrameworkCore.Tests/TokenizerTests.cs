using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests;

public sealed class TokenizerTests
{
    [Test]
    public void Literal_ProducesCorrectSql()
    {
        Tokenizer.Literal.ToString().ShouldBe("pdb.literal");
    }

    [Test]
    public void Unicode_ProducesCorrectSql()
    {
        Tokenizer.Unicode().ToString().ShouldBe("pdb.unicode_words");
    }

    [Test]
    [Arguments(false, "pdb.unicode_words")]
    [Arguments(true, "pdb.unicode_words('remove_emojis=true')")]
    public void Unicode_WithRemoveEmojis_ProducesCorrectSql(bool removeEmojis, string expected)
    {
        Tokenizer.Unicode(removeEmojis).ToString().ShouldBe(expected);
    }

    [Test]
    public void LiteralNormalized_ProducesCorrectSql()
    {
        Tokenizer.LiteralNormalized().ToString().ShouldBe("pdb.literal_normalized");
    }

    [Test]
    public void Whitespace_ProducesCorrectSql()
    {
        Tokenizer.Whitespace().ToString().ShouldBe("pdb.whitespace");
    }

    [Test]
    [Arguments(1, 1, "pdb.ngram(1,1)")]
    [Arguments(2, 5, "pdb.ngram(2,5)")]
    public void Ngram_ProducesCorrectSql(int min, int max, string expected)
    {
        Tokenizer.Ngram(min, max).ToString().ShouldBe(expected);
    }

    [Test]
    [Arguments(1, 1, "pdb.ngram(1, 1, 'prefix_only=true')")]
    public void NgramPrefixOnly_ProducesCorrectSql(int min, int max, string expected)
    {
        Tokenizer.NgramPrefixOnly(min, max).ToString().ShouldBe(expected);
    }

    [Test]
    [Arguments(3, "pdb.ngram(3, 3, 'positions=true')")]
    public void NgramPositions_ProducesCorrectSql(int size, string expected)
    {
        Tokenizer.NgramPositions(size).ToString().ShouldBe(expected);
    }

    [Test]
    [Arguments(2, 5, "pdb.edge_ngram(2,5)")]
    [Arguments(1, 3, "pdb.edge_ngram(1,3)")]
    public void EdgeNgram_ProducesCorrectSql(int min, int max, string expected)
    {
        Tokenizer.EdgeNgram(min, max).ToString().ShouldBe(expected);
    }

    [Test]
    [Arguments(TokenChars.Letter, "pdb.edge_ngram(2,5, 'token_chars=letter')")]
    [Arguments(
        TokenChars.Letter | TokenChars.Digit,
        "pdb.edge_ngram(2,5, 'token_chars=letter,digit')"
    )]
    [Arguments(
        TokenChars.Letter | TokenChars.Digit | TokenChars.Punctuation,
        "pdb.edge_ngram(2,5, 'token_chars=letter,digit,punctuation')"
    )]
    public void EdgeNgram_WithTokenChars_ProducesCorrectSql(TokenChars tokenChars, string expected)
    {
        Tokenizer.EdgeNgram(2, 5, tokenChars).ToString().ShouldBe(expected);
    }

    [Test]
    public void Simple_ProducesCorrectSql()
    {
        Tokenizer.Simple().ToString().ShouldBe("pdb.simple");
    }

    [Test]
    [Arguments(@"\w+", "pdb.regex_pattern('\\w+')")]
    public void RegexPattern_ProducesCorrectSql(string pattern, string expected)
    {
        Tokenizer.RegexPattern(pattern).ToString().ShouldBe(expected);
    }

    [Test]
    public void ChineseCompatible_ProducesCorrectSql()
    {
        Tokenizer.ChineseCompatible().ToString().ShouldBe("pdb.chinese_compatible");
    }

    [Test]
    [Arguments(LinderaLanguage.Chinese, "pdb.lindera(chinese)")]
    [Arguments(LinderaLanguage.Japanese, "pdb.lindera(japanese)")]
    [Arguments(LinderaLanguage.Korean, "pdb.lindera(korean)")]
    public void Lindera_ProducesCorrectSql(LinderaLanguage lang, string expected)
    {
        Tokenizer.Lindera(lang).ToString().ShouldBe(expected);
    }

    [Test]
    [Arguments(LinderaLanguage.Chinese, false, "pdb.lindera(chinese)")]
    [Arguments(LinderaLanguage.Japanese, false, "pdb.lindera(japanese)")]
    [Arguments(LinderaLanguage.Korean, false, "pdb.lindera(korean)")]
    [Arguments(LinderaLanguage.Chinese, true, "pdb.lindera(chinese, 'keep_whitespace=true')")]
    [Arguments(LinderaLanguage.Japanese, true, "pdb.lindera(japanese, 'keep_whitespace=true')")]
    [Arguments(LinderaLanguage.Korean, true, "pdb.lindera(korean, 'keep_whitespace=true')")]
    public void Lindera_WithKeepWhitespace_ProducesCorrectSql(
        LinderaLanguage language,
        bool keepWhitespace,
        string expected
    )
    {
        Tokenizer.Lindera(language, keepWhitespace).ToString().ShouldBe(expected);
    }

    [Test]
    public void Icu_ProducesCorrectSql()
    {
        Tokenizer.Icu().ToString().ShouldBe("pdb.icu");
    }

    [Test]
    public void Jieba_ProducesCorrectSql()
    {
        Tokenizer.Jieba().ToString().ShouldBe("pdb.jieba");
    }

    [Test]
    public void SourceCode_ProducesCorrectSql()
    {
        Tokenizer.SourceCode().ToString().ShouldBe("pdb.source_code");
    }
}
