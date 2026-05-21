using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.IntegrationTests.Query;

public sealed class PhraseTests : TestBase
{
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
    public async Task Phrase_WithInlineArrayAndBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Phrase(p.Description, Pdb.Boost(new[] { "these", "shoes" }, 2.5f))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Phrase_WithArrayVariableAndBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var results = await context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Boost(terms, 2.5f)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Phrase_WithConst_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Const("with", 20.3f)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Phrase_WithInlineArrayAndConst_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Phrase(p.Description, Pdb.Const(new[] { "these", "shoes" }, 20.3f))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Phrase_WithArrayVariableAndConst_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var results = await context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Const(terms, 20.3f)))
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
}
