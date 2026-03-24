using Godot;

namespace OmoriSandbox;

/// <summary>
/// Represents a Battleback that can be displayed on a <see cref="TextureRect"/>.
/// </summary>
public interface IBattleback
{
    /// <summary>
    /// The name of the battleback. Used in menus.
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// The number of frames this battleback has.
    /// </summary>
    public int FrameCount { get; }
    /// <summary>
    /// Retrieves the <see cref="Texture2D"/> at the given frame index.
    /// </summary>
    /// <param name="index">The frame index to retrieve.</param>
    public Texture2D GetFrame(int index);
    /// <summary>
    /// Retrieves the frame delay at the given frame index.
    /// </summary>
    /// <param name="index">The frame index to retrieve.</param>
    public double GetFrameDelay(int index);
}
