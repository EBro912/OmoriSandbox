using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
	[Export] public PackedScene BattlecardUI;
	[Export] public PackedScene EnemyUI;
	[Export] public Control UIParent;
	[Export] public TextureRect BattlebackParent;
	[Export] public Label FPSLabel;

	[Export] public PackedScene[] Followups;

	private readonly Dictionary<string, Type> ValidPartyMembers = [];
	private readonly Dictionary<string, Type> ValidEnemies = [];

	public RandomNumberGenerator Random = new();
	public AnimationManager AnimationManager { get; private set; }

	public string CustomDataPath = "user://custom/";

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

		// these can (potentially) be file-driven instead of class based at some point

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

		Instance = this;

		AnimationManager = new();
		AddChild(AnimationManager);

		AudioManager.Instance.Init();

		// Omori, Aubrey, Hero, Kel
		// TODO: properly handle less than 4 party members
		LoadBattleConfig();
	}

	// TODO: replace with a GUI-based config system
	private void LoadBattleConfig()
	{
		List<PartyMemberComponent> party = [];
		List<EnemyComponent> enemy = [];
		Dictionary<string, int> items = [];
		int FollowupTier = -1;

		ConfigFile config = new();

		if (!FileAccess.FileExists("user://config.ini"))
		{
			// copy default config if it doesn't exist
			GD.PushWarning("Config file does not exist. Creating a new one using default settings...");
			string content = FileAccess.GetFileAsString("res://assets/default_config.ini");
			using var dest = FileAccess.Open("user://config.ini", FileAccess.ModeFlags.Write);
			dest.StoreString(content);
			dest.Close();
		}

		Error err = config.Load("user://config.ini");
		if (err != Error.Ok)
		{
			GD.PrintErr("Failed to load config: " + err);
			return;
		}

		foreach (string s in config.GetSections())
		{
			string section = s.ToLower();
			if (section == "general")
			{
				CustomDataPath = (string)config.GetValue(s, "custom_path");
				AudioManager.Instance.PlayBGM((string)config.GetValue(s, "bgm"));
				string battleback = (string)config.GetValue(s, "battleback");
				if (ResourceLoader.Exists("res://assets/battlebacks/" + battleback + ".png"))
					BattlebackParent.Texture = ResourceLoader.Load<Texture2D>("res://assets/battlebacks/" + battleback + ".png");
				else if (FileAccess.FileExists(CustomDataPath + "/battlebacks/" + battleback + ".png"))
					BattlebackParent.Texture = ImageTexture.CreateFromImage(Image.LoadFromFile(CustomDataPath + "/battlebacks/" + battleback + ".png"));
				else
					GD.PrintErr("No valid battleback found: " + battleback);
				foreach (var kv in (Godot.Collections.Dictionary<string, int>)config.GetValue(s, "snacks"))
					items.Add(kv.Key, kv.Value);
				foreach (var kv in (Godot.Collections.Dictionary<string, int>)config.GetValue(s, "toys"))
					items.Add(kv.Key, kv.Value);
				FollowupTier = (int)config.GetValue(s, "followup_tier");
            }
			else if (section.StartsWith("actor"))
			{
				if (party.Count >= 4)
				{
					GD.PushWarning("Party is full, skipping extra [actor] entry.");
					continue;
				}
				PartyMemberComponent add = SpawnPartyMember(
						(string)config.GetValue(s, "name"),
						Followups[(int)config.GetValue(s, "position") - 1],
						(int)config.GetValue(s, "position"),
						(string)config.GetValue(s, "weapon"),
						(string)config.GetValue(s, "charm"),
						(string[])config.GetValue(s, "skills"),
						(int)config.GetValue(s, "level"),
						(string)config.GetValue(s, "emotion")
					);

				if (add == null)
				{
					GD.PrintErr("Failed to load a party member, please check the config file.");
					continue;
				}
				party.Add(add);
				GD.Print("Loaded actor: " + add.Actor.Name);
			}
			else if (section.StartsWith("enemy"))
			{
				EnemyComponent add = SpawnEnemy(
						(string)config.GetValue(s, "name"),
						(Vector2)config.GetValue(s, "position"),
						(string)config.GetValue(s, "emotion")
					);
				if (add == null)
				{
					GD.PrintErr("Failed to load an enemy, please check the config file.");
					continue;
				}
				enemy.Add(add);
				GD.Print("Loaded enemy: " + add.Actor.Name);
			}
		}

		if (FollowupTier == -1)
		{
			GD.PushWarning("Followup Tier not set in config, defaulting to 1.");
			FollowupTier = 1;
        }
		else
		{
			GD.Print("Using Followup Tier " + FollowupTier);
        }

		BattleManager.Instance.Init(party, enemy, items, FollowupTier);
	}

	private EnemyComponent SpawnEnemy(string who, Vector2 position, string startingEmotion = "neutral")
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
		component.SetEnemy((Enemy)handle, startingEmotion);
		return component;
	}

	private PartyMemberComponent SpawnPartyMember(string who, PackedScene followup, int position, string weapon, string charm, string[] skills, int level = 1, string startingEmotion = "neutral")
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
		component.SetPartyMember((PartyMember)handle, followup, position, startingEmotion, level, weapon, charm, skills);
		return component;
	}


}
