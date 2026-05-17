using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.IntegrationTests.Query;

public sealed class ScoreTests : TestBase
{
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
    public async Task Score_WithoutParadeDbFilter_ShouldThrowException()
    {
        await using var context = DbFixture.CreateContext();

        await Should.ThrowAsync<Exception>(async () =>
        {
            await context
                .Products.Select(p => new
                {
                    p.Id,
                    p.Name,
                    Score = EF.Functions.Score(p.Description),
                })
                .ToListAsync();
        });
    }
}
