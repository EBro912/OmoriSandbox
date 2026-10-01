using Godot;
using Path = System.IO.Path;

namespace OmoriSandbox;

/// <summary>
/// A battleback that endlessly scrolls a repeating texture. Register it from a mod with <c>Mod.RegisterBattleback</c>.
/// </summary>
public sealed class ScrollingBattleback : IBattleback
{
    /// <inheritdoc/>
    public string Name { get; init; }
    /// <inheritdoc/>
    public int FrameCount => 1;

    /// <summary>
    /// How fast the texture scrolls, in pixels per second. Positive X scrolls right, positive Y scrolls down.
    /// </summary>
    public Vector2 ScrollSpeed { get; }

    /// <summary>
    /// Whether the texture also repeats above and below itself.
    /// </summary>
    /// <remarks>
    /// When false, it's drawn as one centered row like other battlebacks, and the Y part of <see cref="ScrollSpeed"/> is ignored.
    /// It always repeats horizontally.
    /// </remarks>
    public bool TileVertically { get; }

    private readonly string TexturePath;
    private Texture2D Texture;
    private bool LoadFailed;

    /// <summary>
    /// Creates a scrolling battleback. Its <see cref="Name"/> defaults to the texture's file name without the extension,
    /// and can be changed with an object initializer.
    /// </summary>
    /// <param name="texturePath">The path to the texture. Must be a full path from the root mods folder.<br/>
    /// Example: <c>MyMod/battlebacks/stars.png</c></param>
    /// <param name="scrollSpeed">How fast the texture scrolls, in pixels per second.</param>
    /// <param name="tileVertically">Whether the texture also repeats vertically. See <see cref="TileVertically"/>.</param>
    public ScrollingBattleback(string texturePath, Vector2 scrollSpeed, bool tileVertically = true)
    {
        ScrollSpeed = scrollSpeed;
        TileVertically = tileVertically;
        if (string.IsNullOrWhiteSpace(texturePath) || texturePath.Contains("..") || texturePath.Contains("://") ||
            Path.IsPathRooted(texturePath))
        {
            GD.PushError($"Invalid battleback path '{texturePath}' (path traversal not allowed)");
            LoadFailed = true;
            return;
        }
        Name = texturePath.GetFile().GetBaseName();
        TexturePath = "user://mods/" + texturePath;
    }

    /// <inheritdoc/>
    public Texture2D GetFrame(int index)
    {
        if (Texture != null || LoadFailed)
            return Texture;

        Image image = FileAccess.FileExists(TexturePath) ? Image.LoadFromFile(TexturePath) : null;
        if (image != null)
            Texture = ImageTexture.CreateFromImage(image);
        else
        {
            LoadFailed = true;
            GD.PushError("Failed to load battleback: " + TexturePath);
        }
        return Texture;
    }

    /// <inheritdoc/>
    public double GetFrameDelay(int index)
    {
        return 0;
    }
}
