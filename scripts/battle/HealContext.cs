using OmoriSandbox.Actors;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Battle;

/// <summary>
/// A heal event that a <see cref="StatModifier"/> is overriding. Passed to
/// <see cref="StatModifier.OverrideHeal(DamagePhase, ref float, HealContext)"/>.
/// </summary>
/// <remarks>
/// One context is created for each <c>BattleManager.Heal</c> or <c>BattleManager.HealJuice</c> call and
/// shared by every phase and modifier of that call, so read its values during the call instead of keeping it.
/// </remarks>
public sealed class HealContext
{
    /// <summary>
    /// The healer.
    /// </summary>
    public Actor Healer { get; }

    /// <summary>
    /// The actor being healed.
    /// </summary>
    public Actor Target { get; }

    /// <summary>
    /// Whether the modifier currently being called belongs to the <see cref="Healer"/>.
    /// False while the <see cref="Target"/>'s modifiers are called.
    /// </summary>
    public bool IsHealer { get; internal set; }

    /// <summary>
    /// True for JUICE (<c>BattleManager.HealJuice</c>), false for HEART (<c>BattleManager.Heal</c>).
    /// </summary>
    public bool IsJuice { get; }

    internal HealContext(Actor healer, Actor target, bool isJuice)
    {
        Healer = healer;
        Target = target;
        IsJuice = isJuice;
    }
}
