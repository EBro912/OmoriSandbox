using OmoriSandbox.Actors;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Battle;

/// <summary>
/// Describes the damage calculation a <see cref="StatModifier"/> is overriding. Passed to
/// <see cref="StatModifier.OverrideDamage(DamagePhase, ref float, DamageContext)"/>.
/// </summary>
/// <remarks>
/// One context is created for each <c>BattleManager.Damage</c> or <c>BattleManager.DamageJuice</c> call and
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
    /// Whether the damage is a critical hit. Always false during <see cref="DamagePhase.PreEmotion"/> and <see cref="DamagePhase.PreCrit"/>, and during <see cref="DamagePhase.PostApply"/> when a hit that isn't absorbed or reflected dealt no HEART damage (for <c>DamageJuice</c>, took no JUICE).
    /// </summary>
    public bool IsCritical { get; internal set; }

    /// <summary>
    /// The options the damage call was made with. See <see cref="DamageOptions"/>.
    /// </summary>
    /// <remarks>
    /// A call with the older <c>BattleManager.Damage</c> parameters gets the matching options. A reflected hit has its own options.
    /// </remarks>
    public DamageOptions Options { get; }

    /// <summary>
    /// Whether the attack can never miss.
    /// </summary>
    public bool NeverMiss => Options.NeverMiss;

    /// <summary>
    /// Whether the attack ignores emotion advantage.
    /// </summary>
    public bool IgnoreEmotion => Options.IgnoreEmotion;

    /// <summary>
    /// The emotion advantage of the hit: 1 for a moving attack, -1 for a dull attack, 0 otherwise.<br/>
    /// Always 0 during <see cref="DamagePhase.PreEmotion"/>, before it's calculated, and whenever <see cref="IgnoreEmotion"/> is set.
    /// </summary>
    public int Effectiveness { get; internal set; }

    /// <summary>
    /// The attack element passed to the damage call, such as <c>"exploit"</c>, or null when the attacker's emotion is used.
    /// </summary>
    public string AttackElement => Options.AttackElement;

    /// <summary>
    /// True for juice damage (<c>BattleManager.DamageJuice</c>), false for HP damage (<c>BattleManager.Damage</c>).
    /// </summary>
    public bool IsJuice { get; }

    /// <summary>
    /// What happens to the hit. See <see cref="DamageOutcome"/>.
    /// </summary>
    /// <remarks>
    /// Change it in <see cref="StatModifier.OverrideDamage(DamagePhase, ref float, DamageContext)"/> during any phase up to and including
    /// <see cref="DamagePhase.PreJuice"/>; later changes are ignored. It starts as <see cref="DamageOutcome.Absorb"/> or
    /// <see cref="DamageOutcome.Reflect"/> when one of the defender's modifiers was built with <see cref="StatModifier.WithAbsorb"/>
    /// or <see cref="StatModifier.WithReflect"/>.
    /// </remarks>
    public DamageOutcome Outcome { get; set; }

    /// <summary>
    /// Whether this hit was reflected from another hit. A reflected hit can't be reflected again.
    /// </summary>
    public bool IsReflected { get; }

    internal string Element { get; }

    internal DamageContext(Actor attacker, Actor defender, DamageOptions options, bool isJuice, string element, bool isReflected)
    {
        Attacker = attacker;
        Defender = defender;
        Options = options;
        IsJuice = isJuice;
        Element = element;
        IsReflected = isReflected;
    }
}
