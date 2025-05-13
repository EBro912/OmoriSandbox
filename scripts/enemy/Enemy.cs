using Godot;

public abstract class Enemy : Actor
{
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

        foreach (string s in EquippedSkills)
        {
            if (SkillDatabase.TryGetSkill(s, out var skill))
            {
                Skills.Add(s, skill);
                continue;
            }
            GD.PrintErr("Unknown skill: " + s);
        }
    }

    protected abstract Stats Stats { get; }
    protected abstract string[] EquippedSkills { get; }
    public abstract string AnimationPath { get; }
    public abstract BattleCommand ProcessAI();
}