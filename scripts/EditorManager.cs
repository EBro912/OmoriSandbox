using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Editor;

internal partial class EditorManager : Node
{
	public override void _Ready()
	{
		Instance = this;
	}

	private GameModeType EditorMode = GameModeType.Normal;
	private const int EnemiesTabIdx = 3;
	private const int BossRushTabIdx = 4;

    public void Init()
    {
	    BattlebackBGMEditor.Init(BGMPreview, BattlebackPreview);
	    
	    PresetFolderButton.Pressed += () =>
	    {
		    Error error = OS.ShellOpen(ProjectSettings.GlobalizePath("user://presets"));
		    if (error != Error.Ok)
		    {
			    GD.PrintErr("Failed to open presets folder");
		    }
	    };
	    
        ReturnButton.Pressed += () =>
		{
			ConfirmationDialog dialog = new()
			{
				Title = "Confirmation",
				DialogText = "Are you sure you return?\nAll unsaved progress will be lost.",
				Unresizable = true
			};
			dialog.Confirmed += () =>
			{
				ReturnToTitle();
				dialog.QueueFree();
			};
			dialog.Canceled += dialog.QueueFree;
			AddChild(dialog);
			dialog.PopupCentered();
			dialog.Show();
		};

		foreach (Control control in AddActorControls)
		{
			control.GetChild<Button>(0).Pressed += () =>
			{
				Control card = BattleCard.Instantiate<Control>();
				control.AddChild(card);
				card.Position = Vector2.Zero;
				PartyMemberEditorComponent editor = PartyMemberEditor.Instantiate<PartyMemberEditorComponent>();
				ActorTabs.AddChild(editor);
				// really dumb, capturing a variable wasn't working here for some reason
				// so we use the name of the parent to get the proper position
				int position = (int)char.GetNumericValue(control.Name.ToString()[^1]) - 1;
				editor.Init(card, position);
			};
		}

		AddEnemyControl.GetChild<Button>(0).Pressed += () =>
		{
			AnimatedSprite2D enemySprite = new();
			AddEnemyControl.AddChild(enemySprite);
			EnemyEditorComponent editor = EnemyEditor.Instantiate<EnemyEditorComponent>();
			EnemyTabs.AddChild(editor);
			editor.Init(enemySprite);
		};

		AddItemButton.Pressed += () =>
		{
			HBoxContainer container = new();
			OptionButton dropdown = new();
			foreach (string item in Database.GetAllItemNames())
				dropdown.AddItem(item);
			dropdown.FitToLongestItem = false;
			container.AddChild(dropdown);
			SpinBox quantity = new()
			{
				MinValue = 1,
				MaxValue = 999,
				Value = 1,
				Rounded = true
			};
			container.AddChild(quantity);
			Button remove = new();
			remove.Text = "X";
			remove.Pressed += () =>
			{
				container.QueueFree();
			};
			container.AddChild(remove);
			ItemContainer.AddChild(container);
		};

		SearchButton.Pressed += () =>
		{
			Results.Text = "Loading...";
			List<string> results = [];
			results.AddRange(Database.GetAllSkillNames().Where(skill => skill.Contains(SearchInput.Text, StringComparison.CurrentCultureIgnoreCase)));
			Results.Text = string.Join(", ", results);
		};

		StartingEnergySlider.ValueChanged += (value) => StartingEnergyValue.Text = value.ToString();
		FollowupTierSlider.ValueChanged += (value) => FollowupTierValue.Text = value.ToString();

		ImportButton.Pressed += () =>
		{
			if (PresetManager.Instance.TryGetPreset(ImportPresetDropdown.GetItemText(ImportPresetDropdown.Selected),
				    out BattlePreset preset))
			{
				BossRushStageEditorComponent editor = StageEditor.Instantiate<BossRushStageEditorComponent>();
				editor.BattlebackBGMEditor.Init(BGMPreview, BattlebackPreview);
				StageTabs.AddChild(editor);
				editor.CopyFrom(preset);
				int total = StageTabs.GetTabCount();
				StageTabs.SetTabTitle(total - 1, total.ToString());
				if (total == 1)
				{
					editor.ShowEnemies();
					BattlebackPreview.SetBattleback(editor.BattlebackBGMEditor.SelectedBattleback);
				}
			}
		};
		
		AddStageButton.Pressed += () =>
		{
			BossRushStageEditorComponent editor = StageEditor.Instantiate<BossRushStageEditorComponent>();
			editor.BattlebackBGMEditor.Init(BGMPreview, BattlebackPreview);
			StageTabs.AddChild(editor);
			int total = StageTabs.GetTabCount();
			StageTabs.SetTabTitle(total - 1, total.ToString());
		};

		DuplicateStageButton.Pressed += () =>
		{
			Control tab = StageTabs.GetTabControl(StageTabs.CurrentTab);
			if (tab is BossRushStageEditorComponent editor)
			{
				BossRushStageEditorComponent newEditor = StageEditor.Instantiate<BossRushStageEditorComponent>();
				StageTabs.AddChild(newEditor);
				newEditor.BattlebackBGMEditor.Init(BGMPreview, BattlebackPreview);
				newEditor.CopyFrom(editor);
				int total = StageTabs.GetTabCount();
				StageTabs.SetTabTitle(total - 1, total.ToString());
			}
		};

		RemoveStageButton.Pressed += () =>
		{
			BGMPreview.Stop();
			Node tab = StageTabs.GetChild(StageTabs.CurrentTab);
			tab.Free();
			for (int i = 1; i <= StageTabs.GetTabCount(); i++)
			{
				StageTabs.SetTabTitle(i - 1, i.ToString());
			}
		};

		MoveStageLeftButton.Pressed += () =>
		{
			int currentTab = StageTabs.CurrentTab;
			if (currentTab < 1)
				return;
			StageTabs.MoveChild(StageTabs.GetChild(currentTab), currentTab - 1);
			StageTabs.SetTabTitle(currentTab - 1, currentTab.ToString());
			StageTabs.SetTabTitle(currentTab, (currentTab + 1).ToString());
		};

		MoveStageRightButton.Pressed += () =>
		{
			int currentTab = StageTabs.CurrentTab;
			if (currentTab == StageTabs.GetTabCount() - 1)
				return;
			StageTabs.MoveChild(StageTabs.GetChild(currentTab), currentTab + 1);
			StageTabs.SetTabTitle(currentTab + 1, (currentTab + 2).ToString());
			StageTabs.SetTabTitle(currentTab, (currentTab + 1).ToString());
		};

		StageTabs.TabChanged += tab =>
		{
			BGMPreview.Stop();
			if (tab == -1)
			{
				BattlebackPreview.SetDefaultBattleback();
				return;
			}
			for (int i = 0; i < StageTabs.GetTabCount(); i++)
			{
				Control node = StageTabs.GetTabControl(i);
				if (node is BossRushStageEditorComponent editor)
				{
					if (i == tab)
					{
						editor.ShowEnemies();
						editor.BattlebackBGMEditor.Load();
					}
					else
					{
						editor.HideEnemies();
						editor.BattlebackBGMEditor.Stop();
					}
				}
			}
		};
		
		SavePresetButton.Pressed += PreSave;
		LoadPresetButton.Pressed += Load;
		DeletePresetButton.Pressed += PreDelete;

		ResetButton.Pressed += PreResetToDefault;

		ResetPresetDropdown();
    }
    
