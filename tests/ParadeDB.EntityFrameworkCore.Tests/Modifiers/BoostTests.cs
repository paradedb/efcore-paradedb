using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests.Modifiers;

public sealed class BoostTests
{
    [Test]
    public void ToString_ProducesCorrectSql()
    {
        Pdb.Boost(2.5f).ToString().ShouldBe("pdb.boost(2.5)");
    }
}
