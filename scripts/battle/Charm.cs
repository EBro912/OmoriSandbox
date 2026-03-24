using OmoriSandbox.Actors;
using System;
using System.Threading.Tasks;

namespace OmoriSandbox.Battle;

/// <summary>
/// A Charm that can be equipped on a <see cref="PartyMember"/>
/// </summary>
public class Charm
{
    /// <summary>
    /// The name of the Charm
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// The Charm's stats
    /// </summary>
    public StatBonus[] Stats { get; }

    /// <summary>
    /// An extra method that allows dynamic StatBonuses to be applied when <see cref="Apply"/>ing a charm.
    /// </summary>
    /// <remarks>
    /// This can be useful for stat changes that rely on game variables, like the Energy bar.
    /// </remarks>
    public Func<StatBonus[]> OnApply { get; private set; } = () => [];

    /// <summary>
    /// What the charm does to its holder at the start of the battle.
    /// </summary>
    /// <remarks>
    /// Mainly used for emotion effects.
    /// </remarks>
    public Func<Actor, Task> StartOfBattle { get; private set; } = _ => Task.CompletedTask;

    /// <summary>
    /// What the charm does to its holder at the start of each turn.
    /// </summary>
    /// <remarks>
    /// Will not be run if the actor is toast.
    /// </remarks>
    public Func<Actor, Task> StartOfTurn { get; private set; } = _ => Task.CompletedTask;

    /// <summary>
    /// A Charm that modifies a single stat.
    /// </summary>
    public Charm(string name, StatBonus stat)
    {
        Name = name;
        Stats = [stat];
    }
    
    /// <summary>
    /// A Charm that modifies a multiple stats.
    /// </summary>
    public Charm(string name, StatBonus[] stats)
    {
        Name = name;
        Stats = stats;
    }

    /// <summary>
    /// A Charm that modifies no stats.
    /// </summary>
    public Charm(string name)
    {
        Name = name;
        Stats = [];
    }

    /// <summary>
    /// Sets the <see cref="OnApply"/> effect for this charm.
    /// </summary>
    /// <param name="onApply">A function that returns a list of <see cref="StatBonus"/>es to apply.</param>
    public Charm WithApplyEffect(Func<StatBonus[]> onApply)
    {
        OnApply = onApply;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="StartOfBattle"/> effect for this charm.
    /// </summary>
    /// <param name="onStartOfBattle">A function with a reference to the charm's user that runs at the start of battle.</param>
    public Charm WithStartOfBattleEffect(Func<Actor, Task> onStartOfBattle)
    {
        StartOfBattle = onStartOfBattle;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="StartOfTurn"/> effect for this charm.
    /// </summary>
    /// <param name="onStartOfTurn">A function with a reference to the charm's user that runs at the start of each turn.</param>
    public Charm WithStartOfTurnEffect(Func<Actor, Task> onStartOfTurn)
    {
        StartOfTurn = onStartOfTurn;
        return this;
    }

    /// <summary>
    /// Applies this Charm's statbonuses to the provided <see cref="Stats"/>.
    /// </summary>
    /// <param name="stats">A reference to the <see cref="Stats"/> to modify.</param>
    public void Apply(ref Stats stats)
    {
        foreach (StatBonus bonus in Stats)
        {
            int stat = stats.GetStat(bonus.Type);
            stat = (int)Math.Round(stat * bonus.Multiplier + bonus.FlatBonus);
            stats.SetStat(bonus.Type, stat);
        }

        if (OnApply != null)
        {
            foreach (StatBonus bonus in OnApply())
            {
                int stat = stats.GetStat(bonus.Type);
                stat = (int)Math.Round(stat * bonus.Multiplier + bonus.FlatBonus);
                stats.SetStat(bonus.Type, stat);
            }
        }
    }
}