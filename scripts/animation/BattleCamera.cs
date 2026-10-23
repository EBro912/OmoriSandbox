using Godot;

namespace OmoriSandbox.Animation;

internal sealed class BattleCamera
{
	private const float Width = 640f;
	private const float Height = 480f;

	private float ZoomTarget = 1f;
	private int ZoomDuration;
	private Vector2 Offset;
	private int CameraDuration;

	internal float Scale { get; private set; } = 1f;
	internal Vector2 Point { get; private set; } = new(Width / 2f, Height / 2f);

	internal void StartZoom(float scale, int frames)
	{
		ZoomTarget = scale;
		ZoomDuration = frames;
	}

	internal void SetOffset(float x, float y)
	{
		Offset = new Vector2(x, y);
	}

	internal void ResetCamera(int frames)
	{
		Offset = Vector2.Zero;
		CameraDuration = frames;
	}

	internal void ResetInstant()
	{
		Scale = 1f;
		ZoomTarget = 1f;
		ZoomDuration = 0;
		Offset = Vector2.Zero;
		CameraDuration = 0;
		Point = new Vector2(Width / 2f, Height / 2f);
	}

	internal void Tick()
	{
		if (ZoomDuration > 0)
		{
			Scale = (Scale * (ZoomDuration - 1) + ZoomTarget) / ZoomDuration;
			ZoomDuration--;
		}
		Vector2 target = new Vector2(Width / 2f, Height / 2f) + Offset;
		if (CameraDuration > 0)
		{
			Point = (Point * (CameraDuration - 1) + target) / CameraDuration;
			CameraDuration--;
		}
		else
			Point = target;
	}

	internal Vector2 FieldPosition()
	{
		float zoom = Scale;
		if (zoom < 1f)
			return new Vector2(RoundHalfUp((Width - Width * zoom) / 2f), RoundHalfUp((Height - Height * zoom) / 2f));
		float x = Mathf.Clamp(-Point.X * zoom + Width / 2f, Width - Width * zoom, 0f);
		float y = Mathf.Clamp(-Point.Y * zoom + Height / 2f, Height - Height * zoom, 0f);
		return new Vector2(RoundHalfUp(x), RoundHalfUp(y));
	}

	private static float RoundHalfUp(float value) => Mathf.Floor(value + 0.5f);
}
