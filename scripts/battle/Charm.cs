using System;

public class Charm
{
    /// <summary>
    /// The name of the Charm
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// The Charm's additive stats
    /// </summary>
    public Stats Stats { get; private set; }

    /// <summary>
    /// Applies the Charm's Stats.
    /// </summary>
    /// <remarks>
    /// Usually this will just return the Charm's <c>Stats</c>.
    /// However, it can be useful for stat changes that rely on game variables, like the Energy bar.
    /// </remarks>
    public Func<Stats> Apply { get; private set; }

    /// <summary>
    /// What the charm does to its holder at the start of the battle.
    /// </summary>
    /// <remarks>
    /// Mainly used for emotion effects.
    /// </remarks>
    public Action<Actor> StartOfBattle { get; private set; }

    /// <summary>
    /// A basic Charm with additive Stats.
    /// </summary>
    /// <remarks>
    /// Note: Will automatically create an <c>Apply</c> method that returns <c>Stats</c>.
    /// </remarks>
    public Charm(string name, Stats stats)
    {
        Name = name;
        Stats = stats;
        Apply = () =>
        {
            return Stats;
        };
    }

    /// <summary>
    /// A Charm with custom behavior when the stats are applied.
    /// </summary>
    /// <remarks>
    /// Useful for stats based on game variables, like the Energy bar.
    /// </remarks>
    public Charm(string name, Func<Stats> apply)
    {
        Name = name;
        Stats = new Stats();
        Apply = apply;
    }

    /// <summary>
    /// A Charm that does something at the start of the battle, usually giving an emotion to the specified <c>Actor</c>.
    /// </summary>
    /// <remarks>
    /// Note: Will automatically create an <c>Apply</c> method that returns <c>Stats</c>.
    /// </remarks>
    public Charm(string name, Stats stats, Action<Actor> startOfBattle)
    {
        Name = name;
        Stats = stats;
        Apply = () =>
        {
            return Stats;
        };
        StartOfBattle = startOfBattle;
    }

    /// <summary>
    /// A Charm that both does something at the start of the battle and when stats are applied.
    /// </summary>
    public Charm(string name, Func<Stats> apply, Action<Actor> startOfBattle)
    {
        Name = name;
        Stats = new Stats();
        Apply = apply;
        StartOfBattle = startOfBattle;
    }
}