using System;
using Godot;

namespace OmoriSandbox.Editor;

/// <summary>
/// Handles the user's settings and keybinds.
/// </summary>
public partial class SettingsMenuManager : Control
{
	public override void _Ready()
	{
		ConfigFile config = new();
		if (config.Load("user://settings.cfg") != Error.Ok)
		{
			GD.PushWarning("Generating new settings file...");
			GenerateDefaultConfig(ref config);
		}

		FullscreenCheckbox.Toggled += value =>
		{
			DisplayServer.WindowSetMode(value ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
		};

		AspectDropdown.ItemSelected += index =>
		{
			DisplayLayout layout = DisplayLayout.Instance;
			if (layout == null)
				return;
			int width = AspectWidths[index];
			int scale = layout.Scale;
			layout.SetCanvasWidth(width);
			// when windowed, the window snaps to the chosen canvas at its current scale, so nothing is letterboxed
			if (width > 0 && IsWindowed())
				ApplyWindowSize(CanvasWindowSize(width, scale));
		};

		PortraitsDropdown.ItemSelected += index =>
		{
			DisplayLayout.Instance?.SetEdgePortraits(index == 1);
			if (index == 1)
				ShowNote("Warning", "Some animations may look odd or broken when using edge portraits.");
		};

		PixelSnappingCheckbox.Toggled += value =>
		{
			ApplyPixelSnapping(value);
		};

		DisplayLayout.Instance.WindowResized += OnWindowSizeChanged;
		
		MasterSlider.ValueChanged += value =>
		{
			SetBusVolume("Master", (float)value);
		};
		
		BGMSlider.ValueChanged += value =>
		{
			SetBusVolume("BGM", (float)value);
		};

		SFXSlider.ValueChanged += value =>
		{
			SetBusVolume("SFX",  (float)value);
		};

		TestSFXButton.Pressed += () =>
		{
			AudioManager.Instance.PlaySFX("BA_basic_attack_omori");
		};

		RestartHoldTimeSlider.ValueChanged += value =>
		{
			RestartHoldTimeLabel.Text = $"{value:0.00}";
		};

		SpeedUpSlider.ValueChanged += value =>
		{
			SpeedUpLabel.Text = $"{value:0.00}x";
		};

		DialogueSpeedSlider.ValueChanged += value =>
		{
			DialogueSpeedLabel.Text = value >= DialogueSpeedSlider.MaxValue ? "Instant" : $"{value:0.00}x";
		};

		SelectionChangeSpeedSlider.ValueChanged += value =>
		{
			SelectionChangeSpeedLabel.Text = $"{value:0.00}";
		};

		SelectionHoldTimeSlider.ValueChanged += value =>
		{
			SelectionHoldTimeLabel.Text = $"{value:0.00}";
		};

		ResetKeybindsButton.Pressed += () =>
		{
			foreach (Node node in KeybindGrid.GetChildren())
			{
				if (node is KeybindButton keybind)
				{
					keybind.Reset();
				}
			}
		};

		BackButton.Pressed += Close;
		
		// calling these after subscribing to the above events
		int canvasWidth = (int)config.GetValue("Settings", "CanvasWidth", 0);
		AspectDropdown.Selected = Math.Max(0, Array.IndexOf(AspectWidths, canvasWidth));
		DisplayLayout.Instance?.SetCanvasWidth(AspectWidths[AspectDropdown.Selected]);
		WindowedSize = new((int)config.GetValue("Settings", "WindowWidth", 640), (int)config.GetValue("Settings", "WindowHeight", 480));
		if (WindowedSize != GetWindow().Size)
			ApplyWindowSize(WindowedSize);
		PortraitsDropdown.Selected = (bool)config.GetValue("Settings", "EdgePortraits", false) ? 1 : 0;
		DisplayLayout.Instance?.SetEdgePortraits(PortraitsDropdown.Selected == 1);
		FullscreenCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "Fullscreen", false);
		PixelSnappingCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "PixelSnapping", false);
		ApplyPixelSnapping(PixelSnappingCheckbox.ButtonPressed);
		MasterSlider.Value = (float)config.GetValue("Settings", "MasterVolume", 0.75f);
		SFXSlider.Value = (float)config.GetValue("Settings", "SFXVolume", 1f);
		BGMSlider.Value = (float)config.GetValue("Settings", "BGMVolume", 0.5f);
		ShowFPSCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "ShowFPS", true);
		LogDebugCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "LogDebugMessages", false);
		PreventRunCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "PreventAccidentalRun", false);
		DisableDamageLimitCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "DisableDamageLimit", false);
		DisableStatLimitCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "DisableStatLimit", false);
		ShowMoreInfoCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "ShowMoreInfo", false);
		ShowStateIconsCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "ShowStateIcons", false);
		EnemySelectionWrappingCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "EnemySelectionWrapping", false);
		SelectionHoldTimeSlider.Value = (double)config.GetValue("Settings", "SelectionHoldTime", 0.5d);
		SelectionChangeSpeedSlider.Value = (double)config.GetValue("Settings", "SelectionChangeSpeed", 0.1d);
		UseConsoleSpdCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "UseConsoleSpd", false);
		UseConsoleDefCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "UseConsoleDef", false);
		InfiniteBuffsDebuffsCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "InfiniteBuffsDebuffs", false);
		VertigoUsesAtkCheckbox.ButtonPressed =  (bool)config.GetValue("Settings", "VertigoUsesAtk", false);
		ToysUseEmotionDamageCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "ToysUseEmotionDamage", false);
		SpaceExHusbandReleaseEnergyCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "SpaceExHusbandReleaseEnergy", false);
		EnableDebugDamageCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "EnableDebugDamage", false);
		CombinedAccuracyCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "CombinedAccuracy", false);
		AllowStateReapplyOnExpiryCheckbox.ButtonPressed = (bool)config.GetValue("Settings", "AllowStateReapplyOnExpiry", false);
		BattlelogSpeedSlider.Value = (int)config.GetValue("Settings", "BattlelogSpeed", 3);
		ActionDelaySlider.Value = (int)config.GetValue("Settings", "ActionDelay", 3);
		DialogueSpeedSlider.Value = (double)config.GetValue("Settings", "DialogueSpeed", 1d);

		LastSelectedPreset = (string)config.GetValue("Settings", "LastSelectedPreset", "");

		RestartHoldTimeSlider.Value = (double)config.GetValue("Keybinds", "RestartHoldTime", 1d);
		SpeedUpSlider.Value = (double)config.GetValue("Keybinds", "SpeedUpMultiplier", 1.5d);
		foreach (Node node in KeybindGrid.GetChildren())
		{
			if (node is KeybindButton keybind)
			{
				Key key = OS.FindKeycodeFromString((string)config.GetValue("Keybinds", keybind.AssociatedAction,
					"unknown"));
				if (key is Key.Unknown)
					keybind.Reset();
				else
					keybind.SetKey(key);
				if (config.HasSectionKey("ControllerBinds", keybind.AssociatedAction))
					keybind.LoadControllerBinding(config.GetValue("ControllerBinds", keybind.AssociatedAction));
			}
		}

		// persist settings whenever the menu is closed so a crash doesn't lose them
		VisibilityChanged += () =>
		{
			if (!Visible)
				SaveSettings();
			else
				FullscreenCheckbox.GrabFocus(hideFocus: !MainMenuManager.Instance.UsingController);
		};

		Instance = this;
	}

	/// <summary>Closes settings and returns to the title controls.</summary>
	public void Close()
	{
		MainControls.Visible = true;
		Logo.Visible = true;
		OmoriFace.Visible = true;
		Visible = false;
	}

	public override void _Process(double delta)
	{
		// a key being captured for a rebind must not also trigger its old/new action
		if (Input.IsActionJustPressed("ToggleFullscreen") && !KeybindButton.IsCapturing)
		{
			FullscreenCheckbox.ButtonPressed = !FullscreenCheckbox.ButtonPressed;
		}
	}

	public override void _ExitTree()
	{
		if (DisplayLayout.Instance != null)
			DisplayLayout.Instance.WindowResized -= OnWindowSizeChanged;
		SaveSettings();
	}

	// fullscreen and maximized sizes belong to the screen; only windowed sizes are remembered
	private void OnWindowSizeChanged()
	{
		if (IsWindowed())
			WindowedSize = GetWindow().Size;
	}

	private bool IsWindowed()
	{
		return GetWindow().Mode is not (Window.ModeEnum.Fullscreen or Window.ModeEnum.ExclusiveFullscreen or Window.ModeEnum.Maximized);
	}

	private void ApplyPixelSnapping(bool enabled)
	{
		GetTree().Root.Snap2DTransformsToPixel = enabled;
		void Redraw(Node node)
		{
			if (node is Viewport viewport)
				viewport.Snap2DTransformsToPixel = enabled;
			if (node is CanvasItem item)
				item.QueueRedraw();
			foreach (Node child in node.GetChildren())
				Redraw(child);
		}
		Redraw(GetTree().Root);
	}

	/// <summary>
	/// Saves the current settings and keybinds to disk.
	/// </summary>
	public void SaveSettings()
	{
		ConfigFile config = new();
		config.SetValue("Settings", "Fullscreen", FullscreenCheckbox.ButtonPressed);
		config.SetValue("Settings", "WindowWidth", WindowedSize.X);
		config.SetValue("Settings", "WindowHeight", WindowedSize.Y);
		config.SetValue("Settings", "CanvasWidth", AspectWidths[AspectDropdown.Selected]);
		config.SetValue("Settings", "EdgePortraits", PortraitsDropdown.Selected == 1);
		config.SetValue("Settings", "PixelSnapping", PixelSnappingCheckbox.ButtonPressed);
		config.SetValue("Settings", "MasterVolume", AudioServer.GetBusVolumeLinear(AudioServer.GetBusIndex("Master")));
		config.SetValue("Settings", "BGMVolume", AudioServer.GetBusVolumeLinear(AudioServer.GetBusIndex("BGM")));
		config.SetValue("Settings", "SFXVolume", AudioServer.GetBusVolumeLinear(AudioServer.GetBusIndex("SFX")));
		config.SetValue("Settings", "BattlelogSpeed", (int)BattlelogSpeedSlider.Value);
		config.SetValue("Settings", "ActionDelay", (int)ActionDelaySlider.Value);
		config.SetValue("Settings", "DialogueSpeed", DialogueSpeedSlider.Value);
		config.SetValue("Settings", "ShowFPS", ShowFPSCheckbox.ButtonPressed);
		config.SetValue("Settings", "LogDebugMessages", LogDebugCheckbox.ButtonPressed);
		config.SetValue("Settings", "PreventAccidentalRun", PreventRunCheckbox.ButtonPressed);
		config.SetValue("Settings", "DisableDamageLimit", DisableDamageLimitCheckbox.ButtonPressed);
		config.SetValue("Settings", "DisableStatLimit", DisableStatLimitCheckbox.ButtonPressed);
		config.SetValue("Settings", "ShowMoreInfo", ShowMoreInfoCheckbox.ButtonPressed);
		config.SetValue("Settings", "ShowStateIcons", ShowStateIconsCheckbox.ButtonPressed);
		config.SetValue("Settings", "EnemySelectionWrapping", EnemySelectionWrappingCheckbox.ButtonPressed);
		config.SetValue("Settings", "SelectionHoldTime", SelectionHoldTimeSlider.Value);
		config.SetValue("Settings", "SelectionChangeSpeed", SelectionChangeSpeedSlider.Value);
		config.SetValue("Settings", "UseConsoleSpd", UseConsoleSpdCheckbox.ButtonPressed);
		config.SetValue("Settings", "UseConsoleDef", UseConsoleDefCheckbox.ButtonPressed);
		config.SetValue("Settings", "InfiniteBuffsDebuffs", InfiniteBuffsDebuffsCheckbox.ButtonPressed);
		config.SetValue("Settings", "VertigoUsesAtk", VertigoUsesAtkCheckbox.ButtonPressed);
		config.SetValue("Settings", "ToysUseEmotionDamage", ToysUseEmotionDamageCheckbox.ButtonPressed);
		config.SetValue("Settings", "SpaceExHusbandReleaseEnergy", SpaceExHusbandReleaseEnergyCheckbox.ButtonPressed);
		config.SetValue("Settings", "EnableDebugDamage", EnableDebugDamageCheckbox.ButtonPressed);
		config.SetValue("Settings", "CombinedAccuracy", CombinedAccuracyCheckbox.ButtonPressed);
		config.SetValue("Settings", "AllowStateReapplyOnExpiry", AllowStateReapplyOnExpiryCheckbox.ButtonPressed);
		config.SetValue("Settings", "LastSelectedPreset", LastSelectedPreset ?? "");

		config.SetValue("Keybinds", "RestartHoldTime", RestartHoldTimeSlider.Value);
		config.SetValue("Keybinds", "SpeedUpMultiplier", SpeedUpSlider.Value);
		foreach (Node node in KeybindGrid.GetChildren())
		{
			if (node is KeybindButton keybind)
			{
				config.SetValue("Keybinds", keybind.AssociatedAction, OS.GetKeycodeString(keybind.CurrentKey));
				config.SetValue("ControllerBinds", keybind.AssociatedAction, ControllerBinding.Serialize(keybind.CurrentControllerBinding));
			}
		}
		
		config.Save("user://settings.cfg");
	}

	/// <summary>
	/// Retrieves the currently bound <see cref="Key"/> for a Godot <see cref="action"/>.
	/// </summary>
	/// <param name="action">The Godot action to get the key for.</param>
	/// <returns>The currently bound <see cref="Key"/>, or <see cref="Key.Unknown"/> if no key is bound.</returns>
	public Key GetKeybindForAction(string action)
	{
		foreach (Node node in KeybindGrid.GetChildren())
		{
			if (node is KeybindButton keybind && keybind.AssociatedAction == action)
				return keybind.CurrentKey;
		}
		return Key.Unknown;
	}

	/// <summary>Returns the keyboard and controller labels for an action's on-screen prompt.</summary>
	public string GetBindingDisplayForAction(string action)
	{
		foreach (Node node in KeybindGrid.GetChildren())
		{
			if (node is KeybindButton keybind && keybind.AssociatedAction == action)
				return keybind.GetBindingDisplayName();
		}
		return "Unbound";
	}

	private void GenerateDefaultConfig(ref ConfigFile config)
	{
		config.SetValue("Settings", "Fullscreen", false);
		config.SetValue("Settings", "WindowWidth", 640);
		config.SetValue("Settings", "WindowHeight", 480);
		config.SetValue("Settings", "CanvasWidth", 0);
		config.SetValue("Settings", "EdgePortraits", false);
		config.SetValue("Settings", "MasterVolume", 0.75f);
		config.SetValue("Settings", "BGMVolume", 0.5f);
		config.SetValue("Settings", "SFXVolume", 1f);
		config.SetValue("Settings", "BattlelogSpeed", 3);
		config.SetValue("Settings", "ActionDelay", 3);
		config.SetValue("Settings", "DialogueSpeed", 1d);
		config.SetValue("Settings", "ShowFPS", true);
		config.SetValue("Settings", "LogDebugMessages", false);
		config.SetValue("Settings", "PreventAccidentalRun", false);
		config.SetValue("Settings", "DisableDamageLimit", false);
		config.SetValue("Settings", "DisableStatLimit", false);
		config.SetValue("Settings", "ShowMoreInfo", false);
		config.SetValue("Settings", "ShowStateIcons", false);
		config.SetValue("Settings", "EnemySelectionWrapping", false);
		config.SetValue("Settings", "SelectionHoldTime", 0.5d);
		config.SetValue("Settings", "SelectionChangeSpeed", 0.1d);
		config.SetValue("Settings","UseConsoleSpd", false);
		config.SetValue("Settings","UseConsoleDef", false);
		config.SetValue("Settings", "InfiniteBuffsDebuffs", false);
		config.SetValue("Settings", "VertigoUsesAtk", false);
		config.SetValue("Settings", "ToysUseEmotionDamage", false);
		config.SetValue("Settings", "SpaceExHusbandReleaseEnergy", false);
		config.SetValue("Settings", "EnableDebugDamage", false);
		config.SetValue("Settings", "CombinedAccuracy", false);
		config.SetValue("Settings", "AllowStateReapplyOnExpiry", false);
		config.SetValue("Settings", "LastSelectedPreset", "");
		config.SetValue("Keybinds", "RestartHoldTime", 1d);
		config.SetValue("Keybinds", "SpeedUpMultiplier", 1.5d);
		foreach (Node node in KeybindGrid.GetChildren())
		{
			if (node is KeybindButton keybind)
			{
				config.SetValue("Keybinds", keybind.AssociatedAction, OS.GetKeycodeString(keybind.DefaultKey));
				config.SetValue("ControllerBinds", keybind.AssociatedAction, ControllerBinding.Serialize(keybind.DefaultControllerBinding));
			}
		}
		config.Save("user://settings.cfg");
	}
	
	private static readonly int[] AspectWidths = [0, 640, 768, 852, 1120];
	private Vector2I WindowedSize = new(640, 480);

	// the chosen canvas at the given whole-number scale, reduced until it fits on the screen
	private static Vector2I CanvasWindowSize(int width, int scale)
	{
		Vector2I screen = DisplayServer.ScreenGetSize();
		while (scale > 1 && screen.X > 0 && screen.Y > 0 && (width * scale > screen.X || DisplayLayout.LogicalHeight * scale > screen.Y))
			scale--;
		return new Vector2I(width * scale, DisplayLayout.LogicalHeight * scale);
	}

	private void ApplyWindowSize(Vector2I size)
	{
		Window window = GetWindow();
		Vector2I screen = DisplayServer.ScreenGetSize();
		if (screen.X > 0 && screen.Y > 0)
			size = new Vector2I(Mathf.Min(size.X, screen.X), Mathf.Min(size.Y, screen.Y));
		window.Size = size;
		window.MoveToCenter();
	}

	private void ShowNote(string title, string message)
	{
		AcceptDialog dialog = new()
		{
			Title = title,
			DialogText = message,
			Unresizable = true
		};
		AddChild(dialog);
		dialog.Confirmed += dialog.QueueFree;
		dialog.Canceled += dialog.QueueFree;
		dialog.PopupCentered();
		dialog.Show();
	}

	private void SetBusVolume(string bus, float volume)
	{
		int index = AudioServer.GetBusIndex(bus);
		if (index == -1)
		{
			GD.PrintErr("Unknown audio bus: " + bus);
			return;
		}
		AudioServer.SetBusVolumeLinear(index, volume);
	}

	public static SettingsMenuManager Instance { get; private set; }


	// fake "setting" that tracks the last selected preset across restarts
	public string LastSelectedPreset { get; internal set; } = "";

	public bool ShowFPS => ShowFPSCheckbox.ButtonPressed;
	public bool LogDebug => LogDebugCheckbox.ButtonPressed;
	public bool PreventAccidentalRun => PreventRunCheckbox.ButtonPressed;
	public bool DisableDamageLimit => DisableDamageLimitCheckbox.ButtonPressed;
	public bool DisableStatLimit => DisableStatLimitCheckbox.ButtonPressed;
	public bool ShowMoreInfo => ShowMoreInfoCheckbox.ButtonPressed;
	public bool ShowStateIcons => ShowStateIconsCheckbox.ButtonPressed;
	public bool UseConsoleSpeed => UseConsoleSpdCheckbox.ButtonPressed;
	public bool UseConsoleDefense => UseConsoleDefCheckbox.ButtonPressed;
	public bool InfiniteBuffsDebuffs => InfiniteBuffsDebuffsCheckbox.ButtonPressed;
	public bool VertigoUsesAtk => VertigoUsesAtkCheckbox.ButtonPressed;
	public bool ToysUseEmotionDamage => ToysUseEmotionDamageCheckbox.ButtonPressed;
	public bool SpaceExHusbandReleaseEnergy => SpaceExHusbandReleaseEnergyCheckbox.ButtonPressed;
	public bool EnableDebugDamage => EnableDebugDamageCheckbox.ButtonPressed;
	public bool CombinedAccuracy => CombinedAccuracyCheckbox.ButtonPressed;
	public bool AllowStateReapplyOnExpiry => AllowStateReapplyOnExpiryCheckbox.ButtonPressed;
	public int BattlelogSpeed => (int)BattlelogSpeedSlider.Value;
	public int ActionDelay => (int)ActionDelaySlider.Value;
	public double DialogueSpeed => DialogueSpeedSlider.Value;
	public bool InstantDialogue => DialogueSpeedSlider.Value >= DialogueSpeedSlider.MaxValue;
	public bool EnemySelectionWrapping => EnemySelectionWrappingCheckbox.ButtonPressed;
	public double SelectionHoldTime => SelectionHoldTimeSlider.Value;
	public double SelectionChangeSpeed => SelectionChangeSpeedSlider.Value;
	public double RestartHoldTime => RestartHoldTimeSlider.Value;
	public double SpeedUpMultiplier => SpeedUpSlider.Value;

	[Export] private TextureRect Logo;
	[Export] private AnimatedSprite2D OmoriFace;
	[Export] private HSlider MasterSlider;
	[Export] private HSlider BGMSlider;
	[Export] private HSlider SFXSlider;
	[Export] private Button TestSFXButton;
	[Export] private CheckBox FullscreenCheckbox;
	[Export] private OptionButton AspectDropdown;
	[Export] private OptionButton PortraitsDropdown;
	[Export] private CheckBox PixelSnappingCheckbox;
	[Export] private HSlider BattlelogSpeedSlider;
	[Export] private HSlider ActionDelaySlider;
	[Export] private HSlider DialogueSpeedSlider;
	[Export] private Label DialogueSpeedLabel;
	[Export] private CheckBox ShowFPSCheckbox;
	[Export] private CheckBox LogDebugCheckbox;
	[Export] private CheckBox PreventRunCheckbox;
	[Export] private CheckBox DisableDamageLimitCheckbox;
	[Export] private CheckBox DisableStatLimitCheckbox;
	[Export] private CheckBox ShowMoreInfoCheckbox;
	[Export] private CheckBox ShowStateIconsCheckbox;
	[Export] private CheckBox EnemySelectionWrappingCheckbox;
	[Export] private HSlider SelectionHoldTimeSlider;
	[Export] private Label SelectionHoldTimeLabel;
	[Export] private HSlider SelectionChangeSpeedSlider;
	[Export] private Label SelectionChangeSpeedLabel;
	[Export] private CheckBox UseConsoleSpdCheckbox;
	[Export] private CheckBox UseConsoleDefCheckbox;
	[Export] private CheckBox InfiniteBuffsDebuffsCheckbox;
	[Export] private CheckBox VertigoUsesAtkCheckbox;
	[Export] private CheckBox ToysUseEmotionDamageCheckbox;
	[Export] private CheckBox SpaceExHusbandReleaseEnergyCheckbox;
	[Export] private CheckBox EnableDebugDamageCheckbox;
	[Export] private CheckBox CombinedAccuracyCheckbox;
	[Export] private CheckBox AllowStateReapplyOnExpiryCheckbox;
	[Export] private HSlider RestartHoldTimeSlider;
	[Export] private Label RestartHoldTimeLabel;
	[Export] private HSlider SpeedUpSlider;
	[Export] private Label SpeedUpLabel;
	[Export] private GridContainer KeybindGrid;
	[Export] private Button ResetKeybindsButton;
	[Export] private Button BackButton;
	[Export] private VBoxContainer MainControls;
}
