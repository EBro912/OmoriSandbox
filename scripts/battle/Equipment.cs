using OmoriSandbox.Actors;
using OmoriSandbox.Battle.Emotions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OmoriSandbox.Battle;

/// <summary>
/// Equipment that can be equipped on a <see cref="PartyMember"/>
/// </summary>
public class Equipment
{
    /// <summary>
    /// The name of the equipment
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// Whether this equipment is a Charm. Otherwise, it will be a Weapon.
    /// </summary>
    public bool IsCharm { get; private set; }
    /// <summary>
    /// The equipment's stats
    /// </summary>
    public StatBonus[] Stats { get; }

    /// <summary>
    /// An extra method that allows dynamic StatBonuses to be applied when <see cref="Apply"/>ing equipment.
    /// </summary>
    /// <remarks>
    /// This can be useful for stat changes that rely on game variables, like the Energy bar.
    /// </remarks>
    public Func<StatBonus[]> OnApply { get; private set; } = () => [];

    /// <summary>
    /// What the equipment does to its holder at the start of the battle.
    /// </summary>
    /// <remarks>
    /// Mainly used for emotion effects.
    /// </remarks>
    public Func<Actor, Task> StartOfBattle { get; private set; } = _ => Task.CompletedTask;

    /// <summary>
    /// What the equipment does to its holder at the start of each turn.
    /// </summary>
    /// <remarks>
    /// Will not be run if the actor is toast.
    /// </remarks>
    public Func<Actor, Task> StartOfTurn { get; private set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Equipment that modifies a single stat.
    /// </summary>
    public Equipment(string name, StatBonus stat, bool isCharm = false)
    {
        Name = name;
        Stats = [stat];
        IsCharm = isCharm;
    }
    
    /// <summary>
    /// Equipment that modifies a multiple stats.
    /// </summary>
    public Equipment(string name, StatBonus[] stats, bool isCharm = false)
    {
        Name = name;
        Stats = stats;
        IsCharm = isCharm;
    }

    /// <summary>
    /// Equipment that modifies no stats.
    /// </summary>
    public Equipment(string name, bool isCharm = false)
    {
        Name = name;
        Stats = [];
        IsCharm = isCharm;
    }

    /// <summary>
    /// Sets the <see cref="OnApply"/> effect for this equipment.
    /// </summary>
    /// <param name="onApply">A function that returns a list of <see cref="StatBonus"/>es to apply.</param>
    public Equipment WithApplyEffect(Func<StatBonus[]> onApply)
    {
        OnApply = onApply;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="StartOfBattle"/> effect for this equipment.
    /// </summary>
    /// <param name="onStartOfBattle">A function with a reference to the equipment's user that runs at the start of battle.</param>
    public Equipment WithStartOfBattleEffect(Func<Actor, Task> onStartOfBattle)
    {
        StartOfBattle = onStartOfBattle;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="StartOfTurn"/> effect for this equipment.
    /// </summary>
    /// <param name="onStartOfTurn">A function with a reference to the equipment's user that runs at the start of each turn.</param>
    public Equipment WithStartOfTurnEffect(Func<Actor, Task> onStartOfTurn)
    {
        StartOfTurn = onStartOfTurn;
        return this;
    }

    private readonly EmotionImmunity Immunity = new();

    /// <summary>
    /// Makes the wearer unable to feel the given emotions.
    /// </summary>
    /// <remarks>
    /// Each id blocks only that emotion, so list every tier to block, e.g. <c>"happy", "ecstatic", "manic"</c>.
    /// Neutral can't be blocked.
    /// </remarks>
    /// <param name="emotionIds">The ids of the emotions to block.</param>
    public Equipment WithEmotionImmunity(params string[] emotionIds)
    {
        Immunity.Add(emotionIds);
        return this;
    }

    /// <summary>
    /// Makes the wearer unable to feel any emotion except neutral and the ones listed. This includes emotions added by mods.
    /// </summary>
    /// <param name="exceptEmotionIds">The ids of emotions that stay allowed.</param>
    public Equipment WithImmunityToAllEmotions(params string[] exceptEmotionIds)
    {
        Immunity.AddAllExcept(exceptEmotionIds);
        return this;
    }

    internal bool BlocksEmotion(Emotion emotion)
    {
        return Immunity.Blocks(emotion);
    }

    private readonly List<float> ActionPlus = [];

    /// <summary>
    /// The chances of an extra action each turn this equipment gives its wearer. See <see cref="WithActionPlus"/>.
    /// </summary>
    public IReadOnlyList<float> ActionPlusChances => ActionPlus;

    /// <summary>
    /// Gives the wearer a chance of one extra action each turn.
    /// </summary>
    /// <remarks>
    /// Each call adds a separate chance, rolled at the start of every turn. A chance of 1 always gives the extra action. See <see cref="Actor.ActionTimes"/>.
    /// </remarks>
    /// <param name="chance">The chance, from 0 to 1.</param>
    public Equipment WithActionPlus(float chance)
    {
        ActionPlus.Add(Math.Clamp(chance, 0f, 1f));
        return this;
    }

    /// <summary>
    /// Applies this equipment's stat bonuses to the provided <see cref="Stats"/>.
    /// </summary>
    /// <param name="stats">A reference to the <see cref="Stats"/> to modify.</param>
    public void Apply(ref Stats stats)
    {
        StatBonus.ApplyAll(ref stats, Stats);

        if (OnApply != null)
            StatBonus.ApplyAll(ref stats, OnApply());
    }
}