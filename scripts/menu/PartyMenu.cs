using Godot;

public partial class PartyMenu : Menu
{
    public override void _Ready()
    {
        Options = ["Fight", "Run"];
        CursorPositions = [new Vector2I(250, 410), new Vector2I(250, 450)];
    }

    protected override void MoveCursor(Vector2I direction)
    {
        CursorIndex = (CursorIndex + direction.Y + Options.Count) % Options.Count;
        UpdateCursor();
        AudioManager.Instance.PlaySFX("SYS_move");
    }

    protected override void OnSelect()
    {
        if (CursorIndex == 0)
        {
            MenuManager.Instance.ShowMenu(MenuState.Battle);
            BattleManager.Instance.OnFightSelected();
            AudioManager.Instance.PlaySFX("SYS_select");
            return;
        }
        AudioManager.Instance.PlaySFX("sys_buzzer");
    }
}