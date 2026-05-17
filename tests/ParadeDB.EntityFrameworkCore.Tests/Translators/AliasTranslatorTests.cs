using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Tests.TestUtils;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests.Translators;

public sealed class AliasTranslatorTests
{
    [Test]
    public void Alias_WithInlineAlias_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Select(p => EF.Functions.Alias(p.Description, "description_simple"))
            .ToQueryString();

        sql.ShouldContain("p.description::pdb.alias('description_simple')");
    }

    [Test]
    public void Alias_WithVariableAlias_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string aliasName = "description_simple";

        var sql = context
            .Products.Select(p => EF.Functions.Alias(p.Description, aliasName))
            .ToQueryString();

        sql.ShouldContain("p.description::pdb.alias('description_simple')");
    }
}