    public void ReturnToTitle()
	{
		MainMenuManager.Instance.ReturnToTitle();
		ResetToDefault();
	}

	public void SetEditorMode(GameModeType type)
	{
		EditorMode = type;
		AddEnemyControl.GetChild<Button>(0).Visible = EditorMode is GameModeType.Normal;
		BattlebackBGMEditor.Visible = EditorMode is GameModeType.Normal;
		MainTabs.SetTabHidden(EnemiesTabIdx, EditorMode is GameModeType.BossRush);
		MainTabs.SetTabHidden(BossRushTabIdx, EditorMode is GameModeType.Normal);
		GameManager.Instance.DiscordManager.SetEditingPreset(EditorMode);
	}

	private void PreSave()
	{
		if (string.IsNullOrWhiteSpace(PresetInput.Text))
			return;

		if (ActorTabs.GetChildCount() == 0)
		{
			ShowWindow("Error", "Preset must have at least one actor");
			return;
		}

		if (EditorMode is GameModeType.Normal && EnemyTabs.GetChildCount() == 0)
		{
			ShowWindow("Error", "Preset must have at least one enemy");
			return;
		}

		if (EditorMode is GameModeType.BossRush)
		{
			if (StageTabs.GetChildCount() == 0)
			{
				ShowWindow("Error", "Preset must have at least one stage");
				return;
			}

			if (StageTabs.GetChildren()
			    .Any(x => x is BossRushStageEditorComponent editor && editor.Enemies.GetTabCount() == 0))
			{
				ShowWindow("Error", "Preset must have at least one enemy in all stages");
				return;
			}
		}
		

		if (PresetManager.Instance.PresetExists(PresetInput.Text))
		{
			ConfirmationDialog dialog = new()
			{
				Title = "Confirmation",
				DialogText = "A preset with this name already exists.\nDo you want to overwrite it?",
				Unresizable = true
			};
			dialog.Confirmed += () =>
			{
				Save();
				dialog.QueueFree();
			};
			dialog.Canceled += dialog.QueueFree;
			AddChild(dialog);
			dialog.PopupCentered();
			dialog.Show();
		}
		else
		{
			Save();
		}
	}

