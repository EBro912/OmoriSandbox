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
	[Export] private EnergyDots EnergyDots;
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
	private readonly MenuRepeat Repeat = new();
	private MenuSkinMode? SkinOverride;

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
		int energy = BattleManager.Instance.Energy;
		EnergyText.Text = $"{energy:00}";
		if (SkinOverride == MenuSkinMode.Blackspace)
			EnergyBar.RegionRect = new Rect2(292f, 340f + (energy > 6 ? 2 : energy > 3 ? 1 : 0) * 45f, 360f, 45f);
		else
			EnergyBar.RegionRect = new Rect2(1f, 1f + Math.Min(energy / 2, 4) * 45f, 360f, 45f);
	}

	internal void SetPresetSkin(string menuSkin, bool itemless)
	{
		SkinOverride = menuSkin?.ToLowerInvariant() switch
		{
			"dreamworld" => MenuSkinMode.Dreamworld,
			"faraway" => MenuSkinMode.Faraway,
			"blackspace" => MenuSkinMode.Blackspace,
			_ => null
		};
		BattleMenu.Itemless = itemless;
		EnergyDots.Origin = SkinOverride == MenuSkinMode.Blackspace ? new Vector2(1, 229) : new Vector2(363, 1);
	}

	public void ShowButtons(bool realWorld)
	{
		MenuSkinMode mode = SkinOverride ?? (realWorld ? MenuSkinMode.Faraway : MenuSkinMode.Dreamworld);
		PartyMenu.SetSkinMode(mode);
		BattleMenu.SetSkinMode(mode);
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
		Repeat.Update(delta);
		if (CurrentState != MenuState.None && Repeat.Direction != Vector2I.Zero)
		{
			if (Repeat.Fresh)
				CurrentMenu.OnInput(Repeat.Direction);
			else
				CurrentMenu.OnRepeat(Repeat.Direction);
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
			EnergyBar.Position = new Vector2(140f, 424f);
		}
		else
		{
			EnergyBarTween?.Kill();
			EnergyBarTween = CreateTween();
			EnergyBarTween.TweenProperty(EnergyBar, "position", new Vector2(140f, 424f), Menu.MoveDuration).SetTrans(Tween.TransitionType.Circ).SetEase(Tween.EaseType.In).SetDelay(Menu.MoveDelay);
		}
	}

	private void MoveEnergyBarUp(bool immediate)
	{
		if (immediate)
		{
			EnergyBar.Position = new Vector2(140f, 340f);
		}
		else
		{
			EnergyBarTween?.Kill();
			EnergyBarTween = CreateTween();
			EnergyBarTween.TweenProperty(EnergyBar, "position", new Vector2(140f, 340f), Menu.MoveDuration).SetTrans(Tween.TransitionType.Circ).SetEase(Tween.EaseType.Out).SetDelay(Menu.MoveDelay);
		}
	}
}
