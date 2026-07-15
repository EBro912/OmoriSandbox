using Godot;
using System.Collections.Generic;
using System.Linq;

namespace OmoriSandbox.Animation;

internal class RPGMAnimatedSprite
{
	public static readonly int SIZE = 192;
	private const float DIVIDEND = 0.03529411764705882f;

	public int Id { get; private set; }
	public int Layer { get; private set; }
	private readonly Texture2D Texture;
	private readonly Texture2D AltTexture;
	private readonly List<List<Frame>> Frames = [];
	private readonly Dictionary<int, List<SFX>> FrameSFX = [];
	private readonly Dictionary<int, Shake> FrameShake = [];
	private readonly int Columns;
	private readonly int AltColumns;

	public RPGMAnimatedSprite(int id, int layer, Texture2D texture, Texture2D altTexture)
	{
		Id = id;
		Layer = layer;
		Texture = texture;
		AltTexture = altTexture;
		if (Texture == null && AltTexture == null)
		{
			GD.PushError($"Created an animation with no textures! (ID: {Id})");
			return;
		}
		Columns = texture?.GetWidth() / SIZE ?? 0;
		AltColumns = altTexture?.GetWidth() / SIZE ?? 0;
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

	// avoid creating a fresh AtlasTexture per cell per redraw
	private readonly Dictionary<int, AtlasTexture> TextureCache = [];

	public AtlasTexture GetTextureAt(int pattern)
	{
		if (pattern < 0)
			return null;

		if (TextureCache.TryGetValue(pattern, out AtlasTexture cached))
			return cached;

		AtlasTexture result = BuildTextureAt(pattern);
		TextureCache[pattern] = result;
		return result;
	}

	private AtlasTexture BuildTextureAt(int pattern)
	{
		if (Texture != null && pattern < 100)
		{
			int column = pattern % Columns;
			int row = pattern / Columns;
			return new AtlasTexture
			{
				Atlas = Texture,
				Region = new Rect2(column * SIZE, row * SIZE, SIZE, SIZE)
			};
		}

		if (AltTexture != null)
		{
			// RPGMaker allocates 100 frame slots to each image even if the image doesn't have that many sprites
			int adjusted = pattern - 100;
			if (adjusted < 199)
			{
				int column = adjusted % AltColumns;
				int row = adjusted / AltColumns;
				return new AtlasTexture
				{
					Atlas = AltTexture,
					Region = new Rect2(column * SIZE, row * SIZE, SIZE, SIZE)
				};
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

	public IEnumerable<List<SFX>> AllSFX => FrameSFX.Values;

	public int FrameCount => Frames.Count;
}

internal struct Frame(int pattern = 0, float x = 0, float y = 0, float scale = 100, float rotation = 0, bool mirror = false, float opacity = 255, int blend = 0)
{
	public readonly int Pattern = pattern;
	public readonly float X = x;
	public readonly float Y = y;
	public readonly float Scale = scale;
	public readonly float Rotation = rotation;
	public readonly bool Mirror = mirror;
	public readonly float Opacity = opacity;
	// RPGM blend modes: 0 = normal, 1 = additive
	public readonly bool Additive = blend == 1;
}

internal struct SFX(string name, float pitch = 100f, float volume = 100f)
{
	public readonly string Name = name;
	public readonly float Pitch = pitch;
	public readonly float Volume = volume;
}

/// <summary>
/// Creates a new screen shake.
/// </summary>
/// <param name="power">The power of the shake. Normal values are 0-255.</param>
/// <param name="speed">The speed of the shake. Normal values are 0-255.</param>
/// <param name="duration">The duration (in frames) of the shake.</param>
public struct Shake(float power, float speed, int duration)
{
	/// <summary>
	/// The power of the shake.
	/// </summary>
	public readonly float Power = power;
	/// <summary>
	/// The speed of the shake.
	/// </summary>
	public readonly float Speed = speed;
	/// <summary>
	/// The duration of the shake, in frames.
	/// </summary>
	public readonly int Duration = duration;
}
