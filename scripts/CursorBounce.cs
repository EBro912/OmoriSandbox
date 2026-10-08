using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox;

internal partial class CursorBounce : Sprite2D
{
	[Export] private BounceDirection Direction = BounceDirection.Horizontal;
	[Export] private float Amplitude = 2.9f;
	[Export] private float HalfPeriod = 20f / 60f;
	private ShaderMaterial Grayscale;
	private bool Bouncing = true;

	private Tween Tween;

	public override void _Ready()
	{
		Grayscale = ResourceLoader.Load<ShaderMaterial>("res://assets/grayscale_shader.tres");
		UseParentMaterial = true;

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
		Bouncing = true;
		Material = null;
		UseParentMaterial = true;
	}

	public void StopBounce()
	{
		Tween.Stop();
		Offset = Vector2.Zero;
		Bouncing = false;
		UseParentMaterial = false;
		RefreshGray();
    }

	internal void RefreshGray()
	{
		if (Bouncing)
			return;
		Node node = GetParent();
		while (node is CanvasItem { UseParentMaterial: true } item)
			node = item.GetParent();
		Material = node is CanvasItem { Material: not null } ? AnimationManager.Instance.UIToneGrayMaterial : Grayscale;
	}

	private enum BounceDirection
	{
		Horizontal,
		Vertical
    }
}
