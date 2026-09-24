using System;
using Godot;

namespace OmoriSandbox.Menu;

internal partial class BattleMenu : Menu, ISkinnableMenu
{
	[Export] private Sprite2D AttackSprite;
	[Export] private Sprite2D SkillSprite;
	[Export] private Sprite2D SnackSprite;
	[Export] private Sprite2D ToySprite;
	
	protected override Vector2 OpenPosition => new(320, 480);
	protected override Vector2 ClosedPosition => new(320, 572);
	
	private Vector2I GridSize = new(2, 2);

	public void SetSkinMode(MenuSkinMode mode)
	{
		switch (mode)
		{
			case MenuSkinMode.Dreamworld:
				AttackSprite.RegionRect = new Rect2(654, 131, 179, 39);
				SkillSprite.RegionRect = new Rect2(835, 131, 179, 39);
				SnackSprite.RegionRect = new Rect2(654, 172, 179, 39);
				ToySprite.RegionRect = new Rect2(835, 172, 179, 39);
				break;
			case MenuSkinMode.Faraway:
				AttackSprite.RegionRect = new Rect2(654, 213, 179, 39);
				SkillSprite.RegionRect = new Rect2(835, 213, 179, 39);
				SnackSprite.RegionRect = new Rect2(654, 254, 179, 39);
				ToySprite.RegionRect = new Rect2(835, 254, 179, 39);
				break;
			case MenuSkinMode.Blackspace:
				GD.PrintErr("MenuSkinMode.Blackspace unimplemented for BattleMenu.");
				break;
			default:
				GD.PrintErr("Unknown MenuSkinMode: " + mode);
				break;
		}
	}

	public override void _Ready()
	{
		Options = ["Attack", "Skill", "Snack", "Toy"];
		CursorPositions = [new Vector2I(-154, -72), new Vector2I(28, -72), new Vector2I(-154, -30), new Vector2I(28, -30)];
	}

	public override void OnOpen(SelectionMemory memory)
	{
		if (memory.SavedState == MenuState.Battle)
			CursorIndex = memory.SavedIndex;
		else if (memory.SavedState == MenuState.Skill)
			CursorIndex = 1;
		else if (memory.SavedState == MenuState.Snack)
			CursorIndex = 2;
		else if (memory.SavedState == MenuState.Toy)
			CursorIndex = 3;
		else
			CursorIndex = 0;
		CursorSprite.StartBounce();
		UpdateCursor();
		Show();
	}

	protected override void MoveCursor(Vector2I direction)
	{
		int old = CursorIndex;
		// omori menus have no wrapping
		// pressing left or right simply increments/decrements the index
		if (direction == Vector2.Left)
			CursorIndex = Math.Max(CursorIndex - 1, 0);
		else if (direction == Vector2.Right)
			CursorIndex = Math.Min(CursorIndex + 1, CursorPositions.Count - 1);
		else if (direction == Vector2.Up)
		{
			if (CursorIndex > 1)
				CursorIndex -= 2;
		}
		else if (direction == Vector2.Down)
		{
			if (CursorIndex < 2)
				CursorIndex += 2;
		}
		UpdateCursor();
		// only play a sound if the cursor actually moved
		if (old != CursorIndex)
			AudioManager.Instance.PlaySFX("SYS_move");
	}

	// maps cursor indices to their public option ids
	private static readonly BattleMenuOption[] IndexToOption =
		[BattleMenuOption.Attack, BattleMenuOption.Skill, BattleMenuOption.Snack, BattleMenuOption.Toy];

	protected override void OnSelect()
	{
		if (!BattleManager.Instance.IsMenuOptionEnabled(IndexToOption[CursorIndex]))
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}
		CursorSprite.StopBounce();
		switch (Options[CursorIndex])
		{
			case "Attack":
				BattleManager.Instance.OnSelectAttack();
				break;
			case "Skill":
				BattleManager.Instance.OnSelectNotAttack(MenuState.Skill);
				break;
			case "Snack":
				BattleManager.Instance.OnSelectNotAttack(MenuState.Snack);
				break;
			case "Toy":
				BattleManager.Instance.OnSelectNotAttack(MenuState.Toy);
				break;
		}
		AudioManager.Instance.PlaySFX("SYS_select");
	}

	protected override bool ShouldCloseVisually(MenuState newState)
	{
		return newState is MenuState.Battle or MenuState.None or MenuState.Party;
	}
}
