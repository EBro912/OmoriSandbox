using System;

namespace OmoriSandbox.Battle;

/// <summary>
/// A bonus that can be applied to an individual stat.
/// </summary>
public struct StatBonus
{
    /// <summary>
    /// Applies a set of bonuses to the given stats, one stat at a time in order.
    /// </summary>
    /// <param name="stats">A reference to the <see cref="Stats"/> to apply the bonuses to.</param>
    /// <param name="bonuses">The bonuses to apply.</param>
    public static void ApplyAll(ref Stats stats, StatBonus[] bonuses)
    {
        foreach (StatBonus bonus in bonuses)
            stats.SetStatExact(bonus.Type, stats.GetStatExact(bonus.Type) * bonus.Multiplier + bonus.FlatBonus);
    }

    /// <summary>
    /// The stat that this bonus modifies.
    /// </summary>
    public readonly StatType Type;
    /// <summary>
    /// The multiplier that is applied to the stat.
    /// </summary>
    public readonly float Multiplier;
    /// <summary>
    /// The addition that is applied to the stat.
    /// </summary>
    public readonly int FlatBonus;

    /// <summary>
    /// Creates a new multiplicative stat bonus.
    /// </summary>
    /// <param name="type">The stat that this bonus modifies.</param>
    /// <param name="multiplier">The multiplier to apply to the stat.</param>
    public StatBonus(StatType type, float multiplier)
    {
        Type = type;
        Multiplier = multiplier;
        FlatBonus = 0;
    }

    /// <summary>
    /// Creates a new flat additive stat bonus.
    /// </summary>
    /// <param name="type">The stat that this bonus modifies.</param>
    /// <param name="flatBonus">The addition to apply to the stat.</param>
    public StatBonus(StatType type, int flatBonus)
    {
        Type = type;
        FlatBonus = flatBonus;
        Multiplier = 1f;
    }

    /// <summary>
    /// Creates a stat bonus that has both a multiplicative and flat additive stat bonus.
    /// </summary>
    /// <remarks>
    /// Note: The addition takes place after the multiplication.
    /// </remarks>
    /// <param name="type">The stat that this bonus modifies.</param>
    /// <param name="multiplier">The multiplier to apply to the stat.</param>
    /// <param name="flatBonus">The addition to apply to the stat.</param>
    public StatBonus(StatType type, float multiplier, int flatBonus)
    {
        Type = type;
        Multiplier = multiplier;
        FlatBonus = flatBonus;
    }
}