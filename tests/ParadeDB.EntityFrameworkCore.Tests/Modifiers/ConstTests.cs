using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests.Modifiers;

public sealed class ConstTests
{
    [Test]
    public void ToString_ProducesCorrectSql()
    {
        Pdb.Const(2.5f).ToString().ShouldBe("pdb.const(2.5)");
    }
}
