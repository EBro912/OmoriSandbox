using Discord;
using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Editor;
using OmoriSandbox.Modding;
using System.Collections.Generic;
using System.Linq;

namespace OmoriSandbox;

/* TODO: Update 1.0
 Features:
 - Modify text speed - done
 - More Info / State Icons - in testing
 - Edit BGM loop point - done
 - Queue restart via keybind during battle - done
 - Text effects - done
 - Boss Alt Stats - in testing
 - Minibosses (Snaley, Shady Mole, etc.) - done
 - Skip dialogue with 'X' - done
 - Modifiable keybinds - done
 - Fullscreen option - done
 - Premade vanilla presets
 - Humphrey - in testing
 - Other Sunny skills - in progress
 - Update wiki
 - Add modded animation support - done
 - Console exclusive stats + mechanics - in testing
 - Add quit button - done
 - Allow damage to be overriden at various points of the calculation - done
 - basil release energy double use bonus - done
 - Other TODOs
 - custom boss rush - in testing
 - auto-generate default mod - done
 Bugfixes:
 - Tiered stat modifiers do not increment - done
 - Enemies can stack on top of each other preventing selection - in testing
 - Capitalize 'emotions' and 'heart' in perfectheart text - done
 - 'OMORI did not succumb' text is missing - done
 - Certain hit sounds do not play, seemingly at random
 - Mr Jawsum redirect damage should ignore juice - done
 - Mr Jawsum shouldn't be crit for 2 damage - done
 - Last Resort shouldn't hurt the user if it misses - done
 - Audio pitch/volume still doesn't get reset after being modified - in testing
 - Boss Hero/Kel/Aubrey shouldn't be able to call themselves - done
 - Pluto (expanded)'s headbutt has bugged text - done
 - Backing out of selecting a target for an item deletes the item - done
 - Bossman Hero's enemy buff should remove all debuffs first - done
 - Basil followups use incorrect targeting - done
 - Using certain enemy skills on party members breaks the game
 - Given key was not present in the dictionary BattleManager.cs:756 - done
 - Poetry book has no animation - done
 - Perfectheart exploit breaks with plot armor - fixed?
 - Fix menu wrapping - done
 - Fix Skill/Snack/Toy menu back going back to menu it came from - done
 - Make skill/toy menu appear on top of battle menu - done
 - Fix followups (party and bosses) with less than 4 party members - done (needs testing for bosses)
 */

 /* Playtesting changes:
 - Make Alt. boss names more descriptive - done
 - Fix duplicate skill entries - done
 - Investigate a fix for certain textures like the energy bar 0 - done i think
 - Fix font size in skill descriptions, title 28 description 20/22 - done
 - Hide infoboxes on quick restart - done
 - Reword infinite buffs/debuffs tooltip - done
 - Permanent stat upgrades (TBD) - in testing
 - Apply attack/skill/snack/toy menu pathing to other menus - potentially done
 - Confirmation for running/quick restart - done
 - Import preset as battle stage
 - King Carnivore and Boss rush humphrey
 - Spawning enemies in the editor initalizes them, this breaks things like HumphreyFace
 - Have a toggleable list of weapons to only show weapons equipable by that actor - done
 - Aubrey's speed stats are wrong, check others - potentially done
 - Enemies that don't fall off the screen in boss rush persist over stages - done
 - Skills can be selected during targeting - done
 - State icons persist over boss rush phases - potentially done
 - Humphrey transitions break if he changes phase at the start of the turn
 - Loading a regular preset after loading a boss rush preset messes up the battleback - done
 - If another damage number spawns on top of another, shift existing ones up - done
 - Revert text sizing - done
 - Only X works to skip text, Z does nothing - done
 - Can't skip text after an input pause - done
 - Follow-up bubbles are inaccurate: the selected follow-up bubble should stay on screen until the basic attack finishes - done
 - PH's Exploit has wrong battletext: it says "EMOTION" while it should say "EMOTIONS" - done
 - State icons don't show for enemies - done
 - Omori's basic attack has a noticeable delay between the animation finishing and the skill dealing damage
 - Release Energy's animation should fade in, currently it cuts with no fade - done
 - Twirl has no battletext (should be "<user> attacks <target>!") - done
 - Encore should not work with afraid - done
 - Revisit font sizing and outlining throughout game
 - Add a method to play animations at specific coordinates - done
 - Backgrounds exactly the size of the screen repeat on the edges - not actually an issue?
 - Aubrey's Counter Attack uses wrong battletext; should be "AUBREY swings back!" - done
 - Boss Rush Pluto Expanded doesn't use his Expand Further skill at 50% HP (I haven't tested the normal version)
 - Important note about force actions:
	 - Whenever an enemy is forced to use a skill by an event and they have NOT acted yet, the enemy will use the forced skill and then immediately act from their normal AI.
	 - Example:
	   - Slime Girls HP is below 75%
	   - Slime Girls are forced to use "SELF ANGRY" by a troop event
	   - Slime Girls will use a skill from their normal AI, regardless of their speed
	 - Note that follow-ups can interrupt this and attack between the enemy's actions:
	   - Omori attacks Slime Girls and activates Trip follow-up. The basic attack brings them below 75% HP.
	   - Slime Girls are forced to use "SELF ANGRY" by a troop event
	   - Omori's Trip skill activates
	   - Slime Girls will use a skill from their normal AI, regardless of their speed
 - Flex should ignore certain hit - done
 - Flex shouldn't be removed if the attack does 0 damage - done
 - Make Encourage only target the leader - done
 - Reset screen tint on battle start - done
 - Add flag to allow modded animations to loop
 - Add layer selector to animation viewer - done
 - Fix certain broken animations - done?
 - Add thematic dialogue (Sweetheart donut, Pluto flex)
 - Improve what is considered a basic attack/followup - done
 - Extract battlebacks into a battleback manager and potentially support animated ones - done
 - Make weapons use StatBonus - done
 - Make followup arrows transparent if the target is invalid - done
 - Adding a non-hidden skill as the basic attack skill breaks the skill menu - fixed?
 - Revamp credits screen - done
 - Grey tint on victory
 - 10x damage on holding shift - done
 */

