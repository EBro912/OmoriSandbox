using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Menu;

internal partial class ItemMenu : Menu
{
	[Export] public AutofitLabel[] ItemLabels;
	[Export] public Label CostText;
	[Export] private Sprite2D PageUpSprite;
	[Export] private Sprite2D PageDownSprite;
	private readonly List<(Item, int)> Items = [];
	private List<(Item, int)> DisplayedItems = [];
	public int Page { get; private set; } = 0;
	private List<Vector2I> Positions = [new(28, 52), new(200, 52), new(28, 76), new(200, 76)];

	protected override Vector2 OpenPosition => new(138, 384);
	protected override Vector2 ClosedPosition => new(138, 490);
	
	private Vector2I GridSize = new(2, 2);
	private int MaxPage => Math.Max(0, (Items.Count - 3) / 2);

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

	private void UpdatePage()
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
        ShowItemInfo();
	}

	protected override void MoveCursor(Vector2I direction)
	{
		if (Empty) return;
		if (BattleManager.Instance.Phase == BattlePhase.TargetSelection) return;
		if (direction == Vector2.Down && Page < MaxPage && CursorIndex > 1)
		{
			Page++;
			CursorIndex -= 2;
			AudioManager.Instance.PlaySFX("SYS_move");
			UpdatePage();
			return;
		}
		if (direction == Vector2.Up && Page > 0 && CursorIndex < 2)
		{
			Page--;
			CursorIndex += 2;
			AudioManager.Instance.PlaySFX("SYS_move");
			UpdatePage();
			return;
		}

		int old = CursorIndex;
		// omori menus have no wrapping
		// pressing left or right simply increments/decrements the index
		if (direction == Vector2.Left)
		{
			if (CursorIndex > 0)
				CursorIndex--;
			else if (Page > 0)
			{
				Page--;
				CursorIndex = 3;
				AudioManager.Instance.PlaySFX("SYS_move");
				UpdatePage();
				return;
			}
		}
		else if (direction == Vector2.Right)
		{
			if (CursorIndex < DisplayedItems.Count - 1)
				CursorIndex++;
			else if (Page < MaxPage)
			{
				Page++;
				CursorIndex = 0;
				AudioManager.Instance.PlaySFX("SYS_move");
				UpdatePage();
				return;
			}
		}
		else if (direction == Vector2.Up)
		{
			if (CursorIndex > 1)
				CursorIndex -= 2;
		}
		else if (direction == Vector2.Down)
		{
			if (CursorIndex < 2 && DisplayedItems.Count > 2)
				CursorIndex = Math.Min(CursorIndex + 2, DisplayedItems.Count - 1);
		}
		if (CursorIndex != old)
		{
			UpdateCursor();
			ShowItemInfo();
			AudioManager.Instance.PlaySFX("SYS_move");
		}
	}

	private void ShowItemInfo()
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
