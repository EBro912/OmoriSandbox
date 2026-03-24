using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace OmoriSandbox;

/// <summary>
/// Represents an animated battleback.
/// </summary>
public sealed class AnimatedBattleback : IBattleback
{
    /// <inheritdoc/>
    public string Name { get; init; }
    /// <inheritdoc/>
    public int FrameCount => Frames.Count;

    private readonly List<(Texture2D Texture, double Delay)> Frames = [];

    /// <summary>
    /// Represents an animated battleback.
    /// </summary>
    /// <param name="resourcePath">The full or Godot-like path to the resource.</param>
    public AnimatedBattleback(string resourcePath)
    {
        Name = resourcePath.GetFile().GetBaseName();
        using Image<Rgba32> gif = Image.Load<Rgba32>(ProjectSettings.GlobalizePath(resourcePath));
        byte[] rgba = new byte[gif.Width * gif.Height * 4];
        for (int i = 0; i < gif.Frames.Count; i++)
        {
            var frame = gif.Frames.CloneFrame(i);

            GifFrameMetadata frameMeta = frame.Frames[0].Metadata.GetGifMetadata();
            int delayCs = frameMeta.FrameDelay;
            if (delayCs <= 0)
                delayCs = 10;
            double frameDelay = delayCs / 100d;

            if (frame.DangerousTryGetSinglePixelMemory(out var memory))
            {
                MemoryMarshal.AsBytes(memory.Span).CopyTo(rgba);
            }
            else
            {
                frame.Frames[0].ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        var row = accessor.GetRowSpan(y);
                        for (int x = 0; x < accessor.Width; x++)
                        {
                            int offset = (y * accessor.Width + x) * 4;
                            rgba[offset] = row[x].R;
                            rgba[offset + 1] = row[x].G;
                            rgba[offset + 2] = row[x].B;
                            rgba[offset + 3] = row[x].A;
                        }
                    }
                });
            }
            Frames.Add((ImageTexture.CreateFromImage(Godot.Image.CreateFromData(frame.Width, frame.Height, false, Godot.Image.Format.Rgba8, rgba)), frameDelay));
        }
    }

    /// <inheritdoc/>
    public Texture2D GetFrame(int index)
    {
        return Frames[index].Texture;
    }
    
    /// <inheritdoc/>
    public double GetFrameDelay(int index)
    {
        return Frames[index].Delay;
    }
}
