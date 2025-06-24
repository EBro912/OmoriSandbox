using Godot;

public partial class CursorBounce : Sprite2D
{
    public override void _Ready()
    {
        Tween tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(this, "offset:x", 3f, 0.25f);
        tween.TweenProperty(this, "offset:x", -3f, 0.25f);
        tween.TweenInterval(0.1f);
        tween.SetLoops();
    }
}