using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Tests.TestUtils;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests.Translators;

public sealed class PhraseTranslatorTests
{
    [Test]
    public void Phrase_WithInlineSearchTerm_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p => EF.Functions.Phrase(p.Description, "running shoes"))
            .ToQueryString();

        sql.ShouldContain("p.description ### 'running shoes'");
    }

    [Test]
    public void Phrase_WithVariableSearchTerm_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p => EF.Functions.Phrase(p.Description, searchTerm))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ### @\w+
            """
        );
    }

    [Test]
    public void Phrase_WithArrayVariable_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p => EF.Functions.Phrase(p.Description, searchTerms))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ### @\w+
            """
        );
    }

    [Test]
    public void Phrase_WithInlineArray_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p => EF.Functions.Phrase(p.Description, new[] { "running", "shoes" }))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ### ARRAY\['running','shoes'\]
            """
        );
    }

    [Test]
    public void Phrase_WithBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Boost("running shoes", 2)))
            .ToQueryString();

        sql.ShouldContain("p.description ### 'running shoes'::pdb.boost(2)");
    }

    [Test]
    public void Phrase_WithVariableSearchTermAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Boost(searchTerm, 2)))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ### @\w+::pdb\.boost\(2\)
            """
        );
    }

    [Test]
    public void Phrase_WithArrayVariableAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Boost(searchTerms, 2)))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ### @\w+::pdb\.boost\(2\)
            """
        );
    }

    [Test]
    public void Phrase_WithInlineArrayAndBoost_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Phrase(p.Description, Pdb.Boost(new[] { "running", "shoes" }, 2))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ### ARRAY\['running','shoes'\]::text\[\]::pdb\.boost\(2\)
            """
        );
    }

    [Test]
    public void Phrase_WithConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Phrase(p.Description, Pdb.Const("running shoes", 20.3f))
            )
            .ToQueryString();

        sql.ShouldContain("p.description ### 'running shoes'::pdb.const(20.3)");
    }

    [Test]
    public void Phrase_WithVariableSearchTermAndConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Const(searchTerm, 20.3f)))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ### @\w+::pdb\.const\(20\.3\)
            """
        );
    }

    [Test]
    public void Phrase_WithArrayVariableAndConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Const(searchTerms, 20.3f)))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ### @\w+::pdb\.const\(20\.3\)
            """
        );
    }

    [Test]
    public void Phrase_WithInlineArrayAndConst_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Phrase(p.Description, Pdb.Const(new[] { "running", "shoes" }, 20.3f))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ### ARRAY\['running','shoes'\]::text\[\]::pdb\.const\(20\.3\)
            """
        );
    }

    [Test]
    public void Phrase_WithSlop_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Slop("running shoes", 2)))
            .ToQueryString();

        sql.ShouldContain("p.description ### 'running shoes'::pdb.slop(2)");
    }

    [Test]
    public void Phrase_WithVariableSearchTermAndSlop_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string searchTerm = "running shoes";

        var sql = context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Slop(searchTerm, 2)))
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description ### @\w+::pdb\.slop\(2\)
            """
        );
    }

    [Test]
    public void Phrase_WithArrayVariableAndSlop_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string[] searchTerms = ["running", "shoes"];

        var sql = context
            .Products.Where(p => EF.Functions.Phrase(p.Description, Pdb.Slop(searchTerms, 2)))
            .ToQueryString();

        sql.ShouldMatch("""p\.description ### @\w+::pdb\.slop\(2\)""");
    }

    [Test]
    public void Phrase_WithInlineArrayAndSlop_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Phrase(p.Description, Pdb.Slop(new[] { "running", "shoes" }, 2))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """p\.description ### ARRAY\['running','shoes'\]::text\[\]::pdb\.slop\(2\)"""
        );
    }
}
