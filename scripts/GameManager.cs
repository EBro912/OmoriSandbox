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

	[Export] public PackedScene OmoriFollowup;
	[Export] public PackedScene AubreyFollowup;
	[Export] public PackedScene HeroFollowup;
	[Export] public PackedScene KelFollowup;

	private readonly Dictionary<string, Type> ValidPartyMembers = [];
	private readonly Dictionary<string, Type> ValidEnemies = [];

	public RandomNumberGenerator Random = new();
	public AnimationManager AnimationManager { get; private set; }

	public static GameManager Instance { get; private set; }

	public override void _PhysicsProcess(double delta)
	{
#if DEBUG
		FPSLabel.Text = $"{Engine.GetFramesPerSecond()} : {OS.GetStaticMemoryUsage() / 1000000}";
#else
		FPSLabel.Text = $"{Engine.GetFramesPerSecond()}";
#endif
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
		ValidEnemies.Add("SlimeGirls", typeof(SlimeGirls));
		ValidEnemies.Add("HumphreyUvula", typeof(HumphreyUvula));
		ValidEnemies.Add("AubreyEnemy", typeof(AubreyEnemy));

		List<PartyMemberComponent> party = [];
		List<EnemyComponent> enemy = [];

        // Omori, Aubrey, Hero, Kel
        // TODO: properly handle less than 4 party members
		party.Add(SpawnPartyMember("Omori", OmoriFollowup, 1, "Dull Knife", level: 30));
		party.Add(SpawnPartyMember("Aubrey", AubreyFollowup, 2, "Mailbox", level: 30));
		party.Add(SpawnPartyMember("Hero", HeroFollowup, 3, "Baking Pan", level: 30));
		party.Add(SpawnPartyMember("Kel", KelFollowup, 4, "Snowball", level: 30));

        enemy.Add(SpawnEnemy("Sweetheart", new Vector2(320, 275)));

        party.RemoveAll(x => x == null);

		Instance = this;

		AnimationManager = new();
		AddChild(AnimationManager);

		BattleManager.Instance.Init(party, enemy);
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

	private PartyMemberComponent SpawnPartyMember(string who, PackedScene followup, int position, string weapon, string charm = null, int level = 1, string startingEmotion = "neutral")
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
		component.SetPartyMember((PartyMember)handle, followup, position, startingEmotion, level, weapon, charm);
		return component;
	}


}
