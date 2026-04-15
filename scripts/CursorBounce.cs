using Godot;
using OmoriSandbox.Actors;

namespace OmoriSandbox;

internal partial class CursorBounce : Sprite2D
{
	[Export] private BounceDirection Direction = BounceDirection.Horizontal;
	private ShaderMaterial Grayscale;

	private Tween Tween;

	public override void _Ready()
	{
		Grayscale = ResourceLoader.Load<ShaderMaterial>("res://assets/grayscale_shader.tres");

        Tween = CreateTween();
        Tween.SetTrans(Tween.TransitionType.Sine);
        string direction = Direction == BounceDirection.Horizontal ? "offset:x" : "offset:y";
        Tween.TweenProperty(this, direction, 3f, 0.25f);
        Tween.TweenProperty(this, direction, -3f, 0.25f);
        Tween.TweenInterval(0.1f);
        Tween.SetLoops();
    }

	public void StartBounce()
	{
		Tween.Play();
		Material = null;
	}

	public void StopBounce()
	{
		Tween.Stop();
		Offset = Vector2.Zero;
		Material = Grayscale;
    }

	private enum BounceDirection
	{
		Horizontal,
		Vertical
    }
}
