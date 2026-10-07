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
	public const string Version = "OmoriSandbox v1.2 (dev build)";
	
	[Export] private PackedScene BattlecardUI;
	[Export] private PackedScene EnemyNode;
	[Export] private BattlebackDisplayComponent BattlebackParent;
	[Export] private Node2D BattlebackRoot;
	[Export] private Label FPSLabel;
	[Export] private Node Party;
	[Export] private ShaderMaterial GreyscaleMaterial;
	[Export] private PackedScene[] Followups;
	[Export] private Control[] WindowFrames;
	[Export] private Sprite2D[] WindowArrows;
	
	private const float GrayscaleFadeDuration = 0.5f;
	private readonly List<CanvasItem> GrayscaleSprites = [];
	private Tween GrayscaleFade;
	internal bool BattlebackLocked { get; set; }

	/// <summary>
	/// A random number generator.
	/// </summary>
	public RandomNumberGenerator Random { get; private set; } = new();
	internal DiscordManager DiscordManager { get; private set; }
	public static GameManager Instance { get; private set; }

	/// <summary>
	/// The part of the battle area currently visible on screen, in battle coordinates.
	/// </summary>
	/// <remarks>
	/// At 4:3 this is (0, 0, 640, 480). In widescreen it extends past both sides of the original 640x480 area.
	/// </remarks>
	public static Rect2 VisibleBattleRect => DisplayLayout.Instance?.VisibleBattleRect ?? new Rect2(0, 0, 640, 480);

	/// <summary>
	/// A node for mods to add battle visuals to. Anything under it stays aligned with the battle in widescreen.
	/// </summary>
	/// <remarks>
	/// Use <see cref="CanvasItem.ZIndex"/> to draw above or below the game's own layers.
	/// The game's main z-indices are:
	/// enemy-side animations -4, battle windows -1 to 2, energy bar 3, party animations 4, 6, 7 and 11, party portraits 9,
	/// damage numbers 10, follow-up bubbles 12 and 13, pictures 20.<br/>
	/// The game never clears this node automatically. Its children can only be freed by your mod.
	/// </remarks>
	public static Node2D ModLayer { get; private set; }

	/// <summary>
	/// Keeps your own <see cref="CanvasLayer"/> aligned with the battle in widescreen.
	/// </summary>
	/// <remarks>
	/// Call this once after creating the layer.
	/// It is only needed for your own <see cref="CanvasLayer"/>, not for nodes under <see cref="ModLayer"/> or other game nodes.
	/// </remarks>
	/// <param name="layer">The layer to keep aligned.</param>
	public static void RegisterCanvasLayer(CanvasLayer layer)
	{
		if (DisplayLayout.Instance == null)
		{
			GD.PushWarning("RegisterCanvasLayer was called before the game was ready, so the layer was not registered.");
			return;
		}
		DisplayLayout.Instance.RegisterLayer(layer);
	}
	
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
		ModLayer = new Node2D { Name = "ModLayer" };
		GetParent().CallDeferred(Node.MethodName.AddChild, ModLayer);
		
		GD.Print("Version: " + Version);

		DiscordManager = new();

		AnimationManager.Instance.Init();
		AudioManager.Instance.Init();
		ModManager.Instance.LoadMods();
		SettingsMenuManager.Instance.PopulateTitleMusic();
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
	/// The name of the battleback currently shown, or an empty string while the default battleback is shown.
	/// </summary>
	public string CurrentBattleback => BattlebackParent.CurrentName;

	/// <summary>
	/// Sets the battleback.
	/// </summary>
	/// <param name="name">The name of the battleback to set.</param>
	public void SetBattleback(string name)
	{
		if (BattlebackLocked)
		{
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print($"Ignored SetBattleback(\"{name}\"), the preset locks the battleback.");
			return;
		}
		ForceSetBattleback(name);
	}

	internal void ForceSetBattleback(string name)
	{
		BattlebackParent.SetBattleback(name);
	}

	/// <summary>
	/// Changes the skin of the battle windows: the battle log, the dialogue and choice boxes, and the skill and item menus.
	/// </summary>
	/// <remarks>
	/// Use <c>"Default"</c> for the game's own skin, or the ID of a skin from a mod (its file name in the mod's <c>windowskins</c> folder, or the ID passed to <c>Mod.RegisterWindowSkin</c>).
	/// The preset's skin comes back when the battle restarts or ends.
	/// </remarks>
	/// <param name="id">The ID of the window skin.</param>
	public void SetWindowSkin(string id)
	{
		WindowSkin skin = Database.GetWindowSkin(id);
		if (skin == null)
		{
			GD.PushWarning($"Unknown window skin \"{id}\"");
			return;
		}
		ApplyWindowSkin(skin);
	}

	internal void ApplyPresetWindowSkin(string id)
	{
		ApplyWindowSkin(Database.GetWindowSkin(Database.ResolveWindowSkinId(id)));
	}

	private void ApplyWindowSkin(WindowSkin skin)
	{
		foreach (Control frame in WindowFrames)
		{
			frame.Set(NinePatchRect.PropertyName.Texture, skin.Texture);
			frame.GetNode<ColorRect>("ColorRect").Visible = !skin.UseBackground;
			SetWindowBackground(frame, "SkinBackground", skin.Background, TextureRect.StretchModeEnum.Scale, skin.UseBackground);
			SetWindowBackground(frame, "SkinBackgroundTile", skin.BackgroundTile, TextureRect.StretchModeEnum.Tile, skin.UseBackground);
		}
		foreach (Sprite2D arrow in WindowArrows)
			arrow.Texture = skin.Texture;
	}

	private static void SetWindowBackground(Control frame, string name, Texture2D texture, TextureRect.StretchModeEnum stretch, bool visible)
	{
		TextureRect background = frame.GetNodeOrNull<TextureRect>(name);
		if (background == null)
		{
			if (!visible)
				return;
			Rect2 region = frame.Get(NinePatchRect.PropertyName.RegionRect).AsRect2();
			background = new TextureRect
			{
				Name = name,
				ZIndex = -1,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = stretch,
				TextureFilter = stretch == TextureRect.StretchModeEnum.Scale ? CanvasItem.TextureFilterEnum.Linear : CanvasItem.TextureFilterEnum.ParentNode,
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			frame.AddChild(background);
			background.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			background.OffsetLeft = 4;
			background.OffsetRight = -4;
			background.OffsetTop = Mathf.Max(0, 4 - region.Position.Y);
			background.OffsetBottom = -Mathf.Max(0, 4 - (96 - region.End.Y));
		}
		background.Texture = texture;
		background.Visible = visible;
	}

	/// <summary>
	/// Enables/disables a greyscale filter on the battleback and enemies.
	/// </summary>
	/// <param name="enabled">Whether the greyscale filter should be enabled.</param>
	public void SetBattlebackGrayscale(bool enabled)
	{
		GrayscaleFade?.Kill();
		if (enabled)
		{
			GrayscaleSprites.Clear();
			foreach (Node child in BattlebackRoot.GetChildren())
			{
				if (child.IsInGroup(EnemyNodeGroup))
					GrayscaleSprites.Add(child.GetNode<CanvasItem>("Sprite"));
			}
		}
		BattlebackRoot.Material = enabled ? GreyscaleMaterial : null;
		SetGrayscaleAmount(enabled ? 1f : 0f);
		if (!enabled)
			GrayscaleSprites.Clear();
	}

	internal void FadeOutBattlebackGrayscale()
	{
		if (BattlebackRoot.Material == null || (GrayscaleFade != null && GrayscaleFade.IsRunning()))
			return;
		GrayscaleFade = CreateTween();
		GrayscaleFade.TweenMethod(Callable.From<float>(SetGrayscaleAmount), 1f, 0f, GrayscaleFadeDuration);
		GrayscaleFade.TweenCallback(Callable.From(() =>
		{
			BattlebackRoot.Material = null;
			GrayscaleSprites.Clear();
		}));
	}

	private void SetGrayscaleAmount(float amount)
	{
		GreyscaleMaterial.SetShaderParameter("amount", amount);
		foreach (CanvasItem sprite in GrayscaleSprites)
		{
			if (IsInstanceValid(sprite))
				sprite.SetInstanceShaderParameter("defeat_tone", amount);
		}
	}

	internal bool LoadBattlePreset(BattlePreset preset, int startingStage)
	{
		preset.RuleOverrides ??= [];
		foreach (string key in preset.RuleOverrides.Keys.Where(key => SettingsMenuManager.Instance.RuleSettings.All(rule => rule.Key != key)))
			GD.PushWarning($"Preset {preset.Name} overrides an unknown rule \"{key}\", ignoring it");
		SettingsMenuManager.Instance.ActiveRuleOverrides = preset.RuleOverrides;
		if (SpawnBattle(preset, startingStage))
			return true;
		SettingsMenuManager.Instance.ActiveRuleOverrides = null;
		return false;
	}

	private bool SpawnBattle(BattlePreset preset, int startingStage)
	{
		List<PartyMemberComponent> party = [];
		List<EnemyComponent> enemy = [];

		if (preset.Type is GameModeType.BossRush)
		{
			if (preset.Stages.Count == 0)
			{
				GD.PrintErr($"Preset {preset.Name} has no Boss Rush stages, not starting the battle.");
				ShowCannotStartDialog("This Boss Rush preset has no stages.\nAdd a stage in the editor.");
				return false;
			}
			int badStage = preset.Stages.FindIndex(stage => !stage.Enemies.Any(x => Database.HasEnemy(x.Name)));
			if (badStage >= 0)
			{
				GD.PrintErr($"Preset {preset.Name}: Boss Rush stage {badStage + 1} has no enemies that could be spawned, not starting the battle.");
				ShowCannotStartDialog($"Boss Rush stage {badStage + 1} has no enemies that could be spawned.\nAdd an enemy to it in the editor, or check the console for errors.");
				return false;
			}
		}

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

		if (party.Count == 0)
		{
			GD.PrintErr($"Preset {preset.Name} has no party members that could be spawned, not starting the battle.");
			ShowCannotStartDialog("This preset has no party members that could be spawned.\nAdd a party member in the editor, or check the console for errors.");
			return false;
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

		if (preset.Type is GameModeType.Normal && enemy.Count == 0)
		{
			GD.PrintErr($"Preset {preset.Name} has no enemies that could be spawned, not starting the battle.");
			DespawnAll();
			ShowCannotStartDialog("This preset has no enemies that could be spawned.\nAdd an enemy in the editor, or check the console for errors.");
			return false;
		}

		DialogueManager.Instance.DialogueDisabled = preset.DisableDialogue;
		DiscordManager.SetBattling(enemy.Count);
		BattleManager.Instance.Init(party, enemy, preset.Stages, preset, startingStage);
		return true;
	}

	private void ShowCannotStartDialog(string text)
	{
		AcceptDialog dialog = new()
		{
			Title = "Cannot Start Battle",
			DialogText = text,
			Unresizable = true
		};
		AddChild(dialog);
		dialog.Confirmed += dialog.QueueFree;
		dialog.Canceled += dialog.QueueFree;
		dialog.PopupCentered();
		dialog.Show();
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
		if (instance.Animation == null)
		{
			GD.PrintErr("Failed to load Sprite animations for Enemy: " + instance.Name);
			return null;
		}
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
		card.ZIndex = 9;
		Party.AddChild(card);
		card.Position = actor.Position switch
		{
			0 => new Vector2(14, 305),
			1 => new Vector2(14, 5),
			2 => new Vector2(512, 305),
			3 => new Vector2(512, 5),
			_ => card.Position
		};
		DisplayLayout.Instance?.PlacePortrait(card, actor.Position);
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
