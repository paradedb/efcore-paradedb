using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.IntegrationTests.Query;

public sealed class MatchDisjunctionTests : TestBase
{
    [Test]
    public async Task MatchDisjunction_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p => EF.Functions.MatchDisjunction(p.Description, "these"))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchDisjunction_WithInlineArray_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, new[] { "these", "shoes" })
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchDisjunction_WithArrayVariable_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var results = await context
            .Products.Where(p => EF.Functions.MatchDisjunction(p.Description, terms))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchDisjunction_WithFuzzy_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, "these", Pdb.Fuzzy(2))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchDisjunction_WithInlineArrayAndFuzzy_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(
                    p.Description,
                    new[] { "these", "shoes" },
                    Pdb.Fuzzy(2)
                )
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchDisjunction_WithArrayVariableAndFuzzy_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var results = await context
            .Products.Where(p => EF.Functions.MatchDisjunction(p.Description, terms, Pdb.Fuzzy(2)))
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchDisjunction_WithBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, "these", Pdb.Boost(2.3f))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchDisjunction_WithInlineArrayAndBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(
                    p.Description,
                    new[] { "these", "shoes" },
                    Pdb.Boost(2.3f)
                )
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchDisjunction_WithArrayVariableAndBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, terms, Pdb.Boost(2.3f))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchDisjunction_WithConst_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, "these", Pdb.Const(20.3f))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchDisjunction_WithInlineArrayAndConst_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(
                    p.Description,
                    new[] { "these", "shoes" },
                    Pdb.Const(20.3f)
                )
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchDisjunction_WithArrayVariableAndConst_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        string[] terms = ["these", "shoes"];

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, terms, Pdb.Const(20.3f))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }

    [Test]
    public async Task MatchDisjunction_WithFuzzyAndBoost_ExecutesSuccessfully()
    {
        await using var context = DbFixture.CreateContext();

        var results = await context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, "these", Pdb.Fuzzy(2), Pdb.Boost(2.3f))
            )
            .ToListAsync();

        results.ShouldNotBeNull();
    }
}
