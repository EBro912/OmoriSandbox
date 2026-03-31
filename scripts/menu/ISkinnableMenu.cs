namespace OmoriSandbox.Menu;

internal interface ISkinnableMenu
{
    void SetSkinMode(MenuSkinMode mode);
}

internal enum MenuSkinMode
{
    Dreamworld,
    Faraway,
    Blackspace
}