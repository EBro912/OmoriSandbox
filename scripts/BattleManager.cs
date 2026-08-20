using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Editor;
using OmoriSandbox.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmoriSandbox.Extensions;

namespace OmoriSandbox;

/// <summary>
/// Handles the bulk flow of battles.
/// </summary>
public partial class BattleManager : Node
{
	[Export] private Label EnergyText;
	[Export] private Sprite2D EnergyBar;
	[Export] private EnergyDots EnergyDots;
	[Export] private VBoxContainer EndOfBattleOptionsContainer;
	[Export] private HBoxContainer StageSelectorContainer;
	[Export] private SpinBox StageSelectorSpinBox;
	[Export] private Label RestartLabel;

	private List<PartyMemberComponent> CurrentParty = [];
	private List<EnemyComponent> Enemies = [];
	private List<BattlePresetBossRushStage> Stages = [];
	private int CurrentStage = -1;
	/// <summary>
	/// The currently loaded preset, if any.
	/// </summary>
	/// <remarks>
	/// You can check <see cref="IsBattling"/> instead to see if a battle is ongoing.
	/// </remarks>
	public string CurrentPresetName { get; private set; }

	private GameModeType GameType = GameModeType.Normal;
	internal BattlePhase Phase { get; private set; } = BattlePhase.PreBattle;
	private int CurrentPartyMember = -1;
	private int CurrentEnemyTarget = -1;
	private int CurrentPartyMemberTarget = -1;
	private readonly Dictionary<Actor, BattleCommand> PlayerCommands = new();
	private readonly HashSet<Actor> ActedThisTurn = [];
	private readonly List<BattleCommand> ForcedCommands = [];
	private PendingFollowupData PendingFollowup = null;

	// vanilla only spends followup energy when the followup is actually used, so the
	// selection records everything needed to re-validate and pay at use time
	private sealed record PendingFollowupData(BattleCommand Command, int TargetPosition, string BaseSkillName)
	{
		public bool IsReleaseEnergy => BaseSkillName.StartsWith("ReleaseEnergy");
	}
	private readonly List<Actor> PriorityActors = [];
	private readonly HashSet<EnemyComponent> DeferredDeathEnemies = [];
	private BattleCommand CurrentCommand = null;
	private Timer Delay;
	private readonly List<Node2D> DyingEnemies = [];
	private Dictionary<string, int> Items = [];
	private BattleAction SelectedAction;
	private int _Energy = 0;