	private void Save()
	{
		BattlePreset preset = new()
		{
			Type = EditorMode,
			Name = PresetInput.Text,
			StartingEnergy = (int)StartingEnergySlider.Value,
			FollowupTier = (int)FollowupTierSlider.Value,
			BasilFollowups = BasilFollowupsCheckbox.ButtonPressed,
			BasilReleaseEnergy = BasilReleaseEnergyCheckbox.ButtonPressed,
			DisableDialogue = DisableDialogue.ButtonPressed,
			DisableDamageNumbers = DisableDamageNumbers.ButtonPressed
		};

		if (EditorMode is GameModeType.Normal)
		{
			preset.Battleback = BattlebackBGMEditor.SelectedBattleback;
			preset.BGM = BattlebackBGMEditor.SelectedBGM;
			preset.BGMPitch = BattlebackBGMEditor.BGMPitchValue;
			preset.BGMLoopPoint = BattlebackBGMEditor.BGMLoopPointValue;
		}
		
		Dictionary<string, int> items = [];
		List<BattlePresetActor> actors = [];
		List<BattlePresetEnemy> enemies = [];
		List<BattlePresetBossRushStage> stages = [];

		foreach (Node child in ItemContainer.GetChildren())
		{
			if (child is HBoxContainer container)
			{
				OptionButton dropdown = container.GetChild<OptionButton>(0);
				SpinBox quantity = container.GetChild<SpinBox>(1);
				items.Add(dropdown.GetItemText(dropdown.Selected), (int)quantity.Value);
			}
		}

		foreach (Node child in ActorTabs.GetChildren())
		{
			if (child is PartyMemberEditorComponent editor)
			{
				string[] skills = new string[5];
				skills[0] = editor.AttackSkill.Text;
				for (int i = 0; i < 4; i++)
					skills[i + 1] = editor.Skills[i].Text;
				BattlePresetActor actor = new()
				{
					Name = editor.ActorDropdown.GetItemText(editor.ActorDropdown.Selected),
					Level = (int)editor.LevelSlider.Value,
					Weapon = editor.WeaponDropdown.GetItemText(editor.WeaponDropdown.Selected),
					Charm = editor.CharmDropdown.GetItemText(editor.CharmDropdown.Selected),
					Emotion = editor.EmotionDropdown.GetItemText(editor.EmotionDropdown.Selected),
					FollowupsDisabled = editor.DisableFollowups.ButtonPressed,
					Skills = skills,
					Position = editor.ActorPosition,
					AdjustedStats = editor.GetAdjustedStats()
				};
				actors.Add(actor);
			}
		}

		if (EditorMode is GameModeType.Normal)
		{
			foreach (Node child in EnemyTabs.GetChildren())
			{
				if (child is EnemyEditorComponent editor)
				{
					BattlePresetEnemy enemy = new()
					{
						Name = editor.EnemyDropdown.GetItemText(editor.EnemyDropdown.Selected),
						Position = GD.VarToStr(new Vector2((float)editor.XPosBox.Value, (float)editor.YPosBox.Value)),
						Emotion = editor.EmotionDropdown.GetItemText(editor.EmotionDropdown.Selected),
						Layer = (int)editor.LayerBox.Value,
						FallsOffScreen = editor.FallsOffScreenCheckbox.ButtonPressed,
						GrayscaleOnDefeat = editor.GrayscaleOnDefeatCheckbox.ButtonPressed
					};
					if (enemy.Emotion is "hurt" or "toast")
						enemy.Emotion = "neutral";
					enemies.Add(enemy);
				}
			}
		}
		else
		{
			foreach (Node child in StageTabs.GetChildren())
			{
				if (child is BossRushStageEditorComponent editor)
				{
					BattlePresetBossRushStage stage = new()
					{
						StageNumber = StageTabs.GetTabIdxFromControl(editor) + 1,
						Battleback = editor.BattlebackBGMEditor.SelectedBattleback,
						BGM = editor.BattlebackBGMEditor.SelectedBGM,
						BGMPitch = editor.BattlebackBGMEditor.BGMPitchValue,
						BGMLoopPoint = editor.BattlebackBGMEditor.BGMLoopPointValue,
						HealParty = editor.HealPartyCheckbox.ButtonPressed,
						KeepEmotion =  editor.KeepEmotionCheckbox.ButtonPressed,
						KeepStatusEffects = editor.KeepStatusEffectsCheckbox.ButtonPressed
					};
					foreach (Node subChild in editor.Enemies.GetChildren())
					{
						if (subChild is EnemyEditorComponent enemyEditor)
						{
							BattlePresetEnemy enemy = new()
							{
								Name = enemyEditor.EnemyDropdown.GetItemText(enemyEditor.EnemyDropdown.Selected),
								Position = GD.VarToStr(new Vector2((float)enemyEditor.XPosBox.Value, (float)enemyEditor.YPosBox.Value)),
								Emotion = enemyEditor.EmotionDropdown.GetItemText(enemyEditor.EmotionDropdown.Selected),
								Layer = (int)enemyEditor.LayerBox.Value,
								FallsOffScreen = enemyEditor.FallsOffScreenCheckbox.ButtonPressed,
								GrayscaleOnDefeat = enemyEditor.GrayscaleOnDefeatCheckbox.ButtonPressed
							};
							if (enemy.Emotion is "hurt" or "toast")
								enemy.Emotion = "neutral";
							stage.Enemies.Add(enemy);
						}
					}
					stages.Add(stage);
				}
			}
		}

		preset.Items = items;
		preset.Actors = actors;
		preset.Enemies = enemies;
		preset.Stages = stages;

		PresetManager.Instance.SavePreset(preset);

		ShowWindow("Success", "Saved preset to user://presets/" + PresetInput.Text + ".json");

		MainMenuManager.Instance.LastLoadedPreset = PresetInput.Text;
		PresetInput.Text = "";
		ResetPresetDropdown();
	}

