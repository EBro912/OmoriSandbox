using System;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle;

/// <summary>
/// Event arguments for <see cref="BattleManager.BattleStarted"/>.
/// </summary>
public sealed class BattleStartedEventArgs : EventArgs
{
    /// <summary>
    /// The name of the preset the battle was started with.
    /// </summary>
    public string PresetName { get; }

    /// <summary>
    /// The Boss Rush stage index the battle started at, or -1 for normal battles.
    /// </summary>
    public int Stage { get; }

    internal BattleStartedEventArgs(string presetName, int stage)
    {
        PresetName = presetName;
        Stage = stage;
    }
}

/// <summary>
/// Event arguments for <see cref="BattleManager.BattleEnded"/>.
/// </summary>
public sealed class BattleEndedEventArgs : EventArgs
{
    /// <summary>
    /// Whether the party won the battle.
    /// </summary>
    public bool Victory { get; }

    internal BattleEndedEventArgs(bool victory)
    {
        Victory = victory;
    }
}

/// <summary>
/// Event arguments for <see cref="BattleManager.DamageDealt"/>.
/// </summary>
public sealed class DamageDealtEventArgs : EventArgs
{
    /// <summary>
    /// The attacker.
    /// </summary>
    public Actor Attacker { get; }

    /// <summary>
    /// The actor that took the damage.
    /// </summary>
    public Actor Target { get; }

    /// <summary>
    /// The HP damage dealt, after every modifier was applied. Always 0 for juice attacks.
    /// </summary>
    public int Damage { get; }

    /// <summary>
    /// The Juice the target lost, including the sad juice bleed for HP attacks, or the full amount for juice attacks.
    /// </summary>
    public int JuiceLost { get; }

    /// <summary>
    /// Whether the hit was a critical hit.
    /// </summary>
    public bool Critical { get; }

    /// <summary>
    /// True if this was a juice attack (see <see cref="BattleManager.DamageJuice"/>), false for a regular HP attack.
    /// </summary>
    public bool JuiceDamage { get; }

    internal DamageDealtEventArgs(Actor attacker, Actor target, int damage, int juiceLost, bool critical, bool juiceDamage)
    {
        Attacker = attacker;
        Target = target;
        Damage = damage;
        JuiceLost = juiceLost;
        Critical = critical;
        JuiceDamage = juiceDamage;
    }
}

/// <summary>
/// Event arguments for <see cref="BattleManager.Healed"/>.
/// </summary>
public sealed class HealedEventArgs : EventArgs
{
    /// <summary>
    /// The healer.
    /// </summary>
    public Actor Healer { get; }

    /// <summary>
    /// The actor that was healed.
    /// </summary>
    public Actor Target { get; }

    /// <summary>
    /// The amount of HEART or JUICE restored.
    /// </summary>
    public int Amount { get; }

    /// <summary>
    /// True if JUICE was restored (see <see cref="BattleManager.HealJuice"/>), false for HEART.
    /// </summary>
    public bool JuiceHeal { get; }

    internal HealedEventArgs(Actor healer, Actor target, int amount, bool juiceHeal)
    {
        Healer = healer;
        Target = target;
        Amount = amount;
        JuiceHeal = juiceHeal;
    }
}
