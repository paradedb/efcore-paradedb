using Microsoft.EntityFrameworkCore;
using ParadeDB.EFCore.Extensions;
using ParadeDB.EFCore.Tests.TestUtils;
using Shouldly;

namespace ParadeDB.EFCore.Tests.Translators;

public sealed class ProximityTranslatorTests
{
    [Test]
    public void Proximity_WithInlineArguments_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(p.Description, Pdb.Proximity("sleek").Within(1, "shoes"))
            )
            .ToQueryString();

        sql.ShouldContain("p.description @@@ (('sleek' ## 1) ## 'shoes')");
    }

    [Test]
    public void Proximity_WithInlineArgumentsAndOrdered_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(p.Description, Pdb.Proximity("sleek").WithinOrdered(1, "shoes"))
            )
            .ToQueryString();

        sql.ShouldContain("p.description @@@ (('sleek' ##> 1) ##> 'shoes')");
    }

    [Test]
    public void Proximity_WithVariableArguments_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string left = "sleek";
        string right = "shoes";
        int distance = 1;

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(p.Description, Pdb.Proximity(left).Within(distance, right))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description @@@ \(\(@\w+ ## @\w+\) ## @\w+\)
            """
        );
    }

    [Test]
    public void Proximity_WithVariableArgumentsAndOrdered_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string left = "sleek";
        string right = "shoes";
        int distance = 1;

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(
                    p.Description,
                    Pdb.Proximity(left).WithinOrdered(distance, right)
                )
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description @@@ \(\(@\w+ ##> @\w+\) ##> @\w+\)
            """
        );
    }

    [Test]
    public void Proximity_WithRegexOnLeft_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(p.Description, Pdb.ProximityRegex("sl.*").Within(1, "shoes"))
            )
            .ToQueryString();

        sql.ShouldContain("p.description @@@ ((pdb.prox_regex('sl.*') ## 1) ## 'shoes')");
    }

    [Test]
    public void Proximity_WithRegexOnRight_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(
                    p.Description,
                    Pdb.Proximity("shoes").Within(1, Pdb.ProximityRegex("sl.*"))
                )
            )
            .ToQueryString();

        sql.ShouldContain("p.description @@@ (('shoes' ## 1) ## pdb.prox_regex('sl.*'))");
    }

    [Test]
    public void Proximity_WithRegexAndVariablePattern_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string pattern = "sl.*";

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(p.Description, Pdb.ProximityRegex(pattern).Within(1, "shoes"))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description @@@ \(\(pdb\.prox_regex\(@\w+\) ## 1\) ## 'shoes'\)
            """
        );
    }

    [Test]
    public void Proximity_WithRegexAndMaxExpansions_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(
                    p.Description,
                    Pdb.ProximityRegex("sl.*", 100).Within(1, "shoes")
                )
            )
            .ToQueryString();

        sql.ShouldContain("p.description @@@ ((pdb.prox_regex('sl.*', 100) ## 1) ## 'shoes')");
    }

    [Test]
    public void Proximity_WithRegexAndVariableMaxExpansions_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string pattern = "sl.*";
        int max = 100;

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(
                    p.Description,
                    Pdb.ProximityRegex(pattern, max).Within(1, "shoes")
                )
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description @@@ \(\(pdb\.prox_regex\(@\w+, @\w+\) ## 1\) ## 'shoes'\)
            """
        );
    }

    [Test]
    public void Proximity_WithArrayOfTokens_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(
                    p.Description,
                    Pdb.ProximityArray("sleek", "white").Within(1, "shoes")
                )
            )
            .ToQueryString();

        sql.ShouldContain("p.description @@@ ((pdb.prox_array('sleek', 'white') ## 1) ## 'shoes')");
    }

    [Test]
    public void Proximity_WithArrayOfVariableTokens_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string t1 = "sleek";
        string t2 = "white";

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(p.Description, Pdb.ProximityArray(t1, t2).Within(1, "shoes"))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description @@@ \(\(pdb\.prox_array\(@\w+, @\w+\) ## 1\) ## 'shoes'\)
            """
        );
    }

    [Test]
    public void Proximity_WithArrayOfMixedOperands_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(
                    p.Description,
                    Pdb.ProximityArray(Pdb.ProximityRegex("sl.*"), Pdb.ProximityArray("white"))
                        .Within(1, "shoes")
                )
            )
            .ToQueryString();

        sql.ShouldContain(
            "p.description @@@ ((pdb.prox_array(pdb.prox_regex('sl.*'), pdb.prox_array('white')) ## 1) ## 'shoes')"
        );
    }

    [Test]
    public void Proximity_WithChainedThreeTerms_TranslatesToSql()
    {
        using var context = new TestDbContext();

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(
                    p.Description,
                    Pdb.Proximity("sleek").Within(1, "running").Within(2, "shoes")
                )
            )
            .ToQueryString();

        sql.ShouldContain("p.description @@@ (((('sleek' ## 1) ## 'running') ## 2) ## 'shoes')");
    }

    [Test]
    public void Proximity_WithChainedVariableArguments_TranslatesToSql()
    {
        using var context = new TestDbContext();

        string t1 = "sleek";
        string t2 = "running";
        string t3 = "shoes";
        int d1 = 1;
        int d2 = 2;

        var sql = context
            .Products.Where(p =>
                EF.Functions.Match(p.Description, Pdb.Proximity(t1).Within(d1, t2).Within(d2, t3))
            )
            .ToQueryString();

        sql.ShouldMatch(
            """
            p\.description @@@ \(\(\(\(@\w+ ## @\w+\) ## @\w+\) ## @\w+\) ## @\w+\)
            """
        );
    }
}
