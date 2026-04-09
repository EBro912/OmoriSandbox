using OmoriSandbox.Actors;
using System;
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

    /// <summary>
    /// Applies this equipment's statbonuses to the provided <see cref="Stats"/>.
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