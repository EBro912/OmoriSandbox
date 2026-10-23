using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier OBSERVE puts on the enemy it locks, under the key <c>"ObserveIgnoreTaunt"</c>.
/// The enemy ignores taunts, and its single-target attacks on the party go to <see cref="ObservedBy"/>.
/// </summary>
/// <remarks>
/// It lasts for the enemy's next 2 actions. CHARM, ENCHANT and MESMERIZE remove it from every enemy.
/// The attacks are not redirected while AUBREY's spot holds a counter, or KEL's spot holds a taunt or a charm.
/// </remarks>
public sealed class ObserveStatModifier : StatModifier
{
    /// <summary>
    /// Creates the modifier.
    /// </summary>
    /// <param name="turns">How many of the enemy's actions it lasts.</param>
    public ObserveStatModifier(int turns) : base(turns) { }
    /// <summary>
    /// The <see cref="PartyMember"/> the enemy is locked onto.
    /// </summary>
    public PartyMember ObservedBy { get; set; }
}
