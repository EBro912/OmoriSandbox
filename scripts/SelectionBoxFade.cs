using Godot;

public partial class SelectionBoxFade : TextureRect
{
    public override void _Ready()
    {
        Tween tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(this, "modulate:a", 0f, 0.5f);
        tween.TweenProperty(this, "modulate:a", 1f, 0.5f);
        tween.TweenInterval(0.5f);
        tween.SetLoops();
    }
}