using System;
using System.Collections.Generic;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Menu;

internal abstract partial class PagedMenu : Menu
{
	public int Page { get; protected set; } = 0;

	protected List<Vector2I> Positions = [new(22, 54), new(192, 54), new(22, 76), new(192, 76)];

	protected int MaxPage => Math.Max(0, (TotalCount - 3) / 2);

	// the size of the full list of the menu
	protected abstract int TotalCount { get; }
	// the number of entries visible on the current page
	protected abstract int DisplayedCount { get; }

	protected abstract void UpdatePage();
	protected abstract void ShowInfo();

	protected virtual bool DownMovesToLast => false;

	protected override void MoveCursor(Vector2I direction, bool wrap)
	{
		if (Empty) return;
		if (BattleManager.Instance.Phase == BattlePhase.TargetSelection) return;
		// scrolling moves the view by one row while the cursor keeps its screen position
		if (direction == Vector2.Up && Page > 0 && CursorIndex < 2)
		{
			Page--;
			AudioManager.Instance.PlaySFX("SYS_move", volume: 0.9f);
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
				AudioManager.Instance.PlaySFX("SYS_move", volume: 0.9f);
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
				AudioManager.Instance.PlaySFX("SYS_move", volume: 0.9f);
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
			int index = Page * 2 + CursorIndex;
			int target = index < TotalCount - 2 ? index + 2
				: DownMovesToLast && TotalCount > 2 && index == TotalCount - 2 ? TotalCount - 1 : index;
			if (target / 2 > Page + 1)
			{
				Page++;
				CursorIndex = target - Page * 2;
				AudioManager.Instance.PlaySFX("SYS_move", volume: 0.9f);
				UpdatePage();
				return;
			}
			CursorIndex = target - Page * 2;
		}
		if (CursorIndex != old)
		{
			UpdateCursor();
			ShowInfo();
			AudioManager.Instance.PlaySFX("SYS_move", volume: 0.9f);
		}
	}
}
