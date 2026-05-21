using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.IntegrationTests.Query;

public sealed class AliasTests : TestBase
{
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
