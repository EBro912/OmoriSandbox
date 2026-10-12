using System;
using System.Collections.Generic;
using Godot;
using OmoriSandbox.Battle;

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

	internal bool Itemless { get; set; }

	public void SetSkinMode(MenuSkinMode mode)
	{
		ApplySkin(Database.GetMenuSkin(mode.ToString()));
	}

	public void ApplySkin(MenuSkin skin)
	{
		if (Itemless || skin.ForcesItemless)
		{
			SetColumnLayout(skin.HasOneColumnCommands ? skin : Database.GetMenuSkin(Database.DefaultMenuSkin));
			return;
		}
		SetGridLayout(skin);
	}

	private void SetGridLayout(MenuSkin skin)
	{
		OneColumn = false;
		AttackSprite.Texture = skin.CommandsTexture;
		SkillSprite.Texture = skin.CommandsTexture;
		SnackSprite.Texture = skin.CommandsTexture;
		ToySprite.Texture = skin.CommandsTexture;
		AttackSprite.RegionRect = skin.Attack;
		SkillSprite.RegionRect = skin.Skill;
		SnackSprite.RegionRect = skin.Snack;
		ToySprite.RegionRect = skin.Toy;
		AttackSprite.Centered = true;
		SkillSprite.Centered = true;
		AttackSprite.Position = new Vector2(-91, -73);
		SkillSprite.Position = new Vector2(92, -73);
		SnackSprite.Visible = true;
		ToySprite.Visible = true;
		Options = ["Attack", "Skill", "Snack", "Toy"];
		CursorPositions = [new Vector2I(-154, -72), new Vector2I(28, -72), new Vector2I(-154, -30), new Vector2I(28, -30)];
	}

	private void SetColumnLayout(MenuSkin skin)
	{
		OneColumn = true;
		AttackSprite.Texture = skin.OneColumnTexture;
		SkillSprite.Texture = skin.OneColumnTexture;
		AttackSprite.RegionRect = skin.OneColumnAttack;
		SkillSprite.RegionRect = skin.OneColumnSkill;
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
		SetGridLayout(Database.GetMenuSkin(Database.DefaultMenuSkin));
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
			AudioManager.Instance.PlaySFX("SYS_move", volume: 0.9f);
	}

	// maps cursor indices to their public option ids
	private static readonly BattleMenuOption[] IndexToOption =
		[BattleMenuOption.Attack, BattleMenuOption.Skill, BattleMenuOption.Snack, BattleMenuOption.Toy];

	protected override void OnSelect()
	{
		if (!BattleManager.Instance.IsMenuOptionEnabled(IndexToOption[CursorIndex]))
		{
			AudioManager.Instance.PlaySFX("sys_buzzer", volume: 0.9f);
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
		AudioManager.Instance.PlaySFX("SYS_select", volume: 0.9f);
	}

	protected override bool ShouldCloseVisually(MenuState newState)
	{
		return newState is MenuState.Battle or MenuState.None or MenuState.Party;
	}
}
