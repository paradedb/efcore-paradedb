using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Tests.TestUtils;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests.Translators;

public sealed class TermTranslatorTests
{
    [Test]
    public void Term_WithInlineSearchTerm_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p => EF.Functions.Term(p.Description, "running shoes"))
            .ToQueryString();

        sql.ShouldContain("p.description === 'running shoes'");
    }

    [Test]
    public void Term_WithVariableSearchTerm_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p => EF.Functions.Term(p.Description, searchTerm))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description === @\w+
            """
        );
    }

    [Test]
    public void Term_WithArrayVariable_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p => EF.Functions.Term(p.Description, searchTerms))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description === @\w+
            """
        );
    }

    [Test]
    public void Term_WithInlineArray_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p => EF.Functions.Term(p.Description, new[] { "running", "shoes" }))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description === ARRAY\['running','shoes'\]
            """
        );
    }

    [Test]
    public void Term_WithFuzzy_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Fuzzy("running shoes", 2)))
            .ToQueryString();

        sql.ShouldContain("p.description === 'running shoes'::pdb.fuzzy(2)");
    }

    [Test]
    public void Term_WithVariableSearchTermAndFuzzy_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Fuzzy(searchTerm, 2)))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description === @\w+::pdb\.fuzzy\(2\)
            """
        );
    }

    [Test]
    public void Term_WithArrayVariableAndFuzzy_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Fuzzy(searchTerms, 2)))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description === @\w+::pdb\.fuzzy\(2\)
            """
        );
    }

    [Test]
    public void Term_WithInlineArrayAndFuzzy_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Term(p.Description, Pdb.Fuzzy(new[] { "running", "shoes" }, 2))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description === ARRAY\['running','shoes'\]::text\[\]::pdb\.fuzzy\(2\)
            """
        );
    }

    [Test]
    public void Term_WithBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Boost("running shoes", 2)))
            .ToQueryString();

        sql.ShouldContain("p.description === 'running shoes'::pdb.boost(2)");
    }

    [Test]
    public void Term_WithVariableSearchTermAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Boost(searchTerm, 2)))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description === @\w+::pdb\.boost\(2\)
            """
        );
    }

    [Test]
    public void Term_WithArrayVariableAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Boost(searchTerms, 2)))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description === @\w+::pdb\.boost\(2\)
            """
        );
    }

    [Test]
    public void Term_WithInlineArrayAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Term(p.Description, Pdb.Boost(new[] { "running", "shoes" }, 2))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description === ARRAY\['running','shoes'\]::text\[\]::pdb\.boost\(2\)
            """
        );
    }

    [Test]
    public void Term_WithConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Term(p.Description, Pdb.Const("running shoes", 20.3f))
            )
            .ToQueryString();

        sql.ShouldContain("p.description === 'running shoes'::pdb.const(20.3)");
    }

    [Test]
    public void Term_WithVariableSearchTermAndConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Const(searchTerm, 20.3f)))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description === @\w+::pdb\.const\(20\.3\)
            """
        );
    }

    [Test]
    public void Term_WithArrayVariableAndConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p => EF.Functions.Term(p.Description, Pdb.Const(searchTerms, 20.3f)))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description === @\w+::pdb\.const\(20\.3\)
            """
        );
    }

    [Test]
    public void Term_WithInlineArrayAndConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Term(p.Description, Pdb.Const(new[] { "running", "shoes" }, 20.3f))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description === ARRAY\['running','shoes'\]::text\[\]::pdb\.const\(20\.3\)
            """
        );
    }

    [Test]
    public void Term_WithFuzzyAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Term(p.Description, Pdb.Boost(Pdb.Fuzzy("running shoes", 1), 3))
            )
            .ToQueryString();

        sql.ShouldContain("p.description === 'running shoes'::pdb.fuzzy(1)::pdb.boost(3)");
    }

    [Test]
    public void Term_WithVariableSearchTermAndFuzzyAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p =>
                EF.Functions.Term(p.Description, Pdb.Boost(Pdb.Fuzzy(searchTerm, 1), 3))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description === @\w+::pdb\.fuzzy\(1\)::pdb\.boost\(3\)
            """
        );
    }

    [Test]
    public void Term_WithVariableSearchTermAndModifierParameters_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";
        int distance = 2;
        bool prefix = true;
        float factor = 3;

        var pattern = $"""
            p\.description === @\w+::pdb\.fuzzy\(2, t\)::pdb\.boost\(3\)
            """;

        var sql = context
            .Products.Where(p =>
                EF.Functions.Term(
                    p.Description,
                    Pdb.Boost(Pdb.Fuzzy(searchTerm, distance, prefix), factor)
                )
            )
            .ToQueryString();

        sql.ShouldMatch(pattern);
    }
}