	public void LoadPreset(int presetIndex)
	{
		PresetDropdown.Selected = presetIndex;
		Load();
	}

	private void Load()
	{
		if (PresetDropdown.Selected == -1)
			return;
		
		string presetName = PresetDropdown.GetItemText(PresetDropdown.Selected);
		string path = "user://presets/" + presetName + ".json";
		if (!PresetManager.Instance.TryGetPreset(presetName, out BattlePreset preset))
		{
			ShowWindow("Error", "Preset file not found at: " + path);
			return;
		}

		ResetToDefault();
		SetEditorMode(preset.Type);
		if (EditorMode is GameModeType.Normal)
		{
			BattlebackBGMEditor.SelectedBattleback = preset.Battleback;
			BattlebackBGMEditor.SelectedBGM = preset.BGM;
			BattlebackBGMEditor.BGMPitchValue = preset.BGMPitch;
			BattlebackBGMEditor.BGMLoopPointValue = preset.BGMLoopPoint;
			BattlebackPreview.SetBattleback(BattlebackBGMEditor.SelectedBattleback);
		}

		StartingEnergySlider.Value = Math.Clamp(preset.StartingEnergy, 0, 10);
		FollowupTierSlider.Value = Math.Clamp(preset.FollowupTier, 1, 3);
		BasilFollowupsCheckbox.ButtonPressed = preset.BasilFollowups;
		BasilReleaseEnergyCheckbox.ButtonPressed = preset.BasilReleaseEnergy;
		DisableDialogue.ButtonPressed = preset.DisableDialogue;
		DisableDamageNumbers.ButtonPressed = preset.DisableDamageNumbers;
		
		foreach (KeyValuePair<string, int> entry in preset.Items)
		{
			HBoxContainer container = new();
			OptionButton dropdown = new();
			foreach (string item in Database.GetAllItemNames())
				dropdown.AddItem(item);
			dropdown.FitToLongestItem = false;
			container.AddChild(dropdown);
			SpinBox quantity = new()
			{
				MinValue = 1,
				MaxValue = 999,
				Value = entry.Value,
				Rounded = true
			};
			container.AddChild(quantity);
			Button remove = new();
			remove.Text = "X";
			remove.Pressed += () =>
			{
				container.QueueFree();
			};
			container.AddChild(remove);
			ItemContainer.AddChild(container);
			for (int i = 0; i < dropdown.ItemCount; i++)
			{
				if (dropdown.GetItemText(i) == entry.Key)
				{
					dropdown.Selected = i;
					break;
				}
			}
		}
		
		foreach (BattlePresetActor entry in preset.Actors)
		{
			Control card = BattleCard.Instantiate<Control>();
			AddActorControls[entry.Position].AddChild(card);
			card.Position = Vector2.Zero;
			PartyMemberEditorComponent editor = PartyMemberEditor.Instantiate<PartyMemberEditorComponent>();
			ActorTabs.AddChild(editor);
			editor.Init(card, entry);
		}

		if (preset.Type is GameModeType.Normal)
		{
			foreach (BattlePresetEnemy entry in preset.Enemies)
			{
				AnimatedSprite2D enemySprite = new();
				AddEnemyControl.AddChild(enemySprite);
				EnemyEditorComponent editor = EnemyEditor.Instantiate<EnemyEditorComponent>();
				EnemyTabs.AddChild(editor);
				editor.Init(enemySprite, entry);
			}
		}
		else
		{
			foreach (BattlePresetBossRushStage entry in preset.Stages)
			{
				BossRushStageEditorComponent editor = StageEditor.Instantiate<BossRushStageEditorComponent>();
				editor.BattlebackBGMEditor.Init(BGMPreview, BattlebackPreview);
				StageTabs.AddChild(editor);
				StageTabs.SetTabTitle(StageTabs.GetTabCount() - 1, entry.StageNumber.ToString());
				editor.BattlebackBGMEditor.SelectedBattleback = entry.Battleback;
				editor.BattlebackBGMEditor.SelectedBGM = entry.BGM;
				editor.BattlebackBGMEditor.BGMPitchValue = entry.BGMPitch;
				editor.BattlebackBGMEditor.BGMLoopPointValue = entry.BGMLoopPoint;
				editor.HealPartyCheckbox.ButtonPressed = entry.HealParty;
				editor.KeepEmotionCheckbox.ButtonPressed = entry.KeepEmotion;
				editor.KeepStatusEffectsCheckbox.ButtonPressed = entry.KeepStatusEffects;
				foreach (BattlePresetEnemy enemy in entry.Enemies)
				{
					AnimatedSprite2D enemySprite = new();
					editor.EnemyParent.AddChild(enemySprite);
					EnemyEditorComponent subEditor = EnemyEditor.Instantiate<EnemyEditorComponent>();
					editor.Enemies.AddChild(subEditor);
					subEditor.Init(enemySprite, enemy);
				}
				editor.HideEnemies();
			}

			Node first = StageTabs.GetTabControl(0);
			if (first is BossRushStageEditorComponent firstEditor)
			{
				firstEditor.ShowEnemies();
				firstEditor.BattlebackBGMEditor.Load();
			}
		}

		PresetInput.Text = presetName;
		MainMenuManager.Instance.LastLoadedPreset = presetName;
	}

