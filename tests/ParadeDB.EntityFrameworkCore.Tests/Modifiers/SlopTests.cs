using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests.Modifiers;

public sealed class SlopTests
{
    [Test]
    public void ToString_ProducesCorrectSql()
    {
        Pdb.Slop(3).ToString().ShouldBe("pdb.slop(3)");
    }
}
