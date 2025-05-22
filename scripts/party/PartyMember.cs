using Godot;

public abstract class PartyMember : Actor
{
    public void Init(AnimatedSprite2D face, string initialState, int level)
    {
        SpriteFrames animation = GD.Load<SpriteFrames>(AnimationPath);
        if (animation == null)
        {
            GD.PrintErr("Failed to load Face animations for PartyMember: " + Name);
            return;
        }
        // init animation
        Sprite = face;
        Sprite.SpriteFrames = animation;
        Sprite.Animation = initialState;
        Sprite.Play();
        CurrentState = initialState;

        // init stats
        int idx = level - 1;
        BaseStats = new Stats(HPTree[idx], JuiceTree[idx], ATKTree[idx], DEFTree[idx], SPDTree[idx], BaseLuck, 0);
        AdjustedStats += Weapon;
        CurrentHP = CurrentStats.HP;
        CurrentJuice = CurrentStats.Juice;

        foreach (string s in EquippedSkills)
        {
            if (Database.TryGetSkill(s, out var skill))
            {
                Skills.Add(s, skill);
                continue;
            }
            GD.PrintErr("Unknown skill: " + s);
        }
    }

    public abstract string AnimationPath { get; }
    public abstract int[] HPTree { get; }
    public abstract int[] JuiceTree { get; }
    public abstract int[] ATKTree { get; }
    public abstract int[] DEFTree { get; }
    public abstract int[] SPDTree { get; }
    public abstract int BaseLuck { get; }
    // TODO: add weapon effects
    // TODO: add charms
    protected abstract string[] EquippedSkills { get; }
    public abstract Stats Weapon { get; }
    public abstract bool IsRealWorld { get; }
}


