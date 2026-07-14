using Godot;
using Path = System.IO.Path;

namespace OmoriSandbox.Modding;

/// <summary>
/// A helper class to build SpriteFrames from a texture atlas.
/// </summary>
public class SpriteFramesBuilder
{
    private SpriteFrames SpriteFrames;
    private Texture2D Texture;
    private int Width;
    private int Height;
    private int Columns;

    /// <summary>
    /// Creates a new SpriteFramesBuilder.<br/>You can call <see cref="AddAnimation"/> to different emotions to the list of animations.
    /// </summary>
    /// <param name="atlasPath">The path to the atlas file. Must be a full path from the root mods folder.<br/>
    /// Example: <c>MyMod/actors/MyActor/atlas.png</c></param>
    /// <param name="width">The width of a single sprite in the atlas.</param>
    /// <param name="height">The height of a single sprite in the atlas.</param>
    public SpriteFramesBuilder(string atlasPath, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(atlasPath) || atlasPath.Contains("..") || atlasPath.Contains("://") ||
            Path.IsPathRooted(atlasPath))
        {
            GD.PushError($"Invalid atlas path '{atlasPath}' (path traversal not allowed)");
            return;
        }
        if (!FileAccess.FileExists("user://mods/" + atlasPath))
        {
            GD.PushError("Failed to find atlas at path: user://mods/" + atlasPath);
            return;
        }
        SpriteFrames = new();
        Texture = ImageTexture.CreateFromImage(Image.LoadFromFile("user://mods/" + atlasPath));
        Width = width;
        Height = height;
        Columns = Texture.GetWidth() / Width;
        if (Columns == 0)
        {
            GD.PushError("Loaded atlas with zero columns! Double check the width and height!\nuser://mods/" + atlasPath);
            SpriteFrames = null;
        }
    }

    /// <summary>
    /// Adds an animation to the current SpriteFramesBuilder.<br/>
    /// </summary>
    /// <param name="animationId">The ID this animation corresponds to.</param>
    /// <param name="fps">The FPS of the animation.</param>
    /// <param name="indices">A list of indices into the atlas. Index 0 would be the top left of your atlas, and increments going left to right.</param>
    /// <returns></returns>
    public SpriteFramesBuilder AddAnimation(string animationId, double fps, params int[] indices)
    {
        // return early if the builder was never properly initialized
        if (SpriteFrames == null)
            return this;
        
        if (SpriteFrames.HasAnimation(animationId))
        {
            GD.PushWarning($"SpriteFrames already has an animation named {animationId}, skipping!");
            return this;
        }
        
        SpriteFrames.AddAnimation(animationId);
        SpriteFrames.SetAnimationSpeed(animationId, fps);
        SpriteFrames.SetAnimationLoop(animationId, true);
        foreach (int index in indices)
        {
            int column = index % Columns;
            int row = index / Columns;
            AtlasTexture tex = new()
            {
                Atlas = Texture,
                Region = new Rect2(column * Width, row * Height, Width, Height)
            };
            SpriteFrames.AddFrame(animationId, tex);
        }

        return this;
    }

    /// <summary>
    /// Disables looping on an existing animation on the current SpriteFramesBuilder.
    /// </summary>
    /// <param name="animationId">The animation ID to disable looping on.</param>
    /// <returns></returns>
    public SpriteFramesBuilder DisableAnimationLoop(string animationId)
    {
        // return early if the builder was never properly initialized
        if (SpriteFrames == null)
            return this;
        
        if (!SpriteFrames.HasAnimation(animationId))
        {
            GD.PushWarning($"SpriteFrames does not have an animation named {animationId}, skipping!");
            return this;
        }
        
        SpriteFrames.SetAnimationLoop(animationId, false);
        return this;
    }

    /// <returns>The built <see cref="SpriteFrames"/> object to use.</returns>
    public SpriteFrames Build()
    {
        return SpriteFrames;
    }
}