using Godot;

public partial class BattleMenu : Menu
{
    private Vector2I GridSize = new(2, 2);

    public override void _Ready()
    {
        Options = ["Attack", "Skill", "Snack", "Toy"];
        CursorPositions = [new Vector2I(165, 410), new Vector2I(350, 410), new Vector2I(165, 450), new Vector2I(350, 450)];
    }

    protected override void MoveCursor(Vector2I direction)
    {
        int x = CursorIndex % 2;
        int y = CursorIndex / 2;
        x = (x + direction.X + GridSize.X) % GridSize.X;
        y = (y + direction.Y + GridSize.Y) % GridSize.Y;
        CursorIndex = y * GridSize.X + x;
        UpdateCursor();
        AudioManager.Instance.PlaySFX("SYS_move");
    }

    protected override void OnSelect()
    {
        switch (Options[CursorIndex])
        {
            case "Attack":
                BattleManager.Instance.OnSelectAttack();
                MenuManager.Instance.ShowMenu(MenuState.None);
                break;
            case "Skill":
                BattleManager.Instance.OnSelectNotAttack();
                MenuManager.Instance.ShowMenu(MenuState.Skill);
                break;
            case "Snack":
                BattleManager.Instance.OnSelectNotAttack();
                MenuManager.Instance.ShowMenu(MenuState.Snack);
                break;
            case "Toy":
                BattleManager.Instance.OnSelectNotAttack();
                MenuManager.Instance.ShowMenu(MenuState.Toy);
                break;
        }
        AudioManager.Instance.PlaySFX("SYS_select");
    }
}