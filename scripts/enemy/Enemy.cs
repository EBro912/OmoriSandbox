using Godot;

public abstract class Enemy : Actor
{
    public AnimatedSprite2D Sprite;

    public void Init(AnimatedSprite2D sprite, string initialState)
    {
        SpriteFrames animation = GD.Load<SpriteFrames>(AnimationPath);
        if (animation == null)
        {
            GD.PrintErr("Failed to load Sprite animations for Enemy: " + Name);
            return;
        }
        // init animation
        Sprite = sprite;
        Sprite.SpriteFrames = animation;
        Sprite.Animation = initialState;
        Sprite.Play();
        CurrentState = initialState;

        // init stats
        HP = Stats.HP;
        MaxHP = Stats.HP;
        Juice = Stats.Juice;
        MaxJuice = Stats.Juice;
        ATK = Stats.ATK;
        DEF = Stats.DEF;
        SPD = Stats.SPD;
        LCK = Stats.LCK;
        HIT = 100;
    }

    public abstract string AnimationPath { get; }
    public abstract EnemyStats Stats { get; }
    public abstract bool IsStateValid(string state);
    public abstract void ProcessAI();
}