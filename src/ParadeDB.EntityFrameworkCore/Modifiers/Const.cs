namespace ParadeDB.EntityFrameworkCore.Modifiers;

public readonly struct Const
{
    private readonly float _value;

    internal Const(float value) => _value = value;

    public override string ToString() => $"pdb.const({_value})";
}
