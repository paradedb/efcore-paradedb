using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.IntegrationTests.Query;

public sealed class SnippetTests : TestBase
{
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
}
