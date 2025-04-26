using Godot;
using System;

public abstract class Actor
{
    public abstract string Name { get; }
    public AnimatedSprite2D Sprite;
    public string CurrentState;
    public bool IsHurt = false;
    public int Level = 1;
    /// <summary>
    /// The Actor's base stats without any modifiers.
    /// </summary>
    public Stats BaseStats;
    /// <summary>
    /// The Actor's standalone modified stats.
    /// </summary>
    public Stats AdjustedStats;
    public int CurrentHP = 0;
    public int CurrentJuice = 0;

    /// <summary>
    /// The Actor's base stats, any adjusted stats, and emotion stats.
    /// </summary>
    public Stats CurrentStats
    {
        get
        {
            Stats current = BaseStats + AdjustedStats;
            switch (CurrentState)
            {
                case "happy":
                    current.LCK *= 2;
                    current.SPD = (int)Math.Round(current.SPD * 1.25f, MidpointRounding.AwayFromZero);
                    current.HIT -= (int)Math.Round(current.HIT * 0.1f, MidpointRounding.AwayFromZero);
                    break;
                case "ecstatic":
                    current.LCK *= 3;
                    current.SPD = (int)Math.Round(current.SPD * 1.5f, MidpointRounding.AwayFromZero);
                    current.HIT -= (int)Math.Round(current.HIT * 0.2f, MidpointRounding.AwayFromZero);
                    break;
                case "manic":
                    current.LCK *= 4;
                    current.SPD = (int)Math.Round(current.SPD * 2f, MidpointRounding.AwayFromZero);
                    current.HIT -= (int)Math.Round(current.HIT * 0.3f, MidpointRounding.AwayFromZero);
                    break;
                case "angry":
                    current.ATK = (int)Math.Round(current.ATK * 1.3f, MidpointRounding.AwayFromZero);
                    current.DEF = (int)Math.Round(current.DEF * 0.5f, MidpointRounding.AwayFromZero);
                    break;
                case "enraged":
                    current.ATK = (int)Math.Round(current.ATK * 1.5f, MidpointRounding.AwayFromZero);
                    current.DEF = (int)Math.Round(current.DEF * 0.3f, MidpointRounding.AwayFromZero);
                    break;
                case "furious":
                    current.ATK = (int)Math.Round(current.ATK * 2f, MidpointRounding.AwayFromZero);
                    current.DEF = (int)Math.Round(current.DEF * 0.15f, MidpointRounding.AwayFromZero);
                    break;
                case "sad":
                    current.DEF = (int)Math.Round(current.DEF * 1.25f, MidpointRounding.AwayFromZero);
                    current.SPD = (int)Math.Round(current.SPD * 0.8f, MidpointRounding.AwayFromZero);
                    break;
                case "depressed":
                    current.DEF = (int)Math.Round(current.DEF * 1.35f, MidpointRounding.AwayFromZero);
                    current.SPD = (int)Math.Round(current.SPD * 0.65f, MidpointRounding.AwayFromZero);
                    break;
                case "miserable":
                    current.DEF = (int)Math.Round(current.DEF * 1.5f, MidpointRounding.AwayFromZero);
                    current.SPD = (int)Math.Round(current.SPD * 0.5f, MidpointRounding.AwayFromZero);
                    break;
            }

            return current;
        }
    }

    public void Damage(int damage)
    {
        CurrentHP -= damage;
        if (CurrentHP < 0)
            CurrentHP = 0;
    }

    public void SetState(string state)
    {
        Sprite.Animation = state;
        CurrentState = state;
    }

    public void SetHurt(bool hurt)
    {
        Sprite.Animation = hurt ? "hurt" : CurrentState;
        IsHurt = hurt;
    }

}