using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Tests.TestUtils;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests.Translators;

public sealed class MatchDisjunctionTranslatorTests
{
    [Test]
    public void MatchDisjunction_WithInlineSearchTerm_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p => EF.Functions.MatchDisjunction(p.Description, "running shoes"))
            .ToQueryString();

        sql.ShouldContain("p.description ||| 'running shoes'");
    }

    [Test]
    public void MatchDisjunction_WithVariableSearchTerm_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p => EF.Functions.MatchDisjunction(p.Description, searchTerm))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ||| @\w+
            """
        );
    }

    [Test]
    public void MatchDisjunction_WithArrayVariable_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p => EF.Functions.MatchDisjunction(p.Description, searchTerms))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ||| @\w+
            """
        );
    }

    [Test]
    public void MatchDisjunction_WithInlineArray_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, new[] { "running", "shoes" })
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ||| ARRAY\['running','shoes'\]
            """
        );
    }

    [Test]
    public void MatchDisjunction_WithFuzzy_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, Pdb.Fuzzy("running shoes", 2))
            )
            .ToQueryString();

        sql.ShouldContain("p.description ||| 'running shoes'::pdb.fuzzy(2)");
    }

    [Test]
    public void MatchDisjunction_WithVariableSearchTermAndFuzzy_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, Pdb.Fuzzy(searchTerm, 2))
            )
            .ToQueryString();

        sql.ShouldMatch("""p\.description ||| @\w+::pdb\.fuzzy\(2\)""");
    }

    [Test]
    public void MatchDisjunction_WithInlineArrayAndFuzzy_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(
                    p.Description,
                    Pdb.Fuzzy(new[] { "running", "shoes" }, 2)
                )
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ||| ARRAY\['running','shoes'\]::pdb\.fuzzy\(2\)
            """
        );
    }

    [Test]
    public void MatchDisjunction_WithArrayVariableAndFuzzy_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, Pdb.Fuzzy(searchTerms, 2))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ||| @\w+::pdb\.fuzzy\(2\)
            """
        );
    }

    [Test]
    public void MatchDisjunction_WithBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, Pdb.Boost("running shoes", 2))
            )
            .ToQueryString();

        sql.ShouldContain("p.description ||| 'running shoes'::pdb.boost(2)");
    }

    [Test]
    public void MatchDisjunction_WithVariableSearchTermAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, Pdb.Boost(searchTerm, 2))
            )
            .ToQueryString();

        sql.ShouldMatch("""p\.description ||| @\w+::pdb\.boost\(2\)""");
    }

    [Test]
    public void MatchDisjunction_WithInlineArrayAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(
                    p.Description,
                    Pdb.Boost(new[] { "running", "shoes" }, 2)
                )
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ||| ARRAY\['running','shoes'\]::pdb\.boost\(2\)
            """
        );
    }

    [Test]
    public void MatchDisjunction_WithArrayVariableAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, Pdb.Boost(searchTerms, 2))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ||| @\w+::pdb\.boost\(2\)
            """
        );
    }

    [Test]
    public void MatchDisjunction_WithConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, Pdb.Const("running shoes", 20.3f))
            )
            .ToQueryString();

        sql.ShouldContain("p.description ||| 'running shoes'::pdb.const(20.3)");
    }

    [Test]
    public void MatchDisjunction_WithVariableSearchTermAndConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, Pdb.Const(searchTerm, 20.3f))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ||| @\w+::pdb\.const\(20\.3\)
            """
        );
    }

    [Test]
    public void MatchDisjunction_WithInlineArrayAndConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(
                    p.Description,
                    Pdb.Const(new[] { "running", "shoes" }, 20.3f)
                )
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ||| ARRAY\['running','shoes'\]::pdb\.const\(20\.3\)
            """
        );
    }

    [Test]
    public void MatchDisjunction_WithArrayVariableAndConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, Pdb.Const(searchTerms, 20.3f))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ||| @\w+::pdb\.const\(20\.3\)
            """
        );
    }

    [Test]
    public void MatchDisjunction_WithFuzzyAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(
                    p.Description,
                    Pdb.Boost(Pdb.Fuzzy("running shoes", 2), 3)
                )
            )
            .ToQueryString();

        sql.ShouldContain("p.description ||| 'running shoes'::pdb.fuzzy(2)::pdb.boost(3)");
    }

    [Test]
    public void MatchDisjunction_WithVariableSearchTermAndFuzzyAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, Pdb.Boost(Pdb.Fuzzy(searchTerm, 2), 3))
            )
            .ToQueryString();

        sql.ShouldMatch("""p\.description ||| @\w+::pdb\.fuzzy\(2\)::pdb\.boost\(3\)""");
    }

    [Test]
    public void MatchDisjunction_WithVariableSearchTermAndModifierParameters_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";
        int distance = 2;
        bool prefix = true;
        float factor = 3;

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(
                    p.Description,
                    Pdb.Boost(Pdb.Fuzzy(searchTerm, distance, prefix), factor)
                )
            )
            .ToQueryString();

        sql.ShouldMatch("""p.description ||| @\w+::pdb\.fuzzy\(2, t\)::pdb\.boost\(3\)""");
    }
}
