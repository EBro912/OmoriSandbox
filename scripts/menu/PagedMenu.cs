using System;
using System.Collections.Generic;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Menu;

internal abstract partial class PagedMenu : Menu
{
	public int Page { get; protected set; } = 0;

	protected List<Vector2I> Positions = [new(28, 52), new(200, 52), new(28, 76), new(200, 76)];

	protected int MaxPage => Math.Max(0, (TotalCount - 3) / 2);

	// the size of the full list of the menu
	protected abstract int TotalCount { get; }
	// the number of entries visible on the current page
	protected abstract int DisplayedCount { get; }

	protected abstract void UpdatePage();
	protected abstract void ShowInfo();

	protected override void MoveCursor(Vector2I direction)
	{
		if (Empty) return;
		if (BattleManager.Instance.Phase == BattlePhase.TargetSelection) return;
		// scrolling moves the view by one row while the cursor keeps its screen position
		if (direction == Vector2.Down && Page < MaxPage && CursorIndex > 1)
		{
			Page++;
			AudioManager.Instance.PlaySFX("SYS_move");
			UpdatePage();
			return;
		}
		if (direction == Vector2.Up && Page > 0 && CursorIndex < 2)
		{
			Page--;
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
				CursorIndex = 1;
				AudioManager.Instance.PlaySFX("SYS_move");
				UpdatePage();
				return;
			}
		}
		else if (direction == Vector2.Right)
		{
			if (CursorIndex < DisplayedCount - 1)
				CursorIndex++;
			else if (Page < MaxPage)
			{
				Page++;
				CursorIndex = 2;
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
			if (CursorIndex < 2 && DisplayedCount > 2)
				CursorIndex = Math.Min(CursorIndex + 2, DisplayedCount - 1);
		}
		if (CursorIndex != old)
		{
			UpdateCursor();
			ShowInfo();
			AudioManager.Instance.PlaySFX("SYS_move");
		}
	}
}
