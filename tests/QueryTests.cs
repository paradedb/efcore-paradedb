using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests;

public sealed class QueryTests : TestBase
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

        var query = context.MockItems.Where(p => EF.Functions.MatchAll(p.Description, "these"));

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description &&& 'these'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithInlineArray()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.MatchAll(p.Description, new[] { "these", "shoes" })
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description &&& ARRAY['these','shoes']::text[]
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithArrayVariable()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.MockItems.Where(p => EF.Functions.MatchAll(p.Description, terms));

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description &&& @terms
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithFuzzy()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Fuzzy("these", 2))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description &&& 'these'::pdb.fuzzy(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithInlineArrayAndFuzzy()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Fuzzy(new[] { "these", "shoes" }, 2))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description &&& ARRAY['these','shoes']::text[]::pdb.fuzzy(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithArrayVariableAndFuzzy()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.MockItems.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Fuzzy(terms, 2))
        );

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description &&& @terms::pdb.fuzzy(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithBoost()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Boost("these", 2.3f))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description &&& 'these'::pdb.boost(2.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithInlineArrayAndBoost()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Boost(new[] { "these", "shoes" }, 2.3f))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description &&& ARRAY['these','shoes']::text[]::pdb.boost(2.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithArrayVariableAndBoost()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.MockItems.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Boost(terms, 2.3f))
        );

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description &&& @terms::pdb.boost(2.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithConst()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Const("these", 20.3f))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description &&& 'these'::pdb.const(20.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithInlineArrayAndConst()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Const(new[] { "these", "shoes" }, 20.3f))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description &&& ARRAY['these','shoes']::text[]::pdb.const(20.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithArrayVariableAndConst()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.MockItems.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Const(terms, 20.3f))
        );

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description &&& @terms::pdb.const(20.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAll_WithFuzzyAndBoost()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.MatchAll(p.Description, Pdb.Boost(Pdb.Fuzzy("these", 2), 2.3f))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description &&& 'these'::pdb.fuzzy(2)::pdb.boost(2.3)
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
        (Tokenizer.Icu(), "::pdb.icu"),
        (Tokenizer.ChineseCompatible([]), "::pdb.chinese_compatible"),
        (Tokenizer.Jieba(), "::pdb.jieba"),
        (
            Tokenizer.Lindera(LinderaLanguage.Chinese, new() { ["keep_whitespace"] = true }),
            "::pdb.lindera('chinese','keep_whitespace=true')"
        ),
        (Tokenizer.Literal(), "::pdb.literal"),
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
        (Tokenizer.SourceCode(), "::pdb.source_code"),
        (Tokenizer.Whitespace(), "::pdb.whitespace"),
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
                .MockItems.Where(p =>
                    EF.Functions.MatchAll(
                        p.Description,
                        EF.Functions.Tokenize("running shoes", tokenizerVariation.Tokenizer)
                    )
                )
                .Select(p => new { p.Id, p.Description });

            var sql = $$"""
                SELECT m.id AS "Id", m.description AS "Description"
                FROM mock_items AS m
                WHERE m.description &&& 'running shoes'{{tokenizerVariation.ExpectedSqlSnippet}}
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

        var query = context.MockItems.Where(p => EF.Functions.MatchAny(p.Description, "these"));

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description ||| 'these'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAny_WithInlineArray()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.MatchAny(p.Description, new[] { "these", "shoes" })
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description ||| ARRAY['these','shoes']::text[]
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAny_WithArrayVariable()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.MockItems.Where(p => EF.Functions.MatchAny(p.Description, terms));

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description ||| @terms
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MatchAny_WithConst()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.MatchAny(p.Description, Pdb.Const("these", 20.3f))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description ||| 'these'::pdb.const(20.3)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p => EF.Functions.Phrase(p.Description, "with"));

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description ### 'with'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase_WithInlineArray()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Phrase(p.Description, new[] { "these", "shoes" })
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description ### ARRAY['these','shoes']::text[]
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase_WithArrayVariable()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.MockItems.Where(p => EF.Functions.Phrase(p.Description, terms));

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description ### @terms
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase_WithBoost()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Phrase(p.Description, Pdb.Boost("with", 2.5f))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description ### 'with'::pdb.boost(2.5)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase_WithSlop()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Phrase(p.Description, Pdb.Slop("with", 2))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description ### 'with'::pdb.slop(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase_WithInlineArrayAndSlop()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Phrase(p.Description, Pdb.Slop(new[] { "these", "shoes" }, 2))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description ### ARRAY['these','shoes']::text[]::pdb.slop(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Phrase_WithArrayVariableAndSlop()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var query = context.MockItems.Where(p =>
            EF.Functions.Phrase(p.Description, Pdb.Slop(terms, 2))
        );

        var sql = """
            -- @terms={ 'these', 'shoes' } (DbType = Object)
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description ### @terms::pdb.slop(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Proximity(p.Description, Pdb.Proximity("sleek").Within(1, "shoes"))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description @@@ (('sleek' ## 1) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_Ordered()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Proximity(
                p.Description,
                Pdb.Proximity("sleek").Within(1, "shoes", ordered: true)
            )
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description @@@ (('sleek' ##> 1) ##> 'shoes')
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

        var query = context.MockItems.Where(p =>
            EF.Functions.Proximity(p.Description, Pdb.Proximity(left).Within(distance, right))
        );

        var sql = """
            -- @left='sleek'
            -- @distance='1'
            -- @right='shoes'
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description @@@ ((@left ## @distance) ## @right)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_WithRegex()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Proximity(p.Description, Pdb.ProximityRegex("sl.*").Within(1, "shoes"))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description @@@ ((pdb.prox_regex('sl.*') ## 1) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_WithRegexVariablePattern()
    {
        await using var context = DbFixture.CreateContext();

        string pattern = "sl.*";

        var query = context.MockItems.Where(p =>
            EF.Functions.Proximity(p.Description, Pdb.ProximityRegex(pattern).Within(1, "shoes"))
        );

        var sql = """
            -- @pattern='sl.*'
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description @@@ ((pdb.prox_regex(@pattern) ## 1) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_WithRegexAndMaxExpansions()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Proximity(
                p.Description,
                Pdb.ProximityRegex("sl.*", 100).Within(1, "shoes")
            )
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description @@@ ((pdb.prox_regex('sl.*', 100) ## 1) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_WithArrayOfTokens()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Proximity(
                p.Description,
                Pdb.ProximityArray("sleek", "white").Within(1, "shoes")
            )
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description @@@ ((pdb.prox_array('sleek', 'white') ## 1) ## 'shoes')
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

        var query = context.MockItems.Where(p =>
            EF.Functions.Proximity(p.Description, Pdb.ProximityArray(t1, t2).Within(1, "shoes"))
        );

        var sql = """
            -- @t1='sleek'
            -- @t2='white'
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description @@@ ((pdb.prox_array(@t1, @t2) ## 1) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_WithArrayOfMixedOperands()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Proximity(
                p.Description,
                Pdb.ProximityArray(Pdb.ProximityRegex("sl.*"), Pdb.ProximityArray("white"))
                    .Within(1, "shoes")
            )
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description @@@ ((pdb.prox_array(pdb.prox_regex('sl.*'), pdb.prox_array('white')) ## 1) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Proximity_Chained()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Proximity(
                p.Description,
                Pdb.Proximity("sleek").Within(1, "running").Within(2, "shoes")
            )
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description @@@ (((('sleek' ## 1) ## 'running') ## 2) ## 'shoes')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Score()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => new
            {
                p.Id,
                p.Category,
                Score = EF.Functions.Score(p.Description),
            });

        var sql = """
            SELECT m.id AS "Id", m.category AS "Category", pdb.score(m.description) AS "Score"
            FROM mock_items AS m
            WHERE m.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task All()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p => EF.Functions.All(p.Id));

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.id @@@ pdb.all()
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Exists()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p => EF.Functions.Exists(p.Id));

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.id @@@ pdb.exists()
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task RangeTerm()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p => EF.Functions.RangeTerm(p.WeightRange, 1));

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.weight_range @@@ pdb.range_term(1)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task RangeTerm_WithRelation()
    {
        await using var context = DbFixture.CreateContext();

        var range = new NpgsqlRange<int>(10, false, 12, true);

        var query = context.MockItems.Where(p =>
            EF.Functions.RangeTerm(p.WeightRange, range, RangeTermRelation.Intersects)
        );

        var sql = """
            -- @range='(10,12]' (DbType = Object)
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.weight_range @@@ pdb.range_term(@range, 'Intersects')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description));

        var sql = """
            SELECT pdb.snippet(m.description)
            FROM mock_items AS m
            WHERE m.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet_WithNullOptions()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, null));

        var sql = """
            SELECT pdb.snippet(m.description)
            FROM mock_items AS m
            WHERE m.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet_WithInlineMaxNumChars()
    {
        await using var context = DbFixture.CreateContext();
        var options = new SnippetOptions(null, null, 50);

        var query = context
            .MockItems.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, options));

        var sql = """
            SELECT pdb.snippet(m.description, max_num_chars => 50)
            FROM mock_items AS m
            WHERE m.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet_WithVariableMaxNumChars()
    {
        await using var context = DbFixture.CreateContext();

        int maxNumChars = 50;
        var options = new SnippetOptions(null, null, maxNumChars);

        var query = context
            .MockItems.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, options));

        var sql = """
            SELECT pdb.snippet(m.description, max_num_chars => 50)
            FROM mock_items AS m
            WHERE m.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet_WithInlineTags()
    {
        await using var context = DbFixture.CreateContext();
        var options = new SnippetOptions("<a>", "</a>", null);

        var query = context
            .MockItems.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, options));

        var sql = """
            SELECT pdb.snippet(m.description, start_tag => '<a>', end_tag => '</a>')
            FROM mock_items AS m
            WHERE m.description === 'rich'
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
        var options = new SnippetOptions(startTag, endTag, null);

        var query = context
            .MockItems.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, options));

        var sql = """
            SELECT pdb.snippet(m.description, start_tag => '<a>', end_tag => '</a>')
            FROM mock_items AS m
            WHERE m.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet_WithInlineTagsAndMaxNumChars()
    {
        await using var context = DbFixture.CreateContext();
        var options = new SnippetOptions("<a>", "</a>", 50);

        var query = context
            .MockItems.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, options));

        var sql = """
            SELECT pdb.snippet(m.description, start_tag => '<a>', end_tag => '</a>', max_num_chars => 50)
            FROM mock_items AS m
            WHERE m.description === 'rich'
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
        var options = new SnippetOptions(startTag, endTag, maxNumChars);

        var query = context
            .MockItems.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, options));

        var sql = """
            SELECT pdb.snippet(m.description, start_tag => '<a>', end_tag => '</a>', max_num_chars => 50)
            FROM mock_items AS m
            WHERE m.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippet_ReturnsNull_WhenNoMatch()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p => EF.Functions.MatchAny(p.Description, Pdb.Fuzzy("your", 2)))
            .Select(p => new { p.Id, Description = EF.Functions.Snippet(p.Description) });

        var sql = """
            SELECT m.id AS "Id", pdb.snippet(m.description) AS "Description"
            FROM mock_items AS m
            WHERE m.description ||| 'your'::pdb.fuzzy(2)
            """;

        AssertSql(query, sql);
        var results = await query.ToListAsync();

        results.ShouldAllBe(r => r.Description == null);
    }

    [Test]
    public async Task Snippets()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippets(p.Description));

        var sql = """
            SELECT pdb.snippets(m.description)
            FROM mock_items AS m
            WHERE m.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Snippets_WithOptions()
    {
        await using var context = DbFixture.CreateContext();
        var options = new SnippetsOptions("<a>", "</a>", 15, 1, 1, "position");

        var query = context
            .MockItems.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippets(p.Description, options));

        var sql = """
            SELECT pdb.snippets(m.description, start_tag => '<a>', end_tag => '</a>', max_num_chars => 15, "limit" => 1, "offset" => 1, sort_by => 'position')
            FROM mock_items AS m
            WHERE m.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task SnippetPositions()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.SnippetPositions(p.Description));

        var sql = """
            SELECT pdb.snippet_positions(m.description)
            FROM mock_items AS m
            WHERE m.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Term()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p => EF.Functions.Term(p.Description, "rich"));

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description === 'rich'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Term_WithInlineArray()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Term(p.Description, new[] { "rich", "cream" })
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description === ARRAY['rich','cream']::text[]
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Term_WithArrayVariable()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["rich", "cream"];

        var query = context.MockItems.Where(p => EF.Functions.Term(p.Description, terms));

        var sql = """
            -- @terms={ 'rich', 'cream' } (DbType = Object)
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description === @terms
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Term_WithFuzzy()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Term(p.Description, Pdb.Fuzzy("rich", 2))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description === 'rich'::pdb.fuzzy(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task FuzzyWithAllParameters()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.Term(p.Description, Pdb.Fuzzy("rich", 2))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description === 'rich'::pdb.fuzzy(2)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();

        query = context.MockItems.Where(p =>
            EF.Functions.Term(p.Description, Pdb.Fuzzy("rich", 2, true))
        );

        sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description === 'rich'::pdb.fuzzy(2, t)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();

        query = context.MockItems.Where(p =>
            EF.Functions.Term(p.Description, Pdb.Fuzzy("rich", 2, false, true))
        );

        sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description === 'rich'::pdb.fuzzy(2, f, t)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();

        query = context.MockItems.Where(p =>
            EF.Functions.Term(p.Description, Pdb.Fuzzy("rich", 2, true, true))
        );

        sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.description === 'rich'::pdb.fuzzy(2, t, t)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Alias_WithInlineAlias()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p =>
                EF.Functions.MatchAny(
                    EF.Functions.Alias(p.Description, "description_simple"),
                    "sleek"
                )
            )
            .Select(p => p.Description);

        var sql = """
            SELECT m.description
            FROM mock_items AS m
            WHERE m.description::pdb.alias('description_simple') ||| 'sleek'
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
            .MockItems.Where(p =>
                EF.Functions.MatchAny(EF.Functions.Alias(p.Description, aliasName), "sleek")
            )
            .Select(p => p.Description);

        var sql = """
            SELECT m.description
            FROM mock_items AS m
            WHERE m.description::pdb.alias('description_simple') ||| 'sleek'
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MoreLikeThis_WithInlineId()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.MoreLikeThis(p.Id, Pdb.DocumentId(5))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.id @@@ pdb.more_like_this(5)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MoreLikeThis_WithVariableId()
    {
        await using var context = DbFixture.CreateContext();

        int id = 5;

        var query = context.MockItems.Where(p =>
            EF.Functions.MoreLikeThis(p.Id, Pdb.DocumentId(id))
        );

        var sql = """
            -- @p='5'
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.id @@@ pdb.more_like_this(@p)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MoreLikeThis_WithInlineDocument()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.MoreLikeThis(p.Id, Pdb.Document("""{"description":"running shoes"}"""))
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.id @@@ pdb.more_like_this('{"description":"running shoes"}')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MoreLikeThis_WithVariableDocument()
    {
        await using var context = DbFixture.CreateContext();

        string document = """{"description":"running shoes"}""";

        var query = context.MockItems.Where(p =>
            EF.Functions.MoreLikeThis(p.Id, Pdb.Document(document))
        );

        var sql = """
            -- @document='{"description":"running shoes"}'
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.id @@@ pdb.more_like_this(@document)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MoreLikeThis_WithInlineParameters()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Where(p =>
            EF.Functions.MoreLikeThis(
                p.Id,
                Pdb.DocumentId(5)
                    .Fields("description", "category")
                    .MinTermFrequency(2)
                    .MinDocFrequency(3)
                    .MaxDocFrequency(100)
                    .MaxQueryTerms(12)
                    .MinWordLength(3)
                    .MaxWordLength(20)
                    .Stopwords("the", "and")
            )
        );

        var sql = """
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.id @@@ pdb.more_like_this(5, ARRAY['description','category']::text[], min_term_frequency => 2, min_doc_frequency => 3, max_doc_frequency => 100, max_query_terms => 12, min_word_length => 3, max_word_length => 20, stopwords => ARRAY['the','and']::text[])
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task MoreLikeThis_WithVariableParameters()
    {
        await using var context = DbFixture.CreateContext();

        int id = 5;
        var fields = new[] { "description", "category" };
        var stopwords = new[] { "the", "and" };
        int minTermFrequency = 2;
        int minDocFrequency = 3;
        int maxDocFrequency = 100;
        int maxQueryTerms = 12;
        int minWordLength = 3;
        int maxWordLength = 20;

        var query = context.MockItems.Where(p =>
            EF.Functions.MoreLikeThis(
                p.Id,
                Pdb.DocumentId(id)
                    .Fields(fields)
                    .MinTermFrequency(minTermFrequency)
                    .MinDocFrequency(minDocFrequency)
                    .MaxDocFrequency(maxDocFrequency)
                    .MaxQueryTerms(maxQueryTerms)
                    .MinWordLength(minWordLength)
                    .MaxWordLength(maxWordLength)
                    .Stopwords(stopwords)
            )
        );

        var sql = """
            -- @p='5'
            -- @fields={ 'description', 'category' } (DbType = Object)
            -- @minTermFrequency='2'
            -- @minDocFrequency='3'
            -- @maxDocFrequency='100'
            -- @maxQueryTerms='12'
            -- @minWordLength='3'
            -- @maxWordLength='20'
            -- @stopwords={ 'the', 'and' } (DbType = Object)
            SELECT m.id, m.category, m.created_at, m.description, m.in_stock, m.last_updated_date, m.latest_available_time, m.metadata, m.rating, m.weight_range
            FROM mock_items AS m
            WHERE m.id @@@ pdb.more_like_this(@p, @fields, min_term_frequency => @minTermFrequency, min_doc_frequency => @minDocFrequency, max_doc_frequency => @maxDocFrequency, max_query_terms => @maxQueryTerms, min_word_length => @minWordLength, max_word_length => @maxWordLength, stopwords => @stopwords)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Aggregate_ValueCount()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Select(p =>
            EF.Functions.Agg(new { value_count = new { field = "rating" } })
        );

        var sql = """
            SELECT pdb.agg('{"value_count":{"field":"rating"}}', TRUE)
            FROM mock_items AS m
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Aggregate_ValueCountOver()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Select(p =>
                EF.Functions.AggOver(new { value_count = new { field = "rating" } })
            )
            .Take(10);

        var sql = """
            -- @p='10'
            SELECT pdb.agg('{"value_count":{"field":"rating"}}', TRUE) OVER ()
            FROM mock_items AS m
            LIMIT @p
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Aggregate_ValueCountFilter()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Select(p =>
            EF.Functions.AggFilter(new { value_count = new { field = "rating" } }, p.Rating >= 4)
        );

        var sql = """
            SELECT pdb.agg('{"value_count":{"field":"rating"}}', TRUE) FILTER (WHERE m.rating >= 4)
            FROM mock_items AS m
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public void Aggregate_ValueCountFilterOver()
    {
        using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Select(p =>
                EF.Functions.AggFilterOver(
                    new { value_count = new { field = "rating" } },
                    p.Rating >= 4
                )
            )
            .Take(10);

        var sql = """
            -- @p='10'
            SELECT pdb.agg('{"value_count":{"field":"rating"}}', TRUE) FILTER (WHERE m.rating >= 4) OVER ()
            FROM mock_items AS m
            LIMIT @p
            """;

        AssertSql(query, sql);
    }

    [Test]
    public async Task Aggregate_MultipleMetrics()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Select(p => new
        {
            Average = EF.Functions.Agg(new { avg = new { field = "rating" } }),
            Sum = EF.Functions.Agg(new { sum = new { field = "rating" } }),
        });

        var sql = """
            SELECT pdb.agg('{"avg":{"field":"rating"}}', TRUE) AS "Average", pdb.agg('{"sum":{"field":"rating"}}', TRUE) AS "Sum"
            FROM mock_items AS m
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Aggregate_Range()
    {
        await using var context = DbFixture.CreateContext();

        var query = context.MockItems.Select(p =>
            EF.Functions.Agg(
                new
                {
                    range = new
                    {
                        field = "rating",
                        ranges = new object[] { new { to = 3.0 }, new { @from = 3.0, to = 6.0 } },
                    },
                }
            )
        );

        var sql = """
            SELECT pdb.agg('{"range":{"field":"rating","ranges":[{"to":3},{"from":3,"to":6}]}}', TRUE)
            FROM mock_items AS m
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Parse()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p =>
                EF.Functions.Parse(p.Description, "description:(sleek shoes) AND rating:>3")
            )
            .Select(p => p.Description);

        var sql = """
            SELECT m.description
            FROM mock_items AS m
            WHERE m.description @@@ pdb.parse('description:(sleek shoes) AND rating:>3')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Parse_Lenient()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p => EF.Functions.Parse(p.Description, "sleek shoes", lenient: true))
            .Select(p => p.Description);

        var sql = """
            SELECT m.description
            FROM mock_items AS m
            WHERE m.description @@@ pdb.parse('sleek shoes', lenient => TRUE)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Parse_ConjunctionMode()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p =>
                EF.Functions.Parse(p.Description, "description:(sleek shoes)", null, true)
            )
            .Select(p => p.Description);

        var sql = """
            SELECT m.description
            FROM mock_items AS m
            WHERE m.description @@@ pdb.parse('description:(sleek shoes)', conjunction_mode => TRUE)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task Parse_WithVariableParameters()
    {
        await using var context = DbFixture.CreateContext();

        string pattern = "description:(sleek shoes)";
        bool lenient = true;
        bool conjunctionMode = true;

        var query = context
            .MockItems.Where(p =>
                EF.Functions.Parse(p.Description, pattern, lenient, conjunctionMode)
            )
            .Select(p => p.Description);

        var sql = """
            -- @pattern='description:(sleek shoes)'
            -- @lenient='True' (Nullable = true)
            -- @conjunctionMode='True' (Nullable = true)
            SELECT m.description
            FROM mock_items AS m
            WHERE m.description @@@ pdb.parse(@pattern, lenient => @lenient, conjunction_mode => @conjunctionMode)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task RegexQuery()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p => EF.Functions.Regex(p.Description, "ru.*"))
            .Select(p => p.Description);

        var sql = """
            SELECT m.description
            FROM mock_items AS m
            WHERE m.description @@@ pdb.regex('ru.*')
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task RegexPhrase()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p =>
                EF.Functions.RegexPhrase(p.Description, new[] { "ru.*", "shoes" })
            )
            .Select(p => p.Description);

        var sql = """
            SELECT m.description
            FROM mock_items AS m
            WHERE m.description @@@ pdb.regex_phrase(ARRAY['ru.*','shoes']::text[])
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task RegexPhraseSlopAndMaxExpansions()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p =>
                EF.Functions.RegexPhrase(p.Description, new[] { "ru.*", "shoes" }, 2, 100)
            )
            .Select(p => p.Description);

        var sql = """
            SELECT m.description
            FROM mock_items AS m
            WHERE m.description @@@ pdb.regex_phrase(ARRAY['ru.*','shoes']::text[], slop => 2, max_expansions => 100)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task RegexPhraseMaxExpansions()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p =>
                EF.Functions.RegexPhrase(p.Description, new[] { "ru.*", "shoes" }, null, 100)
            )
            .Select(p => p.Description);

        var sql = """
            SELECT m.description
            FROM mock_items AS m
            WHERE m.description @@@ pdb.regex_phrase(ARRAY['ru.*','shoes']::text[], max_expansions => 100)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task PhrasePrefix()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p =>
                EF.Functions.PhrasePrefix(p.Description, new[] { "running", "shoes" })
            )
            .Select(p => p.Description);

        var sql = """
            SELECT m.description
            FROM mock_items AS m
            WHERE m.description @@@ pdb.phrase_prefix(ARRAY['running','shoes']::text[])
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }

    [Test]
    public async Task PhrasePrefixMaxExpansions()
    {
        await using var context = DbFixture.CreateContext();

        var query = context
            .MockItems.Where(p =>
                EF.Functions.PhrasePrefix(p.Description, new[] { "running", "shoes" }, 100)
            )
            .Select(p => p.Description);

        var sql = """
            SELECT m.description
            FROM mock_items AS m
            WHERE m.description @@@ pdb.phrase_prefix(ARRAY['running','shoes']::text[], 100)
            """;

        AssertSql(query, sql);
        await query.ToListAsync();
    }
}
