using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Editor;

internal partial class EnemyEditorComponent : Control
{
	[Export] public OptionButton AreaDropdown { get; private set; }
	[Export] public OptionButton EnemyDropdown { get; private set; }
	[Export] public OptionButton EmotionDropdown { get; private set; }
	[Export] public SpinBox XPosBox { get; private set; }
	[Export] public SpinBox YPosBox { get; private set; }
	[Export] public SpinBox LayerBox { get; private set; }
	[Export] public CheckBox FallsOffScreenCheckbox { get; private set; }
	[Export] public CheckBox GrayscaleOnDefeatCheckbox { get; private set; }
	[Export] private CheckBox PreventBattlebackCheckbox;
	[Export] private CheckBox PreventBGMCheckbox;
	[Export] private CheckBox VisibleCheckbox;
	[Export] private Button RemoveButton;
	[Export] private StatAdjustmentEditor StatAdjustmentEditor;

	private AnimatedSprite2D Animator;
	private Enemy CurrentEnemy;
	private string CurrentEnemyId;
	private bool ChangesLocked;
	private bool RememberedPreventBattleback;
	private bool RememberedPreventBGM;

	internal bool PreventBattlebackChanges => ChangesLocked ? RememberedPreventBattleback : PreventBattlebackCheckbox.ButtonPressed;
	internal bool PreventBGMChanges => ChangesLocked ? RememberedPreventBGM : PreventBGMCheckbox.ButtonPressed;

	internal void SetChangesLocked(bool locked)
	{
		if (locked == ChangesLocked)
			return;
		ChangesLocked = locked;
		if (locked)
		{
			RememberedPreventBattleback = PreventBattlebackCheckbox.ButtonPressed;
			RememberedPreventBGM = PreventBGMCheckbox.ButtonPressed;
			PreventBattlebackCheckbox.ButtonPressed = true;
			PreventBGMCheckbox.ButtonPressed = true;
		}
		else
		{
			PreventBattlebackCheckbox.ButtonPressed = RememberedPreventBattleback;
			PreventBGMCheckbox.ButtonPressed = RememberedPreventBGM;
		}
		PreventBattlebackCheckbox.Disabled = locked;
		PreventBGMCheckbox.Disabled = locked;
	}

	// registered emotions plus the pseudo-states enemies can be spawned in
	private static string[] States => [.. Database.GetAllEmotionIds(), "hurt", "toast"];

	public override void _Ready()
	{
		AreaDropdown.AddItem("All");
		AreaDropdown.SetItemMetadata(0, "");
		foreach ((string key, string label) in Database.GetEnemyAreas())
		{
			AreaDropdown.AddItem(label);
			AreaDropdown.SetItemMetadata(AreaDropdown.ItemCount - 1, key);
		}
		AreaDropdown.Selected = 0;
		AreaDropdown.ItemSelected += idx => FillEnemies(AreaDropdown.GetItemMetadata((int)idx).AsString());

		foreach (string member in Database.GetAllEnemyNames())
			EnemyDropdown.AddItem(member);

		EnemyDropdown.Selected = EnemyDropdown.GetItemIndex("LostSproutMole");
		EnemyDropdown.ItemSelected += idx => Populate(EnemyDropdown.GetItemText((int)idx));
		// scroll the (long) enemy list to the current enemy when the dropdown is opened
		PopupMenu popup = EnemyDropdown.GetPopup();
		popup.AboutToPopup += () => popup.SetFocusedItem(EnemyDropdown.Selected);
		StatAdjustmentEditor.StatsAdjusted += RefreshStatDisplay;
		EmotionDropdown.ItemSelected += idx => UpdateState(EmotionDropdown.GetItemText((int)idx));

		VisibleCheckbox.Toggled += pressed => Animator.Visible = pressed;

		XPosBox.ValueChanged += value => Animator.GlobalPosition = new Vector2((float)value, Animator.GlobalPosition.Y);
		YPosBox.ValueChanged += value => Animator.GlobalPosition = new Vector2(Animator.GlobalPosition.X, (float)value);

		LayerBox.ValueChanged += value => Animator.ZIndex = -5 - (int)value;
	}

	public void Init(AnimatedSprite2D animator)
	{
		Animator = animator;
		Animator.Centered = true;
		Animator.ZIndex = -5;
		Animator.GlobalPosition = new Vector2((float)XPosBox.Value, (float)YPosBox.Value);

		RemoveButton.Pressed += () =>
		{
			Animator.QueueFree();
			QueueFree();
		};

		Populate("LostSproutMole");
	}

	public void Init(AnimatedSprite2D animator, BattlePresetEnemy enemy)
	{
		if (!enemy.Position.StartsWith("Vector2"))
			enemy.Position = "Vector2" + enemy.Position;
		Init(animator, enemy.Name, GD.StrToVar(enemy.Position).AsVector2(), enemy.Emotion, (int)enemy.Layer, enemy.FallsOffScreen, enemy.GrayscaleOnDefeat, enemy.AdjustedStats, enemy.PreventBattlebackChanges, enemy.PreventBGMChanges);
	}

