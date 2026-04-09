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

	public PlayingAnimation(RPGMAnimatedSprite animation, Vector2 drawPosition, int layer)
	{
		Animation = animation;
		DrawPosition = drawPosition;
		ZIndex = layer;
		QueueRedraw();
	}

	public override void _Draw()
	{
		foreach (Frame frame in Animation.GetFrame(CurrentFrame))
		{
			AtlasTexture texture = Animation.GetTextureAt(frame.Pattern);
			if (texture == null) continue;
			Vector2 center = -texture.GetSize() / 2f;
			Vector2 scale = new(frame.Scale / 100f, frame.Scale / 100f);
			if (frame.Mirror)
				scale.X *= -1;
			DrawSetTransform(DrawPosition + new Vector2(frame.X, frame.Y), Mathf.DegToRad(frame.Rotation), scale);
			DrawTexture(texture, center, new Color(1f, 1f, 1f, frame.Opacity / 255f));
		}
	}

	public bool AdvanceFrame()
	{
		CurrentFrame++;
		if (CurrentFrame >= Animation.FrameCount)
			return true;
		QueueRedraw();
		return false;
	}
}
