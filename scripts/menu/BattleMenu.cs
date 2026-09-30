using System;
using System.Collections.Generic;
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

	private bool OneColumn;
	private Texture2D GridTexture;

	private static readonly Dictionary<MenuSkinMode, BattleCommandArt> OneColumnCommandArt = new()
	{
		[MenuSkinMode.Dreamworld] = new("res://assets/system/BattleCommands_DW_ATK_SKILL.png", new Rect2(1, 1, 360, 40), new Rect2(1, 41, 360, 40)),
		[MenuSkinMode.Faraway] = new("res://assets/system/BattleCommands_DW_ATK_SKILL.png", new Rect2(1, 1, 360, 40), new Rect2(1, 41, 360, 40)),
		[MenuSkinMode.Blackspace] = new("res://assets/atlases/battleATLAS.png", new Rect2(292, 480, 360, 40), new Rect2(292, 520, 360, 40))
	};

	internal bool Itemless { get; set; }

	public void SetSkinMode(MenuSkinMode mode)
	{
		if (Itemless || mode is MenuSkinMode.Blackspace)
		{
			SetColumnLayout(mode);
			return;
		}
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
			default:
				GD.PrintErr("Unknown MenuSkinMode: " + mode);
				return;
		}
		SetGridLayout();
	}

	private void SetGridLayout()
	{
		OneColumn = false;
		AttackSprite.Texture = GridTexture;
		SkillSprite.Texture = GridTexture;
		AttackSprite.Centered = true;
		SkillSprite.Centered = true;
		AttackSprite.Position = new Vector2(-91, -73);
		SkillSprite.Position = new Vector2(92, -73);
		SnackSprite.Visible = true;
		ToySprite.Visible = true;
		Options = ["Attack", "Skill", "Snack", "Toy"];
		CursorPositions = [new Vector2I(-154, -72), new Vector2I(28, -72), new Vector2I(-154, -30), new Vector2I(28, -30)];
	}

	private void SetColumnLayout(MenuSkinMode mode)
	{
		BattleCommandArt art = OneColumnCommandArt[mode];
		Texture2D texture = GD.Load<Texture2D>(art.Texture);
		OneColumn = true;
		AttackSprite.Texture = texture;
		SkillSprite.Texture = texture;
		AttackSprite.RegionRect = art.Attack;
		SkillSprite.RegionRect = art.Skill;
		AttackSprite.Centered = false;
		SkillSprite.Centered = false;
		AttackSprite.Position = new Vector2(-180, -92);
		SkillSprite.Position = new Vector2(-180, -50);
		SnackSprite.Visible = false;
		ToySprite.Visible = false;
		Options = ["Attack", "Skill"];
		CursorPositions = [new Vector2I(-76, -72), new Vector2I(-76, -30)];
	}

	public override void _Ready()
	{
		GridTexture = AttackSprite.Texture;
		SetGridLayout();
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

	protected override void MoveCursor(Vector2I direction, bool wrap)
	{
		int old = CursorIndex;
		// omori menus have no wrapping
		// pressing left or right simply increments/decrements the index
		if (OneColumn)
		{
			if (direction == Vector2.Up)
				CursorIndex = Math.Max(CursorIndex - 1, 0);
			else if (direction == Vector2.Down)
				CursorIndex = Math.Min(CursorIndex + 1, CursorPositions.Count - 1);
		}
		else if (direction == Vector2.Left)
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

	private readonly record struct BattleCommandArt(string Texture, Rect2 Attack, Rect2 Skill);
}
