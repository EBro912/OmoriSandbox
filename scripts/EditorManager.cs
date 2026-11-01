using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Extensions;

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
	private static readonly string[] RuleChoices = ["Use Global", "On", "Off"];
	private readonly Dictionary<string, OptionButton> RuleDropdowns = [];

	public void Init()
	{
		BattlebackBGMEditor.Init(BGMPreview, BattlebackPreview);
		foreach (string skin in Database.GetAllMenuSkinIds())
			MenuSkinDropdown.AddItem(skin);
		MenuSkinDropdown.ItemSelected += _ => WarnIfNoOneColumnArt();
		ItemlessMode.Pressed += WarnIfNoOneColumnArt;
		LockBGMAndBattleback.Toggled += _ => ApplyChangesLock();
		foreach (string skin in Database.GetAllWindowSkinIds())
			WindowSkinDropdown.AddItem(skin);
		foreach ((string key, CheckBox box) in SettingsMenuManager.Instance.RuleSettings)
			AddRuleOverrideRow(key, box);
		
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
			ShowConfirm("Are you sure you want to return?\nAll unsaved progress will be lost.", ReturnToTitle);
		};

		foreach (Control control in AddActorControls)
		{
			control.GetChild<Button>(0).Pressed += () =>
			{
				// each position holds a single battlecard preview
				if (control.GetChildCount() > 1)
					return;
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
			editor.SetChangesLocked(LockBGMAndBattleback.ButtonPressed);
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
				MaxValue = BattleManager.MaxItemCount,
				Value = 1,
				Rounded = true
			};
			container.AddChild(quantity);
			Button remove = new();
			remove.Text = "X";
			remove.Pressed += container.QueueFree;
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

		StartingEnergySlider.ValueChanged += value => StartingEnergyValue.Text = value.ToString();
		FollowupTierSlider.ValueChanged += value => FollowupTierValue.Text = value.ToString();

		ImportButton.Pressed += () =>
		{
			if (PresetManager.Instance.TryGetPreset(ImportPresetDropdown.GetItemText(ImportPresetDropdown.Selected),
					out BattlePreset preset))
			{
				BossRushStageEditorComponent editor = AddStageEditor(stage => stage.CopyFrom(preset));
				if (StageTabs.GetTabCount() == 1)
				{
					editor.ShowEnemies();
					BattlebackPreview.SetBattleback(editor.BattlebackBGMEditor.SelectedBattleback);
				}
			}
		};
		
		AddStageButton.Pressed += () => AddStageEditor();

		DuplicateStageButton.Pressed += () =>
		{
			Control tab = StageTabs.GetTabControl(StageTabs.CurrentTab);
			if (tab is BossRushStageEditorComponent editor)
				AddStageEditor(stage => stage.CopyFrom(editor));
		};

		RemoveStageButton.Pressed += () =>
		{
			if (StageTabs.GetTabCount() == 0)
				return;
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

		if (!PresetManager.IsValidPresetName(PresetInput.Text))
		{
			ShowWindow("Error", "Preset name contains invalid characters");
			return;
		}

		string mod = PresetManager.Instance.GetPresetMod(PresetInput.Text);
		if (mod != null)
		{
			ShowWindow("Error", $"A preset from the mod \"{mod}\" already uses this name");
			return;
		}

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
			ShowConfirm("A preset with this name already exists.\nDo you want to overwrite it?", Save);
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
			EnergyBarVisibility = (EnergyBarVisibility)EnergyBarVisibilityDropdown.Selected,
			DisableDialogue = DisableDialogue.ButtonPressed,
			DisableDamageNumbers = DisableDamageNumbers.ButtonPressed,
			CombinedBuffsDebuffs = CombinedBuffsDebuffs.ButtonPressed,
			LockBGMAndBattleback = LockBGMAndBattleback.ButtonPressed,
			ItemlessMode = ItemlessMode.ButtonPressed,
			MenuSkin = MenuSkinDropdown.GetItemText(MenuSkinDropdown.Selected),
			Toastless = Toastless.ButtonPressed,
			WindowSkin = WindowSkinDropdown.GetItemText(WindowSkinDropdown.Selected),
			RuleOverrides = GetRuleOverrides()
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
				string key = dropdown.GetItemText(dropdown.Selected);
				if (items.ContainsKey(key))
					items[key] += (int)quantity.Value;
				else
					items.Add(dropdown.GetItemText(dropdown.Selected), (int)quantity.Value);
			}
		}

		foreach (Node child in ActorTabs.GetChildren())
		{
			if (child is PartyMemberEditorComponent editor)
			{
				BattlePresetActor actor = new()
				{
					Name = editor.ActorDropdown.GetItemText(editor.ActorDropdown.Selected),
					Level = (int)editor.LevelSlider.Value,
					Weapon = editor.WeaponDropdown.GetItemText(editor.WeaponDropdown.Selected),
					Charm = editor.CharmDropdown.GetItemText(editor.CharmDropdown.Selected),
					Emotion = editor.EmotionDropdown.GetItemText(editor.EmotionDropdown.Selected),
					FollowupSet = editor.FollowupSetDropdown.GetItemText(editor.FollowupSetDropdown.Selected),
					Skills = editor.GetSkills(),
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
					enemies.Add(ToSavedEnemy(editor));
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
						KeepStatusEffects = editor.KeepStatusEffectsCheckbox.ButtonPressed,
						KeepEnergy = editor.KeepEnergyCheckbox.ButtonPressed
					};
					foreach (Node subChild in editor.Enemies.GetChildren())
					{
						if (subChild is EnemyEditorComponent enemyEditor)
							stage.Enemies.Add(ToSavedEnemy(enemyEditor));
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
		EnergyBarVisibilityDropdown.Selected = Enum.IsDefined(preset.EnergyBarVisibility) ? (int)preset.EnergyBarVisibility : 0;
		DisableDialogue.ButtonPressed = preset.DisableDialogue;
		DisableDamageNumbers.ButtonPressed = preset.DisableDamageNumbers;
		CombinedBuffsDebuffs.ButtonPressed = preset.CombinedBuffsDebuffs;
		LockBGMAndBattleback.ButtonPressed = preset.LockBGMAndBattleback;
		ItemlessMode.Disabled = false;
		ItemlessMode.ButtonPressed = preset.ItemlessMode;
		MenuSkinDropdown.Selected = Math.Max(0, Database.GetAllMenuSkinIds().ToList().IndexOf(Database.ResolveMenuSkinId(preset.MenuSkin)));
		Toastless.ButtonPressed = preset.Toastless;
		WindowSkinDropdown.Selected = Math.Max(0, Database.GetAllWindowSkinIds().ToList().IndexOf(Database.ResolveWindowSkinId(preset.WindowSkin)));
		foreach ((string key, OptionButton dropdown) in RuleDropdowns)
			dropdown.Selected = preset.RuleOverrides != null && preset.RuleOverrides.TryGetValue(key, out bool value) ? (value ? 1 : 2) : 0;

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
				MaxValue = BattleManager.MaxItemCount,
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
			if (entry.Position < 0 || entry.Position >= AddActorControls.Length)
			{
				GD.PrintErr($"Invalid position {entry.Position} for party member {entry.Name}, skipping!");
				continue;
			}
			if (!Database.GetAllPartyMemberNames().Contains(entry.Name))
			{
				GD.PushWarning($"Skipping unknown party member \"{entry.Name}\" in preset");
				continue;
			}
			Control card = BattleCard.Instantiate<Control>();
			AddActorControls[entry.Position].AddChild(card);
			card.Position = Vector2.Zero;
			PartyMemberEditorComponent editor = PartyMemberEditor.Instantiate<PartyMemberEditorComponent>();
			ActorTabs.AddChild(editor);
			editor.Init(card, entry, FollowupSets.ResolveId(preset, entry));
		}

		if (preset.Type is GameModeType.Normal)
		{
			foreach (BattlePresetEnemy entry in preset.Enemies)
			{
				if (!Database.GetAllEnemyNames().Contains(entry.Name))
				{
					GD.PushWarning($"Skipping unknown enemy \"{entry.Name}\" in preset");
					continue;
				}
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
				BossRushStageEditorComponent editor = AddStageEditor();
				StageTabs.SetTabTitle(StageTabs.GetTabCount() - 1, entry.StageNumber.ToString());
				editor.BattlebackBGMEditor.SelectedBattleback = entry.Battleback;
				editor.BattlebackBGMEditor.SelectedBGM = entry.BGM;
				editor.BattlebackBGMEditor.BGMPitchValue = entry.BGMPitch;
				editor.BattlebackBGMEditor.BGMLoopPointValue = entry.BGMLoopPoint;
				editor.HealPartyCheckbox.ButtonPressed = entry.HealParty;
				editor.KeepEmotionCheckbox.ButtonPressed = entry.KeepEmotion;
				editor.KeepStatusEffectsCheckbox.ButtonPressed = entry.KeepStatusEffects;
				editor.KeepEnergyCheckbox.ButtonPressed = entry.KeepEnergy;
				foreach (BattlePresetEnemy enemy in entry.Enemies)
				{
					if (!Database.GetAllEnemyNames().Contains(enemy.Name))
					{
						GD.PushWarning($"Skipping unknown enemy \"{enemy.Name}\" in preset");
						continue;
					}
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

		ApplyChangesLock();
		PresetInput.Text = presetName;
		MainMenuManager.Instance.LastLoadedPreset = presetName;
	}

	private void ApplyChangesLock()
	{
		bool locked = LockBGMAndBattleback.ButtonPressed;
		foreach (Node child in EnemyTabs.GetChildren())
		{
			if (child is EnemyEditorComponent editor)
				editor.SetChangesLocked(locked);
		}
		foreach (Node child in StageTabs.GetChildren())
		{
			if (child is not BossRushStageEditorComponent stage)
				continue;
			stage.ChangesLocked = locked;
			foreach (Node subChild in stage.Enemies.GetChildren())
			{
				if (subChild is EnemyEditorComponent editor)
					editor.SetChangesLocked(locked);
			}
		}
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

	private void AddRuleOverrideRow(string key, CheckBox box)
	{
		Label label = new()
		{
			Text = box.Text,
			TooltipText = box.TooltipText,
			CustomMinimumSize = new Vector2(280, 0),
			ClipText = true,
			TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
			MouseFilter = Control.MouseFilterEnum.Pass
		};
		label.AddThemeFontOverride("font", ItemlessMode.GetThemeFont("font"));
		label.AddThemeFontSizeOverride("font_size", ItemlessMode.GetThemeFontSize("font_size"));
		OptionButton dropdown = new();
		foreach (string choice in RuleChoices)
			dropdown.AddItem(choice);
		dropdown.AddThemeFontOverride("font", MenuSkinDropdown.GetThemeFont("font"));
		dropdown.AddThemeFontSizeOverride("font_size", MenuSkinDropdown.GetThemeFontSize("font_size"));
		foreach (string style in new[] { "normal", "pressed", "hover" })
			dropdown.AddThemeStyleboxOverride(style, MenuSkinDropdown.GetThemeStylebox(style));
		RuleOverridesGrid.AddChild(label);
		RuleOverridesGrid.AddChild(dropdown);
		RuleDropdowns[key] = dropdown;
	}

	private Dictionary<string, bool> GetRuleOverrides()
	{
		Dictionary<string, bool> overrides = [];
		foreach ((string key, OptionButton dropdown) in RuleDropdowns)
		{
			if (dropdown.Selected > 0)
				overrides[key] = dropdown.Selected == 1;
		}
		return overrides;
	}

	private void WarnIfNoOneColumnArt()
	{
		string id = MenuSkinDropdown.GetItemText(MenuSkinDropdown.Selected);
		if (ItemlessMode.ButtonPressed && !Database.GetMenuSkin(id).HasOneColumnCommands)
			ShowWindow("Warning", $"The {id} menu skin has no one-column ATTACK/SKILL art.\nItemless Mode will use the Dreamworld art instead.");
	}

	private void PreDelete()
	{
		if (PresetDropdown.Selected == -1)
			return;

		string preset = PresetDropdown.GetItemText(PresetDropdown.Selected);
		string mod = PresetManager.Instance.GetPresetMod(preset);
		if (mod != null)
		{
			ShowWindow("Error", $"The {preset} preset comes from the mod \"{mod}\" and can't be deleted");
			return;
		}
		ShowConfirm("Are you sure you want to delete the " + preset + " preset?", Delete);
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
		ShowConfirm("Are you sure you want to reset everything to default?\nIf you have not created a preset, this action cannot be undone.", ResetToDefault);
	}

	private void ResetToDefault()
	{
		// remove battlecard previews (child 0 is the add button)
		foreach (Control control in AddActorControls)
		{
			for (int i = control.GetChildCount() - 1; i > 0; i--)
				control.GetChild(i).Free();
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
		StartingEnergySlider.Value = 3;
		EnergyBarVisibilityDropdown.Selected = 0;
		DisableDialogue.ButtonPressed = false;
		DisableDamageNumbers.ButtonPressed = false;
		CombinedBuffsDebuffs.ButtonPressed = false;
		LockBGMAndBattleback.ButtonPressed = false;
		ApplyChangesLock();
		ItemlessMode.Disabled = false;
		ItemlessMode.ButtonPressed = false;
		MenuSkinDropdown.Selected = 0;
		Toastless.ButtonPressed = false;
		WindowSkinDropdown.Selected = 0;
		foreach (OptionButton dropdown in RuleDropdowns.Values)
			dropdown.Selected = 0;

		BGMPreview.Stop();
		BattlebackBGMEditor.Reset();

		PresetInput.Text = string.Empty;
		MainTabs.CurrentTab = 0;
	}

	private static BattlePresetEnemy ToSavedEnemy(EnemyEditorComponent editor)
	{
		BattlePresetEnemy enemy = editor.ToPreset();
		if (enemy.Emotion is "hurt" or "toast")
			enemy.Emotion = "neutral";
		return enemy;
	}

	private BossRushStageEditorComponent AddStageEditor(Action<BossRushStageEditorComponent> setup = null)
	{
		BossRushStageEditorComponent editor = StageEditor.Instantiate<BossRushStageEditorComponent>();
		editor.BattlebackBGMEditor.Init(BGMPreview, BattlebackPreview);
		StageTabs.AddChild(editor);
		setup?.Invoke(editor);
		ApplyChangesLock();
		int total = StageTabs.GetTabCount();
		StageTabs.SetTabTitle(total - 1, total.ToString());
		return editor;
	}

	private void ShowConfirm(string message, Action confirmed)
	{
		ConfirmationDialog dialog = new()
		{
			Title = "Confirmation",
			DialogText = message,
			Unresizable = true
		};
		dialog.Confirmed += () =>
		{
			confirmed();
			dialog.QueueFree();
		};
		dialog.Canceled += dialog.QueueFree;
		AddChild(dialog);
		dialog.PopupCentered();
		dialog.Show();
	}

	private void ShowWindow(string title, string message)
	{
		this.ShowNoteDialog(title, message);
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
	[Export] private OptionButton EnergyBarVisibilityDropdown;
	[Export] private CheckBox DisableDialogue;
	[Export] private CheckBox DisableDamageNumbers;
	[Export] private CheckBox CombinedBuffsDebuffs;
	[Export] private CheckBox LockBGMAndBattleback;
	[Export] private CheckBox ItemlessMode;
	[Export] private OptionButton MenuSkinDropdown;
	[Export] private OptionButton WindowSkinDropdown;
	[Export] private CheckBox Toastless;
	[Export] private GridContainer RuleOverridesGrid;
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