	private void ResetPresetDropdown()
	{
		PresetDropdown.Clear();
		ImportPresetDropdown.Clear();
		MainMenuManager.Instance.ClearPresetDropdown();
		foreach (BattlePreset preset in PresetManager.Instance.GetAllPresets())
		{
			PresetDropdown.AddItem(preset.Name);
			MainMenuManager.Instance.PopulatePresetDropdown(preset.Name);
			if (preset.Type is GameModeType.Normal)
				ImportPresetDropdown.AddItem(preset.Name);
		}
	}

	private void PreDelete()
	{
		if (PresetDropdown.Selected == -1)
			return;

		string preset = PresetDropdown.GetItemText(PresetDropdown.Selected);
		ConfirmationDialog dialog = new()
		{
			Title = "Confirmation",
			DialogText = "Are you sure you want to delete the " + preset + " preset?",
			Unresizable = true
		};
		dialog.Confirmed += () =>
		{
			Delete();
			dialog.QueueFree();
		};
		dialog.Canceled += dialog.QueueFree;
		AddChild(dialog);
		dialog.PopupCentered();
		dialog.Show();
	}

	private void Delete()
	{
		string preset = PresetDropdown.GetItemText(PresetDropdown.Selected);
		string path = "user://presets/" + preset + ".json";
		if (FileAccess.FileExists(path))
		{
			if (PresetManager.Instance.RemovePreset(preset))
			{
				ShowWindow("Success", "Deleted preset " + preset);
				ResetPresetDropdown();
			}
			else
			{
				ShowWindow("Error", "Failed to delete preset " + preset + ". See console/logs for more information.");
			}
		}
	}