	public void Init(AnimatedSprite2D animator, string name, Vector2 position, string emotion, int layer, bool fallsOffScreen, bool grayscaleOnDefeat, Stats adjustedStats = default, bool preventBattlebackChanges = false, bool preventBGMChanges = false)
	{
		Animator = animator;
		Animator.Centered = true;
		Animator.ZIndex = -5 - layer;

		RemoveButton.Pressed += () =>
		{
			Animator.QueueFree();
			QueueFree();
		};

		StatAdjustmentEditor.SetStats(adjustedStats);
		Populate(name);
		EnemyDropdown.Selected = EnemyDropdown.GetItemIndex(name);
		int emotionIndex = EmotionDropdown.GetItemIndex(emotion);
		if (emotionIndex == -1)
		{
			// the preset carries an emotion this enemy doesn't support
			GD.PushWarning($"Enemy {name} cannot be spawned as \"{emotion}\", falling back to neutral.");
			emotionIndex = 0;
			emotion = EmotionDropdown.GetItemText(0);
		}
		EmotionDropdown.Selected = emotionIndex;
		LayerBox.Value = layer;
		XPosBox.SetValueNoSignal(position.X);
		YPosBox.SetValueNoSignal(position.Y);
		Animator.GlobalPosition = position;
		UpdateState(emotion);
		FallsOffScreenCheckbox.ButtonPressed = fallsOffScreen;
		GrayscaleOnDefeatCheckbox.ButtonPressed = grayscaleOnDefeat;
		PreventBattlebackCheckbox.ButtonPressed = preventBattlebackChanges;
		PreventBGMCheckbox.ButtonPressed = preventBGMChanges;
	}

	public void Populate(string who)
	{
		Enemy enemy = Database.CreateEnemy(who);
		if (enemy == null)
			return; // CreateEnemy already logged the unknown name

		SpriteFrames animation = enemy.Animation;
		if (animation == null)
		{
			GD.PrintErr("Failed to load animations for Enemy: " + who);
			return;
		}
		
		Node parent = GetParent();
		if (parent is TabContainer container)
		{
			int index = container.GetTabIdxFromControl(this);
			container.SetTabTitle(index, who);
		}

		Animator.SpriteFrames = animation;
		Animator.Animation = "neutral";
		Animator.Play();

		FallsOffScreenCheckbox.ButtonPressed = enemy.FallsOffScreen;
		GrayscaleOnDefeatCheckbox.ButtonPressed = enemy.GrayscaleOnDefeat;

		// keep the selected emotion across enemy changes when the new enemy supports it
		string previousEmotion = EmotionDropdown.Selected > -1 ? EmotionDropdown.GetItemText(EmotionDropdown.Selected) : null;
		EmotionDropdown.Clear();
		foreach (string state in States)
		{
			bool valid = state switch
			{
				"hurt" => animation.HasAnimation("hurt"),
				"toast" => true,
				_ => Database.TryGetEmotion(state, out Emotion emotion) && enemy.IsEmotionValid(emotion)
			};
			if (valid)
			{
				EmotionDropdown.AddItem(state);
			}
		}
		int restored = previousEmotion != null ? EmotionDropdown.GetItemIndex(previousEmotion) : -1;
		EmotionDropdown.Selected = restored > -1 ? restored : 0;
		UpdateState(EmotionDropdown.GetItemText(EmotionDropdown.Selected));

		CurrentEnemy = enemy;
		CurrentEnemyId = who;
		RefreshStatDisplay();
	}

	private void FillEnemies(string areaKey, bool pickFirst = true)
	{
		EnemyDropdown.Clear();
		foreach (string member in areaKey == "" ? Database.GetAllEnemyNames() : Database.GetEnemyNamesInArea(areaKey))
			EnemyDropdown.AddItem(member);

		int index = CurrentEnemyId != null ? EnemyDropdown.GetItemIndex(CurrentEnemyId) : -1;
		if (index > -1)
		{
			EnemyDropdown.Selected = index;
			return;
		}
		if (EnemyDropdown.ItemCount == 0)
			return;
		EnemyDropdown.Selected = 0;
		if (pickFirst)
			Populate(EnemyDropdown.GetItemText(0));
	}

	private void RefreshStatDisplay()
	{
		if (CurrentEnemy == null)
			return;
		Stats display = CurrentEnemy.DeclaredStats + StatAdjustmentEditor.GetStats();
		// finalize like Actor.CurrentStats so the preview matches in-battle values
		display.ApplyStatLimits();
		StatAdjustmentEditor.UpdateStats(display);
	}

	internal BattlePresetEnemy ToPreset()
	{
		return new BattlePresetEnemy
		{
			Name = EnemyDropdown.GetItemText(EnemyDropdown.Selected),
			Position = GD.VarToStr(new Vector2((float)XPosBox.Value, (float)YPosBox.Value)),
			Emotion = EmotionDropdown.GetItemText(EmotionDropdown.Selected),
			Layer = (int)LayerBox.Value,
			FallsOffScreen = FallsOffScreenCheckbox.ButtonPressed,
			GrayscaleOnDefeat = GrayscaleOnDefeatCheckbox.ButtonPressed,
			PreventBattlebackChanges = PreventBattlebackChanges,
			PreventBGMChanges = PreventBGMChanges,
			AdjustedStats = GetAdjustedStats()
		};
	}

	public Stats GetAdjustedStats()
	{
		return StatAdjustmentEditor.GetStats();
	}

	public void UpdateState(string state)
	{
		Animator.Animation = Database.TryGetEmotion(state, out Emotion emotion) ? Actor.EnemyEmotionAnimation(Animator.SpriteFrames, emotion) : state;
		Animator.Play();
	}
}
