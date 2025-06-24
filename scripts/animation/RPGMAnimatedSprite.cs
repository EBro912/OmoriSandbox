using Godot;
using System.Collections.Generic;
using System.Linq;

public class RPGMAnimatedSprite
{
	public static readonly int SIZE = 192;
	private const float DIVIDEND = 0.03529411764705882f;

	public int Id { get; private set; }
	public int Layer { get; private set; }
	private AtlasTexture Texture;
	private AtlasTexture AltTexture;
	private readonly List<List<Frame>> Frames = [];
	private readonly Dictionary<int, List<SFX>> FrameSFX = [];
	private readonly Dictionary<int, Shake> FrameShake = [];
	private readonly int Columns;

	public RPGMAnimatedSprite(int id, int layer, Texture2D texture)
	{
		Id = id;
		Layer = layer;
		Texture = new()
		{
			Atlas = texture
		};
		Columns = texture.GetWidth() / SIZE;
	}

	public RPGMAnimatedSprite(int id, int layer, Texture2D texture, Texture2D altTexture)
	{
		Id = id;
		Layer = layer;
		Texture = new()
		{
			Atlas = texture
		};
		AltTexture = new()
		{
			Atlas = altTexture
		};
		Columns = texture.GetWidth() / SIZE;
	}

	public void CreateFrame(List<Frame> frames)
	{
		Frames.Add(frames);
	}

	public void SetFrameSFX(int frame, SFX sfx)
	{
		if (FrameSFX.TryGetValue(frame, out List<SFX> value))
		{
			value.Add(sfx);
		}
		else
		{
			FrameSFX[frame] = [sfx];
		}
	}

	public void SetFrameShake(int frame, int power, int speed, int duration)
	{
		FrameShake[frame] = new Shake(power * DIVIDEND, speed * DIVIDEND, duration);
	}

	public AtlasTexture GetTextureAt(int pattern)
	{
		if (pattern < 99)
		{
			int column = pattern % Columns;
			int row = pattern / Columns;
			Texture.Region = new Rect2(column * SIZE, row * SIZE, SIZE, SIZE);
			return Texture;
		}
		else if (AltTexture != null)
		{
			// RPGMaker allocates 100 frame slots to each image even if the image doesn't have that many sprites
			int adjusted = pattern - 100;
			if (adjusted < 199)
			{
				int column = adjusted % Columns;
				int row = adjusted / Columns;
				AltTexture.Region = new Rect2(column * SIZE, row * SIZE, SIZE, SIZE);
				return AltTexture;
			}
		}

		GD.PrintErr($"Invalid pattern number {pattern} for animation {Id}");
		return null;
	}

	public List<Frame> GetFrame(int frame)
	{
		return Frames[frame];
	}

	public bool TryGetFrameSFX(int frame, out List<SFX> sfx)
	{
		return FrameSFX.TryGetValue(frame, out sfx);
	}

	public bool TryGetFrameShake(int frame, out Shake shake)
	{
		return FrameShake.TryGetValue(frame, out shake);
	}

	public IEnumerable<List<SFX>> AllSFX => FrameSFX.Values.ToList();

	public int FrameCount => Frames.Count;
}

public struct Frame(int pattern = 0, float x = 0, float y = 0, float scale = 100, float rotation = 0, bool mirror = false, float opacity = 255)
{
	public readonly int Pattern = pattern;
	public readonly float X = x;
	public readonly float Y = y;
	public readonly float Scale = scale;
	public readonly float Rotation = rotation;
	public readonly bool Mirror = mirror;
	public readonly float Opacity = opacity;
}

public struct SFX(string name, float pitch = 100f, float volume = 100f)
{
	public readonly string Name = name;
	public readonly float Pitch = pitch;
	public readonly float Volume = volume;
}

public struct Shake(float power, float speed, int duration)
{
	public readonly float Power = power;
	public readonly float Speed = speed;
	public readonly int Duration = duration;
}
