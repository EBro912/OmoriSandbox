namespace OmoriSandbox.Menu;

internal interface ISkinnableMenu
{
    void SetSkinMode(MenuSkinMode mode);
}

/// <summary>
/// The skin a battle menu can appear as.
/// </summary>
public enum MenuSkinMode
{
    Dreamworld,
    Faraway,
    Blackspace
}