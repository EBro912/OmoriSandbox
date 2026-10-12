using System.Numerics;
using Godot;
using OmoriSandbox.Actors;
using Vector2 = Godot.Vector2;

namespace OmoriSandbox.Animation;

internal partial class PlayingAnimation : Node2D
{
	[Signal]
	public delegate void FinishedEventHandler();

	private const int Rate = 4;

	public int CurrentFrame { get; private set; } = 0;
	internal float FrameTimer;
	public readonly RPGMAnimatedSprite Animation;
	public Vector2 DrawPosition { get; private set; }
	public readonly Actor Target;

	internal Color ScreenFlashColor;
	internal int ScreenFlashDuration;
	internal Color TargetFlashColor;
	internal int TargetFlashDuration;
	internal int HidingDuration;
	private bool FlashedTarget;
	private bool HidTarget;
	internal Rect2 ScreenFlashRect => DisplayLayout.Instance.VisibleBattleRect.Grow(32);

	// additive cells can't share a CanvasItem with normal cells, so they draw on a child
	// layer with an additive blend material
	private readonly AdditiveLayer Additive;
	private readonly ScreenFlashLayer ScreenFlash;

	public PlayingAnimation(RPGMAnimatedSprite animation, Vector2 drawPosition, int layer, Actor target = null)
	{
		Animation = animation;
		DrawPosition = drawPosition;
		ZIndex = layer;
		Target = target;
		Additive = new AdditiveLayer(this);
		AddChild(Additive);
		ScreenFlash = new ScreenFlashLayer(this);
		AddChild(ScreenFlash);
		QueueRedraw();
	}

	public override void _Draw()
	{
		DrawCells(this, additive: false);
	}

	private void DrawCells(CanvasItem canvas, bool additive)
	{
		foreach (Frame frame in Animation.GetFrame(CurrentFrame))
		{
			if (frame.Additive != additive) continue;
			AtlasTexture texture = Animation.GetTextureAt(frame.Pattern);
			if (texture == null) continue;
			Vector2 half = texture.GetSize() / 2f;
			Vector2 scale = new(frame.Scale / 100f, frame.Scale / 100f);
			if (frame.Mirror)
				scale.X *= -1;
			Transform2D cell = new(Mathf.DegToRad(frame.Rotation), scale, 0f, DrawPosition + new Vector2(frame.X, frame.Y));
			Vector2[] points =
			[
				Truncate(cell * new Vector2(-half.X, -half.Y)),
				Truncate(cell * new Vector2(half.X, -half.Y)),
				Truncate(cell * new Vector2(half.X, half.Y)),
				Truncate(cell * new Vector2(-half.X, half.Y))
			];
			Rect2 region = texture.Region;
			Vector2 atlasSize = texture.Atlas.GetSize();
			Vector2[] uvs =
			[
				region.Position / atlasSize,
				new Vector2(region.End.X, region.Position.Y) / atlasSize,
				region.End / atlasSize,
				new Vector2(region.Position.X, region.End.Y) / atlasSize
			];
			Color color = new(1f, 1f, 1f, frame.Opacity / 255f);
			Color[] colors = [color, color, color, color];
			canvas.DrawPrimitive(points, colors, uvs, texture.Atlas);
		}
	}

	private static Vector2 Truncate(Vector2 v) => new(System.MathF.Truncate(v.X), System.MathF.Truncate(v.Y));

	public bool AdvanceFrame()
	{
		CurrentFrame++;
		if (CurrentFrame >= Animation.FrameCount)
			return true;
		QueueRedraw();
		Additive.QueueRedraw();
		return false;
	}

	public void StartFlash(Flash flash)
	{
		int duration = flash.Duration * Rate;
		switch (flash.Scope)
		{
			case 1:
				TargetFlashColor = flash.Color;
				TargetFlashDuration = duration;
				FlashedTarget = true;
				UpdateTargetFlash();
				break;
			case 2:
				ScreenFlashColor = flash.Color;
				ScreenFlashDuration = duration;
				ScreenFlash.QueueRedraw();
				UpdateScreenFlash();
				break;
			case 3:
				HidingDuration = duration;
				HidTarget = true;
				SetTargetVisible(false);
				UpdateHiding();
				break;
		}
	}

	public void UpdateFlashes()
	{
		UpdateTargetFlash();
		UpdateScreenFlash();
		UpdateHiding();
	}

	public void RestoreTarget()
	{
		if (FlashedTarget)
			(Target as Enemy)?.SetBlendColor(new Color(0, 0, 0, 0));
		if (HidTarget)
			SetTargetVisible(true);
	}

	internal void SetTone(Material tone, Material additiveTone)
	{
		Material = tone;
		Additive.SetTone(additiveTone);
	}

	private void UpdateTargetFlash()
	{
		if (TargetFlashDuration <= 0)
			return;
		int d = TargetFlashDuration--;
		TargetFlashColor.A *= (d - 1f) / d;
		(Target as Enemy)?.SetBlendColor(TargetFlashColor);
	}

	private void UpdateHiding()
	{
		if (HidingDuration <= 0)
			return;
		HidingDuration--;
		if (HidingDuration == 0)
		{
			HidTarget = false;
			SetTargetVisible(true);
		}
	}

	private void SetTargetVisible(bool visible)
	{
		if (Target is Enemy enemy && IsInstanceValid(enemy.Sprite))
			enemy.Sprite.Visible = visible;
	}

	private void UpdateScreenFlash()
	{
		if (ScreenFlashDuration <= 0)
			return;
		int d = ScreenFlashDuration--;
		ScreenFlashColor.A *= (d - 1f) / d;
		ScreenFlash.Visible = ScreenFlashDuration > 0;
		ScreenFlash.QueueRedraw();
	}

	private sealed partial class AdditiveLayer : Node2D
	{
		private readonly PlayingAnimation Owner;
		private readonly Material Blend = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };

		public AdditiveLayer(PlayingAnimation owner)
		{
			Owner = owner;
			Material = Blend;
		}

		public void SetTone(Material tone)
		{
			Material = tone ?? Blend;
		}

		public override void _Draw()
		{
			Owner.DrawCells(this, additive: true);
		}
	}

	private sealed partial class ScreenFlashLayer : Node2D
	{
		private readonly PlayingAnimation Playing;

		public ScreenFlashLayer(PlayingAnimation playing)
		{
			Playing = playing;
			UseParentMaterial = true;
		}

		public override void _Draw()
		{
			if (Playing.ScreenFlashColor.A > 0f)
				DrawRect(Playing.ScreenFlashRect, Playing.ScreenFlashColor);
		}
	}
}
