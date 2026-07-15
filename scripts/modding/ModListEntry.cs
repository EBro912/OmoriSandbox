using Godot;

namespace OmoriSandbox.Modding;

internal partial class ModListEntry : Control
{
    [Export] private TextureRect Icon;
    [Export] private Label NameVersion;
    [Export] private Label Author;
    [Export] private Label Description;

    public void SetData(ModMetadata data)
    {
        NameVersion.Text = $"{data.Name} v{data.Version}";
        Author.Text = "by " + data.Author;
        Description.Text = data.Description;
    }

    public void SetIcon(Texture2D icon)
    {
        Icon.Texture = icon;
    }

    public void MarkErrors()
    {
        NameVersion.Text += " (load errors, see log)";
        NameVersion.AddThemeColorOverride("font_color", Colors.Orange);
    }
}