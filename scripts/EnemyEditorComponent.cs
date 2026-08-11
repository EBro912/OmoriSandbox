using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Editor;

internal partial class EnemyEditorComponent : Control
{
	[Export] public OptionButton EnemyDropdown { get; private set; }
	[Export] public OptionButton EmotionDropdown { get; private set; }
	[Export] public SpinBox XPosBox { get; private set; }
	[Export] public SpinBox YPosBox { get; private set; }
	[Export] public SpinBox LayerBox { get; private set; }
	[Export] public CheckBox FallsOffScreenCheckbox { get; private set; }
	[Export] public CheckBox GrayscaleOnDefeatCheckbox { get; private set; }
	[Export] private CheckBox VisibleCheckbox;
	[Export] private Button RemoveButton;
	[Export] private StatAdjustmentEditor StatAdjustmentEditor;

	private AnimatedSprite2D Animator;
	private Enemy CurrentEnemy;

	// registered emotions plus the pseudo-states enemies can be spawned in
	private static string[] States => [.. Database.GetAllEmotionIds(), "hurt", "toast"];

	public override void _Ready()
	{
		foreach (string member in Database.GetAllEnemyNames())
			EnemyDropdown.AddItem(member);

		EnemyDropdown.Selected = EnemyDropdown.GetItemIndex("LostSproutMole");
		EnemyDropdown.ItemSelected += (idx) => Populate(EnemyDropdown.GetItemText((int)idx));
		// scroll the (long) enemy list to the current enemy when the dropdown is opened
		PopupMenu popup = EnemyDropdown.GetPopup();
		popup.AboutToPopup += () => popup.SetFocusedItem(EnemyDropdown.Selected);
		StatAdjustmentEditor.StatsAdjusted += RefreshStatDisplay;
		EmotionDropdown.ItemSelected += (idx) => UpdateState(EmotionDropdown.GetItemText((int)idx));

		VisibleCheckbox.Toggled += (pressed) => Animator.Visible = pressed;

		XPosBox.ValueChanged += (value) => Animator.GlobalPosition = new Vector2((float)value, Animator.GlobalPosition.Y);
		YPosBox.ValueChanged += (value) => Animator.GlobalPosition = new Vector2(Animator.GlobalPosition.X, (float)value);

		LayerBox.ValueChanged += (value) => Animator.ZIndex = -5 - (int)value;
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
		Init(animator, enemy.Name, GD.StrToVar(enemy.Position).AsVector2(), enemy.Emotion, (int)enemy.Layer, enemy.FallsOffScreen, enemy.GrayscaleOnDefeat, enemy.AdjustedStats);
	}

	public void Init(AnimatedSprite2D animator, string name, Vector2 position, string emotion, int layer, bool fallsOffScreen, bool grayscaleOnDefeat, Stats adjustedStats = default)
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
		RefreshStatDisplay();
	}

	private void RefreshStatDisplay()
	{
		if (CurrentEnemy == null)
			return;
		StatAdjustmentEditor.UpdateStats(CurrentEnemy.DeclaredStats + StatAdjustmentEditor.GetStats());
	}

	public Stats GetAdjustedStats()
	{
		return StatAdjustmentEditor.GetStats();
	}

	public void UpdateState(string state)
	{
		Animator.Animation = state;
		Animator.Play();
	}
}
