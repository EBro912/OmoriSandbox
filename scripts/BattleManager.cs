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
	private int ForcedCountAtCommandStart = 0;
	private IReadOnlyList<Actor> CommandTargets = [];
	private IReadOnlyList<Actor> TierTargets = [];
	private readonly Dictionary<string, List<int>> TierLandings = [];
	private readonly Dictionary<(string, Actor), int> TierSteps = [];
	private int TierReactionDepth;
	private PendingFollowupData PendingFollowup = null;
	private readonly HashSet<BattleMenuOption> DisabledMenuOptions = [];

	// vanilla only spends followup energy when the followup is actually used, so the
	// selection records everything needed to re-validate and pay at use time
	private sealed record PendingFollowupData(BattleCommand Command, PartyMemberComponent User, FollowupEntry Entry);
	private readonly List<Actor> PriorityActors = [];
	private readonly HashSet<EnemyComponent> DeferredDeathEnemies = [];
	private BattleCommand CurrentCommand = null;
	private Timer Delay;
	private readonly List<Node2D> DyingEnemies = [];
	private Tween FallTween;
	/// <summary>
	/// The most of one item the inventory can hold, matching RPGMaker's item cap.
	/// </summary>
	public const int MaxItemCount = 99;
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
			RunAsReaction(() => EnergyChanged?.Invoke(this, EventArgs.Empty));
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
	/// Fired after healing (Heart or Juice) is applied through <see cref="Heal"/> or <see cref="HealJuice"/>.
	/// Healing applied directly to an <see cref="Actor"/> (as most snacks do) does not raise this event.
	/// </summary>
	public event EventHandler<HealedEventArgs> Healed;

	/// <summary>
	/// Fired once an action's targets are resolved, right before the action runs.
	/// Not fired for actions that don't run.
	/// </summary>
	/// <remarks>
	/// An action doesn't run, and raises neither this event nor <see cref="ActionEnded"/>, when:
	/// <list type="bullet">
	/// <item><description>the actor fails the skill's requirements (for example, it is too AFRAID) or can't pay its JUICE cost.</description></item>
	/// <item><description>no valid target is left.</description></item>
	/// </list>
	/// Forced commands skip the requirement and JUICE checks, but are still dropped when no valid target is left or their actor is toast.
	/// A command dropped for having no valid target still spends its JUICE cost.
	/// </remarks>
	public event EventHandler<ActionEventArgs> ActionStarted;

	/// <summary>
	/// Fired after an action's effect has finished running, whether it hit, missed or was evaded, and even if the effect threw.
	/// Every <see cref="ActionStarted"/> is followed by exactly one ActionEnded with the same arguments.
	/// </summary>
	public event EventHandler<ActionEventArgs> ActionEnded;

	/// <summary>
	/// Fired right before an enemy's <see cref="Enemy.ProcessAI"/> decides the action it performs.
	/// Not fired for the turn-order rolls made while <c>Enemy.PreRolling</c> is set, or for forced commands.
	/// </summary>
	public event EventHandler<AIDecidingEventArgs> AIDeciding;

	/// <summary>
	/// Fired right after an enemy's <see cref="Enemy.ProcessAI"/> has decided the action it performs.
	/// Like <see cref="AIDeciding"/>, only the real roll raises it.
	/// Don't change or replace the command here; queue extra actions with <see cref="ForceCommand(Actor, Actor, Skill)"/> instead.
	/// </summary>
	public event EventHandler<AIDecidedEventArgs> AIDecided;

	private bool FollowupActive = false;
	private bool FollowupSelected = false;
	private bool ForceHideFollowup = false;
	private int FollowupTier = 1;
	private bool DamageNumbersDisabled = false;
	private bool DebugDamageHeld = false;

	internal bool CombinedBuffsDebuffs { get; private set; }

	/// <summary>
	/// Whether the current preset is in Toastless mode, where the battle is lost as soon as any party member becomes toast.
	/// </summary>
	public bool ToastlessMode { get; private set; }

	private bool RestartQueued = false;
	private double RestartTimer;

	private readonly MenuRepeat DirectionRepeat = new();

	/// <summary>
	/// Whether a battle is currently ongoing.
	/// </summary>
	public bool IsBattling { get; private set; } = false;

	/// <summary>
	/// The battle's turn count. Starts from 0 at battle start and resets in every new battle and Boss Rush stage.
	/// </summary>
	/// <remarks>
	/// Like base game, it increments when the command menu closes, so turn N reads N from <see cref="Enemy.ProcessStartOfCommands"/> through <see cref="TurnEnded"/>, while <see cref="TurnStarted"/> and the command menu before turn N read N - 1.
	/// </remarks>
	public int TurnCount { get; private set; }

	private bool ProcessedStartOfTurn = false;
	private bool ProcessedStartOfCommands = false;
	private bool ProcessedEndOfTurn = false;
	private bool InEndOfTurnHooks = false;
	private bool ResumingStartOfTurn = false;
	private bool BattleStartForcesOnly = false;

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
			GameManager.Instance.ForceSetBattleback(preset.Battleback);
			AudioManager.Instance.ForcePlayBGM(preset.BGM, 1f, (float)preset.BGMPitch);
			AudioManager.Instance.ForceSetBGMLoopOffset(preset.BGMLoopPoint);
		}
		else
		{
			Enemies = [];
			CurrentStage = startingStage;
			SummonEnemiesForStage(stages[CurrentStage].Enemies);
			StageSelectorSpinBox.MaxValue = stages.Count - 1;
			StageSelectorSpinBox.Value = startingStage;
			GameManager.Instance.ForceSetBattleback(preset.Stages[CurrentStage].Battleback);
			AudioManager.Instance.ForcePlayBGM(preset.Stages[CurrentStage].BGM, 1f, (float)preset.Stages[CurrentStage].BGMPitch);
			AudioManager.Instance.ForceSetBGMLoopOffset(preset.Stages[CurrentStage].BGMLoopPoint);
		}

		Items = preset.Items.ToDictionary(x => x.Key, x => Math.Min(x.Value, MaxItemCount));
		MenuManager.Instance.SetPresetSkin(preset.MenuSkin, preset.ItemlessMode);
		Energy = Math.Clamp(preset.StartingEnergy, 0, 10);
		FollowupTier = preset.FollowupTier;
		DamageNumbersDisabled = preset.DisableDamageNumbers;
		CombinedBuffsDebuffs = preset.CombinedBuffsDebuffs;
		ToastlessMode = preset.Toastless;
		GameManager.Instance.ApplyPresetWindowSkin(preset.WindowSkin);
		AudioManager.Instance.BGMLocked = preset.LockBGMAndBattleback;
		GameManager.Instance.BattlebackLocked = preset.LockBGMAndBattleback;
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
		{
			if (!enemy.Position.StartsWith("Vector2"))
				enemy.Position = "Vector2" + enemy.Position;
			SummonEnemy(enemy, GD.StrToVar(enemy.Position).AsVector2());
		}
	}

	private async void PreBattle()
	{
		// since a party member's start of battle hook could spawn another party member,
		// run while party members have not been processed instead of indexing the party directly
		HashSet<Actor> processed = [];
		// SummonPartyMember already quits early on a full party so a while true here is safe to use
		while (true)
		{
			PartyMemberComponent next = CurrentParty.FirstOrDefault(x => !processed.Contains(x.Actor));
			if (next == null)
				break;
			processed.Add(next.Actor);
			await RunGuarded(() => next.Actor.OnStartOfBattle(), $"{next.Actor.Name}.OnStartOfBattle");
			if (RestartQueued)
			{
				Reset();
				ReloadPreset();
				return;
			}
		}

		foreach (EnemyComponent component in Enemies.ToList())
		{
			if (!Enemies.Contains(component))
				continue;
			Actor enemy = component.Actor;
			await RunGuarded(enemy.OnStartOfBattle, $"{enemy.Name}.OnStartOfBattle");
			if (RestartQueued)
			{
				Reset();
				ReloadPreset();
				return;
			}
		}
		RaiseGuarded(BattleStarted, new BattleStartedEventArgs(CurrentPresetName, CurrentStage), "BattleStarted");
		CurrentParty.ForEach(x => x.UpdateStateIcons());
		if (ForcedCommands.Count > 0)
		{
			foreach (PartyMemberComponent member in CurrentParty)
				ActedThisTurn.Add(member.Actor);
			foreach (EnemyComponent enemy in Enemies)
				ActedThisTurn.Add(enemy.Actor);
			ProcessedStartOfCommands = true;
			ProcessedEndOfTurn = false;
			BattleStartForcesOnly = true;
			SetPhase(BattlePhase.PreCommand);
			return;
		}
		SetPhase(BattlePhase.FightRun);
	}

	public override void _Process(double delta)
	{
		if (!IsBattling)
			return;

		EnergyDots.Tick(delta);
		DirectionRepeat.Update(delta);

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
					} while (CurrentParty[CurrentPartyMember].Actor.IsToast || CurrentParty[CurrentPartyMember].Actor.Stunned);

					Actor prev = CurrentParty[CurrentPartyMember].Actor;
					if (PlayerCommands.TryGetValue(prev, out BattleCommand prevCmd) && prevCmd.Action is Item item)
					{
						if (!Items.TryAdd(item.Id, 1))
							Items[item.Id]++;
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
						if (!Items.TryAdd(i.Id, 1))
							Items[i.Id]++;
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

		if (DirectionRepeat.Direction != Vector2I.Zero)
		{
			InputDirection direction = DirectionRepeat.Direction switch
			{
				{ Y: < 0 } => InputDirection.Up,
				{ Y: > 0 } => InputDirection.Down,
				{ X: < 0 } => InputDirection.Left,
				_ => InputDirection.Right
			};
			HandleInputDirection(direction, DirectionRepeat.Fresh);
		}

		if (Input.IsActionJustPressed("SwitchSides"))
		{
			if (Phase == BattlePhase.TargetSelection && SelectedAction.Target == SkillTarget.AllyOrEnemy)
			{
				if (CurrentPartyMemberTarget > -1)
				{
					CurrentPartyMemberTarget = -1;
					CurrentEnemyTarget = Enemies.FindIndex(x => !x.Actor.IsToast);
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
			if (!RestartQueued)
			{
				RestartTimer -= (float)delta;
				if (RestartTimer <= 0)
				{
					// if the restart is requested during a turn or during the start-of-battle
					// hooks (intro dialogue), queue it to prevent anything breaking
					if (Phase is BattlePhase.PreBattle or
					    BattlePhase.PreCommand or
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

	private void HandleInputDirection(InputDirection direction, bool fresh)
	{
		if (fresh && Phase is BattlePhase.CommandExecute or BattlePhase.WaitForBattleLog)
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
			    && Enemies.Count(x => !x.Actor.IsToast) > 1
			    && direction is InputDirection.Left or InputDirection.Right)
			{
				int old = CurrentEnemyTarget;
				CurrentEnemyTarget = SelectEnemy(CurrentEnemyTarget, direction, fresh);
				// only play a sound if the cursor actually moves
				if (CurrentEnemyTarget != old)
				{
					Enemies[old].ShowInfoBox(false);
					Enemies[CurrentEnemyTarget].ShowInfoBox(true);
					AudioManager.Instance.PlaySFX("SYS_move");
				}
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

	private int SelectEnemy(int current, InputDirection direction, bool allowWrap)
	{
		if (Enemies.Count < 2)
			return current;

		var sortedEnemies = Enemies
			.Select((enemy, index) => new { Enemy = enemy, Index = index })
			.Where(e => !e.Enemy.Actor.IsToast)
			.OrderBy(e => e.Enemy.Actor.CenterPoint.X)
			.ThenBy(e => e.Index)
			.ToList();

		int sortedPosition = sortedEnemies.FindIndex(e => e.Index == current);
		bool wrap = allowWrap && SettingsMenuManager.Instance.EnemySelectionWrapping;

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
		AnimationManager.Instance.StopAllVideos();
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
		ForcedCountAtCommandStart = 0;
		PendingFollowup = null;
		ForceHideFollowup = false;
		PriorityActors.Clear();
		DisabledMenuOptions.Clear();
		DeferredDeathEnemies.Clear();
		DyingEnemies.Clear();
		if (FallTween != null && FallTween.IsValid())
			FallTween.Kill();
		FallTween = null;
		CurrentCommand = null;
		CommandTargets = [];
		EndTiersFromFirstTarget();
		GameManager.Instance.SetBattlebackGrayscale(false);
		AnimationManager.Instance.ResetScreenEffects();
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
		AudioManager.Instance.BGMLocked = false;
		GameManager.Instance.BattlebackLocked = false;
		SettingsMenuManager.Instance.ActiveRuleOverrides = null;
		ToastlessMode = false;
		GameManager.Instance.ApplyPresetWindowSkin(Database.DefaultWindowSkin);
		Phase = BattlePhase.PreBattle;
		TurnCount = 0;
		ProcessedStartOfTurn = false;
		ProcessedStartOfCommands = false;
		ProcessedEndOfTurn = false;
		InEndOfTurnHooks = false;
		ResumingStartOfTurn = false;
		BattleStartForcesOnly = false;
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
		if (!GameManager.Instance.LoadBattlePreset(preset, (int)StageSelectorSpinBox.Value))
			MainMenuManager.Instance.ReturnToTitle();
	}

	internal void OnSelectAttack()
	{
		if (!IsMenuOptionEnabled(BattleMenuOption.Attack))
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}
		SelectedAction = CurrentParty[CurrentPartyMember].Actor.Skills.Values.FirstOrDefault();
		if (SelectedAction == null)
		{
			// the preset gave this actor no skills at all
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}
		PartyMember attacker = CurrentParty[CurrentPartyMember].Actor;
		if (SelectedAction is Skill attack &&
		    (!attack.MeetsRequirements(attacker) || attacker.CurrentJuice - attack.Cost(attacker) < 0))
		{
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
		BattleMenuOption option = what switch
		{
			MenuState.Snack => BattleMenuOption.Snack,
			MenuState.Toy => BattleMenuOption.Toy,
			_ => BattleMenuOption.Skill
		};
		if (!IsMenuOptionEnabled(option))
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}
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

		if (!Items.TryGetValue(item.Id, out int value))
		{
			GD.PrintErr("Selected item is not in the inventory: " + item.Id);
			return false;
		}
		Items[item.Id] = --value;
		if (value == 0)
			Items.Remove(item.Id);

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
					TurnCount++;
					foreach (EnemyComponent enemy in Enemies.ToList())
					{
						if (enemy.Actor.IsToast || !Enemies.Contains(enemy))
							continue;
						await RunGuarded(() => enemy.Actor.ProcessStartOfCommands(),
							$"{enemy.Actor.Name}.ProcessStartOfCommands");
					}
					// vanilla rolls every enemy's action at input time and sorts the turn by that action's speed,
					// re-rolls again whenever the enemy changes emotion group before acting,
					// then rolls it *again* when the enemy actually acts
					foreach (EnemyComponent enemy in Enemies.ToList())
					{
						if (enemy.Actor.IsToast || enemy.Actor.Stunned)
							continue;
						RunGuarded(() => enemy.Actor.PreRollTurnPriority(), $"{enemy.Actor.Name}.PreRollTurnPriority");
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
					if (CurrentCommand.Actor is Enemy actingEnemy)
						actingEnemy.HasPreRolledAction = false;
					if (SettingsMenuManager.Instance.LogDebug)
						GD.Print("Next action: " + CurrentCommand.Action.Name + " by " + CurrentCommand.Actor.Name);
					SetPhase(BattlePhase.CommandExecute);
				}

				break;
			case BattlePhase.PostCommand:
			{
				// handle any stat modifiers that tick at the end of an action
				if (!CurrentCommand.Actor.IsToast)
				{
					// vanilla clears the actor's result before action-end states tick
					CurrentCommand.Actor.ClearResult();
					CurrentCommand.Actor.DecreaseActionStatTurnCounter();
				}

				await ConvertDeadPartyMembers();

				if (CurrentCommand.Actor is PartyMember
				    && CurrentCommand.Action is Skill skill
				    && skill.ShowFollowups)
				{
					PartyMemberComponent component = ((PartyMember)CurrentCommand.Actor).Component;
					if (component.HasFollowup)
					{
						component.FadeOutFollowups();
					}
				}

				FollowupSelected = false;
				FollowupActive = false;

				if (CurrentCommand.Forced)
					await ResolveForcedCommandDeaths();

				foreach (PartyMemberComponent member in CurrentParty.ToList())
				{
					if (member.Actor.IsToast)
						continue;
					await RunGuarded(() => member.Actor.ProcessBattleConditions(),
						$"{member.Actor.Name}.ProcessBattleConditions");
				}

				foreach (EnemyComponent enemy in Enemies.ToList())
				{
					if (enemy.Actor.IsToast || !Enemies.Contains(enemy))
						continue;
					enemy.Actor.SetHurt(false);
					await RunGuarded(() => enemy.Actor.ProcessBattleConditions(),
						$"{enemy.Actor.Name}.ProcessBattleConditions");
					await ResolveEnemyDeath(enemy);
				}
				await ResolveEnemyDeaths();

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
				member.Actor.ClearStatModifiersOnDeath();
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

	private async Task<bool> ResolveEnemyDeath(EnemyComponent enemy)
	{
		// enemy may have been removed during ProcessBattleConditions
		if (!Enemies.Contains(enemy) || enemy.Actor.IsToast)
		{
			DeferredDeathEnemies.Remove(enemy);
			return false;
		}
		bool hasForced = ForcedCommands.Any(f => f.Actor == enemy.Actor);
		if (enemy.Actor.ImmortalReleasePending && !hasForced)
			enemy.Actor.RemoveImmortal();
		if (enemy.Actor.CurrentHP > 0 || enemy.Actor.IsImmortal)
		{
			DeferredDeathEnemies.Remove(enemy);
			return false;
		}
		bool keepsWindow = DeferredDeathEnemies.Contains(enemy)
			? hasForced
			: CurrentCommand is { Forced: false } && ForcedCommands.Skip(ForcedCountAtCommandStart).Any(f => f.Actor == enemy.Actor);
		if (keepsWindow)
		{
			// defer death until all forced commands for this enemy have executed
			DeferredDeathEnemies.Add(enemy);
			return false;
		}
		await DefeatEnemy(enemy);
		return true;
	}

	private async Task ResolveForcedCommandDeaths()
	{
		foreach (EnemyComponent enemy in Enemies.ToList())
		{
			if (!Enemies.Contains(enemy) || enemy.Actor.IsToast || enemy.Actor.CurrentHP > 0 || enemy.Actor.IsImmortal)
				continue;
			if (DeferredDeathEnemies.Contains(enemy) && !CommandTargets.Contains(enemy.Actor))
				continue;
			await DefeatEnemy(enemy);
		}
	}

	private async Task ResolveEnemyDeaths()
	{
		HashSet<EnemyComponent> defeatedThisPass = [];
		bool defeated = true;
		while (defeated)
		{
			defeated = false;
			foreach (EnemyComponent enemy in Enemies.ToList())
			{
				if (defeatedThisPass.Contains(enemy))
					continue;
				if (await ResolveEnemyDeath(enemy))
				{
					defeatedThisPass.Add(enemy);
					defeated = true;
				}
			}
		}
	}

	private async Task DefeatEnemy(EnemyComponent enemy)
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
		enemy.Actor.ClearStatModifiersOnDeath();
		if (enemy.Actor.FallsOffScreen)
			DyingEnemies.Add(enemy.GetParent<Node2D>());
		DeferredDeathEnemies.Remove(enemy);
	}

	private BattleCommand GetNextAction()
	{
		// forced commands have the highest priority
		while (ForcedCommands.Count > 0)
		{
			BattleCommand forced = ForcedCommands[0];
			ForcedCommands.RemoveAt(0);
			// skip forced commands whose actor has died since being queued
			if (!IsInvalidTarget(forced.Actor))
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
				Energy -= followup.Entry.EnergyCost;
				return followup.Command;
			}
			if (followup.Command.Action is Skill followupSkill && followupSkill.ShowFollowups)
				ForceHideFollowup = false;
		}

		if (BattleStartForcesOnly)
			return null;

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
			    && (IsInvalidTarget(member.Actor) || member.Actor.Stunned || !PlayerCommands.ContainsKey(member.Actor)))
			{
				if (PlayerCommands.TryGetValue(member.Actor, out BattleCommand refundCmd)
				    && refundCmd.Action is Item item)
				{
					if (!Items.TryAdd(item.Id, 1))
						Items[item.Id]++;
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
				int priority = 0;
				if (a is PartyMember member && PlayerCommands.TryGetValue(member, out BattleCommand cmd))
					priority = cmd.Action.Priority;
				else if (a is Enemy enemy)
					priority = enemy.TurnPriority;
				return (long)a.CurrentStats.SPD + priority;
			})
			.ThenBy(a =>
			{
				PartyMemberComponent c = (a as PartyMember)?.Component;
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
		{
			RaiseGuarded(AIDeciding, new AIDecidingEventArgs(enemy), "AIDeciding");
			BattleCommand decided = enemy.ProcessAI();
			RaiseGuarded(AIDecided, new AIDecidedEventArgs(enemy, decided), "AIDecided");
			return decided;
		}
		return null;
	}

	// handles the quirk of enemy AI rerolling whenever their emotion group changes if they have not acted yet
	// this only matters for priority skills with high speed as they will reroll again whenever it is time to take their action
	internal void OnEnemyEmotionChanged(Enemy enemy, Emotion previous, Emotion current)
	{
		if (previous.GroupId == current.GroupId)
			return;
		
		// skip if the enemy cannot act
		if (!enemy.HasPreRolledAction || ActedThisTurn.Contains(enemy) || enemy.IsToast || enemy.Stunned)
			return;
		RunGuarded(() => RunAsReaction(() => enemy.PreRollTurnPriority(emotionReroll: true)), $"{enemy.Name}.PreRollTurnPriority");
		if (SettingsMenuManager.Instance.LogDebug)
			GD.Print($"{enemy.Name} changed emotion group to {current.Id} before acting, re-rolled turn priority: {enemy.TurnPriority}");
	}

	private async void HandleFightRun()
	{
		// consume a restart that was queued in the frame between PreBattle's last
		// hook check and the phase change, so it doesn't linger until end of turn
		if (RestartQueued)
		{
			Reset();
			ReloadPreset();
			return;
		}

		if (ResumingStartOfTurn)
			ResumingStartOfTurn = false;
		else
		{
			CurrentPartyMember = -1;
			CurrentEnemyTarget = -1;
			CurrentPartyMemberTarget = -1;
			PlayerCommands.Clear();
			ActedThisTurn.Clear();
			ForcedCommands.Clear();
			ForcedCountAtCommandStart = 0;
			// a followup discarded here never spent its energy, so there is nothing to refund
			PendingFollowup = null;
			ForceHideFollowup = false;
			PriorityActors.Clear();
			DeferredDeathEnemies.Clear();
			CurrentCommand = null;
			ProcessedEndOfTurn = false;
			ProcessedStartOfCommands = false;
			BattleStartForcesOnly = false;
			foreach (EnemyComponent enemy in Enemies)
				enemy.Actor.HasPreRolledAction = false;
			GameManager.Instance.FadeOutBattlebackGrayscale();
		}
		if (!ProcessedStartOfTurn)
		{
			foreach (PartyMemberComponent member in CurrentParty.Where(x => !x.Actor.IsToast))
			{
				foreach (StatModifier modifier in member.Actor.StatModifiers.Values)
				{
					RunGuarded(() => modifier.OnStartOfTurn(member.Actor),
						$"{modifier.GetType().Name}.OnStartOfTurn ({member.Actor.Name})");
				}

				if (member.Actor.Weapon != null)
					await RunGuarded(() => member.Actor.Weapon.StartOfTurn(member.Actor),
						$"weapon {member.Actor.Weapon.Name}.StartOfTurn ({member.Actor.Name})");
				if (member.Actor.Charm != null)
					await RunGuarded(() => member.Actor.Charm.StartOfTurn(member.Actor),
						$"charm {member.Actor.Charm.Name}.StartOfTurn ({member.Actor.Name})");
			}

			foreach (EnemyComponent e in Enemies.ToList())
			{
				if (e.Actor.IsToast || !Enemies.Contains(e))
					continue;
				foreach (StatModifier modifier in e.Actor.StatModifiers.Values)
				{
					RunGuarded(() => modifier.OnStartOfTurn(e.Actor),
						$"{modifier.GetType().Name}.OnStartOfTurn ({e.Actor.Name})");
				}
				
				await RunGuarded(() => e.Actor.ProcessStartOfTurn(), $"{e.Actor.Name}.ProcessStartOfTurn");
			}

			if (Phase == BattlePhase.FightRun)
				await ResolveEnemyDeaths();
			RaiseGuarded(TurnStarted, "TurnStarted");
			ProcessedStartOfTurn = true;
		}

		CurrentParty.ForEach(x => x.UpdateStateIcons());

		// a start-of-turn hook may have ended the battle or changed phase
		// don't show the fight menu over it
		if (Phase != BattlePhase.FightRun)
			return;

		if (DyingEnemies.Count > 0)
		{
			ResumingStartOfTurn = true;
			SetPhase(BattlePhase.EnemyDying);
			return;
		}

		CheckBattleOver();
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

		MenuManager.Instance.ShowButtons();
		MenuManager.Instance.ShowMenu(MenuState.Party);
	}

	private void HandlePlayerCommand()
	{
		// treat stunned and toast actors the same
		// no point in letting a stunned party member choose a command if they can't do anything
		while (CurrentParty[CurrentPartyMember].Actor.IsToast || CurrentParty[CurrentPartyMember].Actor.Stunned)
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
			Equipment weapon = CurrentParty[CurrentPartyMember].Actor.Weapon;
			Equipment charm = CurrentParty[CurrentPartyMember].Actor.Charm;
			BattleLogManager.Instance.ClearAndShowMessage(
				$"What will {CurrentParty[CurrentPartyMember].Actor.Name.ToUpper()} do?" +
				$"[font_size=18]\n[ATK: {stats.ATK}, DEF: {stats.DEF}, SPD: {stats.SPD}, LCK: {stats.LCK}, HIT: {stats.HIT}, EVA: {stats.EVA}]" +
				$"\n[Weapon: [color=#c263e1]{(weapon == null ? "None" : weapon.Name)}[/color], Charm: [color=#c263e1]{(charm == null ? "None" : charm.Name)}[/color]]");
		}
		else
			BattleLogManager.Instance.ClearAndShowMessage("What will " +
			                                              CurrentParty[CurrentPartyMember].Actor.Name.ToUpper() +
			                                              " do?");

		MenuManager.Instance.ShowButtons();
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
				CurrentEnemyTarget = Enemies.FindIndex(x => !x.Actor.IsToast);
				Enemies[CurrentEnemyTarget].ShowInfoBox(true);
				BattleLogManager.Instance.ClearAndShowMessage("Use on whom?");
				MenuManager.Instance.MoveDownOpenMenus(false);
				return;
			case SkillTarget.AllyOrEnemy:
				CurrentPartyMemberTarget = CurrentParty[CurrentPartyMember].Position;
				string key = SettingsMenuManager.Instance.GetBindingDisplayForAction("SwitchSides")
					.ToUpper();
				BattleLogManager.Instance.ClearAndShowMessage($"Use on whom?\nPress {key} to switch sides.");
				return;
		}

		SelectTarget();
	}

	private void SelectTarget()
	{
		if (Enemies.All(x => x.Actor.IsToast))
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
					GetAllPartyMembers(), SelectedAction);
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
		ForcedCountAtCommandStart = ForcedCommands.Count;
		CommandTargets = [];

		if (currentAction.Action is EmptyAction)
		{
			GD.PushWarning("HandleCommandExecute received an EmptyAction from " + currentAction.Actor.Name + ". This is a problem!");
		}

		BattleLogManager.Instance.ClearBattleLog();
		GameManager.Instance.FadeOutBattlebackGrayscale();
		if (SettingsMenuManager.Instance.LogDebug)
			GD.Print("Processing action " + currentAction.Action.Name);
		List<Actor> resolvedTargets = [];
		bool noTarget = false;
		currentAction.Targets = currentAction.Targets?.Select(ResolveTransformed).ToList();
		switch (currentAction.Action.Target)
		{
			case SkillTarget.AllAllies:
				resolvedTargets.AddRange(currentAction.Actor is Enemy
					? GetAllAliveEnemies()
					: GetAlivePartyMembers());
				break;
			case SkillTarget.AllEnemies:
				resolvedTargets.AddRange(currentAction.Actor is Enemy
					? GetAlivePartyMembers()
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
							noTarget = true;
							break;
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
			case SkillTarget.AllyNotSelf:
			case SkillTarget.Enemy:
			case SkillTarget.AllyOrEnemy:
				// in vanilla, automatic retargeting will target the first valid actor in the troop, not a random one
				if (IsInvalidTarget(currentAction.Targets[0]))
				{
					Actor replacement = currentAction.Targets[0] is Enemy
						? GetAllAliveEnemies().FirstOrDefault()
						: GetAlivePartyMembers().FirstOrDefault();
					if (replacement == null)
						noTarget = true;
					else
						resolvedTargets.Add(replacement);
				}
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
						noTarget = true;
					else
						resolvedTargets.Add(newTarget);
				}
				else
					resolvedTargets.Add(currentAction.Targets[0]);

				break;
			case SkillTarget.AllDeadAllies:
				List<PartyMember> deadAllies = GetDeadPartyMembers();
				if (currentAction.Actor is Enemy || (deadAllies.Count == 0 && currentAction.Action is not Item))
					noTarget = true;
				else if (deadAllies.Count > 0)
					resolvedTargets.AddRange(deadAllies);
				else
					// if nobody is toast, pass everybody so items like jam packets can still run the "it had no effect" logic
					resolvedTargets.AddRange(GetAllPartyMembers());
				break;
			default:
				resolvedTargets.AddRange(currentAction.Targets);
				break;
		}

		if (!noTarget && currentAction.Action.Target == SkillTarget.Enemy && resolvedTargets.Count == 1)
			resolvedTargets[0] = ResolveTauntTarget(currentAction.Actor, resolvedTargets[0]);

		if (currentAction.Action is Skill skill)
		{
			if (!currentAction.Forced && !skill.MeetsRequirements(currentAction.Actor))
			{
				if (currentAction.Actor.CurrentEmotion.BlocksActions)
					BattleLogManager.Instance.QueueMessage(currentAction.Actor.Name.ToUpper() +
					                                       " is too AFRAID to move!");
				else if (skill.RequirementFailureMessage != null)
					BattleLogManager.Instance.QueueMessage(currentAction.Actor, skill.RequirementFailureMessage);
				if (BattleLogManager.Instance.IsProcessingMessage)
					SetPhase(BattlePhase.WaitForBattleLog);
				else
					SetPhase(BattlePhase.PostCommand);
				return;
			}
			
			int skillCost = skill.Cost(currentAction.Actor);
			if (skillCost > 0)
			{
				if (!currentAction.Forced && currentAction.Actor.CurrentJuice < skillCost)
				{
					BattleLogManager.Instance.QueueMessage(currentAction.Actor.Name.ToUpper() +
					                                       " does not have enough JUICE!");
					SetPhase(BattlePhase.WaitForBattleLog);
					return;
				}

				currentAction.Actor.CurrentJuice = Math.Max(0, currentAction.Actor.CurrentJuice - skillCost);
			}

			if (noTarget)
			{
				EndCommandWithoutTarget(currentAction);
				return;
			}

			if (currentAction.Actor is PartyMember && skill.ShowFollowups)
			{
				if (ForceHideFollowup)
				{
					ForceHideFollowup = false;
				}
				else
				{
					PartyMemberComponent component = ((PartyMember)currentAction.Actor).Component;
					if (component.HasFollowup && component.FollowupSet != null)
					{
						bool afraid = currentAction.Actor.CurrentEmotion.BlocksActions;
						HashSet<InputDirection> disabledDirections = [];
						foreach (var entry in component.FollowupSet.Entries)
						{
							// each bubble already grays itself on energy via its own cost
							if (afraid || !IsFollowupEntryUsable(component, entry.Value, includeEnergy: false))
								disabledDirections.Add(FollowupSets.DirectionFor(entry.Key, component.Position));
						}

						component.FadeInFollowups(disabledDirections);
						FollowupActive = !afraid;
					}
				}
			}
		}

		if (noTarget)
		{
			EndCommandWithoutTarget(currentAction);
			return;
		}

		if (resolvedTargets.Count > 0 && resolvedTargets[^1] != null)
		{
			switch (currentAction.Action.LastTargetRecord)
			{
				case LastTargetRecord.Target:
					RecordLastTarget(currentAction.Actor, resolvedTargets[^1]);
					break;
				case LastTargetRecord.User:
					RecordLastTarget(currentAction.Actor, currentAction.Actor);
					break;
			}
		}

		CommandTargets = resolvedTargets;
		ActionEventArgs actionArgs = new(currentAction.Actor, currentAction.Action, resolvedTargets);
		RaiseGuarded(ActionStarted, actionArgs, "ActionStarted");

		BeginTiersFromFirstTarget(currentAction.Action, resolvedTargets);
		if (currentAction.Action.Setup != null)
			await RunGuarded(() => currentAction.Action.Setup(currentAction.Actor, resolvedTargets),
				$"setup of {currentAction.Action.Name} used by {currentAction.Actor.Name}");

		// RPGMaker clears the user's and each target's action result when the action applies,
		// so modifiers removed before this point stop blocking re-adds. a setup phase runs before this on purpose
		// to recreate the "bug" of attempting to apply a state before results are cleared
		currentAction.Actor.ClearResult();
		foreach (Actor resolved in resolvedTargets)
			resolved.ClearResult();

		await RunGuarded(() => currentAction.Action.Effect(currentAction.Actor, resolvedTargets),
			$"effect of {currentAction.Action.Name} used by {currentAction.Actor.Name}");
		EndTiersFromFirstTarget();

		RaiseGuarded(ActionEnded, actionArgs, "ActionEnded");

		if (BattleLogManager.Instance.IsProcessingMessage)
			SetPhase(BattlePhase.WaitForBattleLog);
		else
			SetPhase(BattlePhase.PostCommand);
	}

	private void EndCommandWithoutTarget(BattleCommand command)
	{
		BattleLogManager.Instance.QueueMessage(command.Actor.Name.ToUpper() + "'s skill did nothing.");
		SetPhase(BattlePhase.WaitForBattleLog);
	}

	private Actor ResolveTauntTarget(Actor user, Actor target)
	{
		if (target == null || (user is Enemy) == (target is Enemy)
		    || user.HasStatModifier("Charm") || user.HasStatModifier("ObserveIgnoreTaunt"))
			return target;
		List<Actor> taunting = target is Enemy
			? GetAllAliveEnemies().Where(x => x.HasStatModifier("Taunt")).Cast<Actor>().ToList()
			: GetAlivePartyMembers().Where(x => x.HasStatModifier("Taunt")).Cast<Actor>().ToList();
		if (taunting.Count == 0 || taunting.Contains(target))
			return target;
		return taunting[GameManager.Instance.Random.RandiRange(0, taunting.Count - 1)];
	}

	private bool HandleFollowup(InputDirection direction)
	{
		if (!FollowupActive || ForceHideFollowup || !EnergyBar.Visible || FollowupSelected)
			return false;

		PartyMemberComponent current = ((PartyMember)CurrentCommand.Actor).Component;
		FollowupInput? role = FollowupSets.InputFor(direction, current.Position);
		if (role == null || current.FollowupSet == null ||
		    !current.FollowupSet.Entries.TryGetValue(role.Value, out FollowupEntry pair))
			return false;

		if (!IsFollowupEntryUsable(current, pair, includeEnergy: true))
			return false;

		string name = FollowupSets.SkillNameFor(pair, current.FollowupSet, FollowupTier);

		if (!Database.TryGetSkill(name, out Skill skill))
			return false;

		// in base game, followups go after any other forced actions
		BattleCommand command = skill.Target switch
		{
			SkillTarget.AllEnemies => new BattleCommand(current.Actor, GetAllAliveEnemies(), skill),
			SkillTarget.XRandomEnemies when GetRandomAliveEnemy() is Enemy random
				=> new BattleCommand(current.Actor, random, skill),
			SkillTarget.Enemy when SmoothEnemyTarget(current.Actor.LastTargetIndex) is Enemy enemy
				=> new BattleCommand(current.Actor, enemy, skill),
			SkillTarget.Ally when SmoothPartyTarget(current.Actor.LastTargetIndex) is PartyMember member
				=> new BattleCommand(current.Actor, member, skill),
			SkillTarget.Self => new BattleCommand(current.Actor, current.Actor, skill),
			_ => new BattleCommand(current.Actor, CurrentCommand.Targets, skill)
		};
		PendingFollowup = new PendingFollowupData(command, current, pair);

		// prevent followup bubbles from showing again if the followup itself is an attack
		if (skill.ShowFollowups)
			ForceHideFollowup = true;

		return true;
	}

	/// <summary>
	/// Forces a skill command to be executed after the current one.
	/// </summary>
	/// <remarks>
	/// A forced command skips the skill's requirements (including AFRAID) and the JUICE check, but still spends the JUICE cost, down to 0.
	/// It is dropped if the actor is toast by the time it runs, or if no valid target is left, which still spends the JUICE cost.
	/// </remarks>
	/// <param name="self">The actor that the command is being forced upon.</param>
	/// <param name="target">The target of the command.</param>
	/// <param name="skill">The skill that is being forced.</param>
	public void ForceCommand(Actor self, Actor target, Skill skill)
	{
		ForceCommand(self, [target], skill);
	}

	/// <summary>
	/// Forces a skill command to be executed after the current one.
	/// Also clears the currently acting battler's action result, if any.
	/// </summary>
	/// <remarks>
	/// A forced command skips the skill's requirements (including AFRAID) and the JUICE check, but still spends the JUICE cost, down to 0.
	/// It is dropped if the actor is toast by the time it runs, or if no valid target is left, which still spends the JUICE cost.
	/// </remarks>
	/// <param name="self">The actor that the command is being forced upon.</param>
	/// <param name="targets">The targets of the command.</param>
	/// <param name="skill">The skill that is being forced.</param>
	public void ForceCommand(Actor self, IReadOnlyList<Actor> targets, Skill skill)
	{
		CurrentCommand?.Actor.ClearResult();
		EnqueueForcedCommand(self, targets, skill);
	}

	/// <summary>
	/// Forces a skill command like <see cref="ForceCommand(Actor, Actor, Skill)"/>, but first clears every battler's action result,
	/// like YEP's <c>BattleManager.queueForceAction</c>, unless <paramref name="clearResults"/> is false.
	/// </summary>
	/// <remarks>
	/// A forced command skips the skill's requirements (including AFRAID) and the JUICE check, but still spends the JUICE cost, down to 0.
	/// It is dropped if the actor is toast by the time it runs, or if no valid target is left, which still spends the JUICE cost.
	/// </remarks>
	/// <param name="self">The actor that the command is being forced upon.</param>
	/// <param name="target">The target of the command.</param>
	/// <param name="skill">The skill that is being forced.</param>
	/// <param name="clearResults">Whether every battler's action result is cleared first.</param>
	public void QueueForceCommand(Actor self, Actor target, Skill skill, bool clearResults = true)
	{
		QueueForceCommand(self, [target], skill, clearResults);
	}

	/// <summary>
	/// Forces a skill command like <see cref="ForceCommand(Actor, IReadOnlyList{Actor}, Skill)"/>, but first clears every battler's action result,
	/// like YEP's <c>BattleManager.queueForceAction</c>, unless <paramref name="clearResults"/> is false.
	/// </summary>
	/// <remarks>
	/// A forced command skips the skill's requirements (including AFRAID) and the JUICE check, but still spends the JUICE cost, down to 0.
	/// It is dropped if the actor is toast by the time it runs, or if no valid target is left, which still spends the JUICE cost.
	/// </remarks>
	/// <param name="self">The actor that the command is being forced upon.</param>
	/// <param name="targets">The targets of the command.</param>
	/// <param name="skill">The skill that is being forced.</param>
	/// <param name="clearResults">Whether every battler's action result is cleared first.</param>
	public void QueueForceCommand(Actor self, IReadOnlyList<Actor> targets, Skill skill, bool clearResults = true)
	{
		if (clearResults)
		{
			foreach (PartyMemberComponent member in CurrentParty)
				member.Actor.ClearResult();
			foreach (EnemyComponent enemy in Enemies)
				enemy.Actor.ClearResult();
		}
		EnqueueForcedCommand(self, targets, skill);
	}

	private void EnqueueForcedCommand(Actor self, IReadOnlyList<Actor> targets, Skill skill)
	{
		if (self is PartyMember && skill.ShowFollowups)
			// if the forced skill is an attack, hide the followup bubbles
			ForceHideFollowup = true;
		// forced commands execute in the order they were queued
		ForcedCommands.Add(new BattleCommand(self, targets, skill) { Forced = true });
		// if this actor hasn't had their normal turn, queue them for a priority turn after forced commands
		// mimics base game behavior
		if (!ActedThisTurn.Contains(self) && !PriorityActors.Contains(self))
			PriorityActors.Add(self);
	}

	internal bool HasForcedCommandsSinceCommandStart => ForcedCommands.Count > ForcedCountAtCommandStart;

	internal void DropForcedCommandsSinceCommandStart()
	{
		while (ForcedCommands.Count > ForcedCountAtCommandStart)
		{
			BattleCommand dropped = ForcedCommands[^1];
			ForcedCommands.RemoveAt(ForcedCommands.Count - 1);
			if (dropped.Actor is PartyMember && dropped.Action is Skill droppedSkill && droppedSkill.ShowFollowups)
				ForceHideFollowup = false;
		}
	}

	private void ProcessFollowupSuccess(InputDirection direction)
	{
		FollowupSelected = true;
		AudioManager.Instance.PlaySFX("Skill2", 1f, 0.8f);
		((PartyMember)CurrentCommand.Actor).Component.FadeOutFollowupsExcept(direction);
	}
	
	private bool IsFollowupEntryUsable(PartyMemberComponent user, FollowupEntry entry, bool includeEnergy)
	{
		int position = entry.TargetPosition == FollowupEntry.User ? user.Position : entry.TargetPosition;
		PartyMemberComponent target = CurrentParty.FirstOrDefault(x => x.Position == position);
		if (target == null || target.Actor.IsToast)
			return false;

		bool anyParty = entry.IsReleaseEnergy && SettingsMenuManager.Instance.ReleaseEnergyAnyParty;
		if (!anyParty && CurrentParty.Count < entry.MinPartySize)
			return false;
		if (entry.RequiresAllAlive && CurrentParty.Any(x => x.Actor.IsToast))
			return false;
		if (includeEnergy && Energy < entry.EnergyCost)
			return false;

		return entry.Requirement == null || entry.Requirement(user.Actor);
	}

	private bool IsFollowupValid(PendingFollowupData followup)
	{
		if (IsInvalidTarget(followup.Command.Actor) || followup.Command.Actor.Stunned)
			return false;

		return IsFollowupEntryUsable(followup.User, followup.Entry, includeEnergy: true);
	}

	private async void EndOfTurn()
	{
		if (!ProcessedEndOfTurn)
		{
			// tick down stat turn timers
			// vanilla omori bug: stats decrease before of end of turn enemy skills
			// vanilla clears each result before turn-end states expire, so those expiries stay recorded
			CurrentParty.ForEach(x =>
			{
				x.Actor.ClearResult();
				x.Actor.DecreaseStatTurnCounter();
				x.UpdateStateIcons();
			});
			Enemies.ForEach(x =>
			{
				if (!x.Actor.IsToast)
				{
					x.Actor.ClearResult();
					x.Actor.DecreaseStatTurnCounter();
				}
			});

			// in vanilla, a reinforcement added by a TURN END page is not in the action order until next turn,
			// so mid-turn reinforcements are picked up by getNextSubject and act at once (unless stunned)
			InEndOfTurnHooks = true;
			foreach (EnemyComponent enemy in Enemies.ToList())
			{
				if (enemy.Actor.IsToast || !Enemies.Contains(enemy))
					continue;
				await RunGuarded(() => enemy.Actor.ProcessEndOfTurn(), $"{enemy.Actor.Name}.ProcessEndOfTurn");
			}
			await ResolveEnemyDeaths();
			InEndOfTurnHooks = false;

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

		if (DyingEnemies.Count > 0)
		{
			SetPhase(BattlePhase.EnemyDying);
			return;
		}

		// if any forced commands or priority actors were added during ProcessEndOfTurn, run those still
		if (ForcedCommands.Count > 0 ||
		    (!BattleStartForcesOnly && PriorityActors.Any(a => !ActedThisTurn.Contains(a) && !IsInvalidTarget(a))))
		{
			SetPhase(BattlePhase.PreCommand);
			return;
		}

		CheckBattleOver();
		if (Phase == BattlePhase.BattleOver)
			return;

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
		FallTween = tween;
	}

	private void EnemiesDoneDying()
	{
		DyingEnemies.ForEach(x => x.Visible = false);
		DyingEnemies.Clear();
		FallTween = null;
		SetPhase(ProcessedStartOfCommands ? BattlePhase.PreCommand : BattlePhase.FightRun);
	}

	// restores an enemy's on-screen state when it is revived after falling (or while falling)
	internal void OnEnemyRevived(EnemyComponent enemy)
	{
		Node2D parent = enemy.GetParent<Node2D>();
		bool wasFalling = DyingEnemies.Remove(parent);
		if (wasFalling && Phase == BattlePhase.EnemyDying && FallTween != null && FallTween.IsValid())
		{
			// if an enemy was revived mid-fall, the best we can do is just skip ahead
			// this is a rare case but still worth handling
			FallTween.Kill();
			EnemiesDoneDying();
		}
		parent.Visible = true;
		parent.GlobalPosition = enemy.Actor.CenterPoint;
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
		if (omori != null || (ToastlessMode && CurrentParty.Any(x => x.Actor.IsToast)))
		{
			SetPhase(BattlePhase.BattleOver);
			await EndOfBattle(false);
			BattleLogManager.Instance.ClearAndShowMessage(CurrentParty[0].Actor.Name.ToUpper() +
			                                              "'s party was defeated...");
			ShowEndOfBattleOptions();
			return;
		}
		
		if (Enemies.All(x => x.Actor.IsToast))
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
				AudioManager.Instance.ForcePlayBGM("xx_victory", 1f, 1f);
				BattleLogManager.Instance.ClearAndShowMessage(CurrentParty[0].Actor.Name.ToUpper() + "'s party was victorious!");
				CurrentStage++;
				if (CurrentStage < Stages.Count)
				{
					await Wait.Milliseconds(3000);
					await AnimationManager.Instance.WaitForTintScreen(Colors.Black, 0.5f);
					SummonEnemiesForStage(Stages[CurrentStage].Enemies);
					GameManager.Instance.ForceSetBattleback(Stages[CurrentStage].Battleback);
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
							x.Actor.ClearStatModifiersOnRecoverAll();
						x.UpdateStateIcons();
					});
					AudioManager.Instance.ForcePlayBGM(Stages[CurrentStage].BGM, 0.05f,
						(float)Stages[CurrentStage].BGMPitch);
					if (previousBGM == Stages[CurrentStage].BGM)
						AudioManager.Instance.SeekBGM(currentPosition);
					AudioManager.Instance.ForceSetBGMLoopOffset(Stages[CurrentStage].BGMLoopPoint);
					AudioManager.Instance.FadeBGMTo(1f, 0.5f);
					await AnimationManager.Instance.WaitForTintScreen(ColorsExtension.TransparentBlack, 0.5f);
					ForcedCommands.Clear();
					ForcedCountAtCommandStart = 0;
					PendingFollowup = null;
					ForceHideFollowup = false;
					PriorityActors.Clear();
					CurrentCommand = null;
					TurnCount = 0;
					ProcessedStartOfTurn = false;
					ResumingStartOfTurn = false;
					// leave BattleOver so a restart during the next stage's intro dialogue
					// queues (like the first stage's) instead of resetting mid-hook
					SetPhase(BattlePhase.PreBattle);
					CallDeferred(MethodName.PreBattle);
					return;
				}
			}
			else
			{
				AudioManager.Instance.ForcePlayBGM("xx_victory", 1f, 1f);
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
		EndOfBattleOptionsContainer.GetChild(1).GetChild<Button>(0).GrabFocus(hideFocus: !MainMenuManager.Instance.UsingController);
	}

	private async Task EndOfBattle(bool victory)
	{
		// use index loops since the collections may be modified by summons in hooks
		for (int i = 0; i < CurrentParty.Count; i++)
		{
			PartyMember member = CurrentParty[i].Actor;
			await RunGuarded(() => member.OnEndOfBattle(victory), $"{member.Name}.OnEndOfBattle");
		}
		foreach (EnemyComponent component in Enemies.ToList())
		{
			Enemy enemy = component.Actor;
			if (!Enemies.Contains(component) || enemy.IsToast)
				continue;
			await RunGuarded(() => enemy.OnEndOfBattle(victory), $"{enemy.Name}.OnEndOfBattle");
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
			RunGuarded(() => RunAsReaction(() => subscriber(this, EventArgs.Empty)), $"{context} event handler");
	}

	private void RaiseGuarded<T>(EventHandler<T> handler, T args, string context)
	{
		if (handler == null)
			return;
		foreach (EventHandler<T> subscriber in handler.GetInvocationList().Cast<EventHandler<T>>())
			RunGuarded(() => RunAsReaction(() => subscriber(this, args)), $"{context} event handler");
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
		target = ResolveTransformed(target);
		if (!neverMiss && RollMissOrEvade(self, target, silent))
			return -1;

		float damage = Math.Max(0, damageFunc());
		// locked bosses resolve advantage as their locked emotion
		Emotion selfEmotion = self.EffectiveEmotion;
		Emotion targetEmotion = target.EffectiveEmotion;
		DamageContext context = new(self, target, neverMiss, ignoreEmotion, attackElement, false);

		ApplyOverrides(DamagePhase.PreEmotion, ref damage, context);

		int effectiveness = 0;
		if (!ignoreEmotion)
			damage = CalculateEmotionModifiers(selfEmotion, targetEmotion, damage, out effectiveness, attackElement);
		context.Effectiveness = effectiveness;
		bool critical =
			(self.CurrentStats.LCK * .01f >= GameManager.Instance.Random.Randf() || guaranteeCrit ||
			 target.HasStatModifier("Tickle")) && !neverCrit;

		// even though we know if the attack is a crit or not at this stage, pass false
		// this may change in the future depending on feedback and use case
		ApplyOverrides(DamagePhase.PreCrit, ref damage, context);

		if (critical)
		{
			damage *= 1.5f;
			if (!silent) BattleLogManager.Instance.QueueMessage("IT HIT RIGHT IN THE HEART!");
			AudioManager.Instance.PlaySFX("BA_CRITICAL_HIT", volume: 2.5f);
		}

		context.IsCritical = critical;
		ApplyOverrides(DamagePhase.PreFlatCrit, ref damage, context);

		if (critical)
		{
			damage += 1.5f;
		}

		ApplyOverrides(DamagePhase.PreVariance, ref damage, context);

		damage = CalculateVariance(damage, variance);

		ApplyOverrides(DamagePhase.PreRounding, ref damage, context);

		float rounded = (float)Math.Round(damage, MidpointRounding.AwayFromZero);

		// scale before the sad bleed so the juice loss follows the multiplied damage
		if (ShouldDoDebugDamage())
			rounded *= 10;
		if (rounded < 0)
			rounded = 0;
		if (!SettingsMenuManager.Instance.DisableDamageLimit && rounded > 9999)
			rounded = 9999;

		ApplyOverrides(DamagePhase.PreJuice, ref rounded, context);

		// sadness converts part of the damage to juice loss. emotion-locked states keep tier-1
		// element rates but bleed 0.5/1.0, so the bleed follows the displayed emotion, not the lock's advantage emotion
		int juiceLost = 0;
		float bleed = target.CurrentEmotion.JuiceBleedFraction;
		if (bleed > 0)
		{
			juiceLost = Math.Min((int)Math.Floor(rounded * bleed), target.CurrentJuice);
			rounded -= juiceLost;
		}

		target.CurrentJuice -= juiceLost;

		if (rounded < 0)
			rounded = 0;
		if (!SettingsMenuManager.Instance.DisableDamageLimit && rounded > 9999)
			rounded = 9999;

		rounded = (float)Math.Round(rounded, MidpointRounding.AwayFromZero);

		ApplyOverrides(DamagePhase.PreApply, ref rounded, context);

		int roundedInt = (int)rounded;
		
		// if damage ends up being below zero due to a stat modifier, treat it as a pseudo-miss
		// exactly 0 damage still proceeds through as normal
		if (roundedInt < 0)
		{
			// undo the sad juice bleed, the hit never landed
			target.CurrentJuice += juiceLost;
			AudioManager.Instance.PlaySFX("BA_miss");
			SpawnDamageNumber(-1, target, DamageType.Miss);
			if (!silent) BattleLogManager.Instance.QueueMessage(self, "[actor]'s attack did nothing.");
			return -1;
		}
		// scaled above already, before the sad bleed split it
		target.Damage(roundedInt, applyDebugScale: false);
		
		if (target is PartyMember && roundedInt > 0)
		{
			Energy = Math.Min(10, Energy + 1);
			AnimationManager.Instance.InitShake(new Shake(5, 5, 10));
		}

		SpawnDamageNumber(roundedInt, target, critical: critical);
		// we don't need to play a hitsound if the attack is a critical or if there's no damage
		if (roundedInt > 0)
		{
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print("Effectiveness: " + effectiveness);
			if (effectiveness > 0)
			{
				if (!silent) BattleLogManager.Instance.QueueMessage("...It was a moving attack!");
				if (!critical) AudioManager.Instance.PlaySFX("se_impact_double", 1f, 1.5f);
			}
			else if (effectiveness < 0)
			{
				if (!silent) BattleLogManager.Instance.QueueMessage("...It was a dull attack.");
				if (!critical) AudioManager.Instance.PlaySFX("se_impact_soft", 1f, 1.5f);
			}
			else
				AudioManager.Instance.PlaySFX("SE_dig", 0.7f, 0.9f);
		}

		if (!silent) BattleLogManager.Instance.QueueMessage(self, target, "[target] takes " + roundedInt + " damage!");

		if (juiceLost > 0)
		{
			if (!silent) BattleLogManager.Instance.QueueMessage(self, target, "[target] lost " + juiceLost + " JUICE...");
			SpawnDamageNumber(juiceLost, target, DamageType.JuiceLoss);
		}

		ApplyOverrides(DamagePhase.PostApply, ref rounded, context);

		RaiseGuarded(DamageDealt, new DamageDealtEventArgs(self, target, roundedInt, juiceLost, critical, false),
			"DamageDealt");

		return roundedInt;
	}

	private bool RollMissOrEvade(Actor self, Actor target, bool silent)
	{
		int hitRate = self.CurrentStats.HIT;
		int evasion = target.CurrentStats.EVA;

		int roll = GameManager.Instance.Random.RandiRange(1, 100);
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
			evaded = !miss && GameManager.Instance.Random.RandiRange(1, 100) <= evasion;
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
		SpawnDamageNumber(-1, target, DamageType.Miss);
		return true;
	}

	private void ApplyOverrides(DamagePhase phase, ref float damage, DamageContext context)
	{
		TierReactionDepth++;
		try
		{
			context.IsAttacking = true;
			foreach (StatModifier mod in context.Attacker.StatModifiers.Values)
				mod.OverrideDamage(phase, ref damage, context);
			context.IsAttacking = false;
			foreach (StatModifier mod in context.Defender.StatModifiers.Values)
				mod.OverrideDamage(phase, ref damage, context);
		}
		finally
		{
			TierReactionDepth--;
		}
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
		target = ResolveTransformed(target);
		if (!neverMiss && RollMissOrEvade(self, target, silent))
			return -1;

		float damage = damageFunc();
		DamageContext context = new(self, target, neverMiss, ignoreEmotion, attackElement, true);
		// locked bosses resolve advantage as their locked emotion
		ApplyOverrides(DamagePhase.PreEmotion, ref damage, context);

		int effectiveness = 0;
		if (!ignoreEmotion)
			damage = CalculateEmotionModifiers(self.EffectiveEmotion, target.EffectiveEmotion, damage, out effectiveness, attackElement);
		context.Effectiveness = effectiveness;
		bool critical =
			(self.CurrentStats.LCK * .01f >= GameManager.Instance.Random.Randf() || guaranteeCrit ||
			 target.HasStatModifier("Tickle")) && !neverCrit;

		ApplyOverrides(DamagePhase.PreCrit, ref damage, context);

		if (critical)
		{
			damage *= 1.5f;
			if (!silent) BattleLogManager.Instance.QueueMessage("IT HIT RIGHT IN THE HEART!");
			AudioManager.Instance.PlaySFX("BA_CRITICAL_HIT", volume: 2f);
		}

		context.IsCritical = critical;
		ApplyOverrides(DamagePhase.PreFlatCrit, ref damage, context);

		if (critical)
		{
			damage += 1.5f;
		}

		ApplyOverrides(DamagePhase.PreVariance, ref damage, context);

		damage = CalculateVariance(damage, variance);

		ApplyOverrides(DamagePhase.PreRounding, ref damage, context);

		float rounded = (float)Math.Round(damage, MidpointRounding.AwayFromZero);

		if (ShouldDoDebugDamage())
			rounded *= 10;
		if (rounded < 0)
			rounded = 0;
		if (!SettingsMenuManager.Instance.DisableDamageLimit && rounded > 9999)
			rounded = 9999;

		ApplyOverrides(DamagePhase.PreJuice, ref rounded, context);
		ApplyOverrides(DamagePhase.PreApply, ref rounded, context);

		int roundedInt = (int)rounded;
		// a stat modifier fully negated the hit and drove the damage negative — treat it like a miss
		if (roundedInt < 0)
		{
			AudioManager.Instance.PlaySFX("BA_miss");
			SpawnDamageNumber(-1, target, DamageType.Miss);
			return -1;
		}
		target.DamageJuice(roundedInt, applyDebugScale: false);
		SpawnDamageNumber(roundedInt, target, DamageType.JuiceLoss);
		if (!silent) BattleLogManager.Instance.QueueMessage(self, target, "[target] lost " + roundedInt + " JUICE...");
		ApplyOverrides(DamagePhase.PostApply, ref rounded, context);
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
	public int Heal(Actor self, Actor target, Func<float> healFunc, float variance = 0.2f, bool silent = false)
	{
		target = ResolveTransformed(target);
		float baseHealing = healFunc();
		// vanilla's bugged healing reads the raw emotions, ignoring locks
		baseHealing = CalculateEmotionModifiers(self.CurrentEmotion, target.CurrentEmotion, baseHealing, out _);
		baseHealing = CalculateVariance(baseHealing, variance);
		int rounded = (int)Math.Round(baseHealing, MidpointRounding.AwayFromZero);
		rounded = target.Heal(rounded);
		SpawnDamageNumber(rounded, target, DamageType.Heal);
		if (!silent) BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {rounded} HEART!");
		RaiseGuarded(Healed, new HealedEventArgs(self, target, rounded, false), "Healed");
		return rounded;
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
		target = ResolveTransformed(target);
		float baseJuice = healFunc();
		// vanilla's bugged healing reads the raw emotions, ignoring locks
		float finalJuice = CalculateEmotionModifiers(self.CurrentEmotion, target.CurrentEmotion, baseJuice, out _);
		int rounded = (int)Math.Round(finalJuice, MidpointRounding.AwayFromZero);
		rounded = target.HealJuice(rounded);
		SpawnDamageNumber(rounded, target, DamageType.JuiceGain);
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
	/// <param name="who">The <see cref="Actor"/> to change.</param>
	/// <param name="silent">If true, no battle log messages will be shown.</param>
	public void RandomEmotion(Actor who, bool silent = false)
	{
		who = ResolveTransformed(who);
		IReadOnlyList<EmotionGroup> pool = Database.GetRandomEmotionGroups();
		if (pool.Count == 0)
			return;

		EmotionGroup group = pool[GameManager.Instance.Random.RandiRange(0, pool.Count - 1)];
		if (!TryGetNextEmotionInGroup(who, group, out Emotion next))
			return; // already at the group's highest tier

		// unlike MakeEmotion, capped and invalid targets fail silently
		if (who.IsEmotionValid(next))
			who.SetEmotion(next.Id, silent);
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
	/// Spawns a damage number on a specific <paramref name="target"/>.
	/// </summary>
	/// <remarks>
	/// Damage numbers targeting a party member spawn under their face, with each one 40px below the previous one.<br/>
	/// Damage numbers targeting an enemy spawn at its <see cref="Enemy.DamageNumberOffset"/> relative to its feet and push older numbers up.
	/// </remarks>
	/// <param name="damage">The number to display.</param>
	/// <param name="target">The actor the number belongs to.</param>
	/// <param name="type">The <see cref="DamageType"/> of the damage. This value will modify the color of the damage number.</param>
	/// <param name="critical">If true, the damage number will blink red when spawned.</param>
	public void SpawnDamageNumber(int damage, Actor target, DamageType type = DamageType.Damage,
		bool critical = false)
	{
		if (DamageNumbersDisabled)
			return;

		target = ResolveTransformed(target);
		ActorComponent component = target.Component;
		if (component != null)
		{
			component.ShowDamageNumber(damage, type, critical);
			return;
		}

		GD.PushWarning("SpawnDamageNumber target has no component, falling back to actor CenterPoint!");
		SpawnDamageNumber(damage, target.CenterPoint, type, critical);
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
	/// <param name="adjustedStats">Stat adjustments added on top of the enemy's declared stats.</param>
	/// <returns>The spawned <see cref="Enemy"/>, or null if the spawn fails, such as for an unknown name.</returns>
	public Enemy SummonEnemy(string who, Vector2 position, string startingEmotion = "neutral",
		bool fallsOffScreen = true, bool grayscaleOnDefeat = false, int layer = 0, Stats adjustedStats = default)
	{
		BattlePresetEnemy en = new()
		{
			Name = who,
			Emotion = startingEmotion,
			FallsOffScreen = fallsOffScreen,
			GrayscaleOnDefeat = grayscaleOnDefeat,
			Layer = layer,
			AdjustedStats = adjustedStats
		};
		return SummonEnemy(en, position)?.Actor;
	}
	
	internal EnemyComponent SummonEnemy(BattlePresetEnemy en, Vector2 position)
	{
		while (Enemies.Any(x => x.Actor.CenterPoint == position))
		{
			// if this enemy is going to spawn on top of another, nudge them a little
			// this way the targeting system still works
			position.X += 0.1f;
		}

		EnemyComponent enemy = GameManager.Instance.SpawnEnemy(en, position);
		if (enemy == null)
		{
			GD.PushWarning("Failed to spawn enemy: " + en.Name);
			return null;
		}
		Enemies.Add(enemy);

		// mid-battle spawns automatically get an immediate turn via PriorityActors, while end-of-turn spawns wait for next turn
		if (!InEndOfTurnHooks && Phase is BattlePhase.PreCommand or BattlePhase.CommandExecute
		    or BattlePhase.WaitForBattleLog or BattlePhase.PostCommand or BattlePhase.EnemyDying)
		{
			if (!PriorityActors.Contains(enemy.Actor))
				PriorityActors.Add(enemy.Actor);
		}

		return enemy;
	}
	

	/// <summary>
	/// Spawns a party member into the battle at the given battlecard position, falling back to the
	/// next open position if it is already occupied. If the party is already full, this will do nothing.
	/// </summary>
	/// <remarks>
	/// A member summoned during the command phase at a position after the currently selecting member
	/// still chooses a command on the current turn, otherwise they act from the next turn onward.<br/>
	/// </remarks>
	/// <param name="who">Which party member to spawn.</param>
	/// <param name="position">The battlecard position to spawn the member at, from 0 to 3. If the position
	/// is occupied, the next open position (wrapping around) is used instead.</param>
	/// <param name="level">The party member's level.</param>
	/// <param name="weapon">The party member's weapon.</param>
	/// <param name="charm">The party member's charm.</param>
	/// <param name="emotion">The party member's starting emotion.</param>
	/// <param name="skills">The skills to equip, or null for the preset default (attack only).</param>
	/// <param name="followupSet">The followup set id to use. Null uses the position's default set, "None" disables followups.</param>
	/// <param name="adjustedStats">Stat adjustments added on top of the member's base stats.</param>
	/// <returns>The summoned <see cref="PartyMember"/>, or null if the summon fails.</returns>
	public PartyMember SummonPartyMember(string who, int position, int level = 1,
		string weapon = "Knife", string charm = "None", string emotion = "neutral",
		string[] skills = null, string followupSet = null, Stats adjustedStats = default)
	{
		if (!IsBattling || Phase is BattlePhase.BattleOver)
		{
			GD.PushWarning("Cannot summon a party member outside of an active battle: " + who);
			return null;
		}
		if (position is < 0 or > 3)
		{
			GD.PushWarning($"Invalid position {position} for party member {who}");
			return null;
		}
		if (CurrentParty.Count >= 4)
		{
			GD.PushWarning("Party is full, cannot summon " + who);
			return null;
		}
		if (CurrentParty.Any(x => x.Position == position))
		{
			// the party isn't full at this point, so an open slot is guaranteed to exist
			int requested = position;
			do
			{
				position = (position + 1) % 4;
			} while (CurrentParty.Any(x => x.Position == position));
			GD.Print($"Position {requested} is already occupied, summoning {who} at position {position} instead");
		}

		string setId = followupSet ?? FollowupSets.DefaultIdForPosition(position);
		FollowupSet set = FollowupSets.Get(setId);
		BattlePresetActor entry = new()
		{
			Name = who,
			Level = level,
			Weapon = weapon,
			Charm = charm,
			Emotion = emotion,
			FollowupSet = setId,
			Skills = skills ?? ["OAttack", "", "", "", ""],
			Position = position,
			AdjustedStats = adjustedStats
		};
		PartyMemberComponent component = GameManager.Instance.SpawnPartyMember(set, entry);
		if (component == null)
		{
			GD.PushWarning("Failed to spawn party member: " + who);
			return null;
		}

		// insert keeping CurrentParty ordered by Position, without moving a live command cursor
		// off the member that is currently selecting
		PartyMemberComponent selecting = Phase is BattlePhase.PlayerCommand or BattlePhase.SkillSelection or BattlePhase.TargetSelection
			&& CurrentPartyMember > -1 && CurrentPartyMember < CurrentParty.Count
				? CurrentParty[CurrentPartyMember]
				: null;
		int insertAt = CurrentParty.FindIndex(x => x.Position > position);
		if (insertAt < 0)
			CurrentParty.Add(component);
		else
			CurrentParty.Insert(insertAt, component);
		if (selecting != null)
			CurrentPartyMember = CurrentParty.IndexOf(selecting);

		// a summoned member with followups may need to reveal the energy bar
		EnergyBar.Visible = CurrentParty.Any(x => x.HasFollowup);
		return component.Actor;
	}

	/// <summary>
	/// Transforms an enemy into another enemy, despawning the original and spawning the replacement
	/// at the same position and index (see <see cref="GetEnemy"/>). The original enemy is immediately removed from the battle.
	/// </summary>
	/// <remarks>
	/// Special care must be used when using this method, as the old enemy will immediately become invalid.<br/>
	/// Damage, healing, emotion changes and damage numbers that go through <see cref="BattleManager"/> to the old enemy
	/// reach the new one instead, so the rest of a running action still lands. Commands later in the turn that are aimed
	/// at the old enemy also go to the new one. Methods called directly on the old enemy are safe, but they don't reach
	/// the new one. An emotion change on it can still show its battle log message.
	/// </remarks>
	/// <param name="original">The enemy to transform from.</param>
	/// <param name="who">The database name of the enemy to transform into.</param>
	/// <param name="startingEmotion">Starting emotion for the new enemy.</param>
	/// <param name="offset">The position offset relative to the old enemy's position.</param>
	/// <param name="adjustedStats">Stat adjustments for the new enemy. Defaults to the original's <see cref="Enemy.AdjustedStats"/>.</param>
	/// <returns>The new <see cref="Enemy"/>, or null if the original enemy is not in the battle or the new one fails to spawn (the original is removed either way once found).</returns>
	public Enemy TransformEnemy(Enemy original, string who, string startingEmotion = "neutral", Vector2 offset = default, Stats? adjustedStats = null)
	{
		EnemyComponent target = original?.Component;
		if (target == null) return null;
		
		int index = Enemies.IndexOf(target);
		Enemies.Remove(target);
		target.GetParent().QueueFree();
		
		Enemy transformed = SummonEnemy(who, original.CenterPoint + offset, startingEmotion, original.FallsOffScreen, original.GrayscaleOnDefeat, original.Layer, adjustedStats ?? original.AdjustedStats);
		if (transformed != null && index >= 0)
		{
			Enemies.Remove(transformed.Component);
			Enemies.Insert(index, transformed.Component);
		}
		original.TransformedInto = transformed;
		return transformed;
	}

	internal Actor ResolveTransformed(Actor actor)
	{
		while (actor is Enemy { TransformedInto: { } next })
			actor = next;
		return actor;
	}

	internal void BeginTiersFromFirstTarget(BattleAction action, IReadOnlyList<Actor> targets)
	{
		EndTiersFromFirstTarget();
		if (action.TiersFromFirstTarget && targets.Count > 1 && !SettingsMenuManager.Instance.PerTargetBuffTiers)
			TierTargets = targets.ToList();
	}

	internal void EndTiersFromFirstTarget()
	{
		TierTargets = [];
		TierLandings.Clear();
		TierSteps.Clear();
	}

	internal void RunAsReaction(Action reaction)
	{
		TierReactionDepth++;
		try
		{
			reaction();
		}
		finally
		{
			TierReactionDepth--;
		}
	}

	internal bool TryGetModifierLanding(Actor actor, string modifier, out int landing)
	{
		landing = 0;
		if (!IsTierTarget(actor) || Database.CreateModifier(modifier) is not TierStatModifier template)
			return false;
		landing = NextLanding("modifier:" + modifier, actor, first => first.GetStatModifierTier(modifier), template.MaxTier);
		return true;
	}

	internal bool TryGetEmotionLanding(Actor actor, string groupId, out int landing)
	{
		landing = 0;
		if (!IsTierTarget(actor))
			return false;
		int max = 0;
		while (Database.TryGetEmotionByGroupTier(groupId, max + 1, out _))
			max++;
		if (max == 0)
			return false;
		landing = NextLanding("emotion:" + groupId, actor,
			first => first.CurrentEmotion.Group?.Id == groupId ? first.CurrentEmotion.Tier : 0, max);
		return true;
	}

	private bool IsTierTarget(Actor actor)
	{
		if (TierTargets.Count == 0 || TierReactionDepth > 0)
			return false;
		Actor resolved = ResolveTransformed(actor);
		return TierTargets.Any(target => ResolveTransformed(target) == resolved);
	}

	private int NextLanding(string key, Actor actor, Func<Actor, int> heldTier, int maxTier)
	{
		Actor resolved = ResolveTransformed(actor);
		TierSteps.TryGetValue((key, resolved), out int step);
		TierSteps[(key, resolved)] = step + 1;
		if (!TierLandings.TryGetValue(key, out List<int> landings))
		{
			landings = [];
			TierLandings[key] = landings;
		}
		if (step == landings.Count)
		{
			int held = heldTier(ResolveTransformed(TierTargets[0]));
			landings.Add(held <= 0 || held >= maxTier ? 1 : held + 1);
		}
		return landings[step];
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
	
	internal int DebugScale(int amount)
	{
		return ShouldDoDebugDamage() ? amount * 10 : amount;
	}
	
	/// <summary>
	/// Enables or disables one of the battle menu's options. Selecting a disabled option plays a
	/// buzzer and does nothing. Persists across turns (and Boss Rush stages) until re-enabled,
	/// and is cleared when the battle is exited or restarted.
	/// </summary>
	/// <remarks>
	/// RUN cannot be disabled, so the party can always leave the battle.
	/// Disabling an option does not cancel commands that were already queued through it.
	/// </remarks>
	/// <param name="option">The battle menu option to change.</param>
	/// <param name="enabled">Whether the option can be selected.</param>
	public void SetMenuOptionEnabled(BattleMenuOption option, bool enabled)
	{
		if (enabled)
			DisabledMenuOptions.Remove(option);
		else
			DisabledMenuOptions.Add(option);
	}

	/// <summary>
	/// Whether the given battle menu option is currently enabled. See <see cref="SetMenuOptionEnabled"/>.
	/// </summary>
	/// <param name="option">The battle menu option to check.</param>
	public bool IsMenuOptionEnabled(BattleMenuOption option)
	{
		return !DisabledMenuOptions.Contains(option);
	}

	/// <summary>
	/// Makes the given <see cref="Actor"/> feel an emotion of the given group, if possible.
	/// Increases the tier if the actor already feels an emotion of the group.
	/// </summary>
	/// <remarks>
	/// Inside a skill or item marked with <see cref="Skill.WithTiersFromFirstTarget"/>, the tier for one of its targets can follow the first target instead.
	/// </remarks>
	/// <param name="who">The <see cref="Actor"/> to change.</param>
	/// <param name="groupId">The id of the <see cref="EmotionGroup"/> to apply.</param>
	/// <param name="silent">If true, no battle log messages will be shown, including failure and max tier messages.</param>
	public void MakeEmotion(Actor who, string groupId, bool silent = false)
	{
		who = ResolveTransformed(who);
		if (!Database.TryGetEmotionGroup(groupId, out EmotionGroup group))
		{
			GD.PrintErr("Unknown emotion group: " + groupId);
			return;
		}

		if (TryGetEmotionLanding(who, groupId, out int landing))
		{
			MakeEmotionTier(who, groupId, landing, silent);
			return;
		}

		if (!TryGetNextEmotionInGroup(who, group, out Emotion next))
		{
			if (!silent)
				ShowMaxTierMessage(who, group);
			return;
		}

		if (who.IsEmotionValid(next))
			who.SetEmotion(next.Id, silent);
		else if (!silent)
			ShowMaxTierMessage(who, group);
	}
	
	/// <summary>
	/// Makes the given <see cref="Actor"/> feel an emotion of the given group and tier, if possible.
	/// </summary>
	/// <param name="who">The <see cref="Actor"/> to change.</param>
	/// <param name="groupId">The id of the <see cref="EmotionGroup"/> to apply.</param>
	/// <param name="tier">The tier of the emotion to give, if possible.</param>
	/// <param name="silent">If true, no battle log messages will be shown, including failure and max tier messages.</param>
	public void MakeEmotionTier(Actor who, string groupId, int tier, bool silent)
	{
		who = ResolveTransformed(who);
		if (!Database.TryGetEmotionGroup(groupId, out EmotionGroup group)
		    || !Database.TryGetEmotionByGroupTier(groupId, tier, out Emotion next))
		{
			GD.PrintErr("Unknown emotion group tier: " + groupId + " " + tier);
			return;
		}

		int held = who.CurrentEmotion.Group == group ? who.CurrentEmotion.Tier : 0;
		if (held == tier)
		{
			if (!silent && who is Enemy)
				ShowMaxTierMessage(who, group);
			return;
		}
		if (held < tier && who.IsEmotionValid(next))
			who.SetEmotion(next.Id, silent);
		else if (!silent)
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
	/// <param name="silent">If true, no battle log messages will be shown, including failure and max tier messages.</param>
	public void MakeSad(Actor who, bool silent = false)
	{
		MakeEmotion(who, "sad", silent);
	}

	/// <summary>
	/// Makes the given <see cref="Actor"/> happy, if possible. Increases the tier if the actor is already happy.
	/// </summary>
	/// <param name="who">The <see cref="Actor"/> to make happy.</param>
	/// <param name="silent">If true, no battle log messages will be shown, including failure and max tier messages.</param>
	public void MakeHappy(Actor who, bool silent = false)
	{
		MakeEmotion(who, "happy", silent);
	}

	/// <summary>
	/// Makes the given <see cref="Actor"/> angry, if possible. Increases the tier if the actor is already angry.
	/// </summary>
	/// <param name="who">The <see cref="Actor"/> to make angry.</param>
	/// <param name="silent">If true, no battle log messages will be shown, including failure and max tier messages.</param>
	public void MakeAngry(Actor who, bool silent = false)
	{
		MakeEmotion(who, "angry", silent);
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
	/// <remarks>Immortal enemies at 0 HP are included until they're truly defeated. See <see cref="GetAllAliveEnemies"/>.</remarks>
	public Enemy GetRandomAliveEnemy()
	{
		List<EnemyComponent> alive = Enemies.Where(x => !x.Actor.IsToast).ToList();
		return alive.Count == 0 ? null : alive[GameManager.Instance.Random.RandiRange(0, alive.Count - 1)].Actor;
	}

	/// <returns>A random alive <see cref="Enemy"/> that's not the provided actor, or null if none is found.</returns>
	/// <remarks>Immortal enemies at 0 HP are included until they're truly defeated. See <see cref="GetAllAliveEnemies"/>.</remarks>
	public Enemy GetRandomAliveUniqueEnemy(Actor not)
	{
		List<EnemyComponent> alive = Enemies.Where(x => !x.Actor.IsToast && x.Actor != not).ToList();
		return alive.Count == 0 ? null :  alive[GameManager.Instance.Random.RandiRange(0, alive.Count - 1)].Actor;
	}

	/// <returns>All currently alive <see cref="Enemy"/>s.</returns>
	/// <remarks>
	/// An enemy is alive until it's defeated and becomes toast, so this includes enemies at 0 HP that are waiting on forced
	/// commands queued during the action that knocked them out, or that hold IMMORTAL (see <see cref="Enemy.AddImmortal"/>).
	/// </remarks>
	public List<Enemy> GetAllAliveEnemies()
	{
		return Enemies.Select(x => x.Actor).Where(x => !x.IsToast).ToList();
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
		return actor == null || actor.IsToast || actor.Component == null;
	}

	/// <returns>The <see cref="BattleCommand"/> that is currently being processed.</returns>
	public BattleCommand GetCurrentCommand()
	{
		return CurrentCommand;
	}

	/// <summary>
	/// Gets all party members who are not toast.
	/// </summary>
	public List<PartyMember> GetAlivePartyMembers()
	{
		return CurrentParty.Where(x => x.Actor.CurrentHP > 0).Select(x => x.Actor).ToList();
	}

	/// <summary>
	/// Gets all party members who are currently toast.
	/// </summary>
	public List<PartyMember> GetDeadPartyMembers()
	{
		return CurrentParty.Where(x => x.Actor.CurrentHP <= 0 && x.Actor.IsToast).Select(x => x.Actor).ToList();
	}

	/// <summary>
	/// Gets all party members, including ones who are toast.
	/// </summary>
	/// <remarks>
	/// In most situations, such as skill logic, use <see cref="GetAlivePartyMembers"/> instead.<br/>
	/// The list is a copy, so changing it doesn't change the party.
	/// </remarks>
	public List<PartyMember> GetAllPartyMembers()
	{
		return CurrentParty.Select(x => x.Actor).ToList();
	}

	/// <summary>
	/// Whether any living party member has one of the given weapons equipped.
	/// </summary>
	/// <remarks>
	/// Equipment perks (such as the FRYING PAN snack boost) do not apply while their wearer is toast.
	/// </remarks>
	public bool PartyHasLivingWeapon(params string[] weaponNames)
	{
		return GetAlivePartyMembers().Any(x => weaponNames.Contains(x.Weapon?.Name));
	}

	/// <summary>
	/// Whether any living party member has the given charm equipped.
	/// </summary>
	/// <remarks>
	/// Equipment perks (such as the BREADPHONES full-heal revive) do not apply while their wearer is toast.
	/// </remarks>
	public bool PartyHasLivingCharm(string charmName)
	{
		return GetAlivePartyMembers().Any(x => x.Charm?.Name == charmName);
	}

	/// <summary>
	/// Retrieves the <see cref="PartyMember"/> at the given internal array <paramref name="index"/> in the party.
	/// </summary>
	/// <remarks>
	/// Party members are counted in position order (0 to 3), skipping empty positions, so the index matches the position only when no earlier position is empty.<br/>
	/// Use <see cref="GetPartyMemberAtPosition"/> for that purpose instead.
	/// </remarks>
	/// <param name="index"></param>
	public PartyMember GetPartyMember(int index)
	{
		return CurrentParty.ElementAtOrDefault(Math.Clamp(index, 0, 3))?.Actor;
	}

	/// <summary>
	/// Retrieves the <see cref="Enemy"/> at the given <paramref name="index"/>, in <see cref="GetAllEnemies"/> order, or null if it's out of range.
	/// </summary>
	/// <remarks>
	/// Toast enemies keep their index, and summoned enemies are added at the end of the list.
	/// </remarks>
	/// <param name="index">The index to retrieve.</param>
	public Enemy GetEnemy(int index)
	{
		return Enemies.ElementAtOrDefault(index)?.Actor;
	}

	internal void RecordLastTarget(Actor user, Actor target)
	{
		user.LastTarget = target;
		user.LastTargetIndex = target is Enemy
			? Enemies.IndexOf(((Enemy)target).Component)
			: CurrentParty.IndexOf(((PartyMember)target).Component);
	}

	internal Enemy SmoothEnemyTarget(int index)
	{
		Enemy enemy = GetEnemy(index);
		return enemy != null && !enemy.IsToast ? enemy : GetAllAliveEnemies().FirstOrDefault();
	}

	internal PartyMember SmoothPartyTarget(int index)
	{
		PartyMember member = CurrentParty.ElementAtOrDefault(index)?.Actor;
		return member != null && member.CurrentHP > 0 && !member.IsToast ? member : GetAlivePartyMembers().FirstOrDefault();
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
		if (!Items.TryAdd(name, Math.Min(quantity, MaxItemCount)))
			Items[name] = Math.Min(Items[name] + quantity, MaxItemCount);
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
