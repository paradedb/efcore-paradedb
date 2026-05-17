using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Modifiers;
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
                EF.Functions.MatchDisjunction(p.Description, "running shoes", Pdb.Fuzzy(2))
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
                EF.Functions.MatchDisjunction(p.Description, searchTerm, Pdb.Fuzzy(2))
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
                    new[] { "running", "shoes" },
                    Pdb.Fuzzy(2)
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
                EF.Functions.MatchDisjunction(p.Description, searchTerms, Pdb.Fuzzy(2))
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
                EF.Functions.MatchDisjunction(p.Description, "running shoes", Pdb.Boost(2))
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
                EF.Functions.MatchDisjunction(p.Description, searchTerm, Pdb.Boost(2))
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
                    new[] { "running", "shoes" },
                    Pdb.Boost(2)
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
                EF.Functions.MatchDisjunction(p.Description, searchTerms, Pdb.Boost(2))
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
                EF.Functions.MatchDisjunction(p.Description, "running shoes", Pdb.Const(20.3f))
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
                EF.Functions.MatchDisjunction(p.Description, searchTerm, Pdb.Const(20.3f))
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
                    new[] { "running", "shoes" },
                    Pdb.Const(20.3f)
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
                EF.Functions.MatchDisjunction(p.Description, searchTerms, Pdb.Const(20.3f))
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
                    "running shoes",
                    Pdb.Fuzzy(2),
                    Pdb.Boost(3)
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
                EF.Functions.MatchDisjunction(p.Description, searchTerm, Pdb.Fuzzy(2), Pdb.Boost(3))
            )
            .ToQueryString();

        sql.ShouldMatch("""p\.description ||| @\w+::pdb\.fuzzy\(2\)::pdb\.boost\(3\)""");
    }

    [Test]
    [MethodDataSource(
        typeof(OperatorTestDataSources),
        nameof(OperatorTestDataSources.FuzzyBoostTestData)
    )]
    public void MatchDisjunction_WithVariableSearchTermAndModifierParameters_TranslatesToSql(
        Fuzzy fuzzy,
        Boost boost
    )
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p =>
                EF.Functions.MatchDisjunction(p.Description, searchTerm, fuzzy, boost)
            )
            .ToQueryString();

        sql.ShouldMatch(
            $"""p.description ||| @\w+::{Regex.Escape(fuzzy.ToString())}::{Regex.Escape(boost.ToString())}"""
        );
    }
}
