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
        BaseStats = Stats;
        CurrentHP = BaseStats.HP;
        CurrentJuice = BaseStats.Juice;
    }
    
    protected abstract Stats Stats { get; }
    public abstract string AnimationPath { get; }
    public abstract bool IsStateValid(string state);
    public abstract void ProcessAI();
}