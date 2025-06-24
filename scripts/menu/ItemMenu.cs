using System.Collections.Generic;
using Godot;

public partial class ItemMenu : Menu
{
    [Export] public Label[] ItemLabels;
    [Export] public Label CostText;
    private readonly List<(Item, int)> Items = [];
    private List<(Item, int)> DisplayedItems = [];
    private int Page = 0;
    private List<Vector2I> Positions = [new Vector2I(170, 435), new Vector2I(340, 435), new Vector2I(170, 457), new Vector2I(340, 457)];

    private Vector2I GridSize = new(2, 2);
    public void Populate(bool toys)
    {
        Items.Clear();
        Items.AddRange(toys ? BattleManager.Instance.GetToys() : BattleManager.Instance.GetSnacks());
        Page = 0;
        UpdatePage();
    }

    private void UpdatePage()
    {
        int start = Page * 4;
        int end = Mathf.Min(start + 4, Items.Count);
        foreach (Label l in ItemLabels)
            l.Text = "";
        DisplayedItems = Items.GetRange(start, end - start);
        for (int i = 0; i < DisplayedItems.Count; i++)
        {
            // convert to column-by-column
            int offset = (i % 2) * 2 + (i / 2);
            ItemLabels[i].Text = DisplayedItems[i].Item1.Name;
        }
        CursorPositions = Positions.GetRange(0, DisplayedItems.Count);
        CursorIndex = 0;
        UpdateCursor();
        ShowItemInfo();
    }

    protected override void MoveCursor(Vector2I direction)
    {
        int x = CursorIndex % 2;
        int y = CursorIndex / 2;
        x = (x + direction.X + GridSize.X) % GridSize.X;
        y = (y + direction.Y + GridSize.Y) % GridSize.Y;
        int newIndex = y * GridSize.X + x;
        newIndex = Mathf.Min(newIndex, DisplayedItems.Count - 1);
        CursorIndex = newIndex;
        UpdateCursor();

        if (direction.Y > 0 && Page < Mathf.CeilToInt(Items.Count / 4) - 1 && CursorIndex >= DisplayedItems.Count - 1)
        {
            Page++;
            UpdatePage();
        }
        else if (direction.Y < 0 && Page > 0 && CursorIndex == 0)
        {
            Page--;
            UpdatePage();
        }
        ShowItemInfo();
        AudioManager.Instance.PlaySFX("SYS_move");
    }

    private void ShowItemInfo()
    {
        (Item, int) i = DisplayedItems[CursorIndex];
        CostText.Text = "x" + i.Item2.ToString();
        BattleLogManager.Instance.ClearAndShowMessage($"{i.Item1.Name}\n{i.Item1.Description}");
    }

    protected override void OnSelect()
    {
        Item selected = DisplayedItems[CursorIndex].Item1;
        BattleManager.Instance.OnSelectItem(selected);
        MenuManager.Instance.ShowMenu(MenuState.None);
    }
}