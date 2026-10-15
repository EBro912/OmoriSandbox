using OmoriSandbox.Actors;
using OmoriSandbox.Battle.Emotions;
using System;
using System.Collections.Generic;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// A generic stat modifier that can provide a set of <see cref="StatBonus"/>es.
/// </summary>
public class StatModifier
{
    /// <summary>
    /// The category the Sandbox gives its ATTACK, DEFENSE and SPEED Up modifiers. See <see cref="WithCategories"/>.
    /// </summary>
    public const string BuffCategory = "BUFF";

    /// <summary>
    /// The category the Sandbox gives its ATTACK, DEFENSE and SPEED Down modifiers. See <see cref="WithCategories"/>.
    /// </summary>
    public const string DebuffCategory = "DEBUFF";

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
    
    private HashSet<string> AbsorbElements;
    
    private HashSet<string> ReflectElements;

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
    /// Whether this modifier ignores removals unless forced, Recover All included. See <see cref="WithBypassRecoverAllRemoval"/>.
    /// </summary>
    public bool BypassesRecoverAllRemoval { get; private set; }

    /// <summary>
    /// Whether this modifier stays on its holder when they die. See <see cref="WithBypassDeathRemoval"/>.
    /// </summary>
    public bool BypassesDeathRemoval { get; private set; }

    /// <summary>
    /// Whether this modifier can be added again right after it is removed. See <see cref="WithReapplyAllowed"/>.
    /// </summary>
    public bool ReapplyAllowed { get; private set; }

    private readonly HashSet<string> CategorySet = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The categories this modifier is in. See <see cref="WithCategories"/>.
    /// </summary>
    public IReadOnlyCollection<string> Categories => CategorySet;

    internal long AddSequence { get; set; }

    /// <summary>
    /// Whether this modifier's turn counter ticks each time its holder finishes performing an
    /// action, instead of at the end of the turn. Mirrors RPGMaker's action-end state removal
    /// timing (YEP autoRemovalTiming 1).
    /// </summary>
    public bool TicksOnActionEnd { get; private set; }

    // handles the "free state turn" so action states don't get removed at the end of the action that applied them
    internal bool SkipNextActionTick { get; set; }

