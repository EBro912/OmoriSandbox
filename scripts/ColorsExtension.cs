using Godot;

namespace OmoriSandbox.Extensions;

/// <summary>
/// An extension upon the Godot <see cref="Colors"/>
/// </summary>
public static class ColorsExtension
{
    /// <summary>
    /// A transparency with black RGB values.
    /// </summary>
    public static Color TransparentBlack => new(0U);
}