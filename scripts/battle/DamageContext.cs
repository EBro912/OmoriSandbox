using OmoriSandbox.Actors;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Battle;

/// <summary>
/// Describes the damage calculation a <see cref="StatModifier"/> is overriding. Passed to
/// <see cref="StatModifier.OverrideDamage(DamagePhase, ref float, DamageContext)"/>.
/// </summary>
/// <remarks>
/// One context is created for each <see cref="BattleManager.Damage"/> or <see cref="BattleManager.DamageJuice"/> call and
/// shared by every phase and modifier of that call, so read its values during the call instead of keeping it.
/// </remarks>
public sealed class DamageContext
{
    /// <summary>
    /// The attacker.
    /// </summary>
    public Actor Attacker { get; }

    /// <summary>
    /// The target/defender.
    /// </summary>
    public Actor Defender { get; }

    /// <summary>
    /// Whether the modifier currently being called belongs to the <see cref="Attacker"/>.
    /// False while the <see cref="Defender"/>'s modifiers are called.
    /// </summary>
    public bool IsAttacking { get; internal set; }

    /// <summary>
    /// Whether the damage is a critical hit. Always false during <see cref="DamagePhase.PreEmotion"/> and <see cref="DamagePhase.PreCrit"/>.
    /// </summary>
    public bool IsCritical { get; internal set; }

    /// <summary>
    /// Whether the attack can never miss.
    /// </summary>
    public bool NeverMiss { get; }

    /// <summary>
    /// Whether the attack ignores emotion advantage.
    /// </summary>
    public bool IgnoreEmotion { get; }

    /// <summary>
    /// The emotion advantage of the hit: 1 for a moving attack, -1 for a dull attack, 0 otherwise.<br/>
    /// Always 0 during <see cref="DamagePhase.PreEmotion"/>, before it's calculated, and whenever <see cref="IgnoreEmotion"/> is set.
    /// </summary>
    public int Effectiveness { get; internal set; }

    /// <summary>
    /// The attack element passed to the damage call, such as <c>"exploit"</c>, or null when the attacker's emotion is used.
    /// </summary>
    public string AttackElement { get; }

    /// <summary>
    /// True for juice damage (<see cref="BattleManager.DamageJuice"/>), false for HP damage (<see cref="BattleManager.Damage"/>).
    /// </summary>
    public bool IsJuice { get; }

    internal DamageContext(Actor attacker, Actor defender, bool neverMiss, bool ignoreEmotion, string attackElement, bool isJuice)
    {
        Attacker = attacker;
        Defender = defender;
        NeverMiss = neverMiss;
        IgnoreEmotion = ignoreEmotion;
        AttackElement = attackElement;
        IsJuice = isJuice;
    }
}
