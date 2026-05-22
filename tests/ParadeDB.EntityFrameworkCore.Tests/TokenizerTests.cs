using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests;

public sealed class TokenizerTests
{
    [Test]
    public void ToString_EscapesOptionKeysAndStringValues()
    {
        var tokenizer = Tokenizer.Whitespace(new() { ["ali'as"] = "'escape me'", ["trim"] = true });

        tokenizer.ToString().ShouldBe("pdb.whitespace('ali''as=''escape me''','trim=true')");
    }

    [Test]
    public void ToString_EscapesRegexPattern()
    {
        var tokenizer = Tokenizer.RegexPattern("isn't", []);

        tokenizer.ToString().ShouldBe("pdb.regex_pattern('isn''t')");
    }

    [Test]
    public void ToString_ThrowsArgumentExceptionForUnsupportedOptionValue()
    {
        var tokenizer = Tokenizer.Whitespace(new() { ["bad"] = 1m });

        var exception = Should.Throw<ArgumentException>(() => tokenizer.ToString());

        exception.ParamName.ShouldBe("options");
        exception.Message.ShouldContain(
            "Tokenizer option 'bad' has unsupported value type 'Decimal'"
        );
    }
}
