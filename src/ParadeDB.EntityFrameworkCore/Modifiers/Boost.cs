namespace ParadeDB.EntityFrameworkCore.Modifiers;

public readonly struct Boost
{
    private readonly float _factor;

    internal Boost(float factor) => _factor = factor;

    public override string ToString() => $"pdb.boost({_factor})";
}
