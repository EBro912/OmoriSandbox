using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
public partial class BattleManager : Node
{
	private List<PartyMemberComponent> CurrentParty = [];
	private List<EnemyComponent> Enemies = [];

	private BattlePhase Phase = BattlePhase.FightRun;
	private int CurrentPartyMember = -1;
	private int CurrentEnemy = -1;
	private List<BattleCommand> Commands = [];

	public void Init(List<PartyMemberComponent> party, List<EnemyComponent> enemies)
	{
		CurrentParty = party;
		Enemies = enemies;
		StartTurn();
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("Accept"))
		{
			switch (Phase)
			{
				case BattlePhase.FightRun:
					if (MenuManager.Instance.CursorSelection == "Fight")
					{
						Phase = BattlePhase.PlayerCommand;
						CurrentPartyMember = 0;
						CurrentParty[0].SelectionBoxVisible = true;
						GameManager.Instance.ClearAndMessageBattleLog("What will " + CurrentParty[0].Actor.Name.ToUpper() + " do?");
						MenuManager.Instance.ShowMenu("BattleCommand");
					}
					break;
				case BattlePhase.PlayerCommand:
					if (CurrentPartyMember < CurrentParty.Count)
					{
						Phase = BattlePhase.TargetSelection;
						CurrentEnemy = 0;
						Enemies[CurrentEnemy].ShowInfoBox(true);
						MenuManager.Instance.ShowMenu("None");
					}
					break;
				case BattlePhase.TargetSelection:
					CurrentParty[CurrentPartyMember].SelectionBoxVisible = false;
					CurrentPartyMember++;
					if (CurrentPartyMember < CurrentParty.Count)
					{
						CurrentParty[CurrentPartyMember].SelectionBoxVisible = true;
						Enemies[CurrentEnemy].ShowInfoBox(false);
						Commands.Add(new AttackCommand(CurrentParty[CurrentPartyMember].Actor, Enemies[CurrentEnemy].Actor));
						CurrentEnemy = -1;
						Phase = BattlePhase.PlayerCommand;
						GameManager.Instance.ClearAndMessageBattleLog("What will " + CurrentParty[CurrentPartyMember].Actor.Name.ToUpper() + " do?");
						MenuManager.Instance.ShowMenu("BattleCommand");
					}
					else
					{
						ExecuteTurn();
					}
					break;
			}
			return;
		}

		if (Input.IsActionJustPressed("Back"))
		{
			switch (Phase)
			{
				case BattlePhase.FightRun:
					break;
				case BattlePhase.PlayerCommand:
					if (CurrentPartyMember == 0)
					{
						Phase = BattlePhase.FightRun;
						CurrentPartyMember = -1;
						CurrentParty[0].SelectionBoxVisible = false;
						Commands.Clear();
						GameManager.Instance.ClearAndMessageBattleLog("What will " + CurrentParty[0].Actor.Name.ToUpper() + " and friends do?");
						MenuManager.Instance.ShowMenu("PartyCommand");
					}
					else
					{
						CurrentParty[CurrentPartyMember].SelectionBoxVisible = false;
						CurrentPartyMember--;
						Commands.RemoveAt(CurrentPartyMember);
						CurrentParty[CurrentPartyMember].SelectionBoxVisible = true;
						GameManager.Instance.ClearAndMessageBattleLog("What will " + CurrentParty[CurrentPartyMember].Actor.Name.ToUpper() + " do?");
					}
					break;
				case BattlePhase.TargetSelection:
					Enemies[CurrentEnemy].ShowInfoBox(false);
					CurrentEnemy = -1;
					Phase = BattlePhase.PlayerCommand;
					MenuManager.Instance.ShowMenu("BattleCommand");
					break;
			}
			return;
		}

		if (Input.IsActionJustPressed("MenuLeft"))
		{
			switch (Phase)
			{
				case BattlePhase.TargetSelection:
					if (Enemies.Count > 1)
					{
						Enemies[CurrentEnemy].ShowInfoBox(false);
						CurrentEnemy++;
						if (CurrentEnemy >= Enemies.Count)
							CurrentEnemy = 0;
						Enemies[CurrentEnemy].ShowInfoBox(true);
					}
					break;
			}
			return;
		}

		if (Input.IsActionJustPressed("MenuRight"))
		{
			switch (Phase)
			{
				case BattlePhase.TargetSelection:
					if (Enemies.Count > 1)
					{
						Enemies[CurrentEnemy].ShowInfoBox(false);
						CurrentEnemy++;
						if (CurrentEnemy >= Enemies.Count)
							CurrentEnemy = 0;
						Enemies[CurrentEnemy].ShowInfoBox(true);
					}
					break;
			}
			return;
		}
	}

	public void StartTurn()
	{
		Commands.Clear();
		Phase = BattlePhase.FightRun;
		GameManager.Instance.ClearAndMessageBattleLog("What will " + CurrentParty[0].Actor.Name.ToUpper() + " and friends do?");
		MenuManager.Instance.ShowMenu("PartyCommand");
		CurrentPartyMember = -1;
		CurrentEnemy = -1;
	}

	private void ExecuteTurn()
	{
		Phase = BattlePhase.ExecuteTurn;

		foreach (EnemyComponent enemy in Enemies)
		{
			Commands.Add(new DoNothingCommand(enemy.Actor, "LOST SPROUT MOLE is rolling around."));
		}

		// TODO: properly handle speed ties
		Commands = Commands.OrderByDescending(x => x.GoesFirst)
			.ThenByDescending(x => x.Actor.SPD)
			.ToList();

		Task.Run(ExecuteCommands);
	}

	private async Task ExecuteCommands()
	{
		foreach (BattleCommand command in Commands)
		{
			if (command.Actor.HP <= 0)
				continue;
			CallThreadSafe(nameof(GameManager.Instance.ClearBattleLog));
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
            CallThreadSafe(nameof(command.Run));
            await ToSignal(GetTree().CreateTimer(2f), "timeout");
		}

		EndTurn();
	}

	private void EndTurn()
	{
		if (Enemies.All(x => x.Actor.HP == 0))
		{
			CurrentParty.ForEach(x => x.SetState("victory"));
			GameManager.Instance.ClearAndMessageBattleLog(CurrentParty[0].Name + "'s party was victorious!");
			Phase = BattlePhase.BattleOver;
			return;
		}
		// TODO: handle game over
		StartTurn();
	}

}

public enum BattlePhase
{
	FightRun,
	PlayerCommand,
	TargetSelection,
	EnemyCommand,
	ExecuteTurn,
	BattleOver
}
