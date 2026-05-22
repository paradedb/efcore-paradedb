using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.IntegrationTests.Query;

public sealed class MatchAllTests : TestBase
{
    [Test]
    public async Task MatchAll_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.MatchAll(p.Description, "these"))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAll_WithInlineArray_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.MatchAll(p.Description, new[] { "these", "shoes" }))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAll_WithArrayVariable_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var results = await context
            .Products.Where(p => EF.Functions.MatchAll(p.Description, terms))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAll_WithFuzzy_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.MatchAll(p.Description, Pdb.Fuzzy("these", 2)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAll_WithInlineArrayAndFuzzy_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchAll(p.Description, Pdb.Fuzzy(new[] { "these", "shoes" }, 2))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAll_WithArrayVariableAndFuzzy_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var results = await context
            .Products.Where(p => EF.Functions.MatchAll(p.Description, Pdb.Fuzzy(terms, 2)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAll_WithBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.MatchAll(p.Description, Pdb.Boost("these", 2.3f)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAll_WithInlineArrayAndBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchAll(p.Description, Pdb.Boost(new[] { "these", "shoes" }, 2.3f))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAll_WithArrayVariableAndBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var results = await context
            .Products.Where(p => EF.Functions.MatchAll(p.Description, Pdb.Boost(terms, 2.3f)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAll_WithConst_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.MatchAll(p.Description, Pdb.Const("these", 20.3f)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAll_WithInlineArrayAndConst_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchAll(p.Description, Pdb.Const(new[] { "these", "shoes" }, 20.3f))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAll_WithArrayVariableAndConst_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var results = await context
            .Products.Where(p => EF.Functions.MatchAll(p.Description, Pdb.Const(terms, 20.3f)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAll_WithFuzzyAndBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchAll(p.Description, Pdb.Boost(Pdb.Fuzzy("these", 2), 2.3f))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAny_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.MatchAny(p.Description, "these"))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAny_WithInlineArray_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.MatchAny(p.Description, new[] { "these", "shoes" }))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAny_WithArrayVariable_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var results = await context
            .Products.Where(p => EF.Functions.MatchAny(p.Description, terms))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchAny_WithConst_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.MatchAny(p.Description, Pdb.Const("these", 20.3f)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Phrase_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Phrase(p.Description, "with"))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Phrase_WithInlineArray_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Phrase(p.Description, new[] { "these", "shoes" }))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Phrase_WithArrayVariable_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var results = await context
            .Products.Where(p => EF.Functions.Phrase(p.Description, terms))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Phrase_WithBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Boost("with", 2.5f)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Phrase_WithSlop_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Slop("with", 2)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Phrase_WithInlineArrayAndSlop_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Phrase(p.Description, Pdb.Slop(new[] { "these", "shoes" }, 2))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Phrase_WithArrayVariableAndSlop_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var results = await context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Slop(terms, 2)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Proximity_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Match(p.Description, Pdb.Proximity("sleek").Within(1, "shoes"))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Proximity_Ordered_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Match(
                    p.Description,
                    Pdb.Proximity("sleek").Within(1, "shoes", ordered: true)
                )
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Proximity_WithVariableArguments_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string left = "sleek";
        string right = "shoes";
        int distance = 1;

        var results = await context
            .Products.Where(p =>
                EF.Functions.Match(p.Description, Pdb.Proximity(left).Within(distance, right))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Proximity_WithRegex_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Match(p.Description, Pdb.ProximityRegex("sl.*").Within(1, "shoes"))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Proximity_WithRegexVariablePattern_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string pattern = "sl.*";

        var results = await context
            .Products.Where(p =>
                EF.Functions.Match(p.Description, Pdb.ProximityRegex(pattern).Within(1, "shoes"))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Proximity_WithRegexAndMaxExpansions_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Match(
                    p.Description,
                    Pdb.ProximityRegex("sl.*", 100).Within(1, "shoes")
                )
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Proximity_WithArrayOfTokens_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Match(
                    p.Description,
                    Pdb.ProximityArray("sleek", "white").Within(1, "shoes")
                )
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Proximity_WithArrayOfVariableTokens_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string t1 = "sleek";
        string t2 = "white";

        var results = await context
            .Products.Where(p =>
                EF.Functions.Match(p.Description, Pdb.ProximityArray(t1, t2).Within(1, "shoes"))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Proximity_WithArrayOfMixedOperands_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Match(
                    p.Description,
                    Pdb.ProximityArray(Pdb.ProximityRegex("sl.*"), Pdb.ProximityArray("white"))
                        .Within(1, "shoes")
                )
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Proximity_Chained_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Match(
                    p.Description,
                    Pdb.Proximity("sleek").Within(1, "running").Within(2, "shoes")
                )
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Score_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => new
            {
                p.Id,
                p.Name,
                Score = EF.Functions.Score(p.Description),
            })
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Snippet_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Snippet_WithInlineMaxNumChars_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, 50))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Snippet_WithVariableMaxNumChars_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        int maxNumChars = 50;

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, maxNumChars))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Snippet_WithInlineTags_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, "<a>", "</a>"))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Snippet_WithVariableTags_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string startTag = "<a>";
        string endTag = "</a>";

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, startTag, endTag))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Snippet_WithInlineTagsAndMaxNumChars_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, "<a>", "</a>", 50))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Snippet_WithVariableTagsAndMaxNumChars_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string startTag = "<a>";
        string endTag = "</a>";
        int maxNumChars = 50;

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .Select(p => EF.Functions.Snippet(p.Description, startTag, endTag, maxNumChars))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Snippet_ReturnsNull_WhenNoMatch()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.MatchAny(p.Description, Pdb.Fuzzy("your", 2)))
            .Select(p => new { p.Id, Description = EF.Functions.Snippet(p.Description) })
            .ToListAsync();

        results.ShouldAllBe(r => r.Description == null);
    }

    [Test]
    public async Task Term_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, "rich"))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Term_WithInlineArray_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, new[] { "rich", "cream" }))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Term_WithArrayVariable_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["rich", "cream"];

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, terms))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Term_WithFuzzy_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Fuzzy("rich", 2)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Alias_WithInlineAlias_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Items.Where(p =>
                EF.Functions.MatchAny(
                    EF.Functions.Alias(p.Description, "description_simple"),
                    "sleek"
                )
            )
            .Select(p => p.Description)
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Alias_WithVariableAlias_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string aliasName = "description_simple";

        var results = await context
            .Items.Where(p =>
                EF.Functions.MatchAny(EF.Functions.Alias(p.Description, aliasName), "sleek")
            )
            .Select(p => p.Description)
            .ToListAsync();

        results.ShouldNotBeNull();
    }
}
