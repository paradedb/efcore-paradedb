using ParadeDB.EntityFrameworkCore.IntegrationTests.Persistence;

namespace ParadeDB.EntityFrameworkCore.IntegrationTests;

public class TestBase
{
    [ClassDataSource<DbFixture>(Shared = SharedType.PerTestSession)]
    public required DbFixture DbFixture { get; init; }
}
