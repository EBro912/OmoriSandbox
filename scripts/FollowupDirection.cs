using Godot;

namespace OmoriSandbox;

internal partial class FollowupDirection : Sprite2D
{
    [Export] public int Cost { get; private set; } = 3;
    [Export] public InputDirection InputDir { get; private set; }
    public bool Available { get; private set; }
    private CursorBounce Finger;

    public override void _Ready()
    {
        Finger = GetChild<CursorBounce>(0);
        Finger.StopBounce();
        Modulate = Colors.Transparent;
    }

    public void ShowBubble(bool targetAvailable)
    {
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
        Tween tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 0.2f);
        tween.TweenCallback(Callable.From(Finger.StopBounce));
    }
}