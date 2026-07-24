using OmoriSandbox.Actors;
using System;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// A generic stat modifier that can provide a set of <see cref="StatBonus"/>es.
/// </summary>
public class StatModifier
{
    /// <summary>
    /// The bonuses that this stat modifier provides.
    /// </summary>
    protected StatBonus[] Bonuses;

    /// <summary>
    /// How many turns this stat bonus has remaining.
    /// </summary>
    public int TurnsLeft { get; protected set; } = -1;
    
    /// <summary>
    /// The max number of turns this stat bonus can be applied for.
    /// </summary>
    protected int MaxTurns = -1;

    /// <summary>
    /// A list of state icons this stat modifier uses.
    /// </summary>
    protected StateIcon[] StateIcons = [];

    /// <summary>
    /// A generic stat modifier that has no turn counter
    /// </summary>
    /// <param name="bonuses">A list of stat bonuses that will all be applied at once.</param>
    public StatModifier(params StatBonus[] bonuses)
    {
        Bonuses = bonuses;
    }
    
    /// <summary>
    /// A generic stat modifier that has a turn counter. The <paramref name="turns"/> value is also used to set the max turns.
    /// </summary>
    /// <param name="turns">The number of turns this stat modifier lasts for.</param>
    /// <param name="bonuses">A list of stat bonuses that will all be applied at once.</param>
    public StatModifier(int turns, params StatBonus[] bonuses) : this(bonuses)
    {
        TurnsLeft = turns;
        MaxTurns = TurnsLeft;
    }

    /// <summary>
    /// Sets the list of state icons this modifier should use.
    /// </summary>
    /// <param name="icons">A list of state icons this stat modifier should use.</param>
    public StatModifier WithStateIcons(params StateIcon[] icons)
    {
        StateIcons = icons;
        return this;
    }
    
    internal virtual StateIcon[] GetStateIcons()
    {
        return StateIcons;
    }

    /// <summary>
    /// Called when a stat modifier is first given to an actor.
    /// </summary>
    /// <param name="actor">The <see cref="Actor"/> this modifier is being applied to.</param>
    public virtual void OnAdd(Actor actor) { }

    /// <summary>
    /// Called at the start of every turn.
    /// </summary>
    /// <remarks>
    /// If an actor is given a stat modifier at the start of the turn, this will not trigger.
    /// Use <see cref="OnAdd"/> instead.
    /// </remarks>
    /// <param name="actor">The <see cref="Actor"/> this modifier is attached to.</param>
    public virtual void OnStartOfTurn(Actor actor) {}

    /// <summary>
    /// Called when the stats are applied.
    /// </summary>
    /// <param name="stats">A reference to the <see cref="Stats"/> before any stats are applied.</param>
    public virtual void ApplyStats(ref Stats stats)
    {
        // base class simply applies all stat bonuses to the provided stats
        StatBonus.ApplyAll(ref stats, Bonuses);
    }

    /// <summary>
    /// Called during damage calculation to override the damage result at various phases.
    /// </summary>
    /// <param name="phase">The <see cref="DamagePhase"/> at which to override the damage at.</param>
    /// <param name="damage">A reference to the current damage at this point in the damage calculation.</param>
    /// <param name="attacker">The <see cref="Actor"/> that is attacking in the calculation.</param>
    /// <param name="defender">The <see cref="Actor"/> that is defending in the calculation.</param>
    /// <param name="isAttacking">The attack context from whenever this function is called.<br/>For instance, this value will be <c>true</c> whenever the <paramref name="attacker"/>'s modifiers are being calculated.</param>
    /// <param name="isCritical">Whether the damage is currently a critical. This will always be <c>false</c> for pre-crit phases such as <c>PreEmotion</c></param>
    /// <param name="neverMiss">Whether the damage is set to never miss. Similar to the Certain Hit damage type in RPGMaker.</param>
    public virtual void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking, bool isCritical, bool neverMiss) { }
    
    /// <summary>
    /// Allows modifying the cost of skills.
    /// </summary>
    /// <param name="juice">The current juice cost.</param>
    /// <param name="actor">The actor this modifier is attached to.</param>
    public virtual void OverrideJuiceCost(ref int juice, Actor actor) {}

    /// <summary>
    /// Sets the number of turns left on this modifier. Will be clamped to the max turns set previously.
    /// </summary>
    /// <param name="turnsLeft">The number of turns left to set this modifier to.</param>
    public void SetTurnsLeft(int turnsLeft)
    {
        TurnsLeft = turnsLeft;
        if (MaxTurns != -1)
            MaxTurns = Math.Max(MaxTurns, turnsLeft);
    }

    /// <summary>
    /// Refreshes the turn counter to the max turns.
    /// </summary>
    public void RefreshTurns()
    {
        TurnsLeft = MaxTurns;
    }

    /// <summary>
    /// Decrement the number of turns left on this modifier by one.
    /// </summary>
    public void DecreaseTurns()
    {
        if (TurnsLeft > 0)
            TurnsLeft--;
    }
}