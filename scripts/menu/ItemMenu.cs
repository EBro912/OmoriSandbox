using System.Collections.Generic;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Menu;

internal partial class ItemMenu : PagedMenu
{
	[Export] public AutofitLabel[] ItemLabels;
	[Export] public Label CostText;
	[Export] private Sprite2D PageUpSprite;
	[Export] private Sprite2D PageDownSprite;
	private readonly List<(Item, int)> Items = [];
	private List<(Item, int)> DisplayedItems = [];

	protected override Vector2 OpenPosition => new(138, 384);
	protected override Vector2 ClosedPosition => new(138, 490);

	protected override int TotalCount => Items.Count;
	protected override int DisplayedCount => DisplayedItems.Count;

	public override void OnOpen(SelectionMemory memory)
	{
		PageUpSprite.Visible = false;
		PageDownSprite.Visible = false;
		// make sure our previous selection is in-bounds
		// if an item gets removed due to running out of stock, the previous data may be invalid
		if (memory.SavedState == MenuState.Snack &&
			Items.Count > 0 &&
			!Items[0].Item1.IsToy &&
			memory.SavedPage <= MaxPage &&
			memory.SavedIndex < Items.Count)
		{
			CursorIndex = memory.SavedIndex;
			Page = memory.SavedPage;
        }
		else if (memory.SavedState == MenuState.Toy &&
			Items.Count > 0 &&
			Items[0].Item1.IsToy &&
			memory.SavedPage <= MaxPage &&
			memory.SavedIndex < Items.Count)
		{
            CursorIndex = memory.SavedIndex;
            Page = memory.SavedPage;
        }
		else
		{
			CursorIndex = 0;
			Page = 0;
        }
		CursorSprite.StartBounce();
		UpdatePage();
        Show();
	}

    public void Populate(bool toys)
	{
		Items.Clear();
		Items.AddRange(toys ? BattleManager.Instance.GetToys() : BattleManager.Instance.GetSnacks());
		Empty = Items.Count == 0;
	}

	protected override void UpdatePage()
	{
        CostText.Text = "";
        foreach (AutofitLabel l in ItemLabels)
            l.Text = "";
        if (Empty)
		{
			CursorPositions = Positions.GetRange(0, 1);
			CursorIndex = 0;
			UpdateCursor();
			return;
		}

		PageUpSprite.Visible = Page > 0;
		PageDownSprite.Visible = Page < MaxPage;
		int start = Page * 2;
		int end = Mathf.Min(start + 4, Items.Count);
		DisplayedItems = Items.GetRange(start, end - start);
		for (int i = 0; i < DisplayedItems.Count; i++)
		{
			ItemLabels[i].SetFittedText(DisplayedItems[i].Item1.Name);
		}
        CursorPositions = Positions.GetRange(0, DisplayedItems.Count);
        if (CursorIndex >= DisplayedItems.Count)
	        CursorIndex = DisplayedItems.Count - 1;
        UpdateCursor();
        ShowInfo();
	}

	protected override void ShowInfo()
	{
		if (Empty) return;
		(Item, int) i = DisplayedItems[CursorIndex];
		CostText.Text = "x" + i.Item2;
		if (i.Item1.SpriteIndex > -1)
			BattleLogManager.Instance.ClearAndShowMessageWithIcon(
				$"[font_size=28]{i.Item1.Name}\n[font_size=20]{i.Item1.Description}", i.Item1.SpritesheetPath,
				i.Item1.SpriteIndex);
		else
			BattleLogManager.Instance.ClearAndShowMessage(
				$"[font_size=28]{i.Item1.Name}\n[font_size=20]{i.Item1.Description}");
	}

	protected override void OnSelect()
	{
		if (Empty) return;
		Item selected = DisplayedItems[CursorIndex].Item1;
        if (BattleManager.Instance.OnSelectItem(selected))
			CursorSprite.StopBounce();
	}
}
