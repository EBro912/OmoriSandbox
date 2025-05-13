using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BattleManager : Node
{
	private List<PartyMemberComponent> CurrentParty = [];
	private List<EnemyComponent> Enemies = [];

	private BattlePhase Phase = BattlePhase.FightRun;
	private int CurrentPartyMember = -1;
	private int CurrentEnemyTarget = -1;
	private int CurrentPartyMemberTarget = -1;
	private List<BattleCommand> Commands = [];
	private int CommandIndex = -1;
	private Timer Delay;
	private Skill? SelectedSkill;

	public void Init(List<PartyMemberComponent> party, List<EnemyComponent> enemies)
	{
		CurrentParty = party;
		Enemies = enemies;

		Delay = new Timer
		{
			OneShot = true,
			Autostart = false,
		};
		AddChild(Delay);
		Delay.Timeout += OnDelayTimeout;

		SetPhase(BattlePhase.FightRun);
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
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("Accept"))
		{
			switch (Phase)
			{
				case BattlePhase.FightRun:
					if (MenuManager.Instance.CursorSelection == "Run")
					{
						AudioManager.Instance.PlaySFX("sys_buzzer");
						return;
					}
					else
					{
						AudioManager.Instance.PlaySFX("SYS_select");
						do
						{
							CurrentPartyMember++;
							if (CurrentPartyMember >= CurrentParty.Count)
							{
								GameManager.Instance.ClearBattleLog();
								PrepareCommandExecution();
								SetPhase(BattlePhase.PreCommand);
							}
						} while (CurrentParty[CurrentPartyMember].Actor.CurrentState == "toast");
						SetPhase(BattlePhase.PlayerCommand);
					}
					break;
				case BattlePhase.PlayerCommand:
					if (MenuManager.Instance.CursorSelection == "Attack")
					{
						AudioManager.Instance.PlaySFX("SYS_select");
						SelectedSkill = CurrentParty[CurrentPartyMember].Actor.Skills[$"{CurrentParty[CurrentPartyMember].Actor.Name[0]}Attack"];
						SetPhase(BattlePhase.TargetSelection);
					}
					else if (MenuManager.Instance.CursorSelection == "Skill")
					{
						AudioManager.Instance.PlaySFX("SYS_select");
						SetPhase(BattlePhase.SkillSelection);
						MenuManager.Instance.PopulateSkillMenu(CurrentParty[CurrentPartyMember].Actor);
						MenuManager.Instance.ShowMenu("SkillMenu");					
					}
					break;
				case BattlePhase.SkillSelection:
					SelectedSkill = MenuManager.Instance.GetSelectedSkill();
					if ((SelectedSkill?.Target == SkillTarget.DeadAlly || SelectedSkill?.Target == SkillTarget.AllDeadAllies) && !CurrentParty.Any(x => x.Actor.CurrentState == "toast")) {
						AudioManager.Instance.PlaySFX("sys_buzzer");
						return;
					}
					AudioManager.Instance.PlaySFX("SYS_select");
					SetPhase(BattlePhase.TargetSelection);
					break;
				case BattlePhase.TargetSelection:
					SelectTarget();
					break;
			}
		}

		if (Input.IsActionPressed("Back"))
		{
			switch (Phase)
			{
				case BattlePhase.PlayerCommand:
					AudioManager.Instance.PlaySFX("sys_cancel");
					if (CurrentPartyMember == 0)
						SetPhase(BattlePhase.FightRun);
					else
					{
						Commands.RemoveAt(Commands.Count - 1);
						CurrentPartyMember--;
						SetPhase(BattlePhase.PlayerCommand);
					}
					break;
				case BattlePhase.TargetSelection:
					AudioManager.Instance.PlaySFX("sys_cancel");
					CurrentEnemyTarget = -1;
					CurrentPartyMemberTarget = -1;
					SetPhase(BattlePhase.PlayerCommand);
					break;
				case BattlePhase.SkillSelection:
					AudioManager.Instance.PlaySFX("sys_cancel");
					MenuManager.Instance.ShowMenu("BattleCommand");
					SetPhase(BattlePhase.PlayerCommand);
					break;
			}			
		}

		if (Input.IsActionJustPressed("MenuLeft"))
		{
			if (Phase == BattlePhase.TargetSelection)
			{
				AudioManager.Instance.PlaySFX("SYS_move");
				if (SelectedSkill?.Target == SkillTarget.Enemy || (SelectedSkill?.Target == SkillTarget.AllyOrEnemy && CurrentEnemyTarget > -1) && Enemies.Count > 1)
				{
					CurrentEnemyTarget--;
					if (CurrentEnemyTarget < 0)
						CurrentEnemyTarget = Enemies.Count - 1;
					return;
				}
				if (SelectedSkill?.Target == SkillTarget.Ally || (SelectedSkill?.Target == SkillTarget.AllyOrEnemy && CurrentPartyMemberTarget > -1))
				{
					switch(CurrentPartyMemberTarget)
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
			if (Phase == BattlePhase.TargetSelection)
			{
				AudioManager.Instance.PlaySFX("SYS_move");
				if (SelectedSkill?.Target == SkillTarget.Enemy || (SelectedSkill?.Target == SkillTarget.AllyOrEnemy && CurrentEnemyTarget > -1) && Enemies.Count > 1)
				{
					CurrentEnemyTarget++;
					if (CurrentEnemyTarget >= Enemies.Count)
						CurrentEnemyTarget = 0;
					return;
				}
				if (SelectedSkill?.Target == SkillTarget.Ally || (SelectedSkill?.Target == SkillTarget.AllyOrEnemy && CurrentPartyMemberTarget > -1))
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

		if (Input.IsActionJustPressed("MenuUp") || Input.IsActionJustPressed("MenuDown"))
		{
			if (Phase == BattlePhase.TargetSelection)
			{
				if (SelectedSkill?.Target == SkillTarget.Ally || (SelectedSkill?.Target == SkillTarget.AllyOrEnemy && CurrentPartyMemberTarget > -1))
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
			if (Phase == BattlePhase.TargetSelection && SelectedSkill?.Target == SkillTarget.AllyOrEnemy)
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
				Delay.Start(2d);
				break;
		}
	}

	private void OnDelayTimeout()
	{
		switch (Phase)
		{
			case BattlePhase.PreCommand:
				if (CommandIndex >= Commands.Count)
					SetPhase(BattlePhase.FightRun);
				else {
					SetPhase(BattlePhase.CommandExecute);
				}
				break;
			case BattlePhase.PostCommand:
				Actor target = Commands[CommandIndex].Target;
				if (target != null)
				{
					CurrentParty.ForEach(x => x.Actor.SetHurt(false));
					Enemies.ForEach(x => x.Actor.SetHurt(false));
					if (Commands[CommandIndex].Target.CurrentHP == 0)
					{
						if (Commands[CommandIndex].Target is PartyMember)
							CurrentParty.First(x => x.Actor == target).Actor.SetState("toast");
						else
						{
							EnemyComponent enemy = Enemies.First(x => x.Actor == target);
							enemy.Despawn();
							Enemies.Remove(enemy);
							Commands.RemoveAll(x => x.Actor == target);
						}
					}
				}
				CommandIndex++;
				SetPhase(BattlePhase.PreCommand);
				break;
		}
	}

	private void PrepareCommandExecution()
	{
		MenuManager.Instance.ShowMenu("None");
		foreach (EnemyComponent enemy in Enemies)
		{
			Commands.Add(enemy.Actor.ProcessAI());
		}

		Commands = Commands.OrderByDescending(x => x.Skill.GoesFirst)
			.ThenByDescending(x => x.Actor.CurrentStats.SPD)
			.ThenBy(x =>
			{
				PartyMemberComponent c = CurrentParty.FirstOrDefault(y => y.Actor == x.Actor);
				if (c == null)
					return int.MaxValue;
				else
					return CurrentParty.IndexOf(c);
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
		GameManager.Instance.ClearAndMessageBattleLog("What will " + CurrentParty[0].Actor.Name.ToUpper() + " and friends do?");
		MenuManager.Instance.ShowMenu("PartyCommand");
	}

	private void HandlePlayerCommand()
	{
		GameManager.Instance.ClearAndMessageBattleLog("What will " + CurrentParty[CurrentPartyMember].Actor.Name.ToUpper() + " do?");
		MenuManager.Instance.ShowMenu("BattleCommand");
	}

	private void HandleTargetSelection()
	{
		MenuManager.Instance.ShowMenu("None");
		switch (SelectedSkill?.Target)
		{
			case SkillTarget.Ally:
				// keep selection box on current ally for ally targeting
				CurrentPartyMemberTarget = CurrentPartyMember;
				GameManager.Instance.ClearAndMessageBattleLog("Use on whom?");
				return;
			case SkillTarget.Enemy:
				CurrentEnemyTarget++;
				GameManager.Instance.ClearAndMessageBattleLog("Use on whom?");
				return;
			case SkillTarget.AllyOrEnemy:
				CurrentPartyMemberTarget = CurrentPartyMember;
				GameManager.Instance.ClearAndMessageBattleLog("Use on whom?\nPress SHIFT to switch sides.");
				return;
		}
		SelectTarget();
	}

	private void SelectTarget()
	{
		AudioManager.Instance.PlaySFX("SYS_select");
		switch (SelectedSkill?.Target)
		{
			case SkillTarget.Ally:
			case SkillTarget.DeadAlly:
				Commands.Add(new BattleCommand(CurrentParty[CurrentPartyMember].Actor, CurrentParty[CurrentPartyMemberTarget].Actor, SelectedSkill.Value));
				break;
			case SkillTarget.Enemy:
				Commands.Add(new BattleCommand(CurrentParty[CurrentPartyMember].Actor, Enemies[CurrentEnemyTarget].Actor, SelectedSkill.Value));
				break;
			case SkillTarget.AllyOrEnemy:
				if (CurrentEnemyTarget > -1)
					Commands.Add(new BattleCommand(CurrentParty[CurrentPartyMember].Actor, Enemies[CurrentEnemyTarget].Actor, SelectedSkill.Value));
				else
					Commands.Add(new BattleCommand(CurrentParty[CurrentPartyMember].Actor, CurrentParty[CurrentPartyMemberTarget].Actor, SelectedSkill.Value));
				break;
			default:
				// TODO: pass in targets as an array for multi hit skills
				Commands.Add(new BattleCommand(CurrentParty[CurrentPartyMember].Actor, Enemies[0].Actor, SelectedSkill.Value));
				break;
		}

		CurrentEnemyTarget = -1;
		CurrentPartyMemberTarget = -1;
		CurrentPartyMember++;
		if (CurrentPartyMember >= CurrentParty.Count)
		{
			GameManager.Instance.ClearBattleLog();
			PrepareCommandExecution();
			SetPhase(BattlePhase.PreCommand);
		}
		else
			SetPhase(BattlePhase.PlayerCommand);
	}

	private async void HandleCommandExecute()
	{
		GameManager.Instance.ClearBattleLog();
		Actor target = Commands[CommandIndex].Target;
		if (Commands[CommandIndex].Skill.Cost > 0)
		{
			Commands[CommandIndex].Actor.CurrentJuice -= Commands[CommandIndex].Skill.Cost;
		}
		if (target != null)
		{
			if (target.CurrentHP == 0)
			{
				if (target is Enemy)
					target = GameManager.Instance.BattleManager.GetRandomAliveEnemy();
				else
					target = GameManager.Instance.BattleManager.GetRandomAlivePartyMember();
				if (target == null)
				{
					GD.PrintErr("Running Command when all enemies are dead!");
					return;
				}
				Commands[CommandIndex].Target = target;
			}
		}
		
		GD.Print("Processing " + Commands[CommandIndex].GetType());
		await Commands[CommandIndex].Skill.Effect(Commands[CommandIndex].Actor, Commands[CommandIndex].Target, Commands[CommandIndex].Skill);
		SetPhase(BattlePhase.PostCommand);
	}

	private void CheckBattleOver()
	{
		if (Enemies.Count == 0)
		{
			SetPhase(BattlePhase.BattleOver);
			CurrentParty.ForEach(x => x.Actor.SetState("victory"));
			AudioManager.Instance.PlayBGM("xx_victory");
			GameManager.Instance.ClearAndMessageBattleLog(CurrentParty[0].Actor.Name.ToUpper() + "'s party was victorious!");
		}
	}

	public void Damage(Actor self, Actor target, Func<float> damageFunc, bool neverMiss = true)
	{
		if (!neverMiss)
		{
			bool miss = self.CurrentStats.HIT < GameManager.Instance.Random.RandiRange(0, 100);
			if (miss)
			{
				GameManager.Instance.MessageBattleLog(self, target, "[actor]'s attack missed...");
				AudioManager.Instance.PlaySFX("BA_miss");
				return;
			}
		}
		float baseDamage = damageFunc();
		float variance = GameManager.Instance.Random.RandfRange(0.8f, 1.2f);
		bool critical = self.CurrentStats.LCK * .01f >= GameManager.Instance.Random.Randf();
		float finalDamage = baseDamage * variance;
		finalDamage = CalculateEmotionModifiers(self.CurrentState, target.CurrentState, finalDamage);
		if (critical)
		{
			finalDamage = (finalDamage * 1.5f) + 2;
			GameManager.Instance.MessageBattleLog("IT HIT RIGHT IN THE HEART!");
			AudioManager.Instance.PlaySFX("BA_CRITICAL_HIT");
		}
		int rounded = (int)Math.Round(finalDamage, MidpointRounding.AwayFromZero);
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
		GameManager.Instance.MessageBattleLog(self, target, "[target] takes " + rounded + " damage!");
		if (juiceLost > 0)
		{
			GameManager.Instance.MessageBattleLog(self, target, "[target] lost " + juiceLost + " juice...");
		}
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

	private float CalculateEmotionModifiers(string self, string target, float damage)
	{
		int selfIndex = GetEffectivenessIndex(self);
		int targetIndex = GetEffectivenessIndex(target);
		if (selfIndex == -1 || targetIndex == -1)
			return damage;
		int targetTier = GetEmotionTier(target);
		int effectiveness = EffectivenessMatrix[targetIndex, selfIndex];
		float multiplier = 1.0f;

		if (effectiveness > 0)
		{
			GameManager.Instance.MessageBattleLog("...It was a moving attack!");
			multiplier = weakness[targetTier];
		}
		else if (effectiveness < 0)
		{
			GameManager.Instance.MessageBattleLog("...It was a dull attack.");
			multiplier = resistance[targetTier];
		}

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

	public PartyMember GetRandomAlivePartyMember()
	{
		IEnumerable<PartyMemberComponent> alive = CurrentParty.Where(x => x.Actor.CurrentHP > 0);
		return alive.ElementAt(GameManager.Instance.Random.RandiRange(0, alive.Count() - 1)).Actor;
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
}

public enum BattlePhase
{
	FightRun,
	PlayerCommand,
	TargetSelection,
	SkillSelection,
	PreCommand,
	CommandExecute,
	PostCommand,
	BattleOver
}