    /// <summary>
    /// Marks this modifier as ticking at the end of the holder's actions instead of at the end
    /// of the turn. See <see cref="TicksOnActionEnd"/>.
    /// </summary>
    public StatModifier WithActionEndTicking()
    {
        TicksOnActionEnd = true;
        return this;
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

    /// <summary>
    /// Marks this modifier as surviving <see cref="Actors.Actor.RecoverAll"/>, <see cref="Actors.Actor.RemoveStatModifiersInCategory(string, bool)"/>
    /// and any other unforced removal, like the Yanfly "Bypass Recover All Removal" state category.
    /// It still expires from its own turn counter and is cleared on death unless <see cref="WithBypassDeathRemoval"/> is also used.
    /// </summary>
    public StatModifier WithBypassRecoverAllRemoval()
    {
        BypassesRecoverAllRemoval = true;
        return this;
    }

    /// <summary>
    /// Keeps this modifier on its holder when they die, like the Yanfly "Bypass Death Removal" state category.
    /// </summary>
    public StatModifier WithBypassDeathRemoval()
    {
        BypassesDeathRemoval = true;
        return this;
    }

    /// <summary>
    /// Lets this modifier be added again right after it is removed, even while "Allow State Reapply on Removal" is off.
    /// </summary>
    public StatModifier WithReapplyAllowed()
    {
        ReapplyAllowed = true;
        return this;
    }

    /// <summary>
    /// Adds this modifier to the given categories, like Yanfly state categories. Category names ignore case.
    /// </summary>
    /// <param name="categories">The category names to add.</param>
    public StatModifier WithCategories(params string[] categories)
    {
        foreach (string category in categories ?? [])
        {
            if (!string.IsNullOrWhiteSpace(category))
                CategorySet.Add(category.Trim());
        }
        return this;
    }

    /// <summary>
    /// Whether this modifier is in the given category. See <see cref="WithCategories"/>.
    /// </summary>
    /// <param name="category">The category name to check, ignoring case.</param>
    public bool HasCategory(string category)
    {
        return !string.IsNullOrWhiteSpace(category) && CategorySet.Contains(category.Trim());
    }

    private readonly EmotionImmunity Immunity = new();

    /// <summary>
    /// Makes the holder unable to feel the given emotions while this modifier is applied.
    /// </summary>
    /// <remarks>
    /// Each id blocks only that emotion, so list every tier you want to block, e.g. <c>"happy", "ecstatic", "manic"</c>.
    /// Neutral can't be blocked. An actor already feeling a blocked emotion returns to neutral.
    /// </remarks>
    /// <param name="emotionIds">The ids of the emotions to block.</param>
    public StatModifier WithEmotionImmunity(params string[] emotionIds)
    {
        Immunity.Add(emotionIds);
        return this;
    }

    /// <summary>
    /// Makes the actor unable to feel any emotion except neutral and the ones listed while this modifier is applied. This includes emotions added by mods.
    /// </summary>
    /// <remarks>
    /// For example, <c>WithImmunityToAllEmotions("stressed")</c> still lets the actor become STRESSED OUT.
    /// An actor already feeling a blocked emotion returns to neutral.
    /// </remarks>
    /// <param name="exceptEmotionIds">The ids of emotions that stay allowed.</param>
    public StatModifier WithImmunityToAllEmotions(params string[] exceptEmotionIds)
    {
        Immunity.AddAllExcept(exceptEmotionIds);
        return this;
    }

    internal bool BlocksEmotion(Emotion emotion)
    {
        return Immunity.Blocks(emotion);
    }

    /// <summary>
    /// Makes the defender recover HP (or JUICE) from hits instead of taking them.
    /// </summary>
    /// <remarks>
    /// Attack elements are emotion group ids (<c>"happy"</c>, <c>"sad"</c>, <c>"angry"</c> or a mod's group) and <c>"exploit"</c>.
    /// With no elements, every hit is absorbed. Hits from a neutral attacker have no element.
    /// When one modifier absorbs a hit and another reflects it, absorbing wins.
    /// An absorbed hit triggers <see cref="BattleManager.Healed"/>, with the attacker as the healer, and not <see cref="BattleManager.DamageDealt"/>.
    /// </remarks>
    /// <param name="attackElements">The attack elements to absorb. Null or empty means every hit.</param>
    public StatModifier WithAbsorb(params string[] attackElements)
    {
        AbsorbElements = new HashSet<string>(attackElements ?? [], StringComparer.OrdinalIgnoreCase);
        return this;
    }

    /// <summary>
    /// Makes the attacker take hits instead of the defender.
    /// </summary>
    /// <remarks>
    /// The attacker takes the hit's final amount, which goes through the attacker's own modifiers but can't be reflected again.
    /// Attack elements work as in <see cref="WithAbsorb"/>. With no elements, every hit is reflected.
    /// </remarks>
    /// <param name="attackElements">The attack elements to reflect. Null or empty means every hit.</param>
    public StatModifier WithReflect(params string[] attackElements)
    {
        ReflectElements = new HashSet<string>(attackElements ?? [], StringComparer.OrdinalIgnoreCase);
        return this;
    }

    internal bool Absorbs(string element)
    {
        return AbsorbElements != null && (AbsorbElements.Count == 0 || (element != null && AbsorbElements.Contains(element)));
    }

    internal bool Reflects(string element)
    {
        return ReflectElements != null && (ReflectElements.Count == 0 || (element != null && ReflectElements.Contains(element)));
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
    /// Called when the modifier is removed from an actor for any reason (manual, expiry, cleared, combined).
    /// </summary>
    /// <param name="actor">The <see cref="Actor"/> this modifier was attached to.</param>
    public virtual void OnRemove(Actor actor) { }

    /// <summary>
    /// Called at the start of every turn.
    /// </summary>
    /// <remarks>
    /// If an actor is given a stat modifier at the start of the turn, this will not trigger.
    /// Use <see cref="OnAdd"/> instead.
    /// </remarks>
    /// <param name="actor">The <see cref="Actor"/> this modifier is attached to.</param>
    // TODO: OMORI ticks damage/regen-over-time effects inside end-of-turn, we currently do it at the start of the turn
    public virtual void OnStartOfTurn(Actor actor) {}

    /// <summary>
    /// The total multiplier this modifier applies to the given stat, as the product of its
    /// stat bonus multipliers. Flat bonuses are ignored.
    /// </summary>
    /// <param name="stat">The <see cref="StatType"/> to get the multiplier for.</param>
    public virtual float GetParamRate(StatType stat)
    {
        float rate = 1f;
        foreach (StatBonus bonus in Bonuses)
        {
            if (bonus.Type == stat)
                rate *= bonus.Multiplier;
        }
        return rate;
    }

    /// <summary>
    /// The total flat bonus this modifier applies to the given stat, as the sum of its
    /// stat bonus flat values. Multipliers are ignored.
    /// </summary>
    /// <param name="stat">The <see cref="StatType"/> to get the flat bonus for.</param>
    public virtual int GetParamPlus(StatType stat)
    {
        int plus = 0;
        foreach (StatBonus bonus in Bonuses)
        {
            if (bonus.Type == stat)
                plus += bonus.FlatBonus;
        }
        return plus;
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
    /// Called during damage calculation to override the damage result at various phases, with everything known about the
    /// calculation in <paramref name="context"/>.
    /// </summary>
    /// <remarks>
    /// By default, this calls <see cref="OverrideDamage(DamagePhase, ref float, Actor, Actor, bool, bool, bool)"/>, so existing
    /// overrides of that overload keep working. Override one of the two. If you override both, call <c>base</c> from this one to run the other.
    /// </remarks>
    /// <param name="phase">The <see cref="DamagePhase"/> at which to override the damage at.</param>
    /// <param name="damage">A reference to the current damage at this point in the damage calculation.</param>
    /// <param name="context">The attacker, defender and attack details. The same instance is passed to every phase and modifier of one damage call.</param>
    public virtual void OverrideDamage(DamagePhase phase, ref float damage, DamageContext context)
    {
        OverrideDamage(phase, ref damage, context.Attacker, context.Defender, context.IsAttacking, context.IsCritical, context.NeverMiss);
    }
    
    /// <summary>
    /// Allows modifying the cost of skills.
    /// </summary>
    /// <param name="juice">The current juice cost.</param>
    /// <param name="actor">The actor this modifier is attached to.</param>
    public virtual void OverrideJuiceCost(ref int juice, Actor actor) {}

    /// <summary>
    /// Allows modifying the HP cost of skills.
    /// </summary>
    /// <param name="hp">The current HP cost.</param>
    /// <param name="actor">The actor this modifier is attached to.</param>
    public virtual void OverrideHPCost(ref int hp, Actor actor) {}

    /// <summary>
    /// Allows modifying the Energy cost of skills.
    /// </summary>
    /// <param name="energy">The current Energy cost.</param>
    /// <param name="actor">The actor this modifier is attached to.</param>
    public virtual void OverrideEnergyCost(ref int energy, Actor actor) {}

    /// <summary>
    /// Sets the number of turns left on this modifier. If the value exceeds the max turns
    /// set previously, the max is raised to match. An explicit duration on a turnless modifier
    /// converts it into a timed one, so refreshes restore that duration instead of making it infinite.
    /// </summary>
    /// <param name="turnsLeft">The number of turns left to set this modifier to.</param>
    public void SetTurnsLeft(int turnsLeft)
    {
        TurnsLeft = turnsLeft;
        if (MaxTurns == -1)
        {
            if (turnsLeft > -1)
                MaxTurns = turnsLeft;
        }
        else
        {
            MaxTurns = Math.Max(MaxTurns, turnsLeft);
        }
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