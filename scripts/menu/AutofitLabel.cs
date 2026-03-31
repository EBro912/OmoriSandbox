using Godot;

namespace OmoriSandbox.Menu;

/// <summary>
/// A <see cref="Label"/> that can auto-fit its contents.
/// </summary>
public partial class AutofitLabel : Label
{
    /// <summary>
    /// The maximum font size of the label.
    /// </summary>
    [Export] public int MaxSize = 24;
    /// <summary>
    /// The minimum font size of the label
    /// </summary>
    [Export] public int MinSize = 16;

    /// <summary>
    /// Sets the Label's text to <paramref name="text"/> and auto-sizes it to fit.
    /// </summary>
    /// <param name="text">The text to set.</param>
    public void SetFittedText(string text)
    {
        Text = text;
        Fit();
    }

    private void Fit()
    {
        Font font = GetThemeFont("font");
        
        for (int size = MaxSize; size >= MinSize; size--)
        {
            Vector2 textSize = font.GetStringSize(Text, fontSize: size);
            if (textSize.X <= Size.X && textSize.Y <= Size.Y)
            {
                AddThemeFontSizeOverride("font_size", size);
                return;
            }
        }
        
        AddThemeFontSizeOverride("font_size", MinSize);
    }
}