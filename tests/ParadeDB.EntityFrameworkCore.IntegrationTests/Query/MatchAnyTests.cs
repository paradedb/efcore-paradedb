using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.IntegrationTests.Query;

public sealed class MatchAnyTests : TestBase
{
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
}
