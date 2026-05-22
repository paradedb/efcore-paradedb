using ParadeDB.EntityFrameworkCore.Tests.Persistence;

namespace ParadeDB.EntityFrameworkCore.Tests;

public class TestBase
{
    [ClassDataSource<DbFixture>(Shared = SharedType.PerTestSession)]
    public required DbFixture DbFixture { get; init; }
}