/// <summary>
/// The main Game Manager.
/// </summary>
public partial class GameManager : Node
{
	public const string Version = "OmoriSandbox v1.0.0 (dev build)";
	
	[Export] private PackedScene BattlecardUI;
	[Export] private PackedScene EnemyNode;
	[Export] private BattlebackDisplayComponent BattlebackParent;
	[Export] private Label FPSLabel;
	[Export] private Node Party;

	[Export] private PackedScene[] Followups;

	/// <summary>
	/// A random number generator.
	/// </summary>
	public RandomNumberGenerator Random = new();
	internal DiscordManager DiscordManager { get; private set; }
	public static GameManager Instance { get; private set; }

	public override void _PhysicsProcess(double delta)
	{
		FPSLabel.Text = $"{(SettingsMenuManager.Instance.ShowFPS ? Engine.GetFramesPerSecond() : "")} {Version}";

		DiscordManager.Tick();
	}

	public override void _Ready()
	{
		Instance = this;
		
		GD.Print("Version: " + Version);

		DiscordManager = new();

		AnimationManager.Instance.Init();
		AudioManager.Instance.Init();
		ModManager.Instance.LoadMods();
		MainMenuManager.Instance.Init();
		EditorManager.Instance.Init();
	}

	public override void _ExitTree()
	{
		DiscordManager.Shutdown();
	}

	/// <summary>
	/// Sets the battleback.
	/// </summary>
	/// <param name="name">The name of the battleback to set.</param>
	public void SetBattleback(string name)
	{
		BattlebackParent.SetBattleback(name);
	}

