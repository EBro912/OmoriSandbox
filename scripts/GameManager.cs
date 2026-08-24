using Discord;
using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Editor;
using OmoriSandbox.Modding;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmoriSandbox;

/// <summary>
/// The main Game Manager.
/// </summary>
public partial class GameManager : Node
{
	/// <summary>
	/// The current version of OmoriSandbox.
	/// </summary>
	public const string Version = "OmoriSandbox v1.1.2";
	
	[Export] private PackedScene BattlecardUI;
	[Export] private PackedScene EnemyNode;
	[Export] private BattlebackDisplayComponent BattlebackParent;
	[Export] private Node2D BattlebackRoot;
	[Export] private Label FPSLabel;
	[Export] private Node Party;
	[Export] private Material GreyscaleMaterial;

	[Export] private PackedScene[] Followups;

	/// <summary>
	/// A random number generator.
	/// </summary>
	public RandomNumberGenerator Random { get; private set; } = new();
	internal DiscordManager DiscordManager { get; private set; }
	public static GameManager Instance { get; private set; }
	
	private double DisplayedFPS = double.MinValue;

	public override void _PhysicsProcess(double delta)
	{
		// only rebuild the label text when the displayed value actually changes
		double fps = SettingsMenuManager.Instance.ShowFPS ? Engine.GetFramesPerSecond() : -1;
		// possible loss of precision here is fine because we only care about whole number changes
		// ReSharper disable once CompareOfFloatsByEqualityOperator
		if (fps != DisplayedFPS)
		{
			DisplayedFPS = fps;
			FPSLabel.Text = fps >= 0 ? $"{fps} {Version}" : Version;
		}

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
		// the dropdowns are populated by EditorManager.Init, restore the remembered selection after
		MainMenuManager.Instance.RestoreLastSelectedPreset();
	}

	public override void _ExitTree()
	{
		DiscordManager.Shutdown();
	}

	/// <summary>
	/// Creates a timer that waits for the given number of seconds.
	/// </summary>
	/// <remarks>
	/// The <see cref="OmoriSandbox.Wait"/> class can be used as a shorthand of this method.
	/// </remarks>
	/// <param name="seconds">The number of seconds to wait for.</param>
	public async Task Wait(float seconds)
	{
		await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
	}

	/// <summary>
	/// Sets the battleback.
	/// </summary>
	/// <param name="name">The name of the battleback to set.</param>
	public void SetBattleback(string name)
	{
		BattlebackParent.SetBattleback(name);
	}

	/// <summary>
	/// Enables/disables a greyscale filter on the battleback and enemies.
	/// </summary>
	/// <param name="enabled">Whether the greyscale filter should be enabled.</param>
	public void SetBattlebackGrayscale(bool enabled)
	{
		BattlebackRoot.Material = enabled ? GreyscaleMaterial : null;
	}

	internal void LoadBattlePreset(BattlePreset preset, int startingStage)
	{
		List<PartyMemberComponent> party = [];
		List<EnemyComponent> enemy = [];

		foreach (BattlePresetActor entry in preset.Actors)
		{
			if (party.Count >= 4)
			{
				GD.PushWarning("Party is full, skipping extra actor");
				continue;
			}

			if (entry.Position is < 0 or > 3)
			{
				GD.PrintErr($"Invalid position {entry.Position} for party member {entry.Name}, skipping!");
				continue;
			}

			FollowupSet set = FollowupSets.Get(FollowupSets.ResolveId(preset, entry));
			if (set != null)
				FollowupSets.WarnMissingSkills(set, preset);

			PartyMemberComponent actor = SpawnPartyMember(set, entry);

			if (actor == null)
			{
				GD.PrintErr("Failed to spawn party member: " + entry.Name);
				continue;
			}

			party.Add(actor);
		}

		foreach (BattlePresetEnemy entry in preset.Enemies)
		{
			if (string.IsNullOrWhiteSpace(entry.Position))
			{
				GD.PrintErr($"Missing position for enemy {entry.Name}, skipping!");
				continue;
			}
			if (!entry.Position.StartsWith("Vector2"))
				entry.Position = "Vector2" + entry.Position;
			Variant parsedPosition = GD.StrToVar(entry.Position);
			if (parsedPosition.VariantType != Variant.Type.Vector2)
			{
				GD.PrintErr($"Invalid position \"{entry.Position}\" for enemy {entry.Name}, skipping!");
				continue;
			}
			Vector2 position = parsedPosition.AsVector2();
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
		BattleManager.Instance.Init(party, enemy, preset.Stages, preset, startingStage);
	}

	internal void DespawnAll()
	{
		foreach (Node child in Party.GetChildren())
		{
			child.QueueFree();
		}
		
		DespawnEnemies();
	}

	// add enemy nodes to a group to make keeping track of them easier
	// they are mixed in with other non-enemy nodes
	private const string EnemyNodeGroup = "battle_enemies";

	internal void DespawnEnemies()
	{
		foreach (Node child in BattlebackRoot.GetChildren())
		{
			if (child.IsInGroup(EnemyNodeGroup))
				child.QueueFree();
		}
	}

	internal EnemyComponent SpawnEnemy(BattlePresetEnemy enemy, Vector2 position)
	{
		Enemy instance = Database.CreateEnemy(enemy.Name);
		if (instance == null)
			return null;
		Node2D node = EnemyNode.Instantiate<Node2D>();
		node.AddToGroup(EnemyNodeGroup);
		BattlebackRoot.AddChild(node);
		if (SettingsMenuManager.Instance.LogDebug)
			GD.Print("Spawning enemy at: " + position);
		node.GlobalPosition = position;
		EnemyComponent component = new();
		node.AddChild(component);
		node.ZIndex -= (int)enemy.Layer;
		component.SetEnemy(instance, enemy.Emotion, enemy.FallsOffScreen, enemy.GrayscaleOnDefeat, (int)enemy.Layer, enemy.AdjustedStats);
		return component;
	}

	internal PartyMemberComponent SpawnPartyMember(FollowupSet set, BattlePresetActor actor)
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
		// the slot provides the bubble layout, the set only provides graphics and skills
		PackedScene followup = set == null ? null : Followups[actor.Position];
		if (!component.SetPartyMember(instance, followup, set, actor))
		{
			card.QueueFree();
			return null;
		}
		return component;
	}
}
