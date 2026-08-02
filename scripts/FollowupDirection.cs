using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox;

internal partial class FollowupDirection : Sprite2D
{
    [Export] public int Cost { get; private set; } = 3;
    [Export] public InputDirection InputDir { get; private set; }
    public bool Available { get; private set; }
    private CursorBounce Finger;
    // set when the assigned followup set has no entry for this direction (incomplete set)
    private bool Unassigned;

    public override void _Ready()
    {
        Finger = GetChild<CursorBounce>(0);
        Finger.StopBounce();
        Modulate = Colors.Transparent;
    }

    // apply graphics from a followup entry
    // in base game, followup graphics are hardcoded based on actor position
    internal void Apply(FollowupEntry entry)
    {
        Texture = entry.ResolveTexture();
        RegionEnabled = entry.TextureRegion.HasValue;
        if (entry.TextureRegion.HasValue)
            RegionRect = entry.TextureRegion.Value;
        Cost = entry.Cost;
        if (entry.IsReleaseEnergy)
        {
            FollowupFreakOutComponent freakOut = new();
            freakOut.Init(this);
            AddChild(freakOut);
        }
    }

    internal void Disable()
    {
        Unassigned = true;
        Visible = false;
    }

    public void ShowBubble(bool targetAvailable)
    {
        if (Unassigned) return;
        Tween tween = CreateTween();
        Available = targetAvailable && BattleManager.Instance.Energy >= Cost;
        if (Available)
        {
            tween.TweenProperty(this, "modulate:a", 1f, 0.2f);
            Finger.StartBounce();
        }
        else
        {
            tween.TweenProperty(this, "modulate:a", 0.6f, 0.2f);
        }
    }

    public void HideBubble()
    {
        if (Unassigned) return;
        Tween tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 0.2f);
        tween.TweenCallback(Callable.From(Finger.StopBounce));
    }
}