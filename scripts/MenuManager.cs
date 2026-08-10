using Godot;
using OmoriSandbox.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using OmoriSandbox.Editor;

namespace OmoriSandbox.Menu;

internal partial class MenuManager : Node
{
	[Export] private PartyMenu PartyMenu;
	[Export] private BattleMenu BattleMenu;
	[Export] private SkillMenu SkillMenu;
	[Export] private ItemMenu SnackMenu;
	[Export] private ItemMenu ToyMenu;
	[Export] private Sprite2D EnergyBar;
	[Export] private Label EnergyText;
	private Tween EnergyBarTween;

	// the energy bar node, tinted by the DialogueManager while dialogue is on screen
	internal Sprite2D EnergyDisplay => EnergyBar;

	public static MenuManager Instance { get; private set; }

	private const float FightRunOffsetRW = 457f;
	private const float FightRunOffset = 375f;
	private const float BattleOffsetRW = 212f;
	private const float BattleOffset = 130f;

	private MenuState CurrentState = MenuState.None;
	private Menu CurrentMenu;
	private Dictionary<MenuState, Menu> Menus;
	private Dictionary<PartyMember, SelectionMemory> LastSelected = [];
	private readonly HashSet<Menu> LoweredMenus = [];

	public override void _EnterTree()
	{
		Menus = new Dictionary<MenuState, Menu>
		{
			{ MenuState.Party, PartyMenu },
			{ MenuState.Battle, BattleMenu },
			{ MenuState.Skill, SkillMenu },
			{ MenuState.Snack, SnackMenu },
			{ MenuState.Toy, ToyMenu }
		};

		BattleManager.Instance.EnergyChanged += RefreshEnergy;
		Instance = this;
	}

	private void RefreshEnergy(object sender, EventArgs e)
	{
		EnergyText.Text = $"{BattleManager.Instance.Energy:00}";
		EnergyBar.RegionRect = new Rect2(0, (float)Math.Ceiling(BattleManager.Instance.Energy / 3f) * 45f, 362f, 48f);
	}

	public void ShowButtons(bool realWorld)
	{
		if (realWorld)
		{
			PartyMenu.SetSkinMode(MenuSkinMode.Faraway);
			BattleMenu.SetSkinMode(MenuSkinMode.Faraway);
		}
		else
		{
			PartyMenu.SetSkinMode(MenuSkinMode.Dreamworld);
			BattleMenu.SetSkinMode(MenuSkinMode.Dreamworld);
		}
	}

	public void ShowMenu(MenuState state, bool immediate = false, bool ignoreMemory = false)
	{
		LoweredMenus.Clear();
		CurrentState = state;
		if (CurrentState == MenuState.None)
		{
			foreach (Menu open in Menus.Values.Where(x => x.Visible)) {
				open.MoveDown(state, immediate);
			}
			CurrentMenu = null;
			MoveEnergyBarDown(immediate);
			return;
		}

		CurrentMenu?.MoveDown(state, immediate);
		CurrentMenu = Menus[CurrentState];
		PartyMember currentPartyMember = BattleManager.Instance.GetCurrentPartyMember();

		if (CurrentMenu is SkillMenu skill)
		{
			skill.Populate(currentPartyMember);
		}
		else if (CurrentMenu is ItemMenu item)
		{
			item.Populate(CurrentState == MenuState.Toy);
		}

		if (ignoreMemory)
			CurrentMenu.OnOpen(new(CurrentState, CurrentMenu.CursorIndex, (CurrentMenu as PagedMenu)?.Page ?? 0));
		else if (currentPartyMember != null && LastSelected.TryGetValue(currentPartyMember, out var result))
			CurrentMenu.OnOpen(result);
		else
			CurrentMenu.OnOpen(new(CurrentState, 0));
		CurrentMenu.MoveUp(immediate);
		MoveEnergyBarUp(immediate);
	}

	public void MoveDownOpenMenus(bool immediate)
	{
		LoweredMenus.Clear();
		foreach (var menu in Menus) {
			if (menu.Value.Visible)
			{
				// remember which menus are lowered so they can be raised later
				LoweredMenus.Add(menu.Value);
				menu.Value.MoveDown(menu.Key, immediate);
			}
		}
		MoveEnergyBarDown(immediate);
	}

	public void MoveUpOpenMenus(bool immediate)
	{
		foreach (Menu open in Menus.Values.Where(x => x.Visible || LoweredMenus.Contains(x)))
			open.MoveUp(immediate);
		LoweredMenus.Clear();
		MoveEnergyBarUp(immediate);
	}

	public override void _Process(double delta)
	{
		if (CurrentState != MenuState.None)
		{
			if (Input.IsActionJustPressed("MenuUp"))
				CurrentMenu.OnInput(Vector2I.Up);
			else if (Input.IsActionJustPressed("MenuDown"))
				CurrentMenu.OnInput(Vector2I.Down);
			else if (Input.IsActionJustPressed("MenuLeft"))
				CurrentMenu.OnInput(Vector2I.Left);
			else if (Input.IsActionJustPressed("MenuRight"))
				CurrentMenu.OnInput(Vector2I.Right);
		}
	}

	public void Select()
	{
		if (CurrentState != MenuState.None)
		{ 
			CurrentMenu.OnInput(Vector2I.Zero);
		}
	}

	public void SaveLastSelected(PartyMember member)
	{
		if (LastSelected.ContainsKey(member))
		{
			// if the actor selected ATTACK last turn, wipe the memory
			if (CurrentState == MenuState.Battle && CurrentMenu.CursorIndex > 0)
				return;
		}
		if (CurrentMenu is PagedMenu pagedMenu)
		{
			LastSelected[member] = new(CurrentState, pagedMenu.CursorIndex, pagedMenu.Page);
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print($"Saved {member.Name} selection as {CurrentState} at index {CurrentMenu.CursorIndex}, page {pagedMenu.Page}");
		}
		else
		{
			LastSelected[member] = new(CurrentState, CurrentMenu.CursorIndex);
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print($"Saved {member.Name} selection as {CurrentState} at index {CurrentMenu.CursorIndex}");
		}
	}

	public void ClearLastSelected()
	{
		LastSelected.Clear();
	}

	private void MoveEnergyBarDown(bool immediate)
	{
		if (immediate)
		{
			EnergyBar.Position = new Vector2(320f, 450f);
		}
		else
		{
			EnergyBarTween?.Kill();
			EnergyBarTween = CreateTween();
			EnergyBarTween.TweenProperty(EnergyBar, "position", new Vector2(320f, 450f), 0.2f).SetTrans(Tween.TransitionType.Sine);
		}
	}

	private void MoveEnergyBarUp(bool immediate)
	{
		if (immediate)
		{
			EnergyBar.Position = new Vector2(320f, 360f);
		}
		else
		{
			EnergyBarTween?.Kill();
			EnergyBarTween = CreateTween();
			EnergyBarTween.TweenProperty(EnergyBar, "position", new Vector2(320f, 360f), 0.2f).SetTrans(Tween.TransitionType.Sine);
		}
	}
}
