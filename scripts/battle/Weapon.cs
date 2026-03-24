using System;

namespace OmoriSandbox.Battle;

/// <summary>
/// A weapon that can be equipped by a <see cref="Actors.PartyMember"/>
/// </summary>
/// <param name="name">The name of the weapon.</param>
/// <param name="stats">The stats that this weapon provides.</param>
public readonly struct Weapon(string name, params StatBonus[] stats)
{
    /// <summary>
    /// The name of the weapon.
    /// </summary>
    public string Name { get; init; } = name;
    /// <summary>
    /// The stats that this weapon provides.
    /// </summary>
    public StatBonus[] Stats { get; init; } = stats;

    /// <summary>
    /// Applies this Weapon's stat bonuses to the provided <see cref="Stats"/>.
    /// </summary>
    /// <param name="stats">A reference to the <see cref="Stats"/> to modify.</param>
    public void Apply(ref Stats stats)
    {
        foreach (StatBonus bonus in Stats)
        {
            int stat = stats.GetStat(bonus.Type);
            stat = (int)Math.Round(stat * bonus.Multiplier + bonus.FlatBonus);
            stats.SetStat(bonus.Type, stat);
        }
    }
}