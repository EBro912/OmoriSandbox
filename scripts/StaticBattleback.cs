using Godot;

namespace OmoriSandbox;

/// <summary>
/// Represents a static battleback with no animation.
/// </summary>
public sealed class StaticBattleback : IBattleback
{
    /// <inheritdoc/>
    public string Name { get; init; }
    /// <inheritdoc/>
    public int FrameCount => 1;
    private readonly string ResourcePath;
    private Texture2D Texture;
    private bool LoadFailed;

    /// <summary>
    /// Represents a static battleback with no animation.
    /// </summary>
    /// <param name="resourcePath">The full or Godot-like path to the resource.</param>
    public StaticBattleback(string resourcePath)
    {
        Name = resourcePath.GetFile().GetBaseName();
        ResourcePath = resourcePath;
    }

    /// <inheritdoc/>
    public Texture2D GetFrame(int index)
    {
        // loaded lazily to help save memory on large battlebacks
        if (Texture != null || LoadFailed)
            return Texture;

        if (ResourceLoader.Exists(ResourcePath))
            Texture = ResourceLoader.Load<Texture2D>(ResourcePath);
        else
        {
            Image image = Image.LoadFromFile(ResourcePath);
            if (image != null)
                Texture = ImageTexture.CreateFromImage(image);
        }

        if (Texture == null)
        {
            LoadFailed = true;
            GD.PushError("Failed to load battleback: " + ResourcePath);
        }
        return Texture;
    }
    
    /// <inheritdoc/>
    public double GetFrameDelay(int index)
    {
        return 0;
    }
}
