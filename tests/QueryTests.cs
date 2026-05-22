using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests.Query;

public sealed class MatchAllTests : TestBase
{
    private static void AssertSql(IQueryable query, string expected) =>
        NormalizeSql(query.ToQueryString()).ShouldBe(NormalizeSql(expected));

    private static string NormalizeSql(string sql) =>
        Regex.Replace(
            Regex.Replace(sql.ReplaceLineEndings("\n"), @"@__(\w+?)_\d+", "@$1"),
            "(?m)(^-- .+\n)\n(?=SELECT)",
            "$1"
        );

    [Test]
    public async Task MatchAll()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p => EF.Functions.MatchAll(p.Description, "these"));

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description &&& 'these'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithInlineArray()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.MatchAll(p.Description, new[] { "these", "shoes" })
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description &&& ARRAY['these','shoes']::text[]
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithArrayVariable()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.Products.Where(p => EF.Functions.MatchAll(p.Description, terms));

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description &&& @terms
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithFuzzy()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Fuzzy("these", 2))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description &&& 'these'::pdb.fuzzy(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithInlineArrayAndFuzzy()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Fuzzy(new[] { "these", "shoes" }, 2))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description &&& ARRAY['these','shoes']::text[]::pdb.fuzzy(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithArrayVariableAndFuzzy()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.Products.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Fuzzy(terms, 2))
        );

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description &&& @terms::pdb.fuzzy(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithBoost()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Boost("these", 2.3f))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description &&& 'these'::pdb.boost(2.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithInlineArrayAndBoost()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Boost(new[] { "these", "shoes" }, 2.3f))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description &&& ARRAY['these','shoes']::text[]::pdb.boost(2.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithArrayVariableAndBoost()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.Products.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Boost(terms, 2.3f))
        );

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description &&& @terms::pdb.boost(2.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithConst()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Const("these", 20.3f))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description &&& 'these'::pdb.const(20.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithInlineArrayAndConst()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Const(new[] { "these", "shoes" }, 20.3f))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description &&& ARRAY['these','shoes']::text[]::pdb.const(20.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithArrayVariableAndConst()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.Products.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Const(terms, 20.3f))
        );

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description &&& @terms::pdb.const(20.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithFuzzyAndBoost()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Boost(Pdb.Fuzzy("these", 2), 2.3f))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description &&& 'these'::pdb.fuzzy(2)::pdb.boost(2.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    private static readonly (
        Tokenizer Tokenizer,
        string ExpectedSqlSnippet
    )[] TokenizerVariations =
    [
        (
            Tokenizer.Unicode(new() { ["remove_emojis"] = true }),
            "::pdb.unicode_words('remove_emojis=true')"
        ),
        (
            Tokenizer.Simple(new() { ["stemmer"] = "english", ["alias"] = "simple_description" }),
            "::pdb.simple('stemmer=english','alias=simple_description')"
        ),
        (Tokenizer.Icu([]), "::pdb.icu"),
        (Tokenizer.ChineseCompatible([]), "::pdb.chinese_compatible"),
        (Tokenizer.Jieba([]), "::pdb.jieba"),
        (
            Tokenizer.Lindera(LinderaLanguage.Chinese, new() { ["keep_whitespace"] = true }),
            "::pdb.lindera('chinese','keep_whitespace=true')"
        ),
        (Tokenizer.Literal([]), "::pdb.literal"),
        (
            Tokenizer.LiteralNormalized(new() { ["trim"] = true }),
            "::pdb.literal_normalized('trim=true')"
        ),
        (
            Tokenizer.Ngram(3, 3, new() { ["positions"] = true, ["prefix_only"] = true }),
            "::pdb.ngram(3,3,'positions=true','prefix_only=true')"
        ),
        (
            Tokenizer.EdgeNgram(2, 5, new() { ["token_chars"] = "letter,digit,punctuation" }),
            "::pdb.edge_ngram(2,5,'token_chars=letter,digit,punctuation')"
        ),
        (Tokenizer.RegexPattern("[a-z]+", []), "::pdb.regex_pattern('[a-z]+')"),
        (Tokenizer.SourceCode([]), "::pdb.source_code"),
        (Tokenizer.Whitespace([]), "::pdb.whitespace"),
        (
            Tokenizer.Whitespace(new() { ["alias"] = "'escape me'" }),
            "::pdb.whitespace('alias=''escape me''')"
        ),
    ];

    [Test]
    public async Task MatchAll_WithTokenizerVariations()
    {
        await using var context = DbFixture.CreateContext();

        foreach (var tokenizerVariation in TokenizerVariations)
        {
            var query = context
                .Products.Where(p =>
                    EF.Functions.MatchAll(
                        p.Description,
                        EF.Functions.Tokenize("running shoes", tokenizerVariation.Tokenizer)
                    )
                )
                .Select(p => new { p.Id, p.Description });

            var sql = $$"""
                SELECT p.id AS "Id", p.description AS "Description"
                FROM products AS p
                WHERE p.description &&& 'running shoes'{{tokenizerVariation.ExpectedSqlSnippet}}
                """;

            AssertSql(query, sql);
            await query.ToListAsync();
        }
    }

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

    [Test]
    public async Task MatchAny()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p => EF.Functions.MatchAny(p.Description, "these"));

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description ||| 'these'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAny_WithInlineArray()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.MatchAny(p.Description, new[] { "these", "shoes" })
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description ||| ARRAY['these','shoes']::text[]
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAny_WithArrayVariable()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.Products.Where(p => EF.Functions.MatchAny(p.Description, terms));

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description ||| @terms
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAny_WithConst()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.MatchAny(p.Description, Pdb.Const("these", 20.3f))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description ||| 'these'::pdb.const(20.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p => EF.Functions.Phrase(p.Description, "with"));

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description ### 'with'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase_WithInlineArray()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Phrase(p.Description, new[] { "these", "shoes" })
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description ### ARRAY['these','shoes']::text[]
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase_WithArrayVariable()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.Products.Where(p => EF.Functions.Phrase(p.Description, terms));

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description ### @terms
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase_WithBoost()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Phrase(p.Description, Pdb.Boost("with", 2.5f))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description ### 'with'::pdb.boost(2.5)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase_WithSlop()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Phrase(p.Description, Pdb.Slop("with", 2))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description ### 'with'::pdb.slop(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase_WithInlineArrayAndSlop()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Phrase(p.Description, Pdb.Slop(new[] { "these", "shoes" }, 2))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description ### ARRAY['these','shoes']::text[]::pdb.slop(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase_WithArrayVariableAndSlop()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.Products.Where(p =>
            EF.Functions.Phrase(p.Description, Pdb.Slop(terms, 2))
        );

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description ### @terms::pdb.slop(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Match(p.Description, Pdb.Proximity("sleek").Within(1, "shoes"))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description @@@ (('sleek' ## 1) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_Ordered()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Match(
                p.Description,
                Pdb.Proximity("sleek").Within(1, "shoes", ordered: true)
            )
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description @@@ (('sleek' ##> 1) ##> 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_WithVariableArguments()
    {
        await using var context = DbFixture.CreateContext();

        string left = "sleek";
        string right = "shoes";
        int distance = 1;

        var query = context.Products.Where(p =>
            EF.Functions.Match(p.Description, Pdb.Proximity(left).Within(distance, right))
        );

        var sql = """
            -- @left='sleek'
            -- @distance='1'
            -- @right='shoes'
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description @@@ ((@left ## @distance) ## @right)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_WithRegex()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Match(p.Description, Pdb.ProximityRegex("sl.*").Within(1, "shoes"))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description @@@ ((pdb.prox_regex('sl.*') ## 1) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_WithRegexVariablePattern()
    {
        await using var context = DbFixture.CreateContext();

        string pattern = "sl.*";

        var query = context.Products.Where(p =>
            EF.Functions.Match(p.Description, Pdb.ProximityRegex(pattern).Within(1, "shoes"))
        );

        var sql = """
            -- @pattern='sl.*'
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description @@@ ((pdb.prox_regex(@pattern) ## 1) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_WithRegexAndMaxExpansions()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Match(p.Description, Pdb.ProximityRegex("sl.*", 100).Within(1, "shoes"))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description @@@ ((pdb.prox_regex('sl.*', 100) ## 1) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_WithArrayOfTokens()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Match(
                p.Description,
                Pdb.ProximityArray("sleek", "white").Within(1, "shoes")
            )
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description @@@ ((pdb.prox_array('sleek', 'white') ## 1) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_WithArrayOfVariableTokens()
    {
        await using var context = DbFixture.CreateContext();

        string t1 = "sleek";
        string t2 = "white";

        var query = context.Products.Where(p =>
            EF.Functions.Match(p.Description, Pdb.ProximityArray(t1, t2).Within(1, "shoes"))
        );

        var sql = """
            -- @t1='sleek'
            -- @t2='white'
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description @@@ ((pdb.prox_array(@t1, @t2) ## 1) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_WithArrayOfMixedOperands()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Match(
                p.Description,
                Pdb.ProximityArray(Pdb.ProximityRegex("sl.*"), Pdb.ProximityArray("white"))
                    .Within(1, "shoes")
            )
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description @@@ ((pdb.prox_array(pdb.prox_regex('sl.*'), pdb.prox_array('white')) ## 1) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_Chained()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Match(
                p.Description,
                Pdb.Proximity("sleek").Within(1, "running").Within(2, "shoes")
            )
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description @@@ (((('sleek' ## 1) ## 'running') ## 2) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Score()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => new
            {
                p.Id,
                p.Name,
                Score = EF.Functions.Score(p.Description),
            });

        var sql = """
            SELECT p.id AS "Id", p.name AS "Name", pdb.score(p.description) AS "Score"
            FROM products AS p
            WHERE p.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description));

        var sql = """
            SELECT pdb.snippet(p.description)
            FROM products AS p
            WHERE p.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet_WithInlineMaxNumChars()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, 50));

        var sql = """
            SELECT pdb.snippet(p.description, '<b>', '</b>', 50)
            FROM products AS p
            WHERE p.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet_WithVariableMaxNumChars()
    {
        await using var context = DbFixture.CreateContext();

        int maxNumChars = 50;

        var query = context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, maxNumChars));

        var sql = """
            -- @maxNumChars='50'
            SELECT pdb.snippet(p.description, '<b>', '</b>', @maxNumChars)
            FROM products AS p
            WHERE p.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet_WithInlineTags()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, "<a>", "</a>"));

        var sql = """
            SELECT pdb.snippet(p.description, '<a>', '</a>')
            FROM products AS p
            WHERE p.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet_WithVariableTags()
    {
        await using var context = DbFixture.CreateContext();

        string startTag = "<a>";
        string endTag = "</a>";

        var query = context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, startTag, endTag));

        var sql = """
            -- @startTag='<a>'
            -- @endTag='</a>'
            SELECT pdb.snippet(p.description, @startTag, @endTag)
            FROM products AS p
            WHERE p.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet_WithInlineTagsAndMaxNumChars()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, "<a>", "</a>", 50));

        var sql = """
            SELECT pdb.snippet(p.description, '<a>', '</a>', 50)
            FROM products AS p
            WHERE p.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet_WithVariableTagsAndMaxNumChars()
    {
        await using var context = DbFixture.CreateContext();

        string startTag = "<a>";
        string endTag = "</a>";
        int maxNumChars = 50;

        var query = context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, startTag, endTag, maxNumChars));

        var sql = """
            -- @startTag='<a>'
            -- @endTag='</a>'
            -- @maxNumChars='50'
            SELECT pdb.snippet(p.description, @startTag, @endTag, @maxNumChars)
            FROM products AS p
            WHERE p.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet_ReturnsNull_WhenNoMatch()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .Products.Where(p => EF.Functions.MatchAny(p.Description, Pdb.Fuzzy("your", 2)))
            .Select(p => new { p.Id, Description = EF.Functions.Snippet(p.Description) });

        var sql = """
            SELECT p.id AS "Id", pdb.snippet(p.description) AS "Description"
            FROM products AS p
            WHERE p.description ||| 'your'::pdb.fuzzy(2)
            """;

        AssertSql(query, sql);
        var results = await query.ToListAsync();

        results.ShouldAllBe(r => r.Description == null);
    }

    [Test]
    public async Task Term()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p => EF.Functions.Term(p.Description, "rich"));

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Term_WithInlineArray()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Term(p.Description, new[] { "rich", "cream" })
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description === ARRAY['rich','cream']::text[]
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Term_WithArrayVariable()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["rich", "cream"];

        var query = context.Products.Where(p => EF.Functions.Term(p.Description, terms));

        var sql = """
            -- @terms={ 'rich', 'cream' } (DbType = Object)
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description === @terms
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Term_WithFuzzy()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Term(p.Description, Pdb.Fuzzy("rich", 2))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description === 'rich'::pdb.fuzzy(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task FuzzyWithAllParameters()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.Products.Where(p =>
            EF.Functions.Term(p.Description, Pdb.Fuzzy("rich", 2))
        );

        var sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description === 'rich'::pdb.fuzzy(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();

        query = context.Products.Where(p =>
            EF.Functions.Term(p.Description, Pdb.Fuzzy("rich", 2, true))
        );

        sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description === 'rich'::pdb.fuzzy(2, t)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();

        query = context.Products.Where(p =>
            EF.Functions.Term(p.Description, Pdb.Fuzzy("rich", 2, false, true))
        );

        sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description === 'rich'::pdb.fuzzy(2, f, t)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();

        query = context.Products.Where(p =>
            EF.Functions.Term(p.Description, Pdb.Fuzzy("rich", 2, true, true))
        );

        sql = """
            SELECT p.id, p.description, p.name
            FROM products AS p
            WHERE p.description === 'rich'::pdb.fuzzy(2, t, t)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Alias_WithInlineAlias()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .Items.Where(p =>
                EF.Functions.MatchAny(
                    EF.Functions.Alias(p.Description, "description_simple"),
                    "sleek"
                )
            )
            .Select(p => p.Description);

        var sql = """
            SELECT i.description
            FROM items AS i
            WHERE i.description::pdb.alias('description_simple') ||| 'sleek'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Alias_WithVariableAlias()
    {
        await using var context = DbFixture.CreateContext();

        string aliasName = "description_simple";

        var query = context
            .Items.Where(p =>
                EF.Functions.MatchAny(EF.Functions.Alias(p.Description, aliasName), "sleek")
            )
            .Select(p => p.Description);

        var sql = """
            SELECT i.description
            FROM items AS i
            WHERE i.description::pdb.alias('description_simple') ||| 'sleek'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }
}
