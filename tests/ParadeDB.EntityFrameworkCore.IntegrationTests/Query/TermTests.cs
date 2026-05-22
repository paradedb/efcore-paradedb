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
}