	private void PreResetToDefault()
	{
		ConfirmationDialog dialog = new()
		{
			Title = "Confirmation",
			DialogText = "Are you sure you want to reset everything to default?\nIf you have not created a preset, this action cannot be undone.",
			Unresizable = true
		};
		dialog.Confirmed += () =>
		{
			ResetToDefault();
			dialog.QueueFree();
		};
		dialog.Canceled += dialog.QueueFree;
		AddChild(dialog);
		dialog.PopupCentered();
		dialog.Show();
	}

	private void ResetToDefault()
	{
		// remove battlecard previews
		foreach (Control control in AddActorControls)
		{
			if (control.GetChildCount() > 1)
				control.GetChild<Control>(1).Free();
		}

		// remove party member tabs
		foreach (Node child in ActorTabs.GetChildren())
		{
			if (child is PartyMemberEditorComponent editor)
			{
				editor.Free();
			}
		}

		// remove enemy tabs
		foreach (Node child in EnemyTabs.GetChildren())
		{
			if (child is EnemyEditorComponent editor)
			{
				editor.Free();
			}
		}
		
		// remove stage tabs
		foreach (Node child in StageTabs.GetChildren())
		{
			if (child is BossRushStageEditorComponent editor)
			{
				editor.Free();
			}
		}

		// remove enemy sprites
		foreach (Node child in AddEnemyControl.GetChildren())
		{
			if (child is AnimatedSprite2D sprite)
			{
				sprite.Free();
			}
		}

		// remove items
		foreach (Node child in ItemContainer.GetChildren())
		{
			if (child is HBoxContainer container)
			{
				container.Free();
			}
		}
		
		FollowupTierSlider.Value = 1;
		BasilFollowupsCheckbox.ButtonPressed = false;
		BasilReleaseEnergyCheckbox.ButtonPressed = false;
		DisableDialogue.ButtonPressed = false;
		DisableDamageNumbers.ButtonPressed = false;

		BGMPreview.Stop();
		BattlebackBGMEditor.Reset();

		PresetInput.Text = string.Empty;
		MainTabs.CurrentTab = 0;
	}

	private void ShowWindow(string title, string message)
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

	public static EditorManager Instance { get; private set; }
	
    [Export] private AudioStreamPlayer BGMPreview;
    [Export] private BattlebackDisplayComponent BattlebackPreview;
    [Export] private Control[] AddActorControls;
    [Export] private Control AddEnemyControl;
    [Export] private PackedScene PartyMemberEditor;
    [Export] private PackedScene EnemyEditor;
    [Export] private PackedScene StageEditor;
    [Export] private PackedScene BattleCard;
    [Export] private TabContainer ActorTabs;
    [Export] private TabContainer EnemyTabs;
    [Export] private BattlebackBGMEditorComponent BattlebackBGMEditor;
    [Export] private HSlider StartingEnergySlider;
    [Export] private Label StartingEnergyValue;
    [Export] private HSlider FollowupTierSlider;
    [Export] private Label FollowupTierValue;
    [Export] private CheckBox BasilFollowupsCheckbox;
    [Export] private CheckBox BasilReleaseEnergyCheckbox;
    [Export] private CheckBox DisableDialogue;
    [Export] private CheckBox DisableDamageNumbers;
    [Export] private Button AddItemButton;
    [Export] private GridContainer ItemContainer;
    [Export] private LineEdit SearchInput;
    [Export] private Button SearchButton;
    [Export] private TextEdit Results;
    [Export] private Button SavePresetButton;
    [Export] private LineEdit PresetInput;
    [Export] private Button LoadPresetButton;
    [Export] private Button DeletePresetButton;
    [Export] private OptionButton PresetDropdown;
    [Export] private Button ResetButton;
    [Export] private Button ReturnButton;
    [Export] private Button PresetFolderButton;
    [Export] private Button AddStageButton;
    [Export] private Button DuplicateStageButton;
    [Export] private Button RemoveStageButton;
    [Export] private Button MoveStageLeftButton;
    [Export] private Button MoveStageRightButton;
    [Export] private OptionButton ImportPresetDropdown;
    [Export] private Button ImportButton;
    [Export] private TabContainer StageTabs;
    [Export] private TabContainer MainTabs;
}