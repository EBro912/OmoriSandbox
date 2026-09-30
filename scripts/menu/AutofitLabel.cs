using Godot;

namespace OmoriSandbox.Menu;

/// <summary>
/// A <see cref="Label"/> that squeezes its text horizontally to fit its width.
/// </summary>
public partial class AutofitLabel : Label
{
    /// <summary>
    /// The font size of the label.
    /// </summary>
    [Export] public int MaxSize = 24;

    /// <summary>
    /// Sets the Label's text to <paramref name="text"/>, squeezing it horizontally if it is wider than the label.
    /// </summary>
    /// <param name="text">The text to set.</param>
    public void SetFittedText(string text)
    {
        Text = text;
        Fit();
    }

    private void Fit()
    {
        AddThemeFontSizeOverride("font_size", MaxSize);
        float width = OffsetRight - OffsetLeft;
        float textWidth = GetThemeFont("font").GetStringSize(Text, fontSize: MaxSize).X;
        Scale = new Vector2(textWidth > width ? width / textWidth : 1f, 1f);
    }
}
