using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
	[Export] public PackedScene BattlecardUI;
	[Export] public PackedScene EnemyUI;
	[Export] public Control UIParent;
	[Export] public Control BattlebackParent;
	[Export] public Label FPSLabel;

	private readonly Dictionary<string, Type> ValidPartyMembers = [];
	private readonly Dictionary<string, Type> ValidEnemies = [];

	public RandomNumberGenerator Random = new();

	public BattleManager BattleManager { get; private set; }
	public AnimationManager AnimationManager { get; private set; }

	public static GameManager Instance { get; private set; }

	public override void _PhysicsProcess(double delta)
	{
		FPSLabel.Text = Engine.GetFramesPerSecond().ToString();
	}

	public override void _Ready()
	{
		Database.Init();

		ValidPartyMembers.Add("Omori", typeof(Omori));
		ValidPartyMembers.Add("Aubrey", typeof(Aubrey));
		ValidPartyMembers.Add("Hero", typeof(Hero));
		ValidPartyMembers.Add("Kel", typeof(Kel));
		ValidPartyMembers.Add("Tony", typeof(Tony));
		ValidPartyMembers.Add("AubreyRW", typeof(AubreyRW));
		ValidPartyMembers.Add("KelRW", typeof(KelRW));
		ValidPartyMembers.Add("HeroRW", typeof(HeroRW));
		ValidPartyMembers.Add("Sunny", typeof(Sunny));

		ValidEnemies.Add("LostSproutMole", typeof(LostSproutMole));
		ValidEnemies.Add("ForestBunny?", typeof(ForestBunnyQuestion));
		ValidEnemies.Add("Sweetheart", typeof(Sweetheart));

		List<PartyMemberComponent> party = [];
		List<EnemyComponent> enemy = [];

		// Omori, Aubrey, Hero, Kel
		// TODO: properly handle less than 4 party members
		party.Add(SpawnPartyMember("Omori", 1, 20));
		party.Add(SpawnPartyMember("Aubrey", 2, 20));
		party.Add(SpawnPartyMember("Hero", 3, 20));
		party.Add(SpawnPartyMember("Tony", 4, 20));

		enemy.Add(SpawnEnemy("Sweetheart", new Vector2(320, 270)));

		party.RemoveAll(x => x == null);

		Instance = this;

		AnimationManager = new();
		AddChild(AnimationManager);
		BattleManager = new();
		AddChild(BattleManager);
		BattleManager.Init(party, enemy);
	}

	private EnemyComponent SpawnEnemy(string who, Vector2 position)
	{
		if (!ValidEnemies.TryGetValue(who, out Type enemy))
		{
			GD.PrintErr("Unknown enemy: " + who);
			return null;
		}

		object handle = Activator.CreateInstance(enemy);
		Node2D node = EnemyUI.Instantiate<Node2D>();
		BattlebackParent.AddChild(node);
		node.GlobalPosition = position;
		EnemyComponent component = new();
		node.AddChild(component);
		component.SetEnemy((Enemy)handle);
		return component;
	}

	private PartyMemberComponent SpawnPartyMember(string who, int position, int level = 1)
	{
		if (!ValidPartyMembers.TryGetValue(who, out Type member))
		{
			GD.PrintErr("Unknown party member: " + who);
			return null;
		}
		object handle = Activator.CreateInstance(member);
		Control card = BattlecardUI.Instantiate<Control>();
		UIParent.AddChild(card);
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
		PartyMemberComponent component = new();
		card.AddChild(component);
		component.SetPartyMember((PartyMember)handle, level: level);
		return component;
	}


}
