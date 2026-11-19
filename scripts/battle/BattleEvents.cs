using System;
using System.Collections.Generic;
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
    /// How the battle ended.
    /// </summary>
    public BattleOutcome Outcome { get; }

    /// <summary>
    /// Whether the party won the battle.
    /// </summary>
    public bool Victory => Outcome == BattleOutcome.Victory;

    internal BattleEndedEventArgs(BattleOutcome outcome)
    {
        Outcome = outcome;
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
    /// The Juice the target lost, including the sad juice bleed for HP attacks. For juice attacks, it's the JUICE the target lost, never more than the target had.
    /// </summary>
    public int JuiceLost { get; }

    /// <summary>
    /// Whether the hit was a critical hit. False when the hit dealt no damage: no HP for an HP attack, no Juice for a juice attack.
    /// </summary>
    public bool Critical { get; }

    /// <summary>
    /// True if this was a juice attack (see <c>BattleManager.DamageJuice</c>), false for a regular HP attack.
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
/// Why an attack missed. See <see cref="AttackMissedEventArgs.Reason"/>.
/// </summary>
public enum MissReason
{
    /// <summary>
    /// The attacker's HIT roll failed.
    /// </summary>
    Missed,
    /// <summary>
    /// The target evaded the attack.
    /// </summary>
    Evaded,
    /// <summary>
    /// A stat modifier turned the hit into a miss.
    /// </summary>
    Nullified
}

/// <summary>
/// Event arguments for <see cref="BattleManager.AttackMissed"/>.
/// </summary>
public sealed class AttackMissedEventArgs : EventArgs
{
    /// <summary>
    /// The attacker.
    /// </summary>
    public Actor Attacker { get; }

    /// <summary>
    /// The actor the attack was aimed at.
    /// </summary>
    public Actor Target { get; }

    /// <summary>
    /// Why the attack missed.
    /// </summary>
    public MissReason Reason { get; }

    /// <summary>
    /// True if this was a juice attack (see <c>BattleManager.DamageJuice</c>), false for a regular HP attack or a plain <see cref="BattleManager.RollHit"/>.
    /// </summary>
    public bool JuiceDamage { get; }

    internal AttackMissedEventArgs(Actor attacker, Actor target, MissReason reason, bool juiceDamage)
    {
        Attacker = attacker;
        Target = target;
        Reason = reason;
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

/// <summary>
/// Event arguments for <see cref="BattleManager.ActionStarted"/> and <see cref="BattleManager.ActionEnded"/>.
/// </summary>
public sealed class ActionEventArgs : EventArgs
{
    /// <summary>
    /// The actor performing the action.
    /// </summary>
    public Actor Actor { get; }

    /// <summary>
    /// The action being performed.
    /// </summary>
    public BattleAction Action { get; }

    /// <summary>
    /// The targets the action applies to, after invalid targets were replaced.
    /// </summary>
    public IReadOnlyList<Actor> Targets { get; }

    internal ActionEventArgs(Actor actor, BattleAction action, IReadOnlyList<Actor> targets)
    {
        Actor = actor;
        Action = action;
        Targets = targets;
    }
}

/// <summary>
/// Event arguments for <see cref="BattleManager.AIDeciding"/>.
/// </summary>
public sealed class AIDecidingEventArgs : EventArgs
{
    /// <summary>
    /// The enemy about to decide its action.
    /// </summary>
    public Enemy Enemy { get; }

    internal AIDecidingEventArgs(Enemy enemy)
    {
        Enemy = enemy;
    }
}

/// <summary>
/// Event arguments for <see cref="BattleManager.AIDecided"/>.
/// </summary>
public sealed class AIDecidedEventArgs : EventArgs
{
    /// <summary>
    /// The enemy that decided its action.
    /// </summary>
    public Enemy Enemy { get; }

    /// <summary>
    /// The command returned by <see cref="Actors.Enemy.ProcessAI"/>, which the enemy performs next. May be null.
    /// </summary>
    public BattleCommand Command { get; }

    internal AIDecidedEventArgs(Enemy enemy, BattleCommand command)
    {
        Enemy = enemy;
        Command = command;
    }
}
