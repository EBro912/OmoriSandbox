using Godot;

public abstract class PartyMember : Actor
{
    public AnimatedSprite2D Face;

    public void Init(AnimatedSprite2D face, string initialState, int level)
    {
        SpriteFrames animation = GD.Load<SpriteFrames>(AnimationPath);
        if (animation == null)
        {
            GD.PrintErr("Failed to load Face animations for PartyMember: " + Name);
            return;
        }
        // init animation
        Face = face;
        Face.SpriteFrames = animation;
        Face.Animation = initialState;
        Face.Play();
        CurrentState = initialState;

        // init stats
        int idx = level - 1;
        HP = HPTree[idx];
        MaxHP = HPTree[idx];
        Juice = JuiceTree[idx];
        MaxJuice = JuiceTree[idx];
        ATK = ATKTree[idx];
        DEF = DEFTree[idx];
        SPD = SPDTree[idx];
        LCK = BaseLuck;
    }

    public void SetState(string state)
    {
        Face.Animation = state;
        CurrentState = state;
    }

    public abstract string AnimationPath { get; }
    public abstract int[] HPTree { get; }
    public abstract int[] JuiceTree { get; }
    public abstract int[] ATKTree { get; }
    public abstract int[] DEFTree { get; }
    public abstract int[] SPDTree { get; }
    public abstract int BaseLuck { get; }
    public abstract bool IsStateValid(string state);

}
