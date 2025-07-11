using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public partial class BattleManager : Node
{
	[Export] public Label EnergyText;

	private List<PartyMemberComponent> CurrentParty = [];
	private List<EnemyComponent> Enemies = [];

	private BattlePhase Phase = BattlePhase.FightRun;
	private int CurrentPartyMember = -1;
	private int CurrentEnemyTarget = -1;
	private int CurrentPartyMemberTarget = -1;
	private List<BattleCommand> Commands = [];
	private int CommandIndex = -1;
	private Timer Delay;
	private List<Node2D> DyingEnemies = [];
	private Dictionary<string, int> Items = [];
	private BattleAction SelectedAction;
	public int Energy { get; private set; } = 0;
	private bool FollowupActive = true;
	private bool ForceHideFollowup = false;

	public static BattleManager Instance { get; private set; }

	public override void _EnterTree()
	{
		Instance = this;
	}

	public void Init(List<PartyMemberComponent> party, List<EnemyComponent> enemies, Dictionary<string, int> items)
	{
		CurrentParty = party;
		Enemies = enemies;
		Items = items;
		Energy = 3;

		Delay = new Timer
		{
			OneShot = true,
			Autostart = false,
		};
		AddChild(Delay);
		Delay.Timeout += OnDelayTimeout;
		BattleLogManager.Instance.FinishedLogging += OnBattleLogFinished;

		DamageNumber.CacheTexture(GD.Load<Texture2D>("res://assets/system/Damage.png"));

		CallDeferred(MethodName.PreBattle);

		SetPhase(BattlePhase.FightRun);
	}

	private void PreBattle()
	{
		if (CurrentParty.Any(x => x.Actor.Weapon.Name == "Hero's Trophy"))
		{
			foreach (PartyMemberComponent party in CurrentParty)
			{
				if (party.Actor is Hero)
				{
					party.Actor.SetState("sad", true);
				}
			}
		}

		foreach (PartyMemberComponent party in CurrentParty)
		{
			if (party.Actor.Weapon.Name == "LOL Sword")
			{
				party.Actor.SetState("happy", true);
			}
			party.Actor.Charm?.StartOfBattle(party.Actor);
		}
	}

	public override void _Process(double delta)
	{
		for (int i = 0; i < CurrentParty.Count; i++)
		{
			CurrentParty[i].SelectionBoxVisible = (i == CurrentPartyMember || i == CurrentPartyMemberTarget);
		}

		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemies[i].ShowInfoBox(i == CurrentEnemyTarget);
		}

		// TODO: energy bar dots
		MenuManager.Instance.EnergyText.Text = $"{Energy:00}";
		MenuManager.Instance.EnergyBar.RegionRect = new Rect2(0, (float)Math.Ceiling(Energy / 3f) * 45f, 362f, 49f);

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
					if (CurrentPartyMember == 0)
					{
						AudioManager.Instance.PlaySFX("sys_cancel");
						MenuManager.Instance.ShowMenu(MenuState.Party);
						SetPhase(BattlePhase.FightRun);
					}
					else
					{
						if (Commands[^1].Action is Item item)
						{
							if (!Items.ContainsKey(item.Name))
								Items.Add(item.Name, 1);
							else
								Items[item.Name]++;
						}
						Commands.RemoveAt(Commands.Count - 1);
						CurrentPartyMember--;
						AudioManager.Instance.PlaySFX("sys_cancel");
						MenuManager.Instance.ShowMenu(MenuState.Battle);
						SetPhase(BattlePhase.PlayerCommand);
					}
					break;
				case BattlePhase.TargetSelection:
					AudioManager.Instance.PlaySFX("sys_cancel");
					CurrentEnemyTarget = -1;
					CurrentPartyMemberTarget = -1;
					MenuManager.Instance.ShowMenu(MenuState.Battle);
					SetPhase(BattlePhase.PlayerCommand);
					break;
				case BattlePhase.SkillSelection:
					AudioManager.Instance.PlaySFX("sys_cancel");
					MenuManager.Instance.ShowMenu(MenuState.Battle);
					SetPhase(BattlePhase.PlayerCommand);
					break;
			}
		}

		// TODO: refactor target selection

		if (Input.IsActionJustPressed("MenuLeft"))
		{
			if (Phase == BattlePhase.CommandExecute || Phase == BattlePhase.WaitForBattleLog)
			{
				if (HandleFollowup("left"))
				{
					ProcessFollowupSuccess();
				}
			}
			if (Phase == BattlePhase.TargetSelection)
			{
				AudioManager.Instance.PlaySFX("SYS_move");
				if (SelectedAction.Target == SkillTarget.Enemy || (SelectedAction.Target == SkillTarget.AllyOrEnemy && CurrentEnemyTarget > -1) && Enemies.Count > 1)
				{
					CurrentEnemyTarget--;
					if (CurrentEnemyTarget < 0)
						CurrentEnemyTarget = Enemies.Count - 1;
					return;
				}
				if (SelectedAction.Target == SkillTarget.Ally || SelectedAction.Target == SkillTarget.DeadAlly || (SelectedAction.Target == SkillTarget.AllyOrEnemy && CurrentPartyMemberTarget > -1))
				{
					switch (CurrentPartyMemberTarget)
					{
						case 0:
							CurrentPartyMemberTarget = 3;
							break;
						case 1:
							CurrentPartyMemberTarget = 2;
							break;
						case 2:
							CurrentPartyMemberTarget = 1;
							break;
						case 3:
							CurrentPartyMemberTarget = 0;
							break;
					}
				}
			}

		}

		if (Input.IsActionJustPressed("MenuRight"))
		{
			if (Phase == BattlePhase.CommandExecute || Phase == BattlePhase.WaitForBattleLog)
			{
				if (HandleFollowup("right"))
				{
					ProcessFollowupSuccess();
				}
			}
			if (Phase == BattlePhase.TargetSelection)
			{
				AudioManager.Instance.PlaySFX("SYS_move");
				if (SelectedAction.Target == SkillTarget.Enemy || (SelectedAction.Target == SkillTarget.AllyOrEnemy && CurrentEnemyTarget > -1) && Enemies.Count > 1)
				{
					CurrentEnemyTarget++;
					if (CurrentEnemyTarget >= Enemies.Count)
						CurrentEnemyTarget = 0;
					return;
				}
				if (SelectedAction.Target == SkillTarget.Ally || SelectedAction.Target == SkillTarget.DeadAlly || (SelectedAction.Target == SkillTarget.AllyOrEnemy && CurrentPartyMemberTarget > -1))
				{
					switch (CurrentPartyMemberTarget)
					{
						case 0:
							CurrentPartyMemberTarget = 3;
							break;
						case 1:
							CurrentPartyMemberTarget = 2;
							break;
						case 2:
							CurrentPartyMemberTarget = 1;
							break;
						case 3:
							CurrentPartyMemberTarget = 0;
							break;
					}
				}
			}
		}

		if (Input.IsActionJustPressed("MenuUp"))
		{
			if (Phase == BattlePhase.CommandExecute || Phase == BattlePhase.WaitForBattleLog)
			{
				if (HandleFollowup("up"))
				{
					ProcessFollowupSuccess();
				}
			}
			if (Phase == BattlePhase.TargetSelection)
			{
				if (SelectedAction.Target == SkillTarget.Ally || SelectedAction.Target == SkillTarget.DeadAlly || (SelectedAction.Target == SkillTarget.AllyOrEnemy && CurrentPartyMemberTarget > -1))
				{
					AudioManager.Instance.PlaySFX("SYS_move");
					switch (CurrentPartyMemberTarget)
					{
						case 0:
							CurrentPartyMemberTarget = 1;
							break;
						case 1:
							CurrentPartyMemberTarget = 0;
							break;
						case 2:
							CurrentPartyMemberTarget = 3;
							break;
						case 3:
							CurrentPartyMemberTarget = 2;
							break;
					}
				}
			}
		}

		if (Input.IsActionJustPressed("MenuDown"))
		{
			if (Phase == BattlePhase.CommandExecute || Phase == BattlePhase.WaitForBattleLog)
			{
				if (HandleFollowup("down"))
				{
					ProcessFollowupSuccess();
				}
			}
			if (Phase == BattlePhase.TargetSelection)
			{
				if (SelectedAction.Target == SkillTarget.Ally || SelectedAction.Target == SkillTarget.DeadAlly || (SelectedAction.Target == SkillTarget.AllyOrEnemy && CurrentPartyMemberTarget > -1))
				{
					AudioManager.Instance.PlaySFX("SYS_move");
					switch (CurrentPartyMemberTarget)
					{
						case 0:
							CurrentPartyMemberTarget = 1;
							break;
						case 1:
							CurrentPartyMemberTarget = 0;
							break;
						case 2:
							CurrentPartyMemberTarget = 3;
							break;
						case 3:
							CurrentPartyMemberTarget = 2;
							break;
					}
				}
			}
		}

		if (Input.IsActionJustPressed("SwitchSides"))
		{
			if (Phase == BattlePhase.TargetSelection && SelectedAction.Target == SkillTarget.AllyOrEnemy)
			{
				if (CurrentPartyMemberTarget > -1)
				{
					CurrentPartyMemberTarget = -1;
					CurrentEnemyTarget = 0;
				}
				else
				{
					CurrentPartyMemberTarget = 0;
					CurrentEnemyTarget = -1;
				}
			}
		}
	}

	public void OnFightSelected()
	{
		CurrentPartyMember++;
		SetPhase(BattlePhase.PlayerCommand);
	}

	public void OnSelectAttack()
	{
		SelectedAction = CurrentParty[CurrentPartyMember].Actor.Skills.Values.First();
		SetPhase(BattlePhase.TargetSelection);
	}

	// idfk
	public void OnSelectNotAttack()
	{
		SetPhase(BattlePhase.SkillSelection);
	}

	public void OnSelectSkill(Skill skill)
	{
		SelectedAction = skill;
		if (CurrentParty[CurrentPartyMember].Actor.CurrentJuice - (SelectedAction as Skill).Cost < 0)
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}
		if ((SelectedAction.Target == SkillTarget.DeadAlly || SelectedAction.Target == SkillTarget.AllDeadAllies) && !CurrentParty.Any(x => x.Actor.CurrentState == "toast"))
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}
		AudioManager.Instance.PlaySFX("SYS_select");
		SetPhase(BattlePhase.TargetSelection);
	}

	public void OnSelectItem(Item item)
	{
		SelectedAction = item;
		if ((SelectedAction.Target == SkillTarget.DeadAlly || SelectedAction.Target == SkillTarget.AllDeadAllies) && !CurrentParty.Any(x => x.Actor.CurrentState == "toast"))
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}

		Item i = SelectedAction as Item;
		Items[i.Name]--;
		if (Items[i.Name] == 0)
			Items.Remove(i.Name);

		AudioManager.Instance.PlaySFX("SYS_select");
		SetPhase(BattlePhase.TargetSelection);
	}

	private void SetPhase(BattlePhase phase)
	{
		GD.Print("Entering Phase: " + phase);
		Phase = phase;

		switch (Phase)
		{
			case BattlePhase.FightRun:
				CheckBattleOver();
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
				Delay.Start(0.75d);
				break;
			case BattlePhase.CommandExecute:
				HandleCommandExecute(); 
				break;
			case BattlePhase.PostCommand:
				Delay.Start(0.75d);
				break;
			case BattlePhase.EnemyDying:
				HandleEnemyDying();
				break;

		}
	}

	private void OnDelayTimeout()
	{
		switch (Phase)
		{
			case BattlePhase.PreCommand:
				GD.Print("Command Index: " + CommandIndex);
				if (CommandIndex >= Commands.Count)
				{
					Enemies.ForEach(x => x.Actor.ProcessEndOfTurn());
					SetPhase(BattlePhase.FightRun);
				}
				else
				{
					SetPhase(BattlePhase.CommandExecute);
				}
				break;
			case BattlePhase.PostCommand:

				CurrentParty.ForEach(x =>
				{
					x.Actor.SetHurt(false);					
					if (x.Actor.CurrentHP == 0 && x.Actor.CurrentState != "toast")
					{
						x.Actor.SetState("toast", true);
						AudioManager.Instance.PlaySFX("SYS_you died_2", 1.2f);
					}
				});
				if (Commands[CommandIndex].Actor is PartyMember && Commands[CommandIndex].Action.Name.EndsWith("Attack"))
				{
					PartyMemberComponent component = CurrentParty.First(x => x.Actor == Commands[CommandIndex].Actor);
					if (component.HasFollowup)
					{
						component.FadeOutFollowups();
					}
				}
				FollowupActive = false;

				foreach (EnemyComponent enemy in Enemies.ToList())
				{
					enemy.Actor.SetHurt(false);
					if (enemy.Actor.CurrentHP == 0)
					{
						enemy.Actor.SetState("toast", true);
						if (enemy.Actor.FallsOffScreen)
							DyingEnemies.Add(enemy.GetParent<Node2D>());
						Enemies.Remove(enemy);
					}
					enemy.Actor.ProcessBattleConditions();
				}
				CommandIndex++;
				if (DyingEnemies.Count > 0)
					SetPhase(BattlePhase.EnemyDying);
				else
					SetPhase(BattlePhase.PreCommand);
				break;
		}
	}

	private void PrepareCommandExecution()
	{
		MenuManager.Instance.ShowMenu(MenuState.None);
		foreach (EnemyComponent enemy in Enemies)
		{
			// Add an empty action for each enemy so the sorting below still works
			Commands.Add(new BattleCommand(enemy.Actor, null, null));
		}

		Commands = Commands.OrderByDescending(x => x.Action is Skill s && s.GoesFirst)
			.ThenByDescending(x => x.Actor.CurrentStats.SPD)
			.ThenBy(x =>
			{
				PartyMemberComponent c = CurrentParty.FirstOrDefault(y => y.Actor == x.Actor);
				if (c == null)
					return int.MaxValue;
				else
					return c.Position;
			})
			.ToList();

		CommandIndex = 0;
		GD.Print("Preparing to process " + Commands.Count + " commands...");
	}

	private void HandleFightRun()
	{
		CurrentPartyMember = -1;
		CurrentEnemyTarget = -1;
		CurrentPartyMemberTarget = -1;
		CommandIndex = -1;
		Commands.Clear();
		// tick down stat turn timers
		CurrentParty.ForEach(x => x.Actor.DecreaseStatTurnCounter());
		Enemies.ForEach(x =>
		{
			x.Actor.DecreaseStatTurnCounter();
			x.Actor.ProcessStartOfTurn();
		});
		BattleLogManager.Instance.ClearAndShowMessage("What will " + CurrentParty[0].Actor.Name.ToUpper() + " and friends do?");
		MenuManager.Instance.ShowButtons(CurrentParty[0].Actor.IsRealWorld);
		MenuManager.Instance.ShowMenu(MenuState.Party);
	}

	private void HandlePlayerCommand()
	{
		while (CurrentParty[CurrentPartyMember].Actor.CurrentState == "toast")
		{
			CurrentPartyMember++;
			if (CurrentPartyMember >= CurrentParty.Count)
			{
				BattleLogManager.Instance.ClearBattleLog();
				PrepareCommandExecution();
				SetPhase(BattlePhase.PreCommand);
				return;
			}
		}
		BattleLogManager.Instance.ClearAndShowMessage("What will " + CurrentParty[CurrentPartyMember].Actor.Name.ToUpper() + " do?");
		MenuManager.Instance.ShowButtons(CurrentParty[CurrentPartyMember].Actor.IsRealWorld);
		MenuManager.Instance.ShowMenu(MenuState.Battle);
	}

	private void HandleTargetSelection()
	{
		switch (SelectedAction.Target)
		{
			case SkillTarget.Ally:
			case SkillTarget.DeadAlly:
				// keep selection box on current ally for ally targeting
				CurrentPartyMemberTarget = CurrentPartyMember;
				BattleLogManager.Instance.ClearAndShowMessage("Use on whom?");
				MenuManager.Instance.ShowMenu(MenuState.None);
				return;
			case SkillTarget.Enemy:
				CurrentEnemyTarget++;
				BattleLogManager.Instance.ClearAndShowMessage("Use on whom?");
				MenuManager.Instance.ShowMenu(MenuState.None);
				return;
			case SkillTarget.AllyOrEnemy:
				CurrentPartyMemberTarget = CurrentPartyMember;
				BattleLogManager.Instance.ClearAndShowMessage("Use on whom?\nPress SHIFT to switch sides.");
				MenuManager.Instance.ShowMenu(MenuState.None);
				return;
		}
		SelectTarget();
	}

	private void SelectTarget()
	{
		if ((SelectedAction.Target == SkillTarget.Ally || 
			(SelectedAction.Target == SkillTarget.AllyOrEnemy && CurrentPartyMemberTarget > -1)) 
			&& CurrentParty[CurrentPartyMemberTarget].Actor.CurrentState == "toast")
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}

		if ((SelectedAction.Target == SkillTarget.DeadAlly || 
			SelectedAction.Target == SkillTarget.AllDeadAllies && CurrentPartyMemberTarget > -1)
			&& CurrentParty[CurrentPartyMemberTarget].Actor.CurrentState != "toast")
		{
			AudioManager.Instance.PlaySFX("sys_buzzer");
			return;
		}
		
		AudioManager.Instance.PlaySFX("SYS_select");
		switch (SelectedAction.Target)
		{
			case SkillTarget.Self:
				Commands.Add(new BattleCommand(CurrentParty[CurrentPartyMember].Actor, CurrentParty[CurrentPartyMember].Actor, SelectedAction));
				break;
			case SkillTarget.Ally:
			case SkillTarget.DeadAlly:
				Commands.Add(new BattleCommand(CurrentParty[CurrentPartyMember].Actor, CurrentParty[CurrentPartyMemberTarget].Actor, SelectedAction));
				break;
			case SkillTarget.Enemy:
				Commands.Add(new BattleCommand(CurrentParty[CurrentPartyMember].Actor, Enemies[CurrentEnemyTarget].Actor, SelectedAction));
				break;
			case SkillTarget.AllyOrEnemy:
				if (CurrentEnemyTarget > -1)
					Commands.Add(new BattleCommand(CurrentParty[CurrentPartyMember].Actor, Enemies[CurrentEnemyTarget].Actor, SelectedAction));
				else
					Commands.Add(new BattleCommand(CurrentParty[CurrentPartyMember].Actor, CurrentParty[CurrentPartyMemberTarget].Actor, SelectedAction));
				break;
			default:
				// group skills have no "target"
				// targets are selected in the skill itself
				// ... which is probably bad behavior but oh well
				Commands.Add(new BattleCommand(CurrentParty[CurrentPartyMember].Actor, null, SelectedAction));
				break;
		}

		CurrentEnemyTarget = -1;
		CurrentPartyMemberTarget = -1;
		CurrentPartyMember++;
		if (CurrentPartyMember >= CurrentParty.Count)
		{
			BattleLogManager.Instance.ClearBattleLog();
			PrepareCommandExecution();
			SetPhase(BattlePhase.PreCommand);
		}
		else
			SetPhase(BattlePhase.PlayerCommand);
	}

	private async void HandleCommandExecute()
	{
		while (Commands[CommandIndex].Actor.CurrentState == "toast")
		{
			if (Commands[CommandIndex].Action is Item item)
			{
				// refund items if the character died before using it
				Items[item.Name]++;
			}
			CommandIndex++;
			if (CommandIndex >= Commands.Count)
			{
				SetPhase(BattlePhase.FightRun);
				return;
			}
		}

		BattleCommand currentAction = Commands[CommandIndex];

		if (currentAction.Actor is Enemy enemy && currentAction.Action == null)
		{
			// overwrite the empty enemy skill with an actual command
			currentAction = enemy.ProcessAI();
		}

		BattleLogManager.Instance.ClearBattleLog();
		Actor target = currentAction.Target;
		if (target != null)
		{
			if (target.CurrentHP == 0 && currentAction.Action.Target != SkillTarget.DeadAlly && currentAction.Action.Target != SkillTarget.AllDeadAllies)
			{
				if (target is Enemy)
					target = GetRandomAliveEnemy();
				else
					target = GetRandomAlivePartyMember();
				if (target == null)
				{
					GD.PrintErr("Running Command when all targets are dead!");
					return;
				}
				currentAction.Target = target;
			}
		}
		if (currentAction.Action is Skill skill)
		{
			if (skill.Cost > 0)
			{
				currentAction.Actor.CurrentJuice -= skill.Cost;
			}
			if (currentAction.Actor is PartyMember && skill.Name.EndsWith("Attack"))
			{
				if (ForceHideFollowup)
				{
					ForceHideFollowup = false;
				}
				else
				{
					PartyMemberComponent component = CurrentParty.First(x => x.Actor == currentAction.Actor);
					if (component.HasFollowup)
					{
						component.FadeInFollowups(Energy);
						FollowupActive = true;
					}
				}
			}
		}

		await currentAction.Action.Effect(currentAction.Actor, currentAction.Target);

		if (BattleLogManager.Instance.IsProcessingMessage)
			SetPhase(BattlePhase.WaitForBattleLog);
		else
			SetPhase(BattlePhase.PostCommand);
	}

	// TODO: refactor if possible
	// TODO: add other followup tiers
	private bool HandleFollowup(string direction)
	{
		if (Energy < 3)
			return false;

		PartyMemberComponent current = CurrentParty.First(x => x.Actor == Commands[CommandIndex].Actor);
		switch (current.Position)
		{
			case 1:
				if (direction == "up") {
					if (Database.TryGetSkill("AttackAgain1", out Skill skill))
						ForceCommand(current.Actor, Commands[CommandIndex].Target, skill);
					return true;
				}
				if (direction == "right")
				{
					if (Database.TryGetSkill("Trip1", out Skill skill))
						ForceCommand(current.Actor, Commands[CommandIndex].Target, skill);
					return true;
				}
				if (direction == "down")
				{
					if (Energy == 10 && CurrentParty.All(x => x.Actor.CurrentState != "toast"))
					{
						if (Database.TryGetSkill("ReleaseEnergy1", out Skill skill))
							ForceCommand(current.Actor, null, skill);
						return true;
					}
				}
				break;
			case 2:
				if (direction == "up")
				{
					if (GetPartyMember(2).CurrentState == "toast")
						return false;
					if (Database.TryGetSkill("LookAtHero1", out Skill skill))
						ForceCommand(current.Actor, Commands[CommandIndex].Target, skill);
					return true;
				}
				if (direction == "right")
				{
					if (GetPartyMember(3).CurrentState == "toast")
						return false;
					if (Database.TryGetSkill("LookAtKel1", out Skill skill))
						ForceCommand(current.Actor, Commands[CommandIndex].Target, skill);
					return true;
				}
				if (direction == "down")
				{
					if (Database.TryGetSkill("LookAtOmori1", out Skill skill))
						ForceCommand(current.Actor, Commands[CommandIndex].Target, skill);
					return true;
				}
				break;
			case 3:
				if (direction == "up")
				{
					if (GetPartyMember(1).CurrentState == "toast")
						return false;
					if (Database.TryGetSkill("CallAubrey1", out Skill skill))
					{
						ForceCommand(current.Actor, Commands[CommandIndex].Target, skill);
						ForceHideFollowup = true;
					}
					return true;
				}
				if (direction == "left")
				{
					if (Database.TryGetSkill("CallOmori1", out Skill skill))
					{
						ForceCommand(current.Actor, Commands[CommandIndex].Target, skill);
						ForceHideFollowup = true;
					}
					return true;
				}
				if (direction == "down")
				{
					if (GetPartyMember(3).CurrentState == "toast")
						return false;
					if (Database.TryGetSkill("CallKel1", out Skill skill))
					{
						ForceCommand(current.Actor, Commands[CommandIndex].Target, skill);
						ForceHideFollowup = true;
					}
					return true;
				}
				break;
			case 4:
				if (direction == "down")
				{
					if (Database.TryGetSkill("PassToOmori1", out Skill skill))
						ForceCommand(current.Actor, Commands[CommandIndex].Target, skill);
					return true;
				}
				if (direction == "left")
				{
					if (GetPartyMember(1).CurrentState == "toast")
						return false;
					if (Database.TryGetSkill("PassToAubrey1", out Skill skill))
						ForceCommand(current.Actor, Commands[CommandIndex].Target, skill);
					return true;
				}
				if (direction == "up")
				{
					if (GetPartyMember(2).CurrentState == "toast")
						return false;
					if (Database.TryGetSkill("PassToHero1", out Skill skill))
						ForceCommand(current.Actor, Commands[CommandIndex].Target, skill);
					return true;
				}
				break;
		}
		return false;
	}

	public void ForceCommand(Actor self, Actor target, Skill skill)
	{
		Commands.Insert(CommandIndex + 1, new BattleCommand(self, target, skill));
	}

	private void ProcessFollowupSuccess()
	{
		AudioManager.Instance.PlaySFX("Skill2", 1f, 0.8f);
		FollowupActive = false;
		CurrentParty.First(x => x.Actor == Commands[CommandIndex].Actor).FadeOutFollowups();
		if (Commands[CommandIndex + 1].Action.Name.Contains("Release Energy"))
			Energy = 0;
		else
			Energy -= 3;
	}

	public void OnBattleLogFinished()
	{
		if (Phase == BattlePhase.WaitForBattleLog)
			SetPhase(BattlePhase.PostCommand);
	}

	private void HandleEnemyDying()
	{
		Tween tween = CreateTween();
		foreach (Node2D enemy in DyingEnemies)
		{
			tween.TweenInterval(0.5f);
			tween.TweenProperty(enemy, "position", enemy.Position + new Vector2(0, 400f), 0.50f);
		}
		tween.TweenCallback(Callable.From(EnemiesDoneDying));
	}

	private void EnemiesDoneDying()
	{
		DyingEnemies.ForEach(x => x.QueueFree());
		DyingEnemies.Clear();
		SetPhase(BattlePhase.PreCommand);
	}

	private void CheckBattleOver()
	{
		if (Enemies.Count == 0)
		{
			SetPhase(BattlePhase.BattleOver);
			CurrentParty.ForEach(x =>
			{
				if (x.Actor.CurrentState != "toast")
					x.Actor.SetState("victory", true);
			});
			AudioManager.Instance.PlayBGM("xx_victory");
			BattleLogManager.Instance.ClearAndShowMessage(CurrentParty[0].Actor.Name.ToUpper() + "'s party was victorious!");
		}
		if (CurrentParty.All(x => x.Actor.CurrentHP == 0))
		{
			SetPhase(BattlePhase.BattleOver);
			BattleLogManager.Instance.ClearAndShowMessage(CurrentParty[0].Actor.Name.ToUpper() + "'s party was defeated...");
		}
	}

	public bool Damage(Actor self, Actor target, Func<float> damageFunc, bool neverMiss = true, float variance = 0.2f, bool guaranteeCrit = false, bool neverCrit = false)
	{
		if (!neverMiss)
		{
			bool miss = self.CurrentStats.HIT < GameManager.Instance.Random.RandiRange(0, 100);
			if (miss)
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor]'s attack missed...");
				AudioManager.Instance.PlaySFX("BA_miss");
				// Miss text spawns a little further down
				SpawnDamageNumber(-1, target.CenterPoint + new Vector2(0, 30), DamageType.Miss);
				return false;
			}
		}
		float baseDamage = damageFunc();
		float damageVariance = GameManager.Instance.Random.RandfRange(1f - variance, 1f + variance);
		bool critical = self.CurrentStats.LCK * .01f >= GameManager.Instance.Random.Randf() || guaranteeCrit;
		float finalDamage = baseDamage * damageVariance;
		string selfState = self.CurrentState;
		string targetState = target.CurrentState;
		// TODO: handle these better
		if (self.HasStatModifier(Modifier.SweetheartLock))
			selfState = "happy";
		if (target.HasStatModifier(Modifier.SweetheartLock))
			targetState = "happy";
		finalDamage = CalculateEmotionModifiers(selfState, targetState, finalDamage, out int effectiveness);
		if ((critical || target.HasStatModifier(Modifier.Tickle)) && !neverCrit)
		{
			finalDamage = (finalDamage * 1.5f) + 2;
			BattleLogManager.Instance.QueueMessage("IT HIT RIGHT IN THE HEART!");
			AudioManager.Instance.PlaySFX("BA_CRITICAL_HIT", volume: 2f);
		}
		// flex currently works with items
		// not sure if that is intentional or not
		if (self.HasStatModifier(Modifier.Flex))
		{
			finalDamage *= 2.5f;
			self.RemoveStatModifier(Modifier.Flex);
		}
		int rounded;
		if (target.HasStatModifier(Modifier.Guard))
		{
			rounded = (int)Math.Round(finalDamage / 0.5f, MidpointRounding.AwayFromZero);
		}
		else
		{
			rounded = (int)Math.Round(finalDamage, MidpointRounding.AwayFromZero);
		}
		if (target.HasStatModifier(Modifier.PlotArmor))
		{
			rounded = 0;
		}
		if (rounded <= 0)
		{
			BattleLogManager.Instance.QueueMessage(self, target, "[actor]'s attack did nothing.");
			return true;
		}
		int juiceLost = 0;
		switch (target.CurrentState)
		{
			case "miserable":
				juiceLost = rounded;
				rounded = 0;
				break;
			case "depressed":
				int dmg = (int)Math.Round(rounded * 0.5f, MidpointRounding.AwayFromZero);
				juiceLost = dmg;
				rounded = dmg;
				break;
			case "sad":
				juiceLost = (int)Math.Round(rounded * 0.3f, MidpointRounding.AwayFromZero);
				rounded = (int)Math.Round(rounded * 0.7f, MidpointRounding.AwayFromZero);
				break;
		}
		if (target.CurrentJuice - juiceLost < 0)
		{
			juiceLost = target.CurrentJuice;
			rounded = Math.Abs(target.CurrentJuice - juiceLost);
			target.CurrentJuice = 0;
		}
		else
			target.CurrentJuice -= juiceLost;
		target.Damage(rounded);
		if (target is PartyMember)
		{
			Energy++;
			if (Energy > 10)
				Energy = 10;
		}
		SpawnDamageNumber(rounded, target.CenterPoint);
		// we don't need to play a hitsound if the attack is a critical
		if (!critical)
		{
			GD.Print("Effectiveness: " + effectiveness);
			if (effectiveness > 0)
			{
				BattleLogManager.Instance.QueueMessage("...It was a moving attack!");
				AudioManager.Instance.PlaySFX("se_impact_double", 1f, 0.9f);
			}
			else if (effectiveness < 0)
			{
				BattleLogManager.Instance.QueueMessage("...It was a dull attack.");
				AudioManager.Instance.PlaySFX("se_impact_soft", 1f, 0.9f);
			}
			else
				AudioManager.Instance.PlaySFX("SE_dig", 0.7f, 0.9f);
		}
		BattleLogManager.Instance.QueueMessage(self, target, "[target] takes " + rounded + " damage!");
		if (juiceLost > 0)
		{
			BattleLogManager.Instance.QueueMessage(self, target, "[target] lost " + juiceLost + " juice...");
			SpawnDamageNumber(juiceLost, target.CenterPoint + new Vector2(0, 50), DamageType.JuiceLoss);
		}

		return true;
	}

	// some healing and juice skills are affected by emotion

	public void Heal(Actor self, Actor target, Func<float> healFunc, float variance = 0.2f)
	{
		float baseHealing = healFunc();
		float healingVariance = GameManager.Instance.Random.RandfRange(1f - variance, 1f + variance);
		float finalHealing = baseHealing * healingVariance;
		finalHealing = CalculateEmotionModifiers(self.CurrentState, target.CurrentState, finalHealing, out _);
		int rounded = (int)Math.Round(finalHealing, MidpointRounding.AwayFromZero);
		target.Heal(rounded);
		SpawnDamageNumber(rounded, target.CenterPoint, DamageType.Heal);
		BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {rounded} HEART!");
	}

	public void HealJuice(Actor self, Actor target, Func<float> healFunc)
	{
		float baseJuice = healFunc();
		float finalJuice = CalculateEmotionModifiers(self.CurrentState, target.CurrentState, baseJuice, out _);
		int rounded = (int)Math.Round(finalJuice, MidpointRounding.AwayFromZero);
		target.HealJuice(rounded);
		SpawnDamageNumber(rounded, target.CenterPoint, DamageType.JuiceGain);
		BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {rounded} JUICE!");
	}

	private readonly int[,] EffectivenessMatrix = new int[3, 3]
	{
	//			angry sad happy
	/* angry */	{0, -1, 1},
	/* sad   */	{1, 0, -1},
	/* happy */	{-1, 1, 0}
	};
	private readonly float[] weakness = [1.5f, 2f, 2.5f];
	private readonly float[] resistance = [0.8f, 0.65f, 0.5f];

	private float CalculateEmotionModifiers(string self, string target, float damage, out int effect)
	{
		int selfIndex = GetEffectivenessIndex(self);
		int targetIndex = GetEffectivenessIndex(target);
		effect = 0;
		if (selfIndex == -1 || targetIndex == -1)
		{
			return damage;
		}
		int targetTier = GetEmotionTier(target);
		int effectiveness = EffectivenessMatrix[targetIndex, selfIndex];
		float multiplier = 1.0f;

		if (effectiveness > 0)
		{
			multiplier = weakness[targetTier];
		}
		else if (effectiveness < 0)
		{
			multiplier = resistance[targetTier];
		}

		effect = effectiveness;
		return damage * multiplier;
	}

	private int GetEffectivenessIndex(string emotion)
	{
		return emotion switch
		{
			"angry" or "enraged" or "furious" => 0,
			"sad" or "depressed" or "miserable" => 1,
			"happy" or "ecstatic" or "manic" => 2,
			_ => -1
		};;
	}

	private int GetEmotionTier(string emotion)
	{
		return emotion switch
		{
			"miserable" or "manic" or "furious" => 2,
			"depressed" or "ecstatic" or "enraged" => 1,
			"sad" or "happy" or "angry" => 0,
			_ => -1,
		};
	}

	public void RandomEmotion(Actor who)
	{
		int roll = GameManager.Instance.Random.RandiRange(0, 2);
		string state = "";
		switch (roll)
		{
			case 0:
				state = "sad";
				switch (who.CurrentState)
				{
					case "miserable":
						return;
					case "depressed":
						state = "miserable";
						break;
					case "sad":
						state = "depressed";
						break;
				}
				break;
			case 1:
				state = "angry";
				switch (who.CurrentState)
				{
					case "furious":
						return;
					case "enraged":
						state = "furious";
						break;
					case "angry":
						state = "enraged";
						break;
				}
				break;
			case 2:
				state = "happy";
				switch (who.CurrentState)
				{
					case "manic":
						return;
					case "ecstatic":
						state = "manic";
						break;
					case "happy":
						state = "ecstatic";
						break;
				}
				break;
		}
		if (who.IsStateValid(state))
			who.SetState(state);
	}

	public void SpawnDamageNumber(int damage, Vector2 position, DamageType type = DamageType.Damage)
	{
		DamageNumber dmg = new(damage, type)
		{
			Position = position,
			ZAsRelative = false,
			ZIndex = 5
		};
		AddChild(dmg);

		Task.Delay(TimeSpan.FromSeconds(1.5f)).ContinueWith(_ =>
		{
			dmg.CallDeferred(DamageNumber.MethodName.Despawn);
		});
	}

	public void AddEnergy(int amount)
	{
		Energy = Math.Min(Energy + amount, 10);
	}

	public PartyMember GetRandomAlivePartyMember()
	{
		IEnumerable<PartyMemberComponent> alive = CurrentParty.Where(x => x.Actor.CurrentHP > 0);
		return alive.ElementAt(GameManager.Instance.Random.RandiRange(0, alive.Count() - 1)).Actor;
	}

	public PartyMember GetRandomDeadPartyMember()
	{
		PartyMemberComponent result = CurrentParty.FirstOrDefault(x => x.Actor.CurrentHP <= 0);
		return result?.Actor;
	}

	public Enemy GetRandomAliveEnemy()
	{
		IEnumerable<EnemyComponent> alive = Enemies.Where(x => x.Actor.CurrentHP > 0);
		return alive.ElementAt(GameManager.Instance.Random.RandiRange(0, alive.Count() - 1)).Actor;
	}

	public List<Enemy> GetAllEnemies()
	{
		return Enemies.Select(x => x.Actor).ToList();
	}

	/// <summary>
	/// Gets all party members who are not toast.
	/// </summary>
	public List<PartyMemberComponent> GetAlivePartyMembers()
	{
		return CurrentParty.Where(x => x.Actor.CurrentHP > 0).ToList();
	}

	/// <summary>
	/// Gets all party members who are toast.
	/// </summary>
	public List<PartyMemberComponent> GetDeadPartyMembers()
	{
		return CurrentParty.Where(x => x.Actor.CurrentState == "toast").ToList();
	}

	/// <summary>
	/// Gets all party members, including ones who are toast.
	/// </summary>
	/// <remarks>
	/// Should NOT be used for skill logic in most situations, use <see cref="GetAlivePartyMembers"/> instead.
	/// </remarks>
	public List<PartyMemberComponent> GetAllPartyMembers()
	{
		return CurrentParty;
	}

	public PartyMember GetPartyMember(int index)
	{
		// eh who needs bounds checks these days
		return CurrentParty[index].Actor;
	}

	public PartyMember GetCurrentPartyMember()
	{
		return CurrentParty[CurrentPartyMember].Actor;
	}

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

public enum BattlePhase
{
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