	/// <summary>
	/// The amount of Energy the party currently has.
	/// </summary>
	public int Energy
	{
		get => _Energy;
		internal set
		{
			_Energy = value;
			EnergyChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	/// <summary>
	/// Fired whenever the Energy value changes.
	/// </summary>
	public event EventHandler EnergyChanged;

	/// <summary>
	/// Fired once per battle when the battle is exited for any reason
	/// (restart, running away, or returning to the title screen), after the battle state has been fully reset.
	/// Not fired when the end-of-battle results screen appears or between Boss Rush stages.
	/// </summary>
	public event EventHandler BattleExited;

	/// <summary>
	/// Fired when a battle starts, after every actor's <see cref="Actor.OnStartOfBattle"/> hook has run.
	/// In Boss Rush, this is fired again at the start of every stage.
	/// </summary>
	public event EventHandler<BattleStartedEventArgs> BattleStarted;

	/// <summary>
	/// Fired when a battle ends in victory or defeat, after every actor's <see cref="Actor.OnEndOfBattle"/> hook has run.
	/// In Boss Rush, this is fired again at the end of every stage.
	/// </summary>
	/// <remarks>
	/// This is NOT fired when the battle is exited early, see <see cref="BattleExited"/>.
	/// </remarks>
	public event EventHandler<BattleEndedEventArgs> BattleEnded;

	/// <summary>
	/// Fired at the start of each turn, after every start-of-turn stat modifier, weapon, charm,
	/// and enemy hook has run.
	/// </summary>
	public event EventHandler TurnStarted;

	/// <summary>
	/// Fired at the end of each turn, after every enemy's <see cref="Enemy.ProcessEndOfTurn"/> hook has run.
	/// </summary>
	public event EventHandler TurnEnded;

	/// <summary>
	/// Fired after an attack lands and its damage has been applied to any target.
	/// Not fired for misses, evades, or hits a modifier turned into a miss.
	/// </summary>
	public event EventHandler<DamageDealtEventArgs> DamageDealt;

	/// <summary>
	/// Fired after any healing (Heart or Juice) has been applied to any target.
	/// </summary>
	public event EventHandler<HealedEventArgs> Healed;

	private bool FollowupActive = false;
	private bool FollowupSelected = false;
	private bool ForceHideFollowup = false;
	private int FollowupTier = 1;
	private bool DamageNumbersDisabled = false;
	private bool DebugDamageHeld = false;

	internal bool CombinedBuffsDebuffs { get; private set; }

	private bool RestartQueued = false;
	private double RestartTimer;

	private InputDirection? EnemySelectHeldDir = null;
	private double EnemySelectHoldTimer = 0;
	private double EnemySelectRepeatTimer = 0;

	/// <summary>
	/// Whether a battle is currently ongoing.
	/// </summary>
	public bool IsBattling { get; private set; } = false;

	private bool ProcessedStartOfTurn = false;
	private bool ProcessedStartOfCommands = false;
	private bool ProcessedEndOfTurn = false;

	public static BattleManager Instance { get; private set; }

	// table used for handling selecting a party member target
	// has a preferred and fallback target if the preferred party member does not exist
	private Dictionary<(int Position, InputDirection Direction), (int Preferred, int Fallback)> DirectionTable = new()
	{
		{ (0, InputDirection.Up), (1, 3) },
		{ (0, InputDirection.Right), (2, 3) },
		{ (1, InputDirection.Down), (0, 2) },
		{ (1, InputDirection.Right), (3, 2) },
		{ (2, InputDirection.Left), (0, 1) },
		{ (2, InputDirection.Up), (3, 1) },
		{ (3, InputDirection.Left), (1, 0) },
		{ (3, InputDirection.Down), (2, 0) },
	};

	public override void _EnterTree()
	{
		EndOfBattleOptionsContainer.GetChild(1).GetChild<Button>(0).Pressed += () =>
		{
			Reset();
			ReloadPreset();
		};

		EndOfBattleOptionsContainer.GetChild(1).GetChild<Button>(1).Pressed += () =>
		{
			Reset();
			MainMenuManager.Instance.ReturnToTitle();
		};

		StageSelectorContainer.Visible = false;
		StageSelectorSpinBox.Value = 0;
		Instance = this;
	}

	internal void Init(List<PartyMemberComponent> party, List<EnemyComponent> enemies,
		List<BattlePresetBossRushStage> stages, BattlePreset preset, int startingStage)
	{
		GameType = preset.Type;
		CurrentPresetName = preset.Name;
		CurrentParty = party.OrderBy(x => x.Position).ToList();
		Stages = stages;
		
		if (GameType is GameModeType.Normal)
		{
			Enemies = enemies;
			CurrentStage = -1;
			GameManager.Instance.SetBattleback(preset.Battleback);
			AudioManager.Instance.PlayBGM(preset.BGM, 1f, (float)preset.BGMPitch);
			AudioManager.Instance.SetBGMLoopOffset(preset.BGMLoopPoint);
		}
		else
		{
			Enemies = [];
			CurrentStage = startingStage;
			SummonEnemiesForStage(stages[CurrentStage].Enemies);
			StageSelectorSpinBox.MaxValue = stages.Count - 1;
			StageSelectorSpinBox.Value = startingStage;
			GameManager.Instance.SetBattleback(preset.Stages[CurrentStage].Battleback);
			AudioManager.Instance.PlayBGM(preset.Stages[CurrentStage].BGM, 1f, (float)preset.Stages[CurrentStage].BGMPitch);
			AudioManager.Instance.SetBGMLoopOffset(preset.Stages[CurrentStage].BGMLoopPoint);
		}

		Items = preset.Items.ToDictionary();
		Energy = Math.Clamp(preset.StartingEnergy, 0, 10);
		FollowupTier = preset.FollowupTier;
		DamageNumbersDisabled = preset.DisableDamageNumbers;
		CombinedBuffsDebuffs = preset.CombinedBuffsDebuffs;
		EndOfBattleOptionsContainer.Visible = false;
		StageSelectorContainer.Visible = false;

		EnergyBar.Visible = CurrentParty.Any(x => x.HasFollowup);

		BattleLogManager.Instance.Visible = true;
		AnimationManager.Instance.TintScreen(ColorsExtension.TransparentBlack);

		Delay = new Timer
		{
			OneShot = true,
			Autostart = false,
		};
		AddChild(Delay);
		Delay.Timeout += OnDelayTimeout;
		BattleLogManager.Instance.FinishedLogging += OnBattleLogFinished;

		DamageNumber.CacheTexture(ResourceLoader.Load<Texture2D>("res://assets/system/Damage.png"));

		CallDeferred(MethodName.PreBattle);

		MenuManager.Instance.ShowMenu(MenuState.None, true);

		IsBattling = true;
	}

	private void SummonEnemiesForStage(List<BattlePresetEnemy> enemies)
	{
		GameManager.Instance.DespawnEnemies();
		Enemies.Clear();
		foreach (BattlePresetEnemy enemy in enemies)
			SummonEnemy(enemy.Name, GD.StrToVar(enemy.Position).AsVector2(), enemy.Emotion, enemy.FallsOffScreen,
				enemy.GrayscaleOnDefeat, (int)enemy.Layer);
	}

	private async void PreBattle()
	{
		foreach (PartyMemberComponent p in CurrentParty)
			await RunGuarded(() => p.Actor.OnStartOfBattle(), $"{p.Actor.Name}.OnStartOfBattle");
		// use for loop here since collection may be modified by summoned enemies
		for (int i = 0; i < Enemies.Count; i++)
		{
			Actor enemy = Enemies[i].Actor;
			await RunGuarded(() => enemy.OnStartOfBattle(), $"{enemy.Name}.OnStartOfBattle");
		}
		RaiseGuarded(BattleStarted, new BattleStartedEventArgs(CurrentPresetName, CurrentStage), "BattleStarted");
		SetPhase(BattlePhase.FightRun);
	}

	public override void _Process(double delta)
	{
		if (!IsBattling)
			return;

		EnergyDots.Tick(delta);

		foreach (PartyMemberComponent member in CurrentParty)
		{
			member.SelectionBoxVisible = Phase switch
			{
				BattlePhase.TargetSelection => member.Position == CurrentPartyMemberTarget,
				BattlePhase.PlayerCommand => CurrentPartyMember > -1 &&
				                             member.Actor == CurrentParty[CurrentPartyMember].Actor,
				_ => false
			};
		}

		DebugDamageHeld = Input.IsActionPressed("DebugDamage");

		Engine.TimeScale = Input.IsActionPressed("SpeedUp") ? SettingsMenuManager.Instance.SpeedUpMultiplier : 1d;
		
		if (Input.IsActionJustPressed("Accept"))
		{
			if (Phase == BattlePhase.TargetSelection)
			{
				SelectTarget();
			}
			else
			{
				MenuManager.Instance.Select();
			}
		}

		// handle Back here instead of MenuManager to have more control and easier variable access
		if (Input.IsActionJustPressed("Back"))
		{
			switch (Phase)
			{
				case BattlePhase.PlayerCommand:
					do
					{
						CurrentPartyMember--;
						if (CurrentPartyMember < 0)
						{
							AudioManager.Instance.PlaySFX("sys_cancel");
							MenuManager.Instance.ShowMenu(MenuState.Party, true);
							SetPhase(BattlePhase.FightRun);
							return;
						}
					} while (CurrentParty[CurrentPartyMember].Actor.IsToast);

					Actor prev = CurrentParty[CurrentPartyMember].Actor;
					if (PlayerCommands.TryGetValue(prev, out BattleCommand prevCmd) && prevCmd.Action is Item item)
					{
						string name = CapitalizeItemName(item);
						if (!Items.TryAdd(name, 1))
							Items[name]++;
					}

					PlayerCommands.Remove(prev);
					AudioManager.Instance.PlaySFX("sys_cancel");
					MenuManager.Instance.ShowMenu(MenuState.Battle);
					SetPhase(BattlePhase.PlayerCommand);
					break;
				case BattlePhase.TargetSelection:
					MenuState targetMenu;
					if (SelectedAction is Item i)
					{
						targetMenu = i.IsToy ? MenuState.Toy : MenuState.Snack;
						string name = CapitalizeItemName(i);
						if (!Items.TryAdd(name, 1))
							Items[name]++;
					}
					else
						targetMenu = SelectedAction.Name == CurrentParty[CurrentPartyMember].Actor.Skills.Values.FirstOrDefault()?.Name
							? MenuState.Battle
							: MenuState.Skill;
					
					AudioManager.Instance.PlaySFX("sys_cancel");
					if (CurrentEnemyTarget > -1)
						Enemies[CurrentEnemyTarget].ShowInfoBox(false);
					CurrentEnemyTarget = -1;
					CurrentPartyMemberTarget = -1;
					MenuManager.Instance.ShowMenu(targetMenu);
					SetPhase(targetMenu is MenuState.Battle ? BattlePhase.PlayerCommand : BattlePhase.SkillSelection);
					break;
				case BattlePhase.SkillSelection:
					AudioManager.Instance.PlaySFX("sys_cancel");
					MenuManager.Instance.ShowMenu(MenuState.Battle, ignoreMemory: true);
					SetPhase(BattlePhase.PlayerCommand);
					break;
			}
		}

		if (Input.IsActionJustPressed("MenuLeft"))
		{
			HandleInputDirection(InputDirection.Left);
			EnemySelectHeldDir = InputDirection.Left;
			EnemySelectHoldTimer = 0;
			EnemySelectRepeatTimer = 0;
		}
		else if (Input.IsActionJustPressed("MenuRight"))
		{
			HandleInputDirection(InputDirection.Right);
			EnemySelectHeldDir = InputDirection.Right;
			EnemySelectHoldTimer = 0;
			EnemySelectRepeatTimer = 0;
		}
		else if (EnemySelectHeldDir != null)
		{
			string action = EnemySelectHeldDir == InputDirection.Left ? "MenuLeft" : "MenuRight";
			if (Input.IsActionPressed(action) && Phase == BattlePhase.TargetSelection)
			{
				EnemySelectHoldTimer += delta;
				if (EnemySelectHoldTimer >= SettingsMenuManager.Instance.SelectionHoldTime)
				{
					EnemySelectRepeatTimer += delta;
					double changeSpeed = SettingsMenuManager.Instance.SelectionChangeSpeed;
					if (EnemySelectRepeatTimer >= changeSpeed)
					{
						EnemySelectRepeatTimer -= changeSpeed;
						HandleInputDirection(EnemySelectHeldDir.Value);
					}
				}
			}
			else
			{
				EnemySelectHeldDir = null;
			}
		}

		if (Input.IsActionJustPressed("MenuUp"))
		{
			HandleInputDirection(InputDirection.Up);
		}

		if (Input.IsActionJustPressed("MenuDown"))
		{
			HandleInputDirection(InputDirection.Down);
		}

		if (Input.IsActionJustPressed("SwitchSides"))
		{
			if (Phase == BattlePhase.TargetSelection && SelectedAction.Target == SkillTarget.AllyOrEnemy)
			{
				if (CurrentPartyMemberTarget > -1)
				{
					CurrentPartyMemberTarget = -1;
					CurrentEnemyTarget = Enemies.FindIndex(x => x.Actor.CurrentHP > 0);
					Enemies[CurrentEnemyTarget].ShowInfoBox(true);
					MenuManager.Instance.MoveDownOpenMenus(false);
				}
				else
				{
					CurrentPartyMemberTarget = CurrentParty.First(x => x != null).Position;
					Enemies[CurrentEnemyTarget].ShowInfoBox(false);
					CurrentEnemyTarget = -1;
					MenuManager.Instance.MoveUpOpenMenus(false);
				}
			}
		}

		if (Input.IsActionJustPressed("Restart"))
		{
			if (!RestartQueued)
			{
				RestartLabel.Visible = true;
				RestartLabel.Text = "Restarting...";
				RestartTimer = SettingsMenuManager.Instance.RestartHoldTime;
			}
		}

		if (Input.IsActionJustReleased("Restart"))
		{
			if (!RestartQueued)
			{
				RestartLabel.Visible = false;
				RestartTimer = SettingsMenuManager.Instance.RestartHoldTime;
			}
		}

		if (Input.IsActionPressed("Restart"))
		{
			// don't allow restarts during the setup phase
			// prevents stuff from breaking
			if (Phase is BattlePhase.PreBattle)
				return;

			if (!RestartQueued)
			{
				RestartTimer -= (float)delta;
				if (RestartTimer <= 0)
				{
					// if the restart is requested during a turn, queue it to prevent anything breaking
					if (Phase is BattlePhase.PreCommand or
					    BattlePhase.CommandExecute or
					    BattlePhase.WaitForBattleLog or
					    BattlePhase.PostCommand or
					    BattlePhase.EnemyDying)
					{
						RestartQueued = true;
						RestartLabel.Text = "Restart Queued";
					}
					else
					{
						// otherwise, just reset immediately
						Reset();
						ReloadPreset();
					}
				}
			}
		}
	}

	private void HandleInputDirection(InputDirection direction)
	{
		if (Phase == BattlePhase.CommandExecute || Phase == BattlePhase.WaitForBattleLog)
		{
			if (HandleFollowup(direction))
			{
				ProcessFollowupSuccess(direction);
			}
		}

		if (Phase == BattlePhase.TargetSelection)
		{
			if (SelectedAction.Target is SkillTarget.Enemy or SkillTarget.AllyOrEnemy
			    && CurrentEnemyTarget > -1
			    && Enemies.Count(x => x.Actor.CurrentHP > 0) > 1
			    && direction is InputDirection.Left or InputDirection.Right)
			{
				int old = CurrentEnemyTarget;
				Enemies[old].ShowInfoBox(false);
				CurrentEnemyTarget = SelectEnemy(CurrentEnemyTarget, direction);
				Enemies[CurrentEnemyTarget].ShowInfoBox(true);
				// only play a sound if the cursor actually moves
				if (CurrentEnemyTarget != old)
					AudioManager.Instance.PlaySFX("SYS_move");
				return;
			}

			if (SelectedAction.Target is SkillTarget.Ally or SkillTarget.AllyNotSelf or SkillTarget.DeadAlly 
			    || (SelectedAction.Target == SkillTarget.AllyOrEnemy && CurrentPartyMemberTarget > -1))
			{
				int target = SelectPartyMember(CurrentPartyMemberTarget, direction);
				if (target > -1)
				{
					int old = CurrentPartyMemberTarget;
					CurrentPartyMemberTarget = target;
					// only play a sound if the cursor actually moves
					if (CurrentPartyMemberTarget != old)
						AudioManager.Instance.PlaySFX("SYS_move");
				}
			}
		}
	}

	private int SelectEnemy(int current, InputDirection direction)
	{
		if (Enemies.Count < 2)
			return current;

		var sortedEnemies = Enemies
			.Select((enemy, index) => new { Enemy = enemy, Index = index })
			.Where(e => e.Enemy.Actor.CurrentHP > 0)
			.OrderBy(e => e.Enemy.Actor.CenterPoint.X)
			.ThenBy(e => e.Index)
			.ToList();

		int sortedPosition = sortedEnemies.FindIndex(e => e.Index == current);
		bool wrap = SettingsMenuManager.Instance.EnemySelectionWrapping;

		int nextPosition;
		if (direction == InputDirection.Right)
		{
			nextPosition = sortedPosition + 1;
			if (nextPosition >= sortedEnemies.Count)
				nextPosition = wrap ? 0 : sortedPosition;
		}
		else
		{
			nextPosition = sortedPosition - 1;
			if (nextPosition < 0)
				nextPosition = wrap ? sortedEnemies.Count - 1 : sortedPosition;
		}
		return sortedEnemies[nextPosition].Index;
	}

	private int SelectPartyMember(int current, InputDirection direction)
	{
		if (!DirectionTable.TryGetValue((current, direction), out var pair))
			return -1;
		if (CurrentParty.Any(x => x.Position == pair.Preferred))
			return pair.Preferred;
		if (CurrentParty.Any(x => x.Position == pair.Fallback))
			return pair.Fallback;
		return -1;
	}

	internal void OnFightSelected()
	{
		CurrentPartyMember = 0;
		MenuManager.Instance.ShowMenu(MenuState.Battle, true);
		SetPhase(BattlePhase.PlayerCommand);
	}

	internal void Reset()
	{
		bool wasBattling = IsBattling;
		GameManager.Instance.DespawnAll();
		AnimationManager.Instance.DespawnAll();
		AnimationManager.Instance.StopAllAnimations();
		DamageNumber.DespawnAll();
		CurrentParty.Clear();
		Enemies.Clear();
		Items.Clear();
		CurrentPartyMember = -1;
		CurrentEnemyTarget = -1;
		CurrentPartyMemberTarget = -1;
		PlayerCommands.Clear();
		ActedThisTurn.Clear();
		ForcedCommands.Clear();
		PendingFollowup = null;
		ForceHideFollowup = false;
		PriorityActors.Clear();
		DeferredDeathEnemies.Clear();
		CurrentCommand = null;
		GameManager.Instance.SetBattlebackGrayscale(false);
		AnimationManager.Instance.TintScreen(ColorsExtension.TransparentBlack);
		MenuManager.Instance.ShowMenu(MenuState.None, true);
		MenuManager.Instance.ClearLastSelected();
		EnergyBar.Visible = false;
		BattleLogManager.Instance.ClearBattleLog();
		BattleLogManager.Instance.Visible = false;
		Delay.Timeout -= OnDelayTimeout;
		Delay.QueueFree();
		BattleLogManager.Instance.FinishedLogging -= OnBattleLogFinished;
		DialogueManager.Instance.Reset();
		AudioManager.Instance.Reset();
		Phase = BattlePhase.PreBattle;
		ProcessedStartOfTurn = false;
		ProcessedStartOfCommands = false;
		ProcessedEndOfTurn = false;
		RestartLabel.Visible = false;
		RestartQueued = false;
		RestartTimer = 1;
		IsBattling = false;
		if (wasBattling)
			BattleExited?.Invoke(this, EventArgs.Empty);
	}

	private void ReloadPreset()
	{
		string presetName = MainMenuManager.Instance.LastLoadedPreset;
		if (!PresetManager.Instance.TryGetPreset(presetName, out BattlePreset preset))
		{
			GD.PrintErr("Could not find preset: " + presetName);
			return;
		}
		GameManager.Instance.LoadBattlePreset(preset, (int)StageSelectorSpinBox.Value);
	}

	internal void OnSelectAttack()
	{
		SelectedAction = CurrentParty[CurrentPartyMember].Actor.Skills.Values.FirstOrDefault();
		if (SelectedAction == null)
		{
			// the preset gave this actor no skills at all
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}
		MenuManager.Instance.SaveLastSelected(CurrentParty[CurrentPartyMember].Actor);
		MenuManager.Instance.ShowMenu(MenuState.None);
		SetPhase(BattlePhase.TargetSelection);
	}

	// idfk
	internal void OnSelectNotAttack(MenuState what)
	{
		MenuManager.Instance.SaveLastSelected(CurrentParty[CurrentPartyMember].Actor);
		MenuManager.Instance.ShowMenu(what);
		SetPhase(BattlePhase.SkillSelection);
	}

	internal bool OnSelectSkill(Skill skill)
	{
		SelectedAction = skill;
		if (!skill.MeetsRequirements(CurrentParty[CurrentPartyMember].Actor))
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return false;
		}

		if (CurrentParty[CurrentPartyMember].Actor.CurrentJuice - skill.Cost(CurrentParty[CurrentPartyMember].Actor) <
		    0)
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return false;
		}

		if (SelectedAction.Target is SkillTarget.DeadAlly or SkillTarget.AllDeadAllies &&
		    CurrentParty.All(x => !x.Actor.IsToast))
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return false;
		}

