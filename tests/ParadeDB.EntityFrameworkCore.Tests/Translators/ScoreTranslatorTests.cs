using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Tests.TestUtils;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests.Translators;

public sealed class ScoreTranslatorTests
{
    [Test]
    public void Score_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Select(p => new { p.Id, Score = EF.Functions.Score(p.Id) })
            .ToQueryString();

        sql.ShouldContain("pdb.score(p.id)");
    }
}
