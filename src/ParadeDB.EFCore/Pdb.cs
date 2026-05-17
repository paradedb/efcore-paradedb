using ParadeDB.EFCore.Modifiers;

namespace ParadeDB.EFCore;

public static partial class Pdb
{
    public static Boost Boost(float factor) => new(factor);

    public static Fuzzy Fuzzy(
        int distance,
        bool prefix = false,
        bool transpositionCostOne = false
    ) => new(distance, prefix, transpositionCostOne);

    public static Const Const(float value) => new(value);

    public static Slop Slop(int value) => new(value);
}
