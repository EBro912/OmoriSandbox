using System.Numerics;
using Godot;
using Vector2 = Godot.Vector2;

namespace OmoriSandbox.Animation;

internal partial class PlayingAnimation : Node2D
{
	[Signal]
	public delegate void FinishedEventHandler();

	public int CurrentFrame { get; private set; } = 0;
	public readonly RPGMAnimatedSprite Animation;
	public Vector2 DrawPosition { get; private set; }

	// additive cells can't share a CanvasItem with normal cells, so they draw on a child
	// layer with an additive blend material
	private readonly AdditiveLayer Additive;

	public PlayingAnimation(RPGMAnimatedSprite animation, Vector2 drawPosition, int layer)
	{
		Animation = animation;
		DrawPosition = drawPosition;
		ZIndex = layer;
		Additive = new AdditiveLayer(this);
		AddChild(Additive);
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
			Vector2 center = -texture.GetSize() / 2f;
			Vector2 scale = new(frame.Scale / 100f, frame.Scale / 100f);
			if (frame.Mirror)
				scale.X *= -1;
			canvas.DrawSetTransform(DrawPosition + new Vector2(frame.X, frame.Y), Mathf.DegToRad(frame.Rotation), scale);
			canvas.DrawTexture(texture, center, new Color(1f, 1f, 1f, frame.Opacity / 255f));
		}
	}

	public bool AdvanceFrame()
	{
		CurrentFrame++;
		if (CurrentFrame >= Animation.FrameCount)
			return true;
		QueueRedraw();
		Additive.QueueRedraw();
		return false;
	}

	private sealed partial class AdditiveLayer : Node2D
	{
		private readonly PlayingAnimation Owner;

		public AdditiveLayer(PlayingAnimation owner)
		{
			Owner = owner;
			Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
		}

		public override void _Draw()
		{
			Owner.DrawCells(this, additive: true);
		}
	}
}
