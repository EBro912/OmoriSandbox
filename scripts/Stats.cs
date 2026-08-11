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
    public int GetStat(StatType stat)
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
    /// Sets the current value of the given <see cref="StatType"/>.
    /// </summary>
    /// <param name="stat">The <see cref="StatType"/> to set.</param>
    /// <param name="value">The value to set the stat to.</param>"
    public void SetStat(StatType stat, int value)
    {
        if (stat is StatType.MaxJuice or StatType.MaxHP or StatType.EVA)
            value = Math.Max(0, value);
        else
            value = SettingsMenuManager.Instance.DisableStatLimit ? Math.Max(1, value) : Math.Clamp(value, 1, 999);
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
}