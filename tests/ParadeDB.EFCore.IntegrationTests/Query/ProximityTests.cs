using Microsoft.EntityFrameworkCore;
using ParadeDB.EFCore.Extensions;
using Shouldly;

namespace ParadeDB.EFCore.IntegrationTests.Query;

public sealed class ProximityTests : TestBase
{
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
                EF.Functions.Match(p.Description, Pdb.Proximity("sleek").WithinOrdered(1, "shoes"))
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
}