	internal void LoadBattlePreset(BattlePreset preset)
	{
		List<PartyMemberComponent> party = [];
		List<EnemyComponent> enemy = [];

		string battleback = preset.Type is GameModeType.Normal ? preset.Battleback : preset.Stages[0].Battleback;
		string bgm = preset.Type is GameModeType.Normal ? preset.BGM : preset.Stages[0].BGM;
		double pitch = preset.Type is GameModeType.Normal ? preset.BGMPitch : preset.Stages[0].BGMPitch;
		double loopPoint = preset.Type is GameModeType.Normal ? preset.BGMLoopPoint : preset.Stages[0].BGMLoopPoint;
		
		SetBattleback(battleback);
		
		AudioManager.Instance.PlayBGM(bgm, 1f, (float)pitch);
		AudioManager.Instance.SetBGMLoopOffset(loopPoint);

		foreach (BattlePresetActor entry in preset.Actors)
		{
			if (party.Count >= 4)
			{
				GD.PushWarning("Party is full, skipping extra actor");
				continue;
			}

			PackedScene followup = null;
			if (!entry.FollowupsDisabled)
			{
				if (preset.BasilFollowups && entry.Position == 2)
					followup = Followups[4];
				else
					followup = Followups[entry.Position];
			}

			PartyMemberComponent actor = SpawnPartyMember(followup, entry);

			if (actor == null)
			{
				GD.PrintErr("Failed to spawn party member: " + entry.Name);
				continue;
			}

			party.Add(actor);
		}

		foreach (BattlePresetEnemy entry in preset.Enemies)
		{
			if (!entry.Position.StartsWith("Vector2"))
				entry.Position = "Vector2" + entry.Position;
			Vector2 position = GD.StrToVar(entry.Position).AsVector2();
			while (enemy.Any(x => x.Actor.CenterPoint == position))
			{
				// prevent stacking
				position += new Vector2(0.01f, 0f);
			}
			EnemyComponent en = SpawnEnemy(entry, position);
			if (en == null)
			{
				GD.PrintErr("Failed to spawn enemy: " + entry.Name);
				continue;
			}

			enemy.Add(en);
		}

		DialogueManager.Instance.DialogueDisabled = preset.DisableDialogue;
		DiscordManager.SetBattling(enemy.Count);
		BattleManager.Instance.Init(party, enemy, preset.Stages, preset);
	}

	internal void DespawnAll()
	{
		foreach (Node child in Party.GetChildren())
		{
			child.QueueFree();
		}
		
		DespawnEnemies();
	}

	internal void DespawnEnemies()
	{
		// skip the first child as the first child is the FullscreenEffects
		foreach (Node child in BattlebackParent.GetChildren().Skip(1))
		{
			child.QueueFree();
		}
	}

	internal EnemyComponent SpawnEnemy(BattlePresetEnemy enemy, Vector2 position)
	{
		Enemy instance = Database.CreateEnemy(enemy.Name);
		Node2D node = EnemyNode.Instantiate<Node2D>();
		BattlebackParent.AddChild(node);
		GD.Print("Spawning enemy at: " + enemy.Position);
		node.GlobalPosition = position;
		EnemyComponent component = new();
		node.AddChild(component);
		node.ZIndex -= (int)enemy.Layer;
		component.SetEnemy(instance, enemy.Emotion, enemy.FallsOffScreen, (int)enemy.Layer);
		return component;
	}

	private PartyMemberComponent SpawnPartyMember(PackedScene followup, BattlePresetActor actor)
	{
		PartyMember instance = Database.CreatePartyMember(actor.Name);
		if (instance == null)
			return null;
		Control card = BattlecardUI.Instantiate<Control>();
		Party.AddChild(card);
		card.Position = actor.Position switch
		{
			0 => new Vector2(14, 305),
			1 => new Vector2(14, 5),
			2 => new Vector2(512, 305),
			3 => new Vector2(512, 5),
			_ => card.Position
		};
		PartyMemberComponent component = new();
		card.AddChild(component);
		component.SetPartyMember(instance, followup, actor);
		return component;
	}
}
