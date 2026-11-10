using System.Collections.Generic;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The stat changes one tier of a <see cref="TierStatModifier"/> gives.
/// </summary>
public sealed class StatTier
{
    /// <summary>
    /// The stat changes of this tier.
    /// </summary>
    public IReadOnlyList<StatBonus> Bonuses { get; }

    /// <summary>
    /// Creates a tier that gives all of these stat changes. Rates multiply and flat bonuses add, as in a <see cref="StatModifier"/>.
    /// </summary>
    /// <param name="bonuses">The stat changes of this tier. A tier with none changes no stat.</param>
    public StatTier(params StatBonus[] bonuses)
    {
        Bonuses = bonuses;
    }
}