		AudioManager.Instance.PlaySFX("SYS_select");
		MenuManager.Instance.SaveLastSelected(CurrentParty[CurrentPartyMember].Actor);
		SetPhase(BattlePhase.TargetSelection);
		return true;
	}

	internal bool OnSelectItem(Item item)
	{
		SelectedAction = item;
		// VANILLA BUG: OMORI only blocks single-target revival items when nobody is toast
		// multi-target revives, such as jam packets, can still be selected and consumed
		// regardless of toast status
		if (SelectedAction.Target is SkillTarget.DeadAlly &&
		    CurrentParty.All(x => !x.Actor.IsToast))
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return false;
		}

		Item i = SelectedAction as Item;
		string name = CapitalizeItemName(i);
		Items[name]--;
		if (Items[name] == 0)
			Items.Remove(name);

		AudioManager.Instance.PlaySFX("SYS_select");
		MenuManager.Instance.SaveLastSelected(CurrentParty[CurrentPartyMember].Actor);
		SetPhase(BattlePhase.TargetSelection);
		return true;
	}

	private void SetPhase(BattlePhase phase)
	{
		if (SettingsMenuManager.Instance.LogDebug)
			GD.Print("Entering Phase: " + phase);
		Phase = phase;

		double delayTime = SettingsMenuManager.Instance.ActionDelay switch
		{
			1 => 1.25d,
			2 => 1d,
			4 => 0.5d,
			5 => 0.25d,
			_ => 1d
		};

		switch (Phase)
		{
			case BattlePhase.FightRun:
				HandleFightRun();
				break;
			case BattlePhase.PlayerCommand:
				HandlePlayerCommand();
				break;
			case BattlePhase.TargetSelection:
				HandleTargetSelection();
				break;
			case BattlePhase.PreCommand:
				CheckBattleOver();
				Delay.Start(delayTime);
				break;
			case BattlePhase.CommandExecute:
				HandleCommandExecute();
				break;
			case BattlePhase.PostCommand:
				Delay.Start(delayTime);
				break;
			case BattlePhase.EnemyDying:
				HandleEnemyDying();
				break;

		}
	}

	private async void OnDelayTimeout()
	{
		switch (Phase)
		{
			case BattlePhase.PreCommand:
				if (RestartQueued)
				{
					Reset();
					ReloadPreset();
					return;
				}

				if (!ProcessedStartOfCommands)
				{
					foreach (EnemyComponent enemy in Enemies.ToList())
					{
						if (enemy.Actor.IsToast)
							continue;
						await RunGuarded(() => enemy.Actor.ProcessStartOfCommands(),
							$"{enemy.Actor.Name}.ProcessStartOfCommands");
					}
					ProcessedStartOfCommands = true;
				}

				CurrentCommand = GetNextAction();
				if (CurrentCommand == null)
				{
					EndOfTurn();
				}
				else
				{
					if (SettingsMenuManager.Instance.LogDebug)
						GD.Print("Next action: " + CurrentCommand.Action.Name + " by " + CurrentCommand.Actor.Name);
					SetPhase(BattlePhase.CommandExecute);
				}

				break;
			case BattlePhase.PostCommand:
			{
				await ConvertDeadPartyMembers();

				if (CurrentCommand.Actor is PartyMember
				    && CurrentCommand.Action is Skill skill
				    && skill.ShowFollowups)
				{
					PartyMemberComponent component = CurrentParty.First(x => x.Actor == CurrentCommand.Actor);
					if (component.HasFollowup)
					{
						component.FadeOutFollowups();
					}
				}

				FollowupSelected = false;
				FollowupActive = false;

				foreach (EnemyComponent enemy in Enemies.ToList())
				{
					if (enemy.Actor.IsToast)
						continue;
					enemy.Actor.SetHurt(false);
					await RunGuarded(() => enemy.Actor.ProcessBattleConditions(),
						$"{enemy.Actor.Name}.ProcessBattleConditions");
					// enemy may have been removed during ProcessBattleConditions
					if (!Enemies.Contains(enemy))
						continue;
					if (enemy.Actor.CurrentHP == 0)
					{
						if (ForcedCommands.Any(f => f.Actor == enemy.Actor))
						{
							// defer death until all forced commands for this enemy have executed
							DeferredDeathEnemies.Add(enemy);
						}
						else
						{
							if (enemy.Actor.GrayscaleOnDefeat)
							{
								AudioManager.Instance.PlaySFX("BA_explosion_2", volume: 0.9f);
								AnimationManager.Instance.InitShake(new Shake(9, 5, 40));
								GameManager.Instance.SetBattlebackGrayscale(true);
								await Wait.Milliseconds(750);
							}
							await RunGuarded(() => enemy.Actor.OnDefeat(), $"{enemy.Actor.Name}.OnDefeat");
							enemy.Actor.SetToast();
							if (enemy.Actor.FallsOffScreen)
								DyingEnemies.Add(enemy.GetParent<Node2D>());
						}
					}
				}
				// resolve deferred deaths whose forced commands have all been consumed
				foreach (EnemyComponent enemy in DeferredDeathEnemies.ToList())
				{
					if (ForcedCommands.All(f => f.Actor != enemy.Actor) && enemy.Actor.CurrentHP == 0)
					{
						if (enemy.Actor.GrayscaleOnDefeat)
						{
							AudioManager.Instance.PlaySFX("BA_explosion_2", volume: 0.9f);
							AnimationManager.Instance.InitShake(new Shake(9, 5, 40));
							GameManager.Instance.SetBattlebackGrayscale(true);
							await Wait.Milliseconds(750);
						}
						await RunGuarded(() => enemy.Actor.OnDefeat(), $"{enemy.Actor.Name}.OnDefeat");
						enemy.Actor.SetToast();
						if (enemy.Actor.FallsOffScreen)
							DyingEnemies.Add(enemy.GetParent<Node2D>());
						DeferredDeathEnemies.Remove(enemy);
					}
				}

				// enemy ProcessBattleConditions/OnDefeat hooks above may have zeroed party members
				// convert them before they get a turn at 0 HP
				await ConvertDeadPartyMembers();

				if (DyingEnemies.Count > 0)
					SetPhase(BattlePhase.EnemyDying);
				else
					SetPhase(BattlePhase.PreCommand);
				break;
			}
		}
	}
	
	private async Task ConvertDeadPartyMembers()
	{
		foreach (PartyMemberComponent member in CurrentParty)
		{
			member.Actor.SetHurt(false);
			if (member.Actor.CurrentHP == 0 && !member.Actor.IsToast)
			{
				member.Actor.SetToast();
				member.Actor.RemoveAllStatModifiers();
				// remove charm from any enemies
				foreach (EnemyComponent enemy in Enemies)
				{
					if (enemy.Actor.StatModifiers.TryGetValue("Charm", out StatModifier charmMod)
					    && charmMod is CharmStatModifier charm && charm.CharmedBy == member.Actor)
						enemy.Actor.RemoveStatModifier("Charm");
				}
				AudioManager.Instance.PlaySFX("SYS_you died_2", 1.2f);
			}

			member.UpdateStateIcons();

			if (member.Actor.HasStatModifier("PlotArmor")
			    && member.Actor.StatModifiers["PlotArmor"] is PlotArmorStatModifier pa
			    && !pa.HasAnnounced)
			{
				DialogueManager.Instance.QueueMessage($"{member.Actor.Name.ToUpper()} did not succumb.");
				await DialogueManager.Instance.WaitForDialogue();
				pa.HasAnnounced = true;
			}
		}
	}

	private BattleCommand GetNextAction()
	{
		// forced commands have the highest priority
		while (ForcedCommands.Count > 0)
		{
			BattleCommand forced = ForcedCommands[0];
			ForcedCommands.RemoveAt(0);
			// skip forced commands whose actor has died since being queued
			if (!(IsInvalidTarget(forced.Actor) || forced.Actor.Stunned))
				return forced;
			// the dropped command no longer suppresses followup bubbles
			if (forced.Actor is PartyMember && forced.Action is Skill forcedSkill && forcedSkill.ShowFollowups)
				ForceHideFollowup = false;
		}

		// followups
		if (PendingFollowup != null)
		{
			PendingFollowupData followup = PendingFollowup;
			PendingFollowup = null;
			// vanilla re-validates at use time and only then spends the energy
			// an invalid followup is silently skipped and costs nothing
			if (IsFollowupValid(followup))
			{
				Energy = followup.IsReleaseEnergy ? 0 : Energy - 3;
				return followup.Command;
			}
			if (followup.Command.Action is Skill followupSkill && followupSkill.ShowFollowups)
				ForceHideFollowup = false;
		}

		// priority actors, post-forced command actors or mid-battle spawns who haven't had their normal turn
		while (PriorityActors.Count > 0)
		{
			Actor actor = PriorityActors[0];
			PriorityActors.RemoveAt(0);
			if (ActedThisTurn.Contains(actor) || IsInvalidTarget(actor) || actor.Stunned) 
				continue;
			BattleCommand cmd = ResolveCommandForActor(actor);
			if (cmd != null)
				return cmd;
		}

		// regular turn order
		foreach (PartyMemberComponent member in CurrentParty)
		{
			if (!ActedThisTurn.Contains(member.Actor)
			    && (IsInvalidTarget(member.Actor) || !PlayerCommands.ContainsKey(member.Actor)))
			{
				if (PlayerCommands.TryGetValue(member.Actor, out BattleCommand refundCmd)
				    && refundCmd.Action is Item item)
				{
					string name = CapitalizeItemName(item);
					if (!Items.TryAdd(name, 1))
						Items[name]++;
				}
				ActedThisTurn.Add(member.Actor);
			}
		}

		List<Actor> candidates = [];
		foreach (PartyMemberComponent member in CurrentParty)
		{
			if (!ActedThisTurn.Contains(member.Actor) && !IsInvalidTarget(member.Actor)
			    && PlayerCommands.ContainsKey(member.Actor) && !member.Actor.Stunned)
				candidates.Add(member.Actor);
		}
		foreach (EnemyComponent enemy in Enemies)
		{
			if (!ActedThisTurn.Contains(enemy.Actor) && !IsInvalidTarget(enemy.Actor) && !enemy.Actor.Stunned)
				candidates.Add(enemy.Actor);
		}

		if (candidates.Count == 0)
			return null;

		Actor next = candidates
			.OrderByDescending(a =>
			{
				if (a is PartyMember member && PlayerCommands.TryGetValue(member, out BattleCommand cmd))
					return cmd.Action.Priority;
				return SkillPriority.Normal;
			})
			.ThenByDescending(a => a.CurrentStats.SPD)
			.ThenBy(a =>
			{
				PartyMemberComponent c = CurrentParty.FirstOrDefault(y => y.Actor == a);
				return c?.Position ?? int.MaxValue;
			})
			.First();

		return ResolveCommandForActor(next);
	}

	private BattleCommand ResolveCommandForActor(Actor actor)
	{
		ActedThisTurn.Add(actor);
		if (actor is PartyMember member && PlayerCommands.TryGetValue(member, out BattleCommand cmd))
			return cmd;
		if (actor is Enemy enemy)
			return enemy.ProcessAI();
		return null;
	}

	private async void HandleFightRun()
	{
		CurrentPartyMember = -1;
		CurrentEnemyTarget = -1;
		CurrentPartyMemberTarget = -1;
		PlayerCommands.Clear();
		ActedThisTurn.Clear();
		ForcedCommands.Clear();
		// a followup discarded here never spent its energy, so there is nothing to refund
		PendingFollowup = null;
		ForceHideFollowup = false;
		PriorityActors.Clear();
		DeferredDeathEnemies.Clear();
		CurrentCommand = null;
		ProcessedEndOfTurn = false;
		ProcessedStartOfCommands = false;
		GameManager.Instance.SetBattlebackGrayscale(false);
		if (!ProcessedStartOfTurn)
		{
			foreach (PartyMemberComponent member in CurrentParty.Where(x => !x.Actor.IsToast))
			{
				foreach (StatModifier modifier in member.Actor.StatModifiers.Values)
				{
					RunGuarded(() => modifier.OnStartOfTurn(member.Actor),
						$"{modifier.GetType().Name}.OnStartOfTurn ({member.Actor.Name})");
				}

				await RunGuarded(() => member.Actor.Weapon.StartOfTurn(member.Actor),
					$"weapon {member.Actor.Weapon.Name}.StartOfTurn ({member.Actor.Name})");
				if (member.Actor.Charm != null)
					await RunGuarded(() => member.Actor.Charm.StartOfTurn(member.Actor),
						$"charm {member.Actor.Charm.Name}.StartOfTurn ({member.Actor.Name})");
			}

			foreach (EnemyComponent e in Enemies.ToList())
			{
				if (e.Actor.IsToast)
					continue;
				foreach (StatModifier modifier in e.Actor.StatModifiers.Values)
				{
					RunGuarded(() => modifier.OnStartOfTurn(e.Actor),
						$"{modifier.GetType().Name}.OnStartOfTurn ({e.Actor.Name})");
				}
				
				await RunGuarded(() => e.Actor.ProcessStartOfTurn(), $"{e.Actor.Name}.ProcessStartOfTurn");
			}

			RaiseGuarded(TurnStarted, "TurnStarted");
			ProcessedStartOfTurn = true;
		}

		// a start-of-turn hook may have ended the battle or changed phase
		// don't show the fight menu over it
		if (Phase != BattlePhase.FightRun)
			return;

		GameManager.Instance.DiscordManager.SetBattling(Enemies.Count);
		switch (CurrentParty.Count)
		{
			case 1:
				BattleLogManager.Instance.ClearAndShowMessage("What will " + CurrentParty[0].Actor.Name.ToUpper() +
				                                              " do?");
				break;
			case 2:
				BattleLogManager.Instance.ClearAndShowMessage("What will " + CurrentParty[0].Actor.Name.ToUpper() +
				                                              " and " + CurrentParty[1].Actor.Name.ToUpper() + " do?");
				break;
			default:
				BattleLogManager.Instance.ClearAndShowMessage("What will " + CurrentParty[0].Actor.Name.ToUpper() +
				                                              " and friends do?");
				break;
		}

		MenuManager.Instance.ShowButtons(CurrentParty[0].Actor.IsRealWorld);
		MenuManager.Instance.ShowMenu(MenuState.Party);
	}

	private void HandlePlayerCommand()
	{
		while (CurrentParty[CurrentPartyMember].Actor.IsToast)
		{
			CurrentPartyMember++;
			if (CurrentPartyMember >= CurrentParty.Count)
			{
				BattleLogManager.Instance.ClearBattleLog();
				MenuManager.Instance.ShowMenu(MenuState.None);
				SetPhase(BattlePhase.PreCommand);
				return;
			}
		}

		if (SettingsMenuManager.Instance.ShowMoreInfo)
		{
			Stats stats = CurrentParty[CurrentPartyMember].Actor.CurrentStats;
			Equipment charm = CurrentParty[CurrentPartyMember].Actor.Charm;
			BattleLogManager.Instance.ClearAndShowMessage(
				$"What will {CurrentParty[CurrentPartyMember].Actor.Name.ToUpper()} do?" +
				$"[font_size=18]\n[ATK: {stats.ATK}, DEF: {stats.DEF}, SPD: {stats.SPD}, LCK: {stats.LCK}, HIT: {stats.HIT}, EVA: {stats.EVA}]" +
				$"\n[Weapon: [color=#c263e1]{CurrentParty[CurrentPartyMember].Actor.Weapon.Name}[/color], Charm: [color=#c263e1]{(charm == null ? "None" : charm.Name)}[/color]]");
		}
		else
			BattleLogManager.Instance.ClearAndShowMessage("What will " +
			                                              CurrentParty[CurrentPartyMember].Actor.Name.ToUpper() +
			                                              " do?");

		MenuManager.Instance.ShowButtons(CurrentParty[CurrentPartyMember].Actor.IsRealWorld);
	}

	private void HandleTargetSelection()
	{
		switch (SelectedAction.Target)
		{
			case SkillTarget.Ally:
			case SkillTarget.AllyNotSelf:
			case SkillTarget.DeadAlly:
				// keep selection box on current ally for ally targeting
				CurrentPartyMemberTarget = CurrentParty[CurrentPartyMember].Position;
				BattleLogManager.Instance.ClearAndShowMessage("Use on whom?");
				return;
			case SkillTarget.Enemy:
				CurrentEnemyTarget = Enemies.FindIndex(x => x.Actor.CurrentHP > 0);
				Enemies[CurrentEnemyTarget].ShowInfoBox(true);
				BattleLogManager.Instance.ClearAndShowMessage("Use on whom?");
				MenuManager.Instance.MoveDownOpenMenus(false);
				return;
			case SkillTarget.AllyOrEnemy:
				CurrentPartyMemberTarget = CurrentParty[CurrentPartyMember].Position;
				string key = OS.GetKeycodeString(SettingsMenuManager.Instance.GetKeybindForAction("SwitchSides"))
					.ToUpper();
				BattleLogManager.Instance.ClearAndShowMessage($"Use on whom?\nPress {key} to switch sides.");
				return;
		}

		SelectTarget();
	}

	private void SelectTarget()
	{
		if (!Enemies.Any(x => x.Actor.CurrentHP > 0))
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}

		if ((SelectedAction.Target == SkillTarget.Ally ||
		     (SelectedAction.Target == SkillTarget.AllyOrEnemy && CurrentPartyMemberTarget > -1))
		    && CurrentParty.First(x => x.Position == CurrentPartyMemberTarget).Actor.IsToast)
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}

		if ((SelectedAction.Target == SkillTarget.DeadAlly ||
		     SelectedAction.Target == SkillTarget.AllDeadAllies && CurrentPartyMemberTarget > -1)
		    && !CurrentParty.First(x => x.Position == CurrentPartyMemberTarget).Actor.IsToast)
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}

		if (SelectedAction.Target == SkillTarget.AllyNotSelf &&
		    (CurrentPartyMemberTarget == CurrentParty[CurrentPartyMember].Position
		     || CurrentParty.First(x => x.Position == CurrentPartyMemberTarget).Actor.IsToast))
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}

		if (SelectedAction.Target == SkillTarget.XRandomEnemies)
		{
			GD.PrintErr("XRandomEnemies skills are currently unsupported on party members.");
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}
		
		Actor actor = CurrentParty[CurrentPartyMember].Actor;
		switch (SelectedAction.Target)
		{
			case SkillTarget.Self:
				PlayerCommands[actor] = new BattleCommand(actor, actor, SelectedAction);
				AudioManager.Instance.PlaySFX("SYS_select");
				break;
			case SkillTarget.Ally:
			case SkillTarget.AllyNotSelf:
			case SkillTarget.DeadAlly:
				PlayerCommands[actor] = new BattleCommand(actor,
					CurrentParty.First(x => x.Position == CurrentPartyMemberTarget).Actor, SelectedAction);
				AudioManager.Instance.PlaySFX("SYS_select");
				break;
			case SkillTarget.Enemy:
				PlayerCommands[actor] = new BattleCommand(actor,
					Enemies[CurrentEnemyTarget].Actor, SelectedAction);
				AudioManager.Instance.PlaySFX("SYS_select");
				break;
			case SkillTarget.AllyOrEnemy:
				if (CurrentEnemyTarget > -1)
					PlayerCommands[actor] = new BattleCommand(actor,
						Enemies[CurrentEnemyTarget].Actor, SelectedAction);
				else
					PlayerCommands[actor] = new BattleCommand(actor,
						CurrentParty.First(x => x.Position == CurrentPartyMemberTarget).Actor, SelectedAction);
				AudioManager.Instance.PlaySFX("SYS_select");
				break;
			// select all party members for now, these will be resolved later
			case SkillTarget.AllAllies:
			case SkillTarget.AllDeadAllies:
				PlayerCommands[actor] = new BattleCommand(actor,
					GetAllPartyMembers().Select(x => x.Actor).ToList(), SelectedAction);
				break;
			case SkillTarget.AllEnemies:
				PlayerCommands[actor] = new BattleCommand(actor, GetAllAliveEnemies(), SelectedAction);
				break;
			default:
				GD.PrintErr("Unhandled SelectTarget case: " + SelectedAction.Target);
				break;
		}

		if (CurrentEnemyTarget > -1)
			Enemies[CurrentEnemyTarget].ShowInfoBox(false);
		CurrentEnemyTarget = -1;
		CurrentPartyMemberTarget = -1;
		CurrentPartyMember++;
		SelectedAction = null;
		if (CurrentPartyMember >= CurrentParty.Count)
		{
			BattleLogManager.Instance.ClearBattleLog();
			MenuManager.Instance.ShowMenu(MenuState.None);
			SetPhase(BattlePhase.PreCommand);
		}
		else
		{
			MenuManager.Instance.ShowMenu(MenuState.Battle);
			SetPhase(BattlePhase.PlayerCommand);
		}
	}

	private async void HandleCommandExecute()
	{
		BattleCommand currentAction = CurrentCommand;

		if (currentAction.Action is EmptyAction)
		{
			GD.PushWarning("HandleCommandExecute received an EmptyAction from " + currentAction.Actor.Name + ". This is a problem!");
		}

		BattleLogManager.Instance.ClearBattleLog();
		GameManager.Instance.SetBattlebackGrayscale(false);
		if (SettingsMenuManager.Instance.LogDebug)
			GD.Print("Processing action " + currentAction.Action.Name);
		List<Actor> resolvedTargets = [];
		switch (currentAction.Action.Target)
		{
			case SkillTarget.AllAllies:
				resolvedTargets.AddRange(currentAction.Actor is Enemy
					? GetAllAliveEnemies()
					: GetAlivePartyMembers().Select(x => x.Actor));
				break;
			case SkillTarget.AllEnemies:
				resolvedTargets.AddRange(currentAction.Actor is Enemy
					? GetAlivePartyMembers().Select(x => x.Actor)
					: GetAllAliveEnemies());
				break;
			case SkillTarget.XRandomEnemies:
				foreach (Actor target in currentAction.Targets)
				{
					if (IsInvalidTarget(target))
					{
						Actor newTarget = target is Enemy ? GetRandomAliveEnemy() : GetRandomAlivePartyMember();
						if (newTarget == null)
						{
							BattleLogManager.Instance.QueueMessage(currentAction.Actor.Name.ToUpper() +
							                                       "'s skill did nothing.");
							SetPhase(BattlePhase.WaitForBattleLog);
							return;
						}
						resolvedTargets.Add(newTarget);
					}
					else
					{
						resolvedTargets.Add(target);
					}
				}

				break;
			case SkillTarget.Ally:
			case SkillTarget.Enemy:
			case SkillTarget.AllyOrEnemy:
				if (IsInvalidTarget(currentAction.Targets[0]))
					resolvedTargets.Add(currentAction.Targets[0] is Enemy
						? GetRandomAliveEnemy()
						: GetRandomAlivePartyMember());
				else
					resolvedTargets.Add(currentAction.Targets[0]);
				break;
			case SkillTarget.DeadAlly:
				if (!currentAction.Targets[0].IsToast)
				{
					Actor newTarget = currentAction.Targets[0] is Enemy
						? null
						: GetRandomAlivePartyMember();
					if (newTarget == null)
					{
						BattleLogManager.Instance.QueueMessage(currentAction.Actor.Name.ToUpper() +
						                                       "'s skill did nothing.");
						SetPhase(BattlePhase.WaitForBattleLog);
						return;
					}

					resolvedTargets.Add(newTarget);
				}
				else
					resolvedTargets.Add(currentAction.Targets[0]);

				break;
			case SkillTarget.AllDeadAllies:
				List<PartyMember> deadAllies = GetDeadPartyMembers().Select(x => x.Actor).ToList();
				if (currentAction.Actor is Enemy || (deadAllies.Count == 0 && currentAction.Action is not Item))
				{
					BattleLogManager.Instance.QueueMessage(currentAction.Actor.Name.ToUpper() +
					                                       "'s skill did nothing.");
					SetPhase(BattlePhase.WaitForBattleLog);
					return;
				}

				if (deadAllies.Count > 0)
					resolvedTargets.AddRange(deadAllies);
				else
					// if nobody is toast, pass everybody so items like jam packets can still run the "it had no effect" logic
					resolvedTargets.AddRange(GetAllPartyMembers().Select(x => x.Actor));
				break;
			case SkillTarget.AllyNotSelf:
				if (currentAction.Targets[0].IsToast)
				{
					Actor newTarget = currentAction.Targets[0] is Enemy
						? GetRandomAliveUniqueEnemy(currentAction.Actor)
						: GetRandomUniqueAlivePartyMember(currentAction.Actor);
					if (newTarget == null)
					{
						BattleLogManager.Instance.QueueMessage(currentAction.Actor.Name.ToUpper() +
						                                       "'s skill did nothing.");
						SetPhase(BattlePhase.WaitForBattleLog);
						return;
					}

					resolvedTargets.Add(newTarget);
				}
				else
					resolvedTargets.Add(currentAction.Targets[0]);

				break;
			default:
				resolvedTargets.AddRange(currentAction.Targets);
				break;
		}

		if (currentAction.Action is Skill skill)
		{
			if (!skill.MeetsRequirements(currentAction.Actor))
			{
				if (currentAction.Actor.CurrentEmotion.BlocksActions
				    || skill.RequirementFailureMessage == null)
					BattleLogManager.Instance.QueueMessage(currentAction.Actor.Name.ToUpper() +
					                                       " is too AFRAID to move!");
				else
					BattleLogManager.Instance.QueueMessage(currentAction.Actor, skill.RequirementFailureMessage);
				SetPhase(BattlePhase.WaitForBattleLog);
				return;
			}
			
			int skillCost = skill.Cost(currentAction.Actor);
			if (skillCost > 0)
			{
				if (currentAction.Actor.CurrentJuice < skillCost)
				{
					BattleLogManager.Instance.QueueMessage(currentAction.Actor.Name.ToUpper() +
					                                       " does not have enough JUICE!");
					SetPhase(BattlePhase.WaitForBattleLog);
					return;
				}

				currentAction.Actor.CurrentJuice -= skillCost;
			}

			if (currentAction.Actor is PartyMember && skill.ShowFollowups &&
			    !currentAction.Actor.CurrentEmotion.BlocksActions)
			{
				if (ForceHideFollowup)
				{
					ForceHideFollowup = false;
				}
				else
				{
					PartyMemberComponent component = CurrentParty.First(x => x.Actor == currentAction.Actor);
					if (component.HasFollowup && component.FollowupSet != null)
					{
						HashSet<InputDirection> disabledDirections = [];
						foreach (var entry in component.FollowupSet.Entries)
						{
							// each bubble already grays itself on energy via its own cost
							if (!IsFollowupEntryUsable(entry.Value.TargetPosition, entry.Value.BaseSkillName,
								    includeEnergy: false))
								disabledDirections.Add(FollowupSets.DirectionFor(entry.Key, component.Position));
						}

						component.FadeInFollowups(disabledDirections);
						FollowupActive = true;
					}
				}
			}
		}

		await RunGuarded(() => currentAction.Action.Effect(currentAction.Actor, resolvedTargets),
			$"effect of {currentAction.Action.Name} used by {currentAction.Actor.Name}");

		if (BattleLogManager.Instance.IsProcessingMessage)
			SetPhase(BattlePhase.WaitForBattleLog);
		else
			SetPhase(BattlePhase.PostCommand);
	}

	private bool HandleFollowup(InputDirection direction)
	{
		if (Energy < 3 || !FollowupActive || ForceHideFollowup || !EnergyBar.Visible || FollowupSelected)
			return false;

		PartyMemberComponent current = CurrentParty.First(x => x.Actor == CurrentCommand.Actor);
		FollowupInput? role = FollowupSets.InputFor(direction, current.Position);
		if (role == null || current.FollowupSet == null ||
		    !current.FollowupSet.Entries.TryGetValue(role.Value, out FollowupEntry pair))
			return false;

		if (!IsFollowupEntryUsable(pair.TargetPosition, pair.BaseSkillName, includeEnergy: true))
			return false;

		string name = pair.BaseSkillName;
		
		// certain followups like Basil's Release Energy have no tier
		if (name == "ReleaseEnergy")
			name += FollowupTier.ToString();
		else if (name != "ReleaseEnergyBasil" && current.FollowupSet.Tiered)
			name += FollowupTier;

		if (!Database.TryGetSkill(name, out Skill skill))
			return false;

		// in base game, followups go after any other forced actions
		BattleCommand command = skill.Target is SkillTarget.AllEnemies
			? new BattleCommand(current.Actor, GetAllAliveEnemies(), skill)
			: new BattleCommand(current.Actor, CurrentCommand.Targets, skill);
		PendingFollowup = new PendingFollowupData(command, pair.TargetPosition, pair.BaseSkillName);

		// prevent followup bubbles from showing again if the followup itself is an attack
		if (skill.ShowFollowups)
			ForceHideFollowup = true;

		return true;
	}

	/// <summary>
	/// Forces a skill command to be executed after the current one.
	/// </summary>
	/// <param name="self">The actor that the command is being forced upon.</param>
	/// <param name="target">The target of the command.</param>
	/// <param name="skill">The skill that is being forced.</param>
	public void ForceCommand(Actor self, Actor target, Skill skill)
	{
		ForceCommand(self, [target], skill);
	}

	/// <summary>
	/// Forces a skill command to be executed after the current one.
	/// </summary>
	/// <param name="self">The actor that the command is being forced upon.</param>
	/// <param name="targets">The targets of the command.</param>
	/// <param name="skill">The skill that is being forced.</param>
	public void ForceCommand(Actor self, IReadOnlyList<Actor> targets, Skill skill)
	{
		if (self is PartyMember && skill.ShowFollowups)
			// if the forced skill is an attack, hide the followup bubbles
			ForceHideFollowup = true;
		// forced commands execute in the order they were queued
		ForcedCommands.Add(new BattleCommand(self, targets, skill));
		// if this actor hasn't had their normal turn, queue them for a priority turn after forced commands
		// mimics base game behavior
		if (!ActedThisTurn.Contains(self) && !PriorityActors.Contains(self))
			PriorityActors.Add(self);
	}

	private void ProcessFollowupSuccess(InputDirection direction)
	{
		FollowupSelected = true;
		AudioManager.Instance.PlaySFX("Skill2", 1f, 0.8f);
		CurrentParty.First(x => x.Actor == CurrentCommand.Actor).FadeOutFollowupsExcept(direction);
	}
	
	private bool IsFollowupEntryUsable(int targetPosition, string baseSkillName, bool includeEnergy)
	{
		PartyMemberComponent target = CurrentParty.FirstOrDefault(x => x.Position == targetPosition);
		if (target == null || target.Actor.IsToast)
			return false;

		if (baseSkillName.StartsWith("ReleaseEnergy"))
		{
			if (CurrentParty.Any(x => x.Actor.IsToast))
				return false;
			if (includeEnergy && Energy != 10)
				return false;
		}
		else if (includeEnergy && Energy < 3)
			return false;

		// PassToHero reads the position 1 member's ATK (vanilla bug), so that slot must be filled
		if (baseSkillName == "PassToHero" && GetPartyMemberAtPosition(1) == null)
			return false;

		return true;
	}

	private bool IsFollowupValid(PendingFollowupData followup)
	{
		if (IsInvalidTarget(followup.Command.Actor) || followup.Command.Actor.Stunned)
			return false;

		return IsFollowupEntryUsable(followup.TargetPosition, followup.BaseSkillName, includeEnergy: true);
	}

	private async void EndOfTurn()
	{
		if (!ProcessedEndOfTurn)
		{
			// tick down stat turn timers
			// vanilla omori bug: stats decrease before of end of turn enemy skills
			CurrentParty.ForEach(x =>
			{
				x.Actor.DecreaseStatTurnCounter();
				x.UpdateStateIcons();
			});
			Enemies.ForEach(x =>
			{
				if (!x.Actor.IsToast)
					x.Actor.DecreaseStatTurnCounter();
			});

			foreach (EnemyComponent enemy in Enemies.ToList())
			{
				if (enemy.Actor.IsToast)
					continue;
				await RunGuarded(() => enemy.Actor.ProcessEndOfTurn(), $"{enemy.Actor.Name}.ProcessEndOfTurn");
			}

			// expire observe predictions the enemy never consumed during its turn
			// ones set this turn (OBSERVE acts last) survive into the next turn
			foreach (EnemyComponent enemy in Enemies)
			{
				if (enemy.Actor.ObserveSetThisTurn)
				{
					enemy.Actor.ObserveSetThisTurn = false;
				}
				else
				{
					enemy.Actor.ObserveTarget = null;
					enemy.Actor.ObserveMultiTarget = false;
				}
			}

			RaiseGuarded(TurnEnded, "TurnEnded");
			ProcessedEndOfTurn = true;
		}
		
		if (RestartQueued)
		{
			Reset();
			ReloadPreset();
			return;
		}

		// if any forced commands or priority actors were added during ProcessEndOfTurn, run those still
		if (ForcedCommands.Count > 0 || PriorityActors.Any(a => !ActedThisTurn.Contains(a) && !IsInvalidTarget(a)))
		{
			SetPhase(BattlePhase.PreCommand);
			return;
		}

		CheckBattleOver();

		ProcessedStartOfTurn = false;
		SetPhase(BattlePhase.FightRun);
	}

	internal void OnBattleLogFinished()
	{
		if (Phase == BattlePhase.WaitForBattleLog)
			SetPhase(BattlePhase.PostCommand);
	}

	private void HandleEnemyDying()
	{
		Tween tween = CreateTween();
		tween.TweenInterval(0.75f);
		// we need to call a standalone tween property here to make the above delay work
		// otherwise, all the falling tweens will run parallel to the interval, essentially defeating the purpose
		tween.TweenProperty(DyingEnemies[0], "position", new Vector2(DyingEnemies[0].Position.X, 600f), 0.50f);
		foreach (Node2D enemy in DyingEnemies.Skip(1))
		{
			tween.Parallel().TweenProperty(enemy, "position", new Vector2(enemy.Position.X, 600f), 0.50f);
		}

		tween.TweenCallback(Callable.From(EnemiesDoneDying));
	}

	private void EnemiesDoneDying()
	{
		DyingEnemies.ForEach(x => x.Visible = false);
		DyingEnemies.Clear();
		SetPhase(BattlePhase.PreCommand);
	}

	/// <summary>
	/// Runs a check to see if the battle is over.
	/// </summary>
	public async void CheckBattleOver()
	{
		if (CurrentParty.All(x => x.Actor.CurrentHP == 0))
		{
			SetPhase(BattlePhase.BattleOver);
			await EndOfBattle(false);
			BattleLogManager.Instance.ClearAndShowMessage(CurrentParty[0].Actor.Name.ToUpper() +
			                                              "'s party was defeated...");
			ShowEndOfBattleOptions();
			return;
		}

		PartyMemberComponent omori =
			CurrentParty.FirstOrDefault(x => x.Actor is Omori omori && omori.IsToast);
		// if any omori is toast, the battle is over
		// this may change in the future
		if (omori != null)
		{
			SetPhase(BattlePhase.BattleOver);
			await EndOfBattle(false);
			BattleLogManager.Instance.ClearAndShowMessage(CurrentParty[0].Actor.Name.ToUpper() +
			                                              "'s party was defeated...");
			ShowEndOfBattleOptions();
			return;
		}
		
		if (DeferredDeathEnemies.Count > 0)
			return;
		
		if (!Enemies.Any(x => x.Actor.CurrentHP > 0))
		{
			SetPhase(BattlePhase.BattleOver);
			await EndOfBattle(true);
			CurrentParty.ForEach(x =>
			{
				x.Actor.RemoveStatModifier("PlotArmor");
				// the victory animation overrides plot armor
				if (!x.Actor.IsToast)
					x.Actor.PlayAnimation("victory", EmotionAsset.Victory);
			});
			if (GameType is GameModeType.BossRush)
			{
				string previousBGM = Stages[CurrentStage].BGM;
				float currentPosition = AudioManager.Instance.GetBGMPosition();
				AudioManager.Instance.PlayBGM("xx_victory");
				BattleLogManager.Instance.ClearAndShowMessage(CurrentParty[0].Actor.Name.ToUpper() + "'s party was victorious!");
				CurrentStage++;
				if (CurrentStage < Stages.Count)
				{
					await Wait.Milliseconds(3000);
					await AnimationManager.Instance.WaitForTintScreen(Colors.Black, 0.5f);
					SummonEnemiesForStage(Stages[CurrentStage].Enemies);
					GameManager.Instance.SetBattleback(Stages[CurrentStage].Battleback);
					GameManager.Instance.SetBattlebackGrayscale(false);
					BattleLogManager.Instance.ClearBattleLog();
					MenuManager.Instance.ClearLastSelected();
					await Wait.Milliseconds(1000);
					CurrentParty.ForEach(x =>
					{
						x.Actor.HasUsedPlotArmor = false;
						if (Stages[CurrentStage].HealParty)
						{
							if (x.Actor.IsToast)
								x.Actor.Revive(x.Actor.CurrentStats.MaxHP);
							else
								x.Actor.CurrentHP = x.Actor.CurrentStats.MaxHP;
							x.Actor.CurrentJuice = x.Actor.CurrentStats.MaxJuice;
						}
						else if (x.Actor.CurrentHP == 0)
						{
							x.Actor.Revive(1);
							x.Actor.SetEmotion("neutral", true);
						}

						// drop the victory override, the emotion kept underneath shows again
						x.Actor.ClearAnimation();
						if (!Stages[CurrentStage].KeepEmotion)
							x.Actor.SetEmotion("neutral", true);
						if (!Stages[CurrentStage].KeepStatusEffects)
							x.Actor.RemoveAllStatModifiers();
						x.UpdateStateIcons();
					});
					AudioManager.Instance.PlayBGM(Stages[CurrentStage].BGM, 0.05f,
						(float)Stages[CurrentStage].BGMPitch);
					if (previousBGM == Stages[CurrentStage].BGM)
						AudioManager.Instance.SeekBGM(currentPosition);
					AudioManager.Instance.SetBGMLoopOffset(Stages[CurrentStage].BGMLoopPoint);
					AudioManager.Instance.FadeBGMTo(1f, 0.5f);
					await AnimationManager.Instance.WaitForTintScreen(ColorsExtension.TransparentBlack, 0.5f);
					ProcessedStartOfTurn = false;
					CallDeferred(MethodName.PreBattle);
					return;
				}
			}
			else
			{
				AudioManager.Instance.PlayBGM("xx_victory");
				BattleLogManager.Instance.ClearAndShowMessage(CurrentParty[0].Actor.Name.ToUpper() +
				                                              "'s party was victorious!");
			}

			ShowEndOfBattleOptions();
		}
	}

	private void ShowEndOfBattleOptions()
	{
		EndOfBattleOptionsContainer.Visible = true;
		StageSelectorContainer.Visible = GameType is GameModeType.BossRush;
	}

	private async Task EndOfBattle(bool victory)
	{
		foreach (PartyMemberComponent p in CurrentParty)
			await RunGuarded(() => p.Actor.OnEndOfBattle(victory), $"{p.Actor.Name}.OnEndOfBattle");
		foreach (EnemyComponent e in Enemies)
		{
			if (e.Actor.IsToast)
				continue;
			await RunGuarded(() => e.Actor.OnEndOfBattle(victory), $"{e.Actor.Name}.OnEndOfBattle");
		}
		RaiseGuarded(BattleEnded, new BattleEndedEventArgs(victory), "BattleEnded");
	}

	// run all battle-related tasks guarded, so a broken skill or actor doesn't softlock the game
	private static async Task RunGuarded(Func<Task> hook, string context)
	{
		try
		{
			await hook();
		}
		catch (Exception ex)
		{
			GD.PushError($"Unhandled exception in {context}: {ex}");
		}
	}

	private static void RunGuarded(Action hook, string context)
	{
		try
		{
			hook();
		}
		catch (Exception ex)
		{
			GD.PushError($"Unhandled exception in {context}: {ex}");
		}
	}

	// mirrors RunGuarded for events. each subscriber is raised separately, so one throwing
	// handler is logged without skipping the others or breaking the battle flow
	private void RaiseGuarded(EventHandler handler, string context)
	{
		if (handler == null)
			return;
		foreach (EventHandler subscriber in handler.GetInvocationList().Cast<EventHandler>())
			RunGuarded(() => subscriber(this, EventArgs.Empty), $"{context} event handler");
	}

	private void RaiseGuarded<T>(EventHandler<T> handler, T args, string context)
	{
		if (handler == null)
			return;
		foreach (EventHandler<T> subscriber in handler.GetInvocationList().Cast<EventHandler<T>>())
			RunGuarded(() => subscriber(this, args), $"{context} event handler");
	}

	/// <summary>
	/// Calculates damage. Misses, critical hits, emotion effectiveness, sad juice loss, and stat modifiers are all taken into account.
	/// </summary>
	/// <remarks>
	/// On top of calculating damage, this function also handles displaying damage numbers, playing sound effects, and queuing battle log messages for misses and critical hits.
	/// </remarks>
	/// <param name="self">The attacker.</param>
	/// <param name="target">The target/defender.</param>
	/// <param name="damageFunc">The damage function to use in the damage calculation.<br/><br/>
	/// A common example is the calculation for basic attacks, as shown by this example:<br/>
	/// <c>() => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF;</c></param>
	/// <param name="neverMiss">If this attack should never miss.</param>
	/// <param name="variance">The damage variance. Damage will be multiplied between (1 - variance) and (1 + variance).</param>
	/// <param name="guaranteeCrit">If this attack should guarantee a critical hit.</param>
	/// <param name="neverCrit">If this attack should never be a critical hit.</param>
	/// <param name="ignoreEmotion">If this attack should ignore emotion advantage.</param>
	/// <param name="attackElement">Overrides the attacker's emotion "element" in the advantage calculation.<br/>
	/// Currently only <c>"exploit"</c> is supported, used by Perfectheart's EXPLOIT to always deal advantage damage.</param>
	/// <param name="silent">If this attack should not log anything to the BattleLog. Damage numbers will still display.</param>
	/// <returns>The final damage after all critical, emotion, juice loss, and stat modifications have been applied.</returns>
	public int Damage(Actor self, Actor target, Func<float> damageFunc, bool neverMiss = true, float variance = 0.2f,
		bool guaranteeCrit = false, bool neverCrit = false, bool ignoreEmotion = false, string attackElement = null, bool silent = false)
	{
		if (!neverMiss && RollMissOrEvade(self, target, silent))
			return -1;

		float damage = Math.Max(0, damageFunc());
		// locked bosses resolve advantage as their locked emotion
		Emotion selfEmotion = self.EffectiveEmotion;
		Emotion targetEmotion = target.EffectiveEmotion;

		ApplyOverrides(DamagePhase.PreEmotion, ref damage, self, target, false, neverMiss);

		int effectiveness = 0;
		if (!ignoreEmotion)
			damage = CalculateEmotionModifiers(selfEmotion, targetEmotion, damage, out effectiveness, attackElement);
		bool critical =
			(self.CurrentStats.LCK * .01f >= GameManager.Instance.Random.Randf() || guaranteeCrit ||
			 target.HasStatModifier("Tickle")) && !neverCrit;

		// even though we know if the attack is a crit or not at this stage, pass false
		// this may change in the future depending on feedback and use case
		ApplyOverrides(DamagePhase.PreCrit, ref damage, self, target, false, neverMiss);

		if (critical)
		{
			damage *= 1.5f;
			if (!silent) BattleLogManager.Instance.QueueMessage("IT HIT RIGHT IN THE HEART!");
			AudioManager.Instance.PlaySFX("BA_CRITICAL_HIT", volume: 2f);
		}

		ApplyOverrides(DamagePhase.PreFlatCrit, ref damage, self, target, critical, neverMiss);

		if (critical)
		{
			damage += 1.5f;
		}

		ApplyOverrides(DamagePhase.PreVariance, ref damage, self, target, critical, neverMiss);

		damage = CalculateVariance(damage, variance);

		ApplyOverrides(DamagePhase.PreRounding, ref damage, self, target, critical, neverMiss);

		float rounded = (float)Math.Round(damage, MidpointRounding.AwayFromZero);

		if (rounded < 0)
			rounded = 0;
		if (!SettingsMenuManager.Instance.DisableDamageLimit && rounded > 9999)
			rounded = 9999;

		ApplyOverrides(DamagePhase.PreJuice, ref rounded, self, target, critical, neverMiss);

		// sadness converts part of the damage to juice loss, using the lock-resolved emotion's bleed fraction
		int juiceLost = 0;
		if (targetEmotion.JuiceBleedFraction > 0)
		{
			juiceLost = Math.Min((int)Math.Floor(rounded * targetEmotion.JuiceBleedFraction), target.CurrentJuice);
			rounded -= juiceLost;
		}

		target.CurrentJuice -= juiceLost;

		if (ShouldDoDebugDamage())
			rounded *= 10;
		if (rounded < 0)
			rounded = 0;
		if (!SettingsMenuManager.Instance.DisableDamageLimit && rounded > 9999)
			rounded = 9999;

		rounded = (float)Math.Round(rounded, MidpointRounding.AwayFromZero);

		ApplyOverrides(DamagePhase.PreApply, ref rounded, self, target, critical, neverMiss);

		int roundedInt = (int)rounded;
		
		// if damage ends up being below zero due to a stat modifier, treat it as a pseudo-miss
		// exactly 0 damage still proceeds through as normal
		if (roundedInt < 0)
		{
			// undo the sad juice bleed, the hit never landed
			target.CurrentJuice += juiceLost;
			AudioManager.Instance.PlaySFX("BA_miss");
			SpawnDamageNumber(-1, target.CenterPoint, DamageType.Miss);
			BattleLogManager.Instance.QueueMessage(self, "[actor]'s attack did nothing.");
			return -1;
		}
		target.Damage(roundedInt);
		if (target is PartyMember)
		{
			Energy = Math.Min(10, Energy + 1);
		}

		SpawnDamageNumber(roundedInt, target.CenterPoint, critical: critical);
		// we don't need to play a hitsound if the attack is a critical or if there's no damage
		if (!critical && roundedInt > 0)
		{
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print("Effectiveness: " + effectiveness);
			if (effectiveness > 0)
			{
				if (!silent) BattleLogManager.Instance.QueueMessage("...It was a moving attack!");
				AudioManager.Instance.PlaySFX("se_impact_double", 1f, 0.9f);
			}
			else if (effectiveness < 0)
			{
				if (!silent) BattleLogManager.Instance.QueueMessage("...It was a dull attack.");
				AudioManager.Instance.PlaySFX("se_impact_soft", 1f, 0.9f);
			}
			else
				AudioManager.Instance.PlaySFX("SE_dig", 0.7f, 0.9f);
		}

		if (!silent) BattleLogManager.Instance.QueueMessage(self, target, "[target] takes " + roundedInt + " damage!");

		if (juiceLost > 0)
		{
			if (!silent) BattleLogManager.Instance.QueueMessage(self, target, "[target] lost " + juiceLost + " JUICE...");
			SpawnDamageNumber(juiceLost, target.CenterPoint, DamageType.JuiceLoss);
		}

		ApplyOverrides(DamagePhase.PostApply, ref rounded, self, target, critical, neverMiss);

		RaiseGuarded(DamageDealt, new DamageDealtEventArgs(self, target, roundedInt, juiceLost, critical, false),
			"DamageDealt");

		return roundedInt;
	}

	private bool RollMissOrEvade(Actor self, Actor target, bool silent)
	{
		int hitRate = self.CurrentStats.HIT;
		int evasion = target.CurrentStats.EVA;
		int roll = GameManager.Instance.Random.RandiRange(0, 100);
		bool miss, evaded;
		if (SettingsMenuManager.Instance.CombinedAccuracy)
		{
			// most OMORI mods use a combined accuracy roll
			miss = hitRate - evasion < roll;
			evaded = miss && roll <= hitRate;
		}
		else
		{
			//  base RPGMaker uses two separate rolls in this order
			miss = hitRate < roll;
			evaded = !miss && GameManager.Instance.Random.RandiRange(0, 100) < evasion;
		}

		if (!miss && !evaded)
			return false;

		if (evaded)
		{
			if (!silent) BattleLogManager.Instance.QueueMessage(self, target, "[target] evaded the attack!");
			AudioManager.Instance.PlaySFX("GEN_Swish", volume: 0.9f);
		}
		else
		{
			if (!silent) BattleLogManager.Instance.QueueMessage(self, target, "[actor]'s attack missed...");
			AudioManager.Instance.PlaySFX("BA_miss");
		}
		// Miss text spawns a little further down
		SpawnDamageNumber(-1, target.CenterPoint, DamageType.Miss);
		return true;
	}

	private void ApplyOverrides(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isCritical,
		bool neverMiss)
	{
		foreach (StatModifier mod in attacker.StatModifiers.Values)
			mod.OverrideDamage(phase, ref damage, attacker, defender, true, isCritical, neverMiss);
		foreach (StatModifier mod in defender.StatModifiers.Values)
			mod.OverrideDamage(phase, ref damage, attacker, defender, false, isCritical, neverMiss);
	}

	/// <summary>
	/// Calculates juice damage. Misses, critical hits, emotion effectiveness, and stat modifiers are all taken into account. Sadness damage reduction, however, is not.
	/// </summary>
	/// /// <remarks>
	/// Unlike <see cref="Damage(Actor, Actor, Func{float}, bool, float, bool, bool, bool, string, bool)"/>, this method does not play hit sounds, however it does display damage numbers and queues the battle log.
	/// </remarks>
	/// <param name="self">The attacker.</param>
	/// <param name="target">The target/defender.</param>
	/// <param name="damageFunc">The damage function to use in the damage calculation.</param>
	/// <param name="neverMiss">If this attack should never miss.</param>
	/// <param name="variance">The damage variance. Damage will be multiplied between (1 - variance) and (1 + variance).</param>
	/// <param name="guaranteeCrit">If this attack should guarantee a critical hit.</param>
	/// <param name="neverCrit">If this attack should never be a critical hit.</param>
	/// <param name="ignoreEmotion">If this attack should ignore emotion advantage.</param>
	/// <param name="attackElement">Overrides the attacker's emotion "element" in the advantage calculation.<br/>
	/// Currently only <c>"exploit"</c> is supported, used by Perfectheart's EXPLOIT to always deal advantage damage.</param>
	/// <param name="silent">If this attack should not log anything to the BattleLog. Damage numbers will still display.</param>
	/// <returns>The final juice damage after all critical, emotion, and stat modifications have been applied.</returns>
	public int DamageJuice(Actor self, Actor target, Func<float> damageFunc, bool neverMiss = true,
		float variance = 0.2f, bool guaranteeCrit = false, bool neverCrit = false, bool ignoreEmotion = false, string attackElement = null, bool silent = false)
	{
		if (!neverMiss && RollMissOrEvade(self, target, silent))
			return -1;

		float damage = damageFunc();
		// locked bosses resolve advantage as their locked emotion
		ApplyOverrides(DamagePhase.PreEmotion, ref damage, self, target, false, neverMiss);

		damage = CalculateEmotionModifiers(self.EffectiveEmotion, target.EffectiveEmotion, damage, out _, attackElement);
		bool critical =
			(self.CurrentStats.LCK * .01f >= GameManager.Instance.Random.Randf() || guaranteeCrit ||
			 target.HasStatModifier("Tickle")) && !neverCrit;

		ApplyOverrides(DamagePhase.PreCrit, ref damage, self, target, false, neverMiss);

		if (critical)
		{
			damage *= 1.5f;
			if (!silent) BattleLogManager.Instance.QueueMessage("IT HIT RIGHT IN THE HEART!");
			AudioManager.Instance.PlaySFX("BA_CRITICAL_HIT", volume: 2f);
		}

		ApplyOverrides(DamagePhase.PreFlatCrit, ref damage, self, target, critical, neverMiss);

		if (critical)
		{
			damage += 1.5f;
		}

		ApplyOverrides(DamagePhase.PreVariance, ref damage, self, target, critical, neverMiss);

		damage = CalculateVariance(damage, variance);

		ApplyOverrides(DamagePhase.PreRounding, ref damage, self, target, critical, neverMiss);

		float rounded = (float)Math.Round(damage, MidpointRounding.AwayFromZero);

		if (ShouldDoDebugDamage())
			rounded *= 10;
		if (rounded < 0)
			rounded = 0;
		if (!SettingsMenuManager.Instance.DisableDamageLimit && rounded > 9999)
			rounded = 9999;

		ApplyOverrides(DamagePhase.PreJuice, ref rounded, self, target, critical, neverMiss);
		ApplyOverrides(DamagePhase.PreApply, ref rounded, self, target, critical, neverMiss);

		int roundedInt = (int)rounded;
		// a stat modifier fully negated the hit and drove the damage negative — treat it like a miss
		if (roundedInt < 0)
		{
			AudioManager.Instance.PlaySFX("BA_miss");
			SpawnDamageNumber(-1, target.CenterPoint, DamageType.Miss);
			return -1;
		}
		target.DamageJuice(roundedInt);
		SpawnDamageNumber(roundedInt, target.CenterPoint, DamageType.JuiceLoss);
		if (!silent) BattleLogManager.Instance.QueueMessage(self, target, "[target] lost " + roundedInt + " JUICE...");
		ApplyOverrides(DamagePhase.PostApply, ref rounded, self, target, critical, neverMiss);
		RaiseGuarded(DamageDealt, new DamageDealtEventArgs(self, target, 0, roundedInt, critical, true),
			"DamageDealt");
		return roundedInt;
	}

	// some healing and juice skills are affected by emotion

	/// <summary>
	/// Calculates emotion-based healing to the <paramref name="target"/>.
	/// </summary>
	/// <remarks>
	/// Some healing in OMORI is "bugged" and is influenced by emotion, which this method replicates.
	/// </remarks>
	/// <param name="self">The healer.</param>
	/// <param name="target">The target being healed.</param>
	/// <param name="healFunc">The function to use in the heal calculation.</param>
	/// <param name="variance">The healing variance. Healed HP will be multiplied between (1 - variance) and (1 + variance).</param>
	/// <param name="silent">If this healing should not log anything to the BattleLog. Damage numbers will still be displayed.</param>
	public void Heal(Actor self, Actor target, Func<float> healFunc, float variance = 0.2f, bool silent = false)
	{
		float baseHealing = healFunc();
		// vanilla's bugged healing reads the raw emotions, ignoring locks
		baseHealing = CalculateEmotionModifiers(self.CurrentEmotion, target.CurrentEmotion, baseHealing, out _);
		baseHealing = CalculateVariance(baseHealing, variance);
		int rounded = (int)Math.Round(baseHealing, MidpointRounding.AwayFromZero);
		target.Heal(rounded);
		SpawnDamageNumber(rounded, target.CenterPoint, DamageType.Heal);
		if (!silent) BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {rounded} HEART!");
		RaiseGuarded(Healed, new HealedEventArgs(self, target, rounded, false), "Healed");
	}

	/// <summary>
	/// Calculates emotion-based juice healing to the <paramref name="target"/>.
	/// </summary>
	/// /// <remarks>
	/// Some juice healing in OMORI is "bugged" and is influenced by emotion, which this method replicates.
	/// </remarks>
	/// <param name="self">The healer.</param>
	/// <param name="target">The target being healed.</param>
	/// <param name="healFunc">The healing variance. Healed Juice will be multiplied between (1 - variance) and (1 + variance).</param>
	/// <param name="silent">If this healing should not log anything to the BattleLog. Damage numbers will still be displayed.</param>
	public void HealJuice(Actor self, Actor target, Func<float> healFunc, bool silent = false)
	{
		float baseJuice = healFunc();
		// vanilla's bugged healing reads the raw emotions, ignoring locks
		float finalJuice = CalculateEmotionModifiers(self.CurrentEmotion, target.CurrentEmotion, baseJuice, out _);
		int rounded = (int)Math.Round(finalJuice, MidpointRounding.AwayFromZero);
		target.HealJuice(rounded);
		SpawnDamageNumber(rounded, target.CenterPoint, DamageType.JuiceGain);
		if (!silent) BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {rounded} JUICE!");
		RaiseGuarded(Healed, new HealedEventArgs(self, target, rounded, true), "Healed");
	}

	// RPGMaker applyVariance method
	private float CalculateVariance(float damage, float variance)
	{
		int amp = (int)Math.Floor(Math.Max(Math.Abs(damage) * variance, 0));
		int v = GameManager.Instance.Random.RandiRange(0, amp) + GameManager.Instance.Random.RandiRange(0, amp) - amp;
		return damage + v;
	}

	// weaknesses and resistances by tier
	private readonly float[] weakness = [1.5f, 2f, 2.5f];
	private readonly float[] resistance = [0.8f, 0.65f, 0.5f];

	private float CalculateEmotionModifiers(Emotion self, Emotion target, float damage, out int effect, string attackElement = null)
	{
		Emotion neutral = Database.NeutralEmotion;

		// the exploit attack element is always a "moving" hit against any emotion
		if (attackElement == "exploit" && target != neutral)
		{
			effect = 1;
			if (target.DefensiveRateOverrides.TryGetValue("exploit", out float exploitRate))
				return damage * exploitRate;
			if (target.Group == null)
			{
				GD.PushWarning("Emotion " + target.Id + " has no group or exploit rate, EXPLOIT will deal normal damage to it.");
				return damage;
			}

			return damage * weakness[TierIndex(target)];
		}

		// a rate keyed by the attacker's group overrides the tier-based multiplier
		if (self.Group != null && target.DefensiveRateOverrides.TryGetValue(self.Group.Id, out float groupRate))
		{
			effect = groupRate > 1f ? 1 : groupRate < 1f ? -1 : 0;
			return damage * groupRate;
		}

		// emotions that are weak to all emotions like afraid check for any emotion
		if (self != neutral && target.DefensiveRateOverrides.TryGetValue("emotion", out float emotionRate))
		{
			effect = emotionRate > 1f ? 1 : emotionRate < 1f ? -1 : 0;
			return damage * emotionRate;
		}

		effect = 0;
		if (self.Group == null || target.Group == null)
		{
			return damage;
		}

		float multiplier = 1.0f;
		if (self.Group.BeatsGroupId == target.Group.Id)
		{
			effect = 1;
			multiplier = weakness[TierIndex(target)];
		}
		else if (target.Group.BeatsGroupId == self.Group.Id)
		{
			effect = -1;
			multiplier = resistance[TierIndex(target)];
		}

		return damage * multiplier;
	}
	
	private int TierIndex(Emotion emotion)
	{
		return Math.Clamp(emotion.Tier - 1, 0, weakness.Length - 1);
	}

	/// <summary>
	/// Gives the provided <see cref="Actor"/> a random emotion.<br/>If the actor already has that emotion, it will be upgraded.
	/// </summary>
	/// <param name="who"></param>
	public void RandomEmotion(Actor who)
	{
		IReadOnlyList<EmotionGroup> pool = Database.GetRandomEmotionGroups();
		if (pool.Count == 0)
			return;

		EmotionGroup group = pool[GameManager.Instance.Random.RandiRange(0, pool.Count - 1)];
		if (!TryGetNextEmotionInGroup(who, group, out Emotion next))
			return; // already at the group's highest tier

		// unlike MakeEmotion, capped and invalid targets fail silently
		if (who.IsEmotionValid(next))
			who.SetEmotion(next.Id);
	}

	/// <summary>
	/// Spawns a damage number at the specified <paramref name="position"/>.
	/// </summary>
	/// <remarks>
	/// If a damage number already exists at the given <paramref name="position"/>, it will be moved down until an empty space is found<br/>
	/// This can be useful to spawn multiple damage numbers without having to calculate offsets yourself.
	/// </remarks>
	/// <param name="damage">The number to display.</param>
	/// <param name="position">The screen position to spawn the damage number at.</param>
	/// <param name="type">The <see cref="DamageType"/> of the damage. This value will modify the color of the damage number.</param>
	/// <param name="critical">If true, the damage number will blink red when spawned.</param>
	public void SpawnDamageNumber(int damage, Vector2 position, DamageType type = DamageType.Damage,
		bool critical = false)
	{
		if (DamageNumbersDisabled)
			return;

		DamageNumber dmg = new(damage, position, type, critical);
		AddChild(dmg);

		GetTree().CreateTimer(1.5f).Timeout += () =>
		{
			if (IsInstanceValid(dmg))
				dmg.Despawn();
		};
	}

	/// <summary>
	/// Spawns an enemy at the given screen <paramref name="position"/>.
	/// </summary>
	/// <param name="who">Which enemy to spawn.</param>
	/// <param name="position">The screen position to spawn the enemy at. The enemy will spawn centered at this position.</param>
	/// <param name="startingEmotion">The enemy's starting emotion.</param>
	/// <param name="fallsOffScreen">Whether this enemy should fall off-screen when defeated.</param>
	/// <param name="grayscaleOnDefeat">Whether this enemy should trigger a grayscale effect when defeated.</param>
	/// <param name="layer">The layer to spawn this enemy on.</param>
	/// <returns>The <see cref="EnemyComponent"/> of the spawned enemy.</returns>
	public EnemyComponent SummonEnemy(string who, Vector2 position, string startingEmotion = "neutral",
		bool fallsOffScreen = true, bool grayscaleOnDefeat = false, int layer = 0)
	{
		while (Enemies.Any(x => x.Actor.CenterPoint == position))
		{
			// if this enemy is going to spawn on top of another, nudge them a little
			// this way the targeting system still works
			position.X += 0.1f;
		}

		BattlePresetEnemy en = new()
		{
			Name = who,
			Emotion = startingEmotion,
			FallsOffScreen = fallsOffScreen,
			GrayscaleOnDefeat = grayscaleOnDefeat,
			Layer = layer
		};
		EnemyComponent enemy = GameManager.Instance.SpawnEnemy(en, position);
		Enemies.Add(enemy);

		// mid-battle spawns automatically get an immediate turn via PriorityActors
		if (Phase is BattlePhase.PreCommand or BattlePhase.CommandExecute
		    or BattlePhase.WaitForBattleLog or BattlePhase.PostCommand or BattlePhase.EnemyDying)
		{
			if (!PriorityActors.Contains(enemy.Actor))
				PriorityActors.Add(enemy.Actor);
		}

		return enemy;
	}
	

	/// <summary>
	/// Transforms an enemy into another enemy, despawning the original and spawning the replacement
	/// at the same position. The original enemy is immediately removed from the battle.
	/// </summary>
	/// <remarks>Special care must be used when using this method, as the old enemy will immediately become invalid.</remarks>
	/// <param name="original">The enemy to transform from.</param>
	/// <param name="who">The database name of the enemy to transform into.</param>
	/// <param name="startingEmotion">Starting emotion for the new enemy.</param>
	/// <param name="offset">The position offset relative to the old enemy's position.</param>
	/// <returns>The newly spawned <see cref="EnemyComponent"/>.</returns>
	public EnemyComponent TransformEnemy(Enemy original, string who, string startingEmotion = "neutral", Vector2 offset = default)
	{
		EnemyComponent target = Enemies.FirstOrDefault(x => x.Actor == original);
		if (target == null) return null;
		
		Enemies.Remove(target);
		target.GetParent().QueueFree();
		
		return SummonEnemy(who, original.CenterPoint + offset, startingEmotion, original.FallsOffScreen, original.GrayscaleOnDefeat, original.Layer);
	}

	/// <summary>
	/// Adds the given <paramref name="amount"/> to the energy bar, up to a maximum of 10.
	/// </summary>
	/// <param name="amount">The amount of energy to add.</param>
	public void AddEnergy(int amount)
	{
		Energy = Math.Clamp(Energy + amount, 0, 10);
	}

	/// <summary>
	/// Returns true if "Enable Debug Damage" is enabled and if the user is holding down the Debug Damage key.
	/// </summary>
	public bool ShouldDoDebugDamage()
	{
		return DebugDamageHeld && SettingsMenuManager.Instance.EnableDebugDamage;
	}
	
	/// <summary>
	/// Makes the given <see cref="Actor"/> feel an emotion of the given group, if possible.
	/// Increases the tier if the actor already feels an emotion of the group.
	/// </summary>
	/// <param name="who">The <see cref="Actor"/> to change.</param>
	/// <param name="groupId">The id of the <see cref="EmotionGroup"/> to apply.</param>
	public void MakeEmotion(Actor who, string groupId)
	{
		if (!Database.TryGetEmotionGroup(groupId, out EmotionGroup group))
		{
			GD.PrintErr("Unknown emotion group: " + groupId);
			return;
		}

		if (!TryGetNextEmotionInGroup(who, group, out Emotion next))
		{
			ShowMaxTierMessage(who, group);
			return;
		}

		if (who.IsEmotionValid(next))
			who.SetEmotion(next.Id);
		else
			ShowMaxTierMessage(who, group);
	}
	
	private bool TryGetNextEmotionInGroup(Actor who, EmotionGroup group, out Emotion next)
	{
		Emotion current = who.CurrentEmotion;
		int tier = current.Group == group ? current.Tier + 1 : 1;
		return Database.TryGetEmotionByGroupTier(group.Id, tier, out next);
	}

	private void ShowMaxTierMessage(Actor who, EmotionGroup group)
	{
		if (group.MaxTierMessage != null)
			BattleLogManager.Instance.QueueMessage(null, who, group.MaxTierMessage);
	}

	/// <summary>
	/// Makes the given <see cref="Actor"/> sad, if possible. Increases the tier if the actor is already sad.
	/// </summary>
	/// <param name="who">The <see cref="Actor"/> to make sad.</param>
	public void MakeSad(Actor who)
	{
		MakeEmotion(who, "sad");
	}

	/// <summary>
	/// Makes the given <see cref="Actor"/> happy, if possible. Increases the tier if the actor is already happy.
	/// </summary>
	/// <param name="who">The <see cref="Actor"/> to make happy.</param>
	public void MakeHappy(Actor who)
	{
		MakeEmotion(who, "happy");
	}

	/// <summary>
	/// Makes the given <see cref="Actor"/> angry, if possible. Increases the tier if the actor is already angry.
	/// </summary>
	/// <param name="who">The <see cref="Actor"/> to make angry.</param>
	public void MakeAngry(Actor who)
	{
		MakeEmotion(who, "angry");
	}

	/// <summary>
	/// Whether the given <see cref="Actor"/> has already acted this turn.
	/// </summary>
	/// <param name="who">The actor to check.</param>
	/// <returns>True if the actor has acted this turn.</returns>
	public bool HasActedThisTurn(Actor who)
	{
		return ActedThisTurn.Contains(who);
	}

	/// <returns>A random alive <see cref="PartyMember"/>, or null if no party members are alive.</returns>
	public PartyMember GetRandomAlivePartyMember()
	{
		List<PartyMemberComponent> alive = CurrentParty.Where(x => x.Actor.CurrentHP > 0).ToList();
		return alive.Count == 0 ? null : alive[GameManager.Instance.Random.RandiRange(0, alive.Count - 1)].Actor;
	}
	
	/// <returns>Returns a random alive <see cref="PartyMember"/> that's not the provided actor, or null if no match is found.</returns>
	public PartyMember GetRandomUniqueAlivePartyMember(Actor not)
	{
		List<PartyMemberComponent> alive = CurrentParty.Where(x => x.Actor.CurrentHP > 0 && x.Actor != not).ToList();
		return alive.Count == 0 ? null : alive[GameManager.Instance.Random.RandiRange(0, alive.Count - 1)].Actor;
	}

	/// <returns>A random <see cref="PartyMember"/> that is toast, or null if none is found.</returns>
	public PartyMember GetRandomDeadPartyMember()
	{
		PartyMemberComponent result = CurrentParty.FirstOrDefault(x => x.Actor.CurrentHP <= 0);
		return result?.Actor;
	}

	/// <returns>A random alive <see cref="Enemy"/>, or null if none is found.</returns>
	public Enemy GetRandomAliveEnemy()
	{
		List<EnemyComponent> alive = Enemies.Where(x => x.Actor.CurrentHP > 0).ToList();
		return alive.Count == 0 ? null : alive[GameManager.Instance.Random.RandiRange(0, alive.Count - 1)].Actor;
	}

	/// <returns>A random alive <see cref="Enemy"/> that's not the provided actor, or null if none is found.</returns>
	public Enemy GetRandomAliveUniqueEnemy(Actor not)
	{
		List<EnemyComponent> alive = Enemies.Where(x => x.Actor.CurrentHP > 0 && x.Actor != not).ToList();
		return alive.Count == 0 ? null :  alive[GameManager.Instance.Random.RandiRange(0, alive.Count - 1)].Actor;
	}

	/// <returns>All currently alive <see cref="Enemy"/>s.</returns>
	public List<Enemy> GetAllAliveEnemies()
	{
		return Enemies.Select(x => x.Actor).Where(x => x.CurrentHP > 0).ToList();
	}

	/// <returns>All current <see cref="Enemy"/>s, including both alive and dead enemies. See <see cref="GetAllAliveEnemies"/> to only select alive enemies.</returns>
	public List<Enemy> GetAllEnemies()
	{
		return Enemies.Select(x => x.Actor).ToList();
	}

	/// <summary>
	/// Checks if an <see cref="Actor"/> is valid for targeting, as in, they are not toast and have not been despawned.
	/// </summary>
	/// <param name="actor">The <see cref="Actor"/> to check.</param>
	/// <returns>True if the actor is considered invalid.</returns>
	public bool IsInvalidTarget(Actor actor)
	{
		return actor == null || actor.IsToast || (actor is Enemy ? Enemies.All(x => x.Actor != actor) : CurrentParty.All(x => x.Actor != actor));
	}

	/// <returns>The <see cref="BattleCommand"/> that is currently being processed.</returns>
	public BattleCommand GetCurrentCommand()
	{
		return CurrentCommand;
	}

	/// <summary>
	/// Gets the <see cref="PartyMemberComponent"/> of all party members who are not toast.
	/// </summary>
	public List<PartyMemberComponent> GetAlivePartyMembers()
	{
		return CurrentParty.Where(x => x.Actor.CurrentHP > 0).ToList();
	}

	/// <summary>
	/// Gets the <see cref="PartyMemberComponent"/> of all party members who are currently toast.
	/// </summary>
	public List<PartyMemberComponent> GetDeadPartyMembers()
	{
		return CurrentParty.Where(x => x.Actor.CurrentHP <= 0 && x.Actor.IsToast).ToList();
	}

	/// <summary>
	/// Gets all party members, including ones who are toast.
	/// </summary>
	/// <remarks>
	/// In most situations, such as skill logic, use <see cref="GetAlivePartyMembers"/> instead.
	/// </remarks>
	public List<PartyMemberComponent> GetAllPartyMembers()
	{
		return CurrentParty;
	}

	/// <summary>
	/// Whether any living party member has one of the given weapons equipped.
	/// </summary>
	/// <remarks>
	/// Equipment perks (such as the FRYING PAN snack boost) do not apply while their wearer is toast.
	/// </remarks>
	public bool PartyHasLivingWeapon(params string[] weaponNames)
	{
		return GetAlivePartyMembers().Any(x => weaponNames.Contains(x.Actor.Weapon?.Name));
	}

	/// <summary>
	/// Whether any living party member has the given charm equipped.
	/// </summary>
	/// <remarks>
	/// Equipment perks (such as the BREADPHONES full-heal revive) do not apply while their wearer is toast.
	/// </remarks>
	public bool PartyHasLivingCharm(string charmName)
	{
		return GetAlivePartyMembers().Any(x => x.Actor.Charm?.Name == charmName);
	}

	/// <summary>
	/// Retrieves the <see cref="PartyMember"/> at the given internal array <paramref name="index"/> in the party.
	/// </summary>
	/// <remarks>
	/// This is the <i>order the PartyMembers are added to the party</i>, not their on-screen position.<br/>
	/// Use <see cref="GetPartyMemberAtPosition"/> for that purpose instead.
	/// </remarks>
	/// <param name="index"></param>
	public PartyMember GetPartyMember(int index)
	{
		return CurrentParty.ElementAtOrDefault(Math.Clamp(index, 0, 3))?.Actor;
	}

	/// <summary>
	/// Retrieves the <see cref="PartyMember"/> at the given <paramref name="position"/> in the party.
	/// If no party member is present at the given position, null is returned instead.
	/// </summary>
	/// <remarks>
	/// Valid <paramref name="position"/> values include 0 (Bottom Left), 1 (Top Left), 2 (Bottom Right), and 3 (Top Right).
	/// </remarks>
	/// <param name="position">The position to retrieve.</param>
	/// <returns>The corresponding <see cref="PartyMember"/> if present, otherwise null.</returns>
	public PartyMember GetPartyMemberAtPosition(int position)
	{
		PartyMemberComponent member = CurrentParty.FirstOrDefault(x => x.Position == position);
		return member?.Actor;
	}

	/// <summary>
	/// Retrieves the <see cref="PartyMember"/> who is currently selecting their action.
	/// </summary>
	/// <returns>The <see cref="PartyMember"/> who is currently selecting their action, otherwise null.</returns>
	public PartyMember GetCurrentPartyMember()
	{
		return CurrentParty.ElementAtOrDefault(CurrentPartyMember)?.Actor;
	}

	/// <summary>
	/// Adds an item to the party's inventory.
	/// </summary>
	/// <param name="name">The database name of the item.</param>
	/// <param name="quantity">The item quantity to give.</param>
	public void AddItem(string name, int quantity)
	{
		if (!Database.TryGetItem(name, out Item item))
		{
			GD.PrintErr("Unknown item: " + name);
			return;
		}
		if (!Items.TryAdd(name, quantity))
			Items[name] += quantity;
	}

	/// <summary>
	/// Retrieves all snacks in the inventory, as well as their quantities.
	/// </summary>
	public List<(Item, int)> GetSnacks()
	{
		List<(Item, int)> result = [];
		foreach (var entry in Items)
		{
			if (Database.TryGetItem(entry.Key, out Item item))
			{
				if (!item.IsToy)
					result.Add((item, entry.Value));
			}
		}

		return result;
	}

	/// <summary>
	/// Retrieves all toys in the inventory, as well as their quantities.
	/// </summary>
	public List<(Item, int)> GetToys()
	{
		List<(Item, int)> result = [];
		foreach (var entry in Items)
		{
			if (Database.TryGetItem(entry.Key, out Item item))
			{
				if (item.IsToy)
					result.Add((item, entry.Value));
			}
		}

		return result;
	}

	// converts item name to CamelCase for dictionary lookup
	private string CapitalizeItemName(Item item)
	{
		// Godot's Captialize treats '-' as a regular character and puts a space after it
		// manually fix that for sno-cone
		return item.Name == "SNO-CONE" ? "Sno-Cone" : item.Name.Capitalize();
	}
}

internal enum BattlePhase
{
	PreBattle,
	FightRun,
	PlayerCommand,
	TargetSelection,
	SkillSelection,
	PreCommand,
	CommandExecute,
	WaitForBattleLog,
	PostCommand,
	EnemyDying,
	BattleOver
}
