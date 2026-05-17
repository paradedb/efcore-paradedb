namespace ParadeDB.EntityFrameworkCore.Modifiers;

public readonly struct Fuzzy
{
    private readonly int _distance;
    private readonly bool _prefix;
    private readonly bool _transpositionCostOne;

    internal Fuzzy(int distance, bool prefix, bool transpositionCostOne)
    {
        _distance = distance;
        _prefix = prefix;
        _transpositionCostOne = transpositionCostOne;
    }

    public override string ToString()
    {
        return (_prefix, _transpositionCostOne) switch
        {
            (false, false) => $"pdb.fuzzy({_distance})",
            (true, false) => $"pdb.fuzzy({_distance}, t)",
            (false, true) => $"pdb.fuzzy({_distance}, f, t)",
            (true, true) => $"pdb.fuzzy({_distance}, t, t)",
        };
    }
}
