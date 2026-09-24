using Godot;
using OmoriSandbox.Actors;

namespace OmoriSandbox;

internal partial class CursorBounce : Sprite2D
{
	[Export] private BounceDirection Direction = BounceDirection.Horizontal;
	[Export] private float Amplitude = 2.9f;
	[Export] private float HalfPeriod = 20f / 60f;
	private ShaderMaterial Grayscale;

	private Tween Tween;

	public override void _Ready()
	{
		Grayscale = ResourceLoader.Load<ShaderMaterial>("res://assets/grayscale_shader.tres");

		Tween = CreateTween();
		Tween.SetTrans(Tween.TransitionType.Sine);
		string direction = Direction == BounceDirection.Horizontal ? "offset:x" : "offset:y";
		Tween.TweenProperty(this, direction, Amplitude, HalfPeriod);
		Tween.TweenProperty(this, direction, -Amplitude, HalfPeriod);
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
