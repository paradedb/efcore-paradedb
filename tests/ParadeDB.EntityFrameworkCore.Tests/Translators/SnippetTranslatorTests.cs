using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Tests.TestUtils;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests.Translators;

public sealed class SnippetTranslatorTests
{
    [Test]
    public void Snippet_WithDefaultArguments_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context.Products.Select(p => EF.Functions.Snippet(p.Description)).ToQueryString();

        sql.ShouldContain("pdb.snippet(p.description)");
    }

    [Test]
    public void Snippet_WithInlineMaxNumChars_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Select(p => EF.Functions.Snippet(p.Description, 70))
            .ToQueryString();

        sql.ShouldContain("pdb.snippet(p.description, '<b>', '</b>', 70)");
    }

    [Test]
    public void Snippet_WithVariableMaxNumChars_TranslatesToSql()
    {
        using var context = new TestDbContext();

        int maxNumChars = 70;

        var sql = context
            .Products.Select(p => EF.Functions.Snippet(p.Description, maxNumChars))
            .ToQueryString();

        sql.ShouldMatch(
            """
            pdb\.snippet\(p\.description, '<b>', '</b>', @\w+\)
            """
        );
    }

    [Test]
    public void Snippet_WithInlineTags_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Select(p => EF.Functions.Snippet(p.Description, "<a>", "</a>"))
            .ToQueryString();

        sql.ShouldContain("pdb.snippet(p.description, '<a>', '</a>')");
    }

    [Test]
    public void Snippet_WithVariableTags_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string startTag = "<a>";
        string endTag = "</a>";

        var sql = context
            .Products.Select(p => EF.Functions.Snippet(p.Description, startTag, endTag))
            .ToQueryString();

        sql.ShouldMatch(
            """
            pdb\.snippet\(p\.description, @\w+, @\w+\)
            """
        );
    }

    [Test]
    public void Snippet_WithInlineTagsAndMaxNumChars_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Select(p => EF.Functions.Snippet(p.Description, "<div>", "</div>", 235))
            .ToQueryString();

        sql.ShouldContain("pdb.snippet(p.description, '<div>', '</div>', 235)");
    }

    [Test]
    public void Snippet_WithVariableTagsAndMaxNumChars_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string startTag = "<div>";
        string endTag = "</div>";
        int maxNumChars = 235;

        var sql = context
            .Products.Select(p =>
                EF.Functions.Snippet(p.Description, startTag, endTag, maxNumChars)
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            pdb\.snippet\(p\.description, @\w+, @\w+, @\w+\)
            """
        );
    }
}
