namespace OmoriSandbox.Menu;

internal interface ISkinnableMenu
{
    void ApplySkin(MenuSkin skin);
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