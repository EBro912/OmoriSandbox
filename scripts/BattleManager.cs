using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class BattleManager : Node
{
	private List<PartyMemberComponent> CurrentParty = [];
	private List<EnemyComponent> Enemies = [];

	private BattlePhase Phase = BattlePhase.FightRun;
	private int CurrentPartyMember = -1;
	private int CurrentEnemy = -1;
	private List<BattleCommand> Commands = [];
	private int CommandIndex = -1;
	private Timer Delay;

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

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("Accept"))
		{
			switch (Phase)
			{
				case BattlePhase.FightRun:
					if (MenuManager.Instance.CursorSelection == "Run")
					{
						AudioManager.Instance.PlaySFX("Buzzer");
						return;
					}
					else
					{
						AudioManager.Instance.PlaySFX("Select");
						CurrentPartyMember++;
						SetPhase(BattlePhase.PlayerCommand);
					}
					break;
				case BattlePhase.PlayerCommand:
					AudioManager.Instance.PlaySFX("Select");
					CurrentEnemy++;
					SetPhase(BattlePhase.TargetSelection);
					break;
				case BattlePhase.TargetSelection:
					AudioManager.Instance.PlaySFX("Select");
					Commands.Add(new AttackCommand(CurrentParty[CurrentPartyMember].Actor, Enemies[CurrentEnemy].Actor));
					CurrentParty[CurrentPartyMember].SelectionBoxVisible = false;
					Enemies[CurrentEnemy].ShowInfoBox(false);
					CurrentEnemy = -1;
					CurrentPartyMember++;
					if (CurrentPartyMember >= CurrentParty.Count)
					{
						PrepareCommandExecution();
						SetPhase(BattlePhase.PreCommand);
					}
					else
						SetPhase(BattlePhase.PlayerCommand);
					break;
			}
		}

		if (Input.IsActionPressed("Back"))
		{
			switch (Phase)
			{
				case BattlePhase.PlayerCommand:
					AudioManager.Instance.PlaySFX("Cancel");
					if (CurrentPartyMember == 0)
						SetPhase(BattlePhase.FightRun);
					else
					{
						CurrentParty[CurrentPartyMember].SelectionBoxVisible = false;
						Commands.RemoveAt(Commands.Count - 1);
						CurrentPartyMember--;
						SetPhase(BattlePhase.PlayerCommand);
					}
					break;
				case BattlePhase.TargetSelection:
					AudioManager.Instance.PlaySFX("Cancel");
					Enemies[CurrentEnemy].ShowInfoBox(false);
					CurrentEnemy = -1;
					SetPhase(BattlePhase.PlayerCommand);
					break;
			}			
		}

		if (Input.IsActionJustPressed("MenuLeft"))
		{
			if (Phase == BattlePhase.TargetSelection)
				MoveEnemySelection(false);
		}

		if (Input.IsActionJustPressed("MenuRight"))
		{
			if (Phase == BattlePhase.TargetSelection)
				MoveEnemySelection(true);
		}
	}

	private void SetPhase(BattlePhase phase)
	{
		GD.Print("Entering Phase: " + phase);
		Phase = phase;

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
				Delay.Start(0.5d);
				break;
			case BattlePhase.CommandExecute:
				HandleCommandExecute(); 
				break;
			case BattlePhase.PostCommand:
				Delay.Start(2.0d);
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
				else
					SetPhase(BattlePhase.CommandExecute);
				break;
			case BattlePhase.PostCommand:
				if (Commands[CommandIndex].Target != null)
				{
					if (Commands[CommandIndex].Target.IsHurt)
					{
                        Commands[CommandIndex].Target.SetHurt(false);
					}
				}
				CommandIndex++;
				SetPhase(BattlePhase.PreCommand);
				break;
		}
	}

	private void MoveEnemySelection(bool direction)
	{
		if (Enemies.Count > 1)
		{
			Enemies[CurrentEnemy].ShowInfoBox(false);
			if (direction)
			{
				CurrentEnemy++;
				if (CurrentEnemy >= Enemies.Count)
					CurrentEnemy = 0;
			}
			else
			{
				CurrentEnemy--;
				if (CurrentEnemy < 0)
				{
					CurrentEnemy = Enemies.Count - 1;
				}
			}
			Enemies[CurrentEnemy].ShowInfoBox(true);
		}
	}

	private void PrepareCommandExecution()
	{
		// TODO: enemy ai
		foreach (EnemyComponent enemy in Enemies)
		{
			Commands.Add(new DoNothingCommand(enemy.Actor, "LOST SPROUT MOLE is rolling around."));
		}

		Commands = Commands.OrderByDescending(x => x.GoesFirst)
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
		CurrentEnemy = -1;
		CommandIndex = -1;
		CurrentParty[0].SelectionBoxVisible = false;
		Commands.Clear();
		GameManager.Instance.ClearAndMessageBattleLog("What will " + CurrentParty[0].Actor.Name.ToUpper() + " and friends do?");
		MenuManager.Instance.ShowMenu("PartyCommand");
	}

	private void HandlePlayerCommand()
	{
		CurrentParty[CurrentPartyMember].SelectionBoxVisible = true;
		GameManager.Instance.ClearAndMessageBattleLog("What will " + CurrentParty[CurrentPartyMember].Actor.Name.ToUpper() + " do?");
		MenuManager.Instance.ShowMenu("BattleCommand");
	}

	private void HandleTargetSelection()
	{
		Enemies[CurrentEnemy].ShowInfoBox(true);
		MenuManager.Instance.ShowMenu("None");
	}

	private void HandleCommandExecute()
	{
		GD.Print("Processing " + Commands[CommandIndex].GetType());
		Commands[CommandIndex].Run();
		SetPhase(BattlePhase.PostCommand);
	}

	// TODO: handle battle over
}

public enum BattlePhase
{
	FightRun,
	PlayerCommand,
	TargetSelection,
	EnemyCommand,
	ExecuteTurn,
	PreCommand,
	CommandExecute,
	PostCommand,
	BattleOver
}
