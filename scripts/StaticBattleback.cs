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
    private Texture2D Texture { get; }

    /// <summary>
    /// Represents a static battleback with no animation.
    /// </summary>
    /// <param name="resourcePath">The full or Godot-like path to the resource.</param>
    public StaticBattleback(string resourcePath)
    {
        Name = resourcePath.GetFile().GetBaseName();
        if (ResourceLoader.Exists(resourcePath))
            Texture = ResourceLoader.Load<Texture2D>(resourcePath);
        else
            Texture = ImageTexture.CreateFromImage(Image.LoadFromFile(resourcePath));
    }
    
    /// <inheritdoc/>
    public Texture2D GetFrame(int index)
    {
        return Texture;
    }
    
    /// <inheritdoc/>
    public double GetFrameDelay(int index)
    {
        return 0;
    }
}
