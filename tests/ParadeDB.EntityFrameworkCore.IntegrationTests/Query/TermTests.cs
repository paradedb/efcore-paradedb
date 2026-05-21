using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.IntegrationTests.Query;

public sealed class TermTests : TestBase
{
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
    public async Task Term_WithInlineArrayAndFuzzy_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Term(p.Description, Pdb.Fuzzy(new[] { "rich", "cream" }, 2))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Term_WithArrayVariableAndFuzzy_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["rich", "cream"];

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Fuzzy(terms, 2)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Term_WithBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Boost("rich", 2.3f)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Term_WithInlineArrayAndBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Term(p.Description, Pdb.Boost(new[] { "rich", "cream" }, 2.3f))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Term_WithArrayVariableAndBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["rich", "cream"];

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Boost(terms, 2.3f)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Term_WithConst_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Const("rich", 20.3f)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Term_WithInlineArrayAndConst_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Term(p.Description, Pdb.Const(new[] { "rich", "cream" }, 20.3f))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Term_WithArrayVariableAndConst_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["rich", "cream"];

        var results = await context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Const(terms, 20.3f)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task Term_WithFuzzyAndBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.Term(p.Description, Pdb.Boost(Pdb.Fuzzy("rich", 2), 2.3f))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }
}
