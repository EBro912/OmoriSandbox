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
	public override void _Ready()
	{
		Instance = this;
	}

	public void Init()
	{
		VersionLabel.Text = GameManager.Version;
		
		AudioManager.Instance.PlayBGM("ow_cattail_fields");

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
		};
	}

	private void EnterEditor()
	{
		AudioManager.Instance.StopBGM();
		MainMenu.Visible = false;
		Editor.Visible = true;
	}

	public void AddModListEntry(ModMetadata data, Texture2D icon = null)
	{
		ModListEntry entry = ModListEntry.Instantiate<ModListEntry>();
		entry.SetData(data);
		if (icon != null)
			entry.SetIcon(icon);
		ModListParent.GetChild(1).GetChild(0).AddChild(entry);
	}

	public void UpdateModsLoaded(int count, int total)
	{
		ModsLoaded.Text = $"{count} mod{(count > 1 ? "s" : "")} loaded ({total} installed)";
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
