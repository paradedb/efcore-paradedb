using ParadeDB.EntityFrameworkCore.Modifiers;

namespace ParadeDB.EntityFrameworkCore.Tests.TestUtils;

public static class OperatorTestDataSources
{
    public static IEnumerable<Func<(Fuzzy, Boost)>> FuzzyBoostTestData()
    {
        yield return () => (Pdb.Fuzzy(0), Pdb.Boost(3));
        yield return () => (Pdb.Fuzzy(0, false), Pdb.Boost(2.3f));
        yield return () => (Pdb.Fuzzy(1, true), Pdb.Boost(2.3f));
        yield return () => (Pdb.Fuzzy(1, false, false), Pdb.Boost(2.5f));
        yield return () => (Pdb.Fuzzy(2, true, false), Pdb.Boost(2.5f));
        yield return () => (Pdb.Fuzzy(2, false, true), Pdb.Boost(2.5f));
        yield return () => (Pdb.Fuzzy(2, true, true), Pdb.Boost(2.5f));
    }
}
