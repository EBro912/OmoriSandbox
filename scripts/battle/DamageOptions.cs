using System;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle;

/// <summary>
/// The settings of one <see cref="BattleManager.Damage(Actor, Actor, Func{float}, DamageOptions)"/> or
/// <see cref="BattleManager.DamageJuice(Actor, Actor, Func{float}, DamageOptions)"/> call.
/// </summary>
/// <remarks>
/// Every setting defaults to what a plain damage call does, so set only the ones you need, for example
/// <c>new DamageOptions { NeverMiss = false, Crit = CritMode.Never }</c> or <c>DamageOptions.Default with { Variance = 0f }</c>.
/// </remarks>
public sealed record DamageOptions
{
    /// <summary>
    /// The default settings, the same as <c>new DamageOptions()</c>.
    /// </summary>
    public static DamageOptions Default { get; } = new();

    /// <summary>
    /// The default settings, but with <c>NeverMiss</c> set to <c>false</c>/
    /// </summary>
    public static DamageOptions DefaultCanMiss { get; } = new() { NeverMiss = false };
    
    /// <summary>
    /// If the attack can never miss. Defaults to true.
    /// </summary>
    /// <remarks>
    /// When false, the hit is rolled first, like <see cref="BattleManager.RollHit"/>, and modifiers such as FLEX treat the attack as one that can miss.
    /// </remarks>
    public bool NeverMiss { get; init; } = true;

    /// <summary>
    /// The damage variance. Damage will be multiplied between (1 - variance) and (1 + variance). Defaults to 0.2.
    /// </summary>
    public float Variance { get; init; } = 0.2f;

    /// <summary>
    /// Whether the attack can be a critical hit. Defaults to <see cref="CritMode.Roll"/>.
    /// </summary>
    public CritMode Crit { get; init; } = CritMode.Roll;

    /// <summary>
    /// What a critical hit multiplies the damage by, before its flat +1.5. Defaults to 1.5.
    /// </summary>
    public double CritMultiplier { get; init; } = 1.5;

    /// <summary>
    /// If the attack ignores emotion advantage.
    /// </summary>
    public bool IgnoreEmotion { get; init; }

    /// <summary>
    /// Overrides the attacker's emotion "element" in the advantage calculation.
    /// </summary>
    /// <remarks>
    /// Currently only <c>"exploit"</c> is supported, used by Perfectheart's EXPLOIT to always deal advantage damage.
    /// </remarks>
    public string AttackElement { get; init; }

    /// <summary>
    /// If the attack should not log anything to the BattleLog. Damage numbers will still display.
    /// </summary>
    public bool Silent { get; init; }

    /// <summary>
    /// If a SAD target's JUICE loss shows its battle log line and damage number. Defaults to true.
    /// </summary>
    /// <remarks>
    /// The JUICE is lost either way. Juice damage ignores this setting.
    /// </remarks>
    public bool ShowJuiceLoss { get; init; } = true;

    /// <summary>
    /// Runs once the hit has landed, before its sound and battle log lines.
    /// </summary>
    public Action OnHit { get; init; }
}
