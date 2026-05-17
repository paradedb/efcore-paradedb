using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Modifiers;
using ParadeDB.EntityFrameworkCore.Tests.TestUtils;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests.Translators;

public sealed class MatchConjunctionTranslatorTests
{
    [Test]
    public void MatchConjunction_WithInlineSearchTerm_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p => EF.Functions.MatchConjunction(p.Description, "running shoes"))
            .ToQueryString();

        sql.ShouldContain("p.description &&& 'running shoes'");
    }

    [Test]
    public void MatchConjunction_WithVariableSearchTerm_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p => EF.Functions.MatchConjunction(p.Description, searchTerm))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description &&& @\w+
            """
        );
    }

    [Test]
    public void MatchConjunction_WithArrayVariable_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p => EF.Functions.MatchConjunction(p.Description, searchTerms))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description &&& @\w+
            """
        );
    }

    [Test]
    public void MatchConjunction_WithInlineArray_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(p.Description, new[] { "running", "shoes" })
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description &&& ARRAY\['running','shoes'\]
            """
        );
    }

    [Test]
    public void MatchConjunction_WithFuzzy_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(p.Description, "running shoes", Pdb.Fuzzy(2))
            )
            .ToQueryString();

        sql.ShouldContain("p.description &&& 'running shoes'::pdb.fuzzy(2)");
    }

    [Test]
    public void MatchConjunction_WithVariableSearchTermAndFuzzy_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(p.Description, searchTerm, Pdb.Fuzzy(2))
            )
            .ToQueryString();

        sql.ShouldMatch("""p\.description &&& @\w+::pdb\.fuzzy\(2\)""");
    }

    [Test]
    public void MatchConjunction_WithInlineArrayAndFuzzy_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(
                    p.Description,
                    new[] { "running", "shoes" },
                    Pdb.Fuzzy(2)
                )
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description &&& ARRAY\['running','shoes'\]::text\[\]::pdb\.fuzzy\(2\)
            """
        );
    }

    [Test]
    public void MatchConjunction_WithArrayVariableAndFuzzy_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(p.Description, searchTerms, Pdb.Fuzzy(2))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description &&& @\w+::pdb\.fuzzy\(2\)
            """
        );
    }

    [Test]
    public void MatchConjunction_WithBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(p.Description, "running shoes", Pdb.Boost(2))
            )
            .ToQueryString();

        sql.ShouldContain("p.description &&& 'running shoes'::pdb.boost(2)");
    }

    [Test]
    public void MatchConjunction_WithVariableSearchTermAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(p.Description, searchTerm, Pdb.Boost(2))
            )
            .ToQueryString();

        sql.ShouldMatch("""p\.description &&& @\w+::pdb\.boost\(2\)""");
    }

    [Test]
    public void MatchConjunction_WithInlineArrayAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(
                    p.Description,
                    new[] { "running", "shoes" },
                    Pdb.Boost(2)
                )
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description &&& ARRAY\['running','shoes'\]::text\[\]::pdb\.boost\(2\)
            """
        );
    }

    [Test]
    public void MatchConjunction_WithArrayVariableAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(p.Description, searchTerms, Pdb.Boost(2))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description &&& @\w+::pdb\.boost\(2\)
            """
        );
    }

    [Test]
    public void MatchConjunction_WithConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(p.Description, "running shoes", Pdb.Const(20.3f))
            )
            .ToQueryString();

        sql.ShouldContain("p.description &&& 'running shoes'::pdb.const(20.3)");
    }

    [Test]
    public void MatchConjunction_WithVariableSearchTermAndConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(p.Description, searchTerm, Pdb.Const(20.3f))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description &&& @\w+::pdb\.const\(20\.3\)
            """
        );
    }

    [Test]
    public void MatchConjunction_WithInlineArrayAndConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(
                    p.Description,
                    new[] { "running", "shoes" },
                    Pdb.Const(20.3f)
                )
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description &&& ARRAY\['running','shoes'\]::text\[\]::pdb\.const\(20\.3\)
            """
        );
    }

    [Test]
    public void MatchConjunction_WithArrayVariableAndConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(p.Description, searchTerms, Pdb.Const(20.3f))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description &&& @\w+::pdb\.const\(20\.3\)
            """
        );
    }

    [Test]
    public void MatchConjunction_WithFuzzyAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(
                    p.Description,
                    "running shoes",
                    Pdb.Fuzzy(2),
                    Pdb.Boost(3)
                )
            )
            .ToQueryString();

        sql.ShouldContain("p.description &&& 'running shoes'::pdb.fuzzy(2)::pdb.boost(3)");
    }

    [Test]
    public void MatchConjunction_WithVariableSearchTermAndFuzzyAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(p.Description, searchTerm, Pdb.Fuzzy(2), Pdb.Boost(3))
            )
            .ToQueryString();

        sql.ShouldMatch("""p\.description &&& @\w+::pdb\.fuzzy\(2\)::pdb\.boost\(3\)""");
    }

    [Test]
    [MethodDataSource(
        typeof(OperatorTestDataSources),
        nameof(OperatorTestDataSources.FuzzyBoostTestData)
    )]
    public void MatchConjunction_WithVariableSearchTermAndModifierParameters_TranslatesToSql(
        Fuzzy fuzzy,
        Boost boost
    )
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchConjunction(p.Description, searchTerm, fuzzy, boost)
            )
            .ToQueryString();

        sql.ShouldMatch(
            $"""p.description &&& @\w+::{Regex.Escape(fuzzy.ToString())}::{Regex.Escape(boost.ToString())}"""
        );
    }
}
