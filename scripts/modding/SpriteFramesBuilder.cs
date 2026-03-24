using Godot;

namespace OmoriSandbox.Modding;

/// <summary>
/// A helper class to build SpriteFrames from a texture atlas.
/// </summary>
public class SpriteFramesBuilder
{
    private SpriteFrames spriteFrames;
    private Texture2D texture;
    private int Width;
    private int Height;
    private int Columns;

    /// <summary>
    /// Creates a new SpriteFramesBuilder.<br/>You can call <see cref="AddEmotion(string, double, int[])"/> to different emotions to the list of animations.
    /// </summary>
    /// <param name="atlasPath">The path to the atlas file. Must be a full path from the root mods folder.<br/>
    /// Example: <c>MyMod/actors/MyActor/atlas.png</c></param>
    /// <param name="width">The width of a single sprite in the atlas.</param>
    /// <param name="height">The height of a single sprite in the atlas.</param>
    public SpriteFramesBuilder(string atlasPath, int width, int height)
    {
        if (!FileAccess.FileExists("user://mods/" + atlasPath))
        {
            GD.PushError("Failed to find atlas at path: user://mods/" + atlasPath);
            return;
        }
        spriteFrames = new();
        texture = ImageTexture.CreateFromImage(Image.LoadFromFile("user://mods/" + atlasPath));
        Width = width;
        Height = height;
        Columns = texture.GetWidth() / Width;
        if (Columns == 0)
            GD.PushError("Loaded atlas with zero columns! Double check the width and height!\nuser://mods/" + atlasPath);
    }

    /// <summary>
    /// Adds an emotion animation to the current SpriteFramesBuilder.<br/>
    /// </summary>
    /// <param name="emotion">The emotion this animation corresponds to.</param>
    /// <param name="fps">The FPS of the animation.</param>
    /// <param name="indices">A list of indices into the atlas. Index 0 would be the top left of your atlas, and increments going left to right.</param>
    /// <returns></returns>
    public SpriteFramesBuilder AddEmotion(string emotion, double fps, params int[] indices)
    {
        if (spriteFrames.HasAnimation(emotion))
        {
            GD.PushWarning($"SpriteFrames already has an animation named {emotion}, skipping!");
            return this;
        }
        
        spriteFrames.AddAnimation(emotion);
        spriteFrames.SetAnimationSpeed(emotion, fps);
        spriteFrames.SetAnimationLoop(emotion, true);
        foreach (int index in indices)
        {
            int column = index % Columns;
            int row = index / Columns;
            AtlasTexture tex = new()
            {
                Atlas = texture,
                Region = new Rect2(column * Width, row * Height, Width, Height)
            };
            spriteFrames.AddFrame(emotion, tex);
        }

        return this;
    }

    /// <returns>The built <see cref="SpriteFrames"/> object to use.</returns>
    public SpriteFrames Build()
    {
        return spriteFrames;
    }
}