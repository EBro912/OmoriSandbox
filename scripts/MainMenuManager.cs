using System;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;
using OmoriSandbox.Animation;
using OmoriSandbox.Extensions;
using OmoriSandbox.Modding;

namespace OmoriSandbox.Editor;

internal partial class MainMenuManager : Node
{
	private int ActiveController = -1;
	internal bool UsingController => ActiveController >= 0;

	public override void _Ready()
	{
		Instance = this;
		GetWindow().WindowInput += TrackInputDevice;
		Input.JoyConnectionChanged += OnJoyConnectionChanged;
	}

	public override void _ExitTree()
	{
		GetWindow().WindowInput -= TrackInputDevice;
		Input.JoyConnectionChanged -= OnJoyConnectionChanged;
	}

	private void TrackInputDevice(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventJoypadButton { Pressed: true }:
			case InputEventJoypadMotion motion when Mathf.Abs(motion.AxisValue) >= 0.75f:
				ActiveController = @event.Device;
				break;
			case InputEventKey { Pressed: true, Echo: false }:
			case InputEventMouseButton { Pressed: true }:
			case InputEventMouseMotion mouse when mouse.Relative != Vector2.Zero:
			case InputEventScreenTouch { Pressed: true }:
			case InputEventScreenDrag:
				ActiveController = -1;
				break;
		}
	}

	private void OnJoyConnectionChanged(long device, bool connected)
	{
		if (!connected && device == ActiveController)
			ActiveController = -1;
	}

	public override void _Process(double delta)
	{
		Window window = GetWindow().GetLastExclusiveWindow() ?? GetWindow();
		Control focus = window.GuiGetFocusOwner();
		// check if the user has swapped to controller input and grab focus
		if (focus != null && focus is not (LineEdit or TextEdit) &&
		    focus.HasFocus(ignoreHiddenFocus: true) != UsingController)
			focus.GrabFocus(hideFocus: !UsingController);
	}

	public override void _Input(InputEvent @event)
	{
		if (!MainMenu.Visible || KeybindButton.IsCapturing || !ControllerBinding.IsController(@event) ||
		    !(@event is InputEventJoypadButton { Pressed: true } ||
		      @event is InputEventJoypadMotion motion && Mathf.Abs(motion.AxisValue) >= 0.75f))
			return;
		if (GetViewport().GuiGetFocusOwner() is { } focus && focus.IsVisibleInTree())
			return;
		Control first = Settings.Visible ? Settings.GetNode<Button>("BackButton") :
			CreditsPanel.Visible ? CreditsBackButton : PlayButton.Visible ? PlayButton : LoadExistingButton;
		first.GrabFocus();
		GetViewport().SetInputAsHandled();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!MainMenu.Visible || KeybindButton.IsCapturing || !ControllerBinding.IsController(@event) ||
		    !@event.IsActionPressed("ui_cancel"))
			return;
		if (Settings.Visible)
			Settings.Close();
		else if (CreditsPanel.Visible)
			CreditsBackButton.EmitSignal(BaseButton.SignalName.Pressed);
		else if (QuitButton.Text == "Back")
			QuitButton.EmitSignal(BaseButton.SignalName.Pressed);
		else
			return;
		GetViewport().SetInputAsHandled();
	}

	public void Init()
	{
		VersionLabel.Text = GameManager.Version;
		
		AudioManager.Instance.PlayBGM("ow_cattail_fields");
		PlayButton.GrabFocus(hideFocus: !UsingController);
		Settings.VisibilityChanged += () =>
		{
			if (!Settings.Visible && MainMenu.Visible && MainControls.Visible)
				SettingsButton.GrabFocus(hideFocus: !UsingController);
		};

		PlayButton.Pressed += () =>
		{
			if (TitlePresetDropdown.Selected == -1)
				return;

			string presetName = TitlePresetDropdown.GetItemText(TitlePresetDropdown.Selected);
			if (!PresetManager.Instance.TryGetPreset(presetName, out BattlePreset preset))
			{
				GD.PrintErr($"Preset {presetName} not found.");
				return;
			}

			LastLoadedPreset = presetName;
			GameManager.Instance.LoadBattlePreset(preset, (int)StageSelectorSpinBox.Value);
			MainMenu.Visible = false;
		};

		ConfigureButton.Pressed += () =>
		{
			PlayButton.Visible = false;
			EditorSettingsContainer.Visible = false;
			LoadExistingButton.Visible = true;
			NewPresetContainer.Visible = true;
			QuitButton.Text = "Back";
		};

		NormalPresetButton.Pressed += () =>
		{
			EditorManager.Instance.SetEditorMode(GameModeType.Normal);
			EnterEditor();
		};

		BossRushPresetButton.Pressed += () =>
		{
			EditorManager.Instance.SetEditorMode(GameModeType.BossRush);
			EnterEditor();
		};

		LoadExistingButton.Pressed += () =>
		{
			EditorManager.Instance.LoadPreset(TitlePresetDropdown.Selected);
			EnterEditor();
		};

		SettingsButton.Pressed += () =>
		{
			MainControls.Visible = false;
			Logo.Visible = false;
			OmoriFace.Visible = false;
			Settings.Visible = true;
		};

		CreditsButton.Pressed += () =>
		{
			MainControls.Visible = false;
			Logo.Visible = false;
			OmoriFace.Visible = false;
			CreditsPanel.Visible = true;
			CreditsButton.GetParent<Control>().Visible = false;
		};

		CreditsBackButton.Pressed += () =>
		{
			MainControls.Visible = true;
			Logo.Visible = true;
			OmoriFace.Visible = true;
			CreditsPanel.Visible = false;
			CreditsButton.GetParent<Control>().Visible = true;
			CreditsButton.GrabFocus(hideFocus: !UsingController);
		};

		QuitButton.Pressed += () =>
		{
			if (QuitButton.Text == "Back")
			{
				PlayButton.Visible = true;
				EditorSettingsContainer.Visible = true;
				LoadExistingButton.Visible = false;
				NewPresetContainer.Visible = false;
				QuitButton.Text = "Quit";
			}
			else
			{
				GetTree().Quit();
			}
		};

		ModFolderButton.Pressed += () =>
		{
			Error error = OS.ShellOpen(ProjectSettings.GlobalizePath("user://mods"));
			if (error != Error.Ok)
			{
				GD.PrintErr("Failed to open mods folder");
			}
		};

		ShowModsButton.Pressed += () =>
		{
			if (ShowModsButton.Text == "View Mods")
			{
				ModListParent.Visible = true;
				ShowModsButton.Text = "Hide Mods";
			}
			else
			{
				ModListParent.Visible = false;
				ShowModsButton.Text = "View Mods";
			}
		};

		GithubButton.Pressed += () =>
		{
			Error error = OS.ShellOpen("https://github.com/EBro912/OmoriSandbox");
			if (error != Error.Ok)
			{
				GD.PrintErr("Failed to open Github link");
			}
		};

		TitlePresetDropdown.ItemSelected += index =>
		{
			if (index == -1)
				return;
			UpdateStageSelectorVisiblity((int)index);
			// remember the selection across game restarts
			SettingsMenuManager.Instance.LastSelectedPreset = TitlePresetDropdown.GetItemText((int)index);
			SettingsMenuManager.Instance.SaveSettings();
		};
	}

	// re-selects the preset remembered in the settings
	// if the preset no longer exists, the first option is chosen
	public void RestoreLastSelectedPreset()
	{
		string saved = SettingsMenuManager.Instance.LastSelectedPreset;
		if (string.IsNullOrEmpty(saved))
			return;

		int index = TitlePresetDropdown.GetItemIndex(saved);
		if (index == -1)
			return;

		TitlePresetDropdown.Selected = index;
		UpdateStageSelectorVisiblity(index);
	}

	private void EnterEditor()
	{
		AudioManager.Instance.StopBGM();
		MainMenu.Visible = false;
		Editor.Visible = true;
	}

	public void AddModListEntry(ModMetadata data, Texture2D icon = null, bool hasErrors = false)
	{
		ModListEntry entry = ModListEntry.Instantiate<ModListEntry>();
		entry.SetData(data);
		if (icon != null)
			entry.SetIcon(icon);
		if (hasErrors)
			entry.MarkErrors();
		ModListParent.GetChild(1).GetChild(0).AddChild(entry);
	}

	public void UpdateModsLoaded(int count, int total)
	{
		ModsLoaded.Text = $"{count} mod{(count != 1 ? "s" : "")} loaded ({total} installed)";
	}

	public void ClearPresetDropdown()
	{
		TitlePresetDropdown.Clear();
	}

	public void PopulatePresetDropdown(string entry)
	{
		TitlePresetDropdown.AddItem(entry);
	}

	public void ReturnToTitle()
	{
		int index = TitlePresetDropdown.GetItemIndex(LastLoadedPreset);
		if (index > -1)
		{
			TitlePresetDropdown.Selected = index;
			UpdateStageSelectorVisiblity(index);
			SettingsMenuManager.Instance.LastSelectedPreset = LastLoadedPreset;
			SettingsMenuManager.Instance.SaveSettings();
		}

		AnimationManager.Instance.StopAllAnimations();
		AudioManager.Instance.PlayBGM("ow_cattail_fields");
		PlayButton.Visible = true;
		EditorSettingsContainer.Visible = true;
		LoadExistingButton.Visible = false;
		NewPresetContainer.Visible = false;
		QuitButton.Text = "Quit";
		MainMenu.Visible = true;
		Editor.Visible = false;
		PlayButton.GrabFocus(hideFocus: !UsingController);
		GameManager.Instance.DiscordManager.SetMainMenu();
		Engine.TimeScale = 1f;
	}

	private void UpdateStageSelectorVisiblity(int index)
	{
		string selected = TitlePresetDropdown.GetItemText(index);
		if (PresetManager.Instance.TryGetPreset(selected, out BattlePreset preset))
		{
			if (preset.Type is GameModeType.BossRush)
			{
				StageSelectorParent.Visible = true;
				StageSelectorSpinBox.MaxValue = preset.Stages.Count - 1;
				StageSelectorSpinBox.Value = 0;
			}
			else
			{
				StageSelectorParent.Visible = false;
			}
		}
	}

	public static MainMenuManager Instance;
	public string LastLoadedPreset = "";

	[Export] private Label VersionLabel;
	[Export] private TextureRect Logo;
	[Export] private AnimatedSprite2D OmoriFace; 
	[Export] private PackedScene ModListEntry;
	[Export] private Panel ModListParent;
	[Export] private Button ShowModsButton;
	[Export] private CanvasLayer MainMenu;
	[Export] private CanvasLayer Editor;
	[Export] private Button PlayButton;
	[Export] private Button ConfigureButton;
	[Export] private Button SettingsButton;
	[Export] private Button QuitButton;
	[Export] private VBoxContainer MainControls;
	[Export] private SettingsMenuManager Settings;
	[Export] private HBoxContainer EditorSettingsContainer;
	[Export] private HBoxContainer NewPresetContainer;
	[Export] private Button LoadExistingButton;
	[Export] private Button NormalPresetButton;
	[Export] private Button BossRushPresetButton;
	[Export] private Button ModFolderButton;
	[Export] private Label ModsLoaded;
	[Export] private Button GithubButton;
	[Export] private Button CreditsButton;
	[Export] private Button CreditsBackButton;
	[Export] private Panel CreditsPanel;
	[Export] private OptionButton TitlePresetDropdown;
	[Export] private HBoxContainer StageSelectorParent;
	[Export] private SpinBox StageSelectorSpinBox;
}
