using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests.Modifiers;

public sealed class FuzzyTests
{
    [Test]
    public void ToString_DistanceOnly_ProducesCorrectSql()
    {
        Pdb.Fuzzy(2).ToString().ShouldBe("pdb.fuzzy(2)");
    }

    [Test]
    public void ToString_WithPrefix_ProducesCorrectSql()
    {
        Pdb.Fuzzy(2, true).ToString().ShouldBe("pdb.fuzzy(2, t)");
    }

    [Test]
    public void ToString_WithTranspositionCostOne_ProducesCorrectSql()
    {
        Pdb.Fuzzy(2, false, true).ToString().ShouldBe("pdb.fuzzy(2, f, t)");
    }

    [Test]
    public void ToString_WithBothBooleans_ProducesCorrectSql()
    {
        Pdb.Fuzzy(2, true, true).ToString().ShouldBe("pdb.fuzzy(2, t, t)");
    }
}
