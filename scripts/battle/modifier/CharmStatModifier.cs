using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used by the Charm skill.
/// Forces the affected enemy to target a specific <see cref="PartyMember"/>.
/// </summary>
public sealed class CharmStatModifier : StatModifier
{
    public CharmStatModifier(int turns) : base(turns) { }
    /// <summary>
    /// The <see cref="PartyMember"/> that the enemy will target.
    /// </summary>
    public PartyMember CharmedBy { get; set; }
}