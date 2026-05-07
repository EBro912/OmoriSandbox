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
	public const string Version = "OmoriSandbox v1.0.1";
	
	[Export] private PackedScene BattlecardUI;
	[Export] private PackedScene EnemyNode;
	[Export] private BattlebackDisplayComponent BattlebackParent;
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
		BattlebackParent.Material = enabled ? GreyscaleMaterial : null;
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
		if (instance == null)
			return null;
		Node2D node = EnemyNode.Instantiate<Node2D>();
		BattlebackParent.AddChild(node);
		GD.Print("Spawning enemy at: " + position);
		node.GlobalPosition = position;
		EnemyComponent component = new();
		node.AddChild(component);
		node.ZIndex -= (int)enemy.Layer;
		component.SetEnemy(instance, enemy.Emotion, enemy.FallsOffScreen, enemy.GrayscaleOnDefeat, (int)enemy.Layer);
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
