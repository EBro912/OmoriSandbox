using System;
using OmoriSandbox.Editor;

namespace OmoriSandbox.Battle;

/// <summary>
/// Represets a set of stats an <see cref="Actors.Actor"/> can have.
/// </summary>
public struct Stats
{
#pragma warning disable CS1591
    public int HP;
    public int MaxHP;
    public int Juice;
    public int MaxJuice;
    public int ATK;
    public int DEF;
    public int SPD;
    public int LCK;
    public int HIT;
    public int EVA;

    // fractional remainders to avoid rounding loss between calculation steps
    private float MaxHPFraction;
    private float MaxJuiceFraction;
    private float ATKFraction;
    private float DEFFraction;
    private float SPDFraction;
    private float LCKFraction;
    private float HITFraction;
    private float EVAFraction;

    public Stats(int hp = 0, int juice = 0, int atk = 0, int def = 0, int spd = 0, int lck = 0, int hit = 0, int eva = 0)
    {
        HP = hp;
        MaxHP = hp;
        Juice = juice;
        MaxJuice = juice;
        ATK = atk;
        DEF = def;
        SPD = spd;
        LCK = lck;
        HIT = hit;
        EVA = eva;
    }

    public static Stats operator +(Stats a, Stats b) {
        Stats result = new(
            Math.Max(1, a.HP + b.HP),
            Math.Max(0, a.Juice + b.Juice),
            Math.Max(0, a.ATK + b.ATK),
            Math.Max(0, a.DEF + b.DEF),
            Math.Max(0, a.SPD + b.SPD),
            Math.Max(0, a.LCK + b.LCK),
            Math.Max(0, a.HIT + b.HIT),
            Math.Max(0, a.EVA + b.EVA))
            {
                MaxHP = Math.Max(1, a.HP + b.HP),
                MaxJuice = Math.Max(0, a.Juice + b.Juice)
            };
        return result;
    }
#pragma warning restore CS1591

    /// <summary>
    /// Retrives the current value of the given <see cref="StatType"/>.
    /// </summary>
    /// <param name="stat">The <see cref="StatType"/> to retrieve.</param>
    public readonly int GetStat(StatType stat)
    {
        return stat switch
        {
            StatType.MaxHP => MaxHP,
            StatType.MaxJuice => MaxJuice,
            StatType.ATK => ATK,
            StatType.DEF => DEF,
            StatType.SPD => SPD,
            StatType.LCK => LCK,
            StatType.HIT => HIT,
            StatType.EVA => EVA,
            _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
        };
    }

    /// <summary>
    /// Retrieves the exact, unrounded value of the given <see cref="StatType"/>.
    /// Equal to <see cref="GetStat"/> when no fractional remainder is present.
    /// </summary>
    /// <param name="stat">The <see cref="StatType"/> to retrieve.</param>
    public readonly float GetStatExact(StatType stat)
    {
        return GetStat(stat) + GetFraction(stat);
    }

    /// <summary>
    /// Sets the current value of the given <see cref="StatType"/>.
    /// </summary>
    /// <param name="stat">The <see cref="StatType"/> to set.</param>
    /// <param name="value">The value to set the stat to.</param>
    public void SetStat(StatType stat, int value)
    {
        if (stat is StatType.MaxJuice or StatType.MaxHP or StatType.EVA)
            value = Math.Max(0, value);
        else
            value = Math.Max(1, value);
        SetStatInternal(stat, value);
        SetFraction(stat, 0f);
    }

    /// <summary>
    /// Sets the exact value of the given <see cref="StatType"/>, carrying the fractional remainder.
    /// </summary>
    /// <param name="stat">The <see cref="StatType"/> to set.</param>
    /// <param name="value">The exact value to set the stat to.</param>
    public void SetStatExact(StatType stat, float value)
    {
        int mirror = (int)Math.Round(value, MidpointRounding.AwayFromZero);
        if (stat is StatType.MaxJuice or StatType.MaxHP or StatType.EVA or StatType.HIT)
            mirror = Math.Max(0, mirror);
        else
            mirror = Math.Max(1, mirror);
        SetStatInternal(stat, mirror);
        SetFraction(stat, value - mirror);
    }

    /// <summary>
    /// Applies the base game's stat floors and (unless the Disable Stat Limit setting is enabled)
    /// the 999 stat cap, discarding any fractional remainders.
    /// HIT, EVA, Max HP and Max Juice are never capped.
    /// </summary>
    /// <remarks>
    /// Called automatically at the end of <see cref="Actors.Actor.CurrentStats"/>.
    /// </remarks>
    public void ApplyStatLimits()
    {
        bool disableLimit = SettingsMenuManager.Instance?.DisableStatLimit ?? false;
        MaxHP = Math.Max(0, MaxHP);
        MaxJuice = Math.Max(0, MaxJuice);
        EVA = Math.Max(0, EVA);
        HIT = Math.Max(1, HIT);
        ATK = disableLimit ? Math.Max(1, ATK) : Math.Clamp(ATK, 1, 999);
        DEF = disableLimit ? Math.Max(1, DEF) : Math.Clamp(DEF, 1, 999);
        SPD = disableLimit ? Math.Max(1, SPD) : Math.Clamp(SPD, 1, 999);
        LCK = disableLimit ? Math.Max(1, LCK) : Math.Clamp(LCK, 1, 999);
        MaxHPFraction = 0f;
        MaxJuiceFraction = 0f;
        ATKFraction = 0f;
        DEFFraction = 0f;
        SPDFraction = 0f;
        LCKFraction = 0f;
        HITFraction = 0f;
        EVAFraction = 0f;
    }

    private void SetStatInternal(StatType stat, int value)
    {
        switch (stat)
        {
            case StatType.MaxHP: MaxHP = value; break;
            case StatType.MaxJuice: MaxJuice = value; break;
            case StatType.ATK: ATK = value; break;
            case StatType.DEF: DEF = value; break;
            case StatType.SPD: SPD = value; break;
            case StatType.LCK: LCK = value; break;
            case StatType.HIT: HIT = value; break;
            case StatType.EVA: EVA = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
        }
    }

    private readonly float GetFraction(StatType stat)
    {
        return stat switch
        {
            StatType.MaxHP => MaxHPFraction,
            StatType.MaxJuice => MaxJuiceFraction,
            StatType.ATK => ATKFraction,
            StatType.DEF => DEFFraction,
            StatType.SPD => SPDFraction,
            StatType.LCK => LCKFraction,
            StatType.HIT => HITFraction,
            StatType.EVA => EVAFraction,
            _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
        };
    }

    private void SetFraction(StatType stat, float value)
    {
        switch (stat)
        {
            case StatType.MaxHP: MaxHPFraction = value; break;
            case StatType.MaxJuice: MaxJuiceFraction = value; break;
            case StatType.ATK: ATKFraction = value; break;
            case StatType.DEF: DEFFraction = value; break;
            case StatType.SPD: SPDFraction = value; break;
            case StatType.LCK: LCKFraction = value; break;
            case StatType.HIT: HITFraction = value; break;
            case StatType.EVA: EVAFraction = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
        }
    }
}
