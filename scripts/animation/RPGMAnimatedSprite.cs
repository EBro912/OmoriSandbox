using Godot;
using System.Collections.Generic;

public class RPGMAnimatedSprite
{
    public static readonly int SIZE = 192;
    private const float DIVIDEND = 0.03529411764705882f;

    public string Name { get; private set; }
    public int Layer { get; private set; }
    private AtlasTexture Texture;
    private readonly List<List<Frame>> Frames = [];
    private readonly Dictionary<int, string> FrameSFX = [];
    private readonly Dictionary<int, Shake> FrameShake = [];
    private readonly int Columns;

    public RPGMAnimatedSprite(string name, int layer, Texture2D texture)
    {
        Name = name;
        Layer = layer;
        Texture = new()
        {
            Atlas = texture
        };
        Columns = texture.GetWidth() / SIZE;
    }

    public void CreateFrame(List<Frame> frames)
    {
        Frames.Add(frames);
    }

    public void SetFrameSFX(int frame, string sfx)
    {
        FrameSFX[frame] = sfx;
    }

    public void SetFrameShake(int frame, int power, int speed, int duration)
    {
        FrameShake[frame] = new Shake(power * DIVIDEND, speed * DIVIDEND, duration);
    }

    public AtlasTexture GetTextureAt(int pattern)
    {
        int column = pattern % Columns;
        int row = pattern / Columns;
        Texture.Region = new Rect2(column * SIZE, row * SIZE, SIZE, SIZE);
        return Texture;
    }

    public List<Frame> GetFrame(int frame)
    {
        return Frames[frame];
    }

    public bool TryGetFrameSFX(int frame, out string sfx)
    {
        return FrameSFX.TryGetValue(frame, out sfx);
    }

    public bool TryGetFrameShake(int frame, out Shake shake)
    {
        return FrameShake.TryGetValue(frame, out shake);
    }

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

public struct Shake(float power, float speed, int duration)
{
    public readonly float Power = power;
    public readonly float Speed = speed;
    public readonly int Duration = duration;
}