using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests;

public sealed class TokenFilterTests
{
    [Test]
    public void AlphaNumericOnly_ProducesCorrectSql()
    {
        TokenFilter.AlphaNumericOnly.ToString().ShouldBe("'alpha_num_only=true'");
    }

    [Test]
    public void AsciiFolding_ProducesCorrectSql()
    {
        TokenFilter.AsciiFolding.ToString().ShouldBe("'ascii_folding=true'");
    }

    [Test]
    public void PreserveCase_ProducesCorrectSql()
    {
        TokenFilter.PreserveCase.ToString().ShouldBe("'lowercase=false'");
    }

    [Test]
    [Arguments(StopwordsLanguage.English, "'stopwords_language=english'")]
    [Arguments(StopwordsLanguage.Hungarian, "'stopwords_language=hungarian'")]
    [Arguments(StopwordsLanguage.German, "'stopwords_language=german'")]
    public void RemoveStopwords_SingleLanguage_ProducesCorrectSql(
        StopwordsLanguage language,
        string expected
    )
    {
        TokenFilter.RemoveStopwords(language).ToString().ShouldBe(expected);
    }

    [Test]
    public void RemoveStopwords_MultipleLanguages_ProducesCorrectSql()
    {
        TokenFilter
            .RemoveStopwords(
                StopwordsLanguage.English,
                StopwordsLanguage.French,
                StopwordsLanguage.German
            )
            .ToString()
            .ShouldBe("'stopwords_language=english,french,german'");
    }

    [Test]
    [Arguments(StemmerLanguage.English, "'stemmer=english'")]
    [Arguments(StemmerLanguage.Hungarian, "'stemmer=hungarian'")]
    [Arguments(StemmerLanguage.German, "'stemmer=german'")]
    public void Stemmer_ProducesCorrectSql(StemmerLanguage language, string expected)
    {
        TokenFilter.Stemmer(language).ToString().ShouldBe(expected);
    }

    [Test]
    [Arguments(10, "'remove_long=10'")]
    [Arguments(255, "'remove_long=255'")]
    public void RemoveLong_ProducesCorrectSql(int maxBytes, string expected)
    {
        TokenFilter.RemoveLong(maxBytes).ToString().ShouldBe(expected);
    }

    [Test]
    [Arguments(1, "'remove_short=1'")]
    [Arguments(3, "'remove_short=3'")]
    public void RemoveShort_ProducesCorrectSql(int minBytes, string expected)
    {
        TokenFilter.RemoveShort(minBytes).ToString().ShouldBe(expected);
    }

    [Test]
    public void Trim_ProducesCorrectSql()
    {
        TokenFilter.Trim.ToString().ShouldBe("'trim=true'");
    }
}
