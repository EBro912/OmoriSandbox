using Godot;
using System.Collections.Generic;

public partial class GameManager : Node
{
	[Export] public PackedScene BattlecardUI;
	[Export] public PackedScene EnemyUI;
	[Export] public Control UIParent;

	[Export] public Label[] BattleLog;

	private readonly Dictionary<string, PartyMember> ValidPartyMembers = [];
	private readonly Dictionary<string, Enemy> ValidEnemies = [];

	private string[] states = ["neutral", "victory", "toast", "happy", "ecstatic", "manic", "sad", "depressed", "miserable", "angry", "enraged", "furious", "afraid", "stressed"];

	public RandomNumberGenerator Random = new();

	public BattleManager BattleManager { get; private set; }

	public static GameManager Instance;

	public override void _Ready()
	{
		ValidPartyMembers.Add("Omori", new Omori());
		ValidPartyMembers.Add("Aubrey", new Aubrey());
		ValidPartyMembers.Add("Hero", new Hero());
		ValidPartyMembers.Add("Kel", new Kel());

		ValidEnemies.Add("LostSproutMole", new LostSproutMole());

		List<PartyMemberComponent> party = [];
		List<EnemyComponent> enemy = [];

		party.Add(SpawnPartyMember("Omori", 1));
		party.Add(SpawnPartyMember("Aubrey", 2));
		party.Add(SpawnPartyMember("Hero", 3));
		party.Add(SpawnPartyMember("Kel", 4));

		enemy.Add(SpawnEnemy("LostSproutMole", new Vector2(233, 240)));
		enemy.Add(SpawnEnemy("LostSproutMole", new Vector2(407, 240)));

		party.RemoveAll(x => x == null);

		Instance = this;

		BattleManager = new();
		AddChild(BattleManager);
		BattleManager.Init(party, enemy);
	}

	public void ClearAndMessageBattleLog(string message)
	{
		ClearBattleLog();
		MessageBattleLog(message);
	}

	public void MessageBattleLog(string message)
	{
		foreach (Label l in BattleLog)
		{
			if (string.IsNullOrWhiteSpace(l.Text))
			{
				l.Text = message;
				return;
			}
		}
		BattleLog[0].Text = BattleLog[1].Text;
		BattleLog[1].Text = BattleLog[2].Text;
		BattleLog[2].Text = message;
	}

	public void ClearBattleLog()
	{
		foreach (Label l in BattleLog)
		{
			l.Text = "";
		}
	}

	private EnemyComponent SpawnEnemy(string who, Vector2 position)
	{
		if (!ValidEnemies.TryGetValue(who, out Enemy enemy))
		{
			GD.PrintErr("Unknown enemy: " + who);
			return null;
		}

		Node2D node = EnemyUI.Instantiate<Node2D>();
		UIParent.AddChild(node);
		EnemyComponent component = new();
		node.AddChild(component);
		component.SetEnemy(enemy);
		node.Position = position;
		return component;
	}

	private PartyMemberComponent SpawnPartyMember(string who, int position, int level = 1)
	{
		if (!ValidPartyMembers.TryGetValue(who, out PartyMember member))
		{
			GD.PrintErr("Unknown party member: " + who);
			return null;
		}
		Control card = BattlecardUI.Instantiate<Control>();
		UIParent.AddChild(card);
		PartyMemberComponent component = new();
		card.AddChild(component);
		component.SetPartyMember(member, level: level);
		switch (position)
		{
			case 1:
				card.Position = new Vector2(20, 306);
				break;
			case 2:
				card.Position = new Vector2(20, 5);
				break;
			case 3:
				card.Position = new Vector2(506, 5);
				break;
			case 4:
				card.Position = new Vector2(506, 306);
				break;
		}
		return component;
	}
}
