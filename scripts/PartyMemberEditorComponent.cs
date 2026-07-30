using System.Collections.Generic;
using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Extensions;
using System.Linq;

namespace OmoriSandbox.Editor;
internal partial class PartyMemberEditorComponent : Control
{
	[Export] public OptionButton ActorDropdown { get; private set; }
	[Export] public OptionButton WeaponDropdown { get; private set; }
	[Export] public OptionButton CharmDropdown { get; private set; }
	[Export] public OptionButton EmotionDropdown { get; private set; }
	[Export] public HSlider LevelSlider { get; private set; }
	[Export] private Label LevelSliderValue;
	[Export] public OptionButton FollowupSetDropdown { get; private set; }
	[Export] public LineEdit AttackSkill { get; private set; }
	[Export] public LineEdit[] Skills;
	[Export] private Button RemoveButton;
	[Export] private CheckBox FilterEquippableCheckbox;
	[Export] private StatAdjustmentEditor StatAdjustmentEditor;

	private Control BattleCard;
	private AnimatedSprite2D Face;
	private StateAnimator Animator;
	private Label HealthLabel;
	private Label JuiceLabel;
	private PartyMember SelectedPartyMember;
	private Equipment SelectedWeapon;
	private Equipment SelectedCharm;
	private Stats BaseStats;
	private Emotion SelectedEmotion;

	public int ActorPosition { get; private set; }

	// registered emotions plus the pseudo-states party members can be spawned in
	private static string[] States => [.. Database.GetAllEmotionIds(), "hurt", "toast", "victory"];
	private string[] EquippableWeapons = [];
	
	public override void _Ready()
	{
		foreach (string member in Database.GetAllPartyMemberNames())
			ActorDropdown.AddItem(member);

		ActorDropdown.Selected = ActorDropdown.GetItemIndex("Omori");
		ActorDropdown.ItemSelected += (idx) =>
		{
			Populate(ActorDropdown.GetItemText((int)idx));
			FilterWeapons(FilterEquippableCheckbox.ButtonPressed);
			RefreshSelected((int)LevelSlider.Value - 1);
			RecalculateStats();
		};
		EmotionDropdown.ItemSelected += (idx) =>
		{
			UpdateState(EmotionDropdown.GetItemText((int)idx));
			RecalculateStats();
		};

		foreach (string weapon in Database.GetAllWeaponNames())
			WeaponDropdown.AddItem(weapon);
		WeaponDropdown.ItemSelected += (item) =>
		{
			SelectWeapon(WeaponDropdown.GetItemText((int)item));
			RecalculateStats();
		};

		CharmDropdown.AddItem("None");
		foreach (string charm in Database.GetAllCharmNames())
			CharmDropdown.AddItem(charm);
		CharmDropdown.ItemSelected += (item) =>
		{
			if (item == 0)
				SelectedCharm = null;
			else 
				SelectCharm(CharmDropdown.GetItemText((int)item));
			RecalculateStats();
		};

		FollowupSetDropdown.AddItem(FollowupSets.NoneId);
		foreach (FollowupSet set in FollowupSets.All)
			FollowupSetDropdown.AddItem(set.Id);

		FilterEquippableCheckbox.Toggled += (toggled) =>
		{
			FilterWeapons(toggled);
			RecalculateStats();
		};

		StatAdjustmentEditor.StatsAdjusted += RecalculateStats;

		LevelSlider.ValueChanged += (value) =>
		{
			LevelSliderValue.Text = value.ToString();
			int level = (int)value - 1;
			BaseStats = new Stats(SelectedPartyMember.HPTree[level], SelectedPartyMember.JuiceTree[level],
				SelectedPartyMember.ATKTree[level], SelectedPartyMember.DEFTree[level],
				SelectedPartyMember.SPDTree[level], SelectedPartyMember.BaseLuck, 0);
			RecalculateStats();
		};
	}

	public void Init(Control battleCard, int position)
	{
		BattleCard = battleCard;
		Animator = BattleCard.GetNode<StateAnimator>("Battlecard/StateAnimatorComponent");
		Face = BattleCard.GetNode<AnimatedSprite2D>("Battlecard/Face");
		HealthLabel = BattleCard.GetNode<Label>("Battlecard/HealthLabel");
		JuiceLabel = BattleCard.GetNode<Label>("Battlecard/JuiceLabel");
		
		ActorPosition = position;
		FollowupSetDropdown.Selected = FollowupSetDropdown.GetItemIndex(FollowupSets.DefaultIdForPosition(position));

		RemoveButton.Pressed += () =>
		{
			BattleCard.QueueFree();
			QueueFree();
		};

		WeaponDropdown.Selected = 0;
		// charms are optional so we can leave it unselected
		Populate("Omori");
		RefreshSelected(0);
		RecalculateStats();
	}

	public void Init(Control battleCard, BattlePresetActor actor, string followupSetId)
	{
		Init(battleCard, actor.Name, actor.Weapon, actor.Charm, actor.Level, followupSetId, actor.Emotion, actor.Skills, actor.Position, actor.AdjustedStats);
	}

	public void Init(Control battleCard, string name, string weapon, string charm, int level, string followupSetId, string emotion, string[] skills, int position, Stats adjustedStats)
	{
		BattleCard = battleCard;
		Animator = BattleCard.GetNode<StateAnimator>("Battlecard/StateAnimatorComponent");
		Face = BattleCard.GetNode<AnimatedSprite2D>("Battlecard/Face");
		HealthLabel = BattleCard.GetNode<Label>("Battlecard/HealthLabel");
		JuiceLabel = BattleCard.GetNode<Label>("Battlecard/JuiceLabel");
		StatAdjustmentEditor.SetStats(adjustedStats);

		ActorPosition = position;

		RemoveButton.Pressed += () =>
		{
			BattleCard.QueueFree();
			QueueFree();
		};
		
		ActorDropdown.Selected = ActorDropdown.GetItemIndex(name);
		Populate(name);
		WeaponDropdown.Selected = WeaponDropdown.GetItemIndex(weapon);
		CharmDropdown.Selected = CharmDropdown.GetItemIndex(charm);
		EmotionDropdown.Selected = EmotionDropdown.GetItemIndex(emotion);
		int followupIndex = FollowupSetDropdown.GetItemIndex(followupSetId);
		FollowupSetDropdown.Selected = followupIndex != -1
			? followupIndex
			: FollowupSetDropdown.GetItemIndex(FollowupSets.DefaultIdForPosition(position));
		LevelSlider.SetValueNoSignal(level);
		LevelSliderValue.Text = level.ToString();
		UpdateState(emotion);
		if (skills.Length > 0)
		{
			// first index should always be the attack skill
			AttackSkill.Text = skills[0];
			for (int i = 1; i < skills.Length; i++)
			{
				Skills[i - 1].Text = skills[i];
			}
		}

		int index = level - 1;
		RefreshSelected(index);
		RecalculateStats();
	}

	public void Populate(string who)
	{
		Node parent = GetParent();
		if (parent is TabContainer container)
		{
			int index = container.GetTabIdxFromControl(this);
			container.SetTabTitle(index, who);
		}
		
		SelectedPartyMember = Database.CreatePartyMember(who);
		if (SelectedPartyMember == null)
			return; // CreatePartyMember already logged the unknown name

		string attackSkill;
		if (SelectedPartyMember is SunnyAlt)
			attackSkill = "SRWAltAttack";
		else if (SelectedPartyMember.IsRealWorld)
			attackSkill = SelectedPartyMember.Name[0] + "RWAttack";
		else
			attackSkill = SelectedPartyMember.Name[0] + "Attack";

		if (Database.TryGetSkill(attackSkill, out _))
			AttackSkill.Text = attackSkill;

		SpriteFrames animation = SelectedPartyMember.Animation;
		if (animation == null)
		{
			GD.PrintErr("Failed to load Face animations for PartyMember: " + who);
			return;
		}

		Face.SpriteFrames = animation;
		Face.Play("neutral");
		Animator.SetState("neutral");
		SelectedEmotion = null;

		LevelSlider.SetValueNoSignal(1);
		LevelSliderValue.Text = "1";
		LevelSlider.MinValue = 1;
		LevelSlider.MaxValue = SelectedPartyMember.HPTree.Length;

		EmotionDropdown.Clear();
		foreach (string state in States.Except(SelectedPartyMember.InvalidStates))
			EmotionDropdown.AddItem(state);
		EmotionDropdown.Selected = 0;

		EquippableWeapons = SelectedPartyMember.EquippableWeapons;
	}

	private void UpdateState(string state)
	{
		Face.Animation = state;
		switch (state)
		{
			// hurt doesn't update the emotion HUD
			case "hurt":
				break;
			case "toast":
				Animator.ShowAsset(EmotionAsset.Toast);
				break;
			case "victory":
				Animator.ShowAsset(EmotionAsset.Victory);
				break;
			default:
				Animator.SetState(state);
				break;
		}
		// pseudo-states like hurt/toast/victory aren't emotions and preview no stat bonuses
		SelectedEmotion = Database.TryGetEmotion(state, out Emotion emotion) ? emotion : null;
	}

	public Stats GetAdjustedStats()
	{
		return StatAdjustmentEditor.GetStats();
	}

	private void FilterWeapons(bool toggled)
	{
		string original = WeaponDropdown.GetItemText(WeaponDropdown.Selected);
		WeaponDropdown.Clear();
		IEnumerable<string> weapons = Database.GetAllWeaponNames();
		if (toggled && EquippableWeapons != null && EquippableWeapons.Length > 0)
			weapons = weapons.Where(w => EquippableWeapons.Contains(w));
		foreach (string weapon in weapons)
			WeaponDropdown.AddItem(weapon);
		int index = WeaponDropdown.GetItemIndex(original);
		if (index != -1)
			WeaponDropdown.Selected = index;
		SelectWeapon(WeaponDropdown.GetItemText(WeaponDropdown.Selected));
	}

	private void RecalculateStats()
	{
		Stats stats = BaseStats + StatAdjustmentEditor.GetStats();
		SelectedWeapon.Apply(ref stats);
		SelectedCharm?.Apply(ref stats);
		if (SelectedEmotion != null)
			StatBonus.ApplyAll(ref stats, SelectedEmotion.StatBonuses);
		HealthLabel.Text = $"{stats.MaxHP}/{stats.MaxHP}";
		JuiceLabel.Text = $"{stats.MaxJuice}/{stats.MaxJuice}";
		StatAdjustmentEditor.UpdateStats(stats);
	}

	private void RefreshSelected(int index)
	{
		BaseStats = new Stats(SelectedPartyMember.HPTree[index], SelectedPartyMember.JuiceTree[index], 
			SelectedPartyMember.ATKTree[index], SelectedPartyMember.DEFTree[index],SelectedPartyMember.SPDTree[index], SelectedPartyMember.BaseLuck, 0);
		SelectWeapon(WeaponDropdown.GetItemText(WeaponDropdown.Selected));
		SelectCharm(CharmDropdown.GetItemText(CharmDropdown.Selected));
	}

	private void SelectWeapon(string name)
	{
		if (Database.TryGetEquipment(name, out Equipment weapon))
			SelectedWeapon = weapon;
	}

	private void SelectCharm(string name)
	{
		if (Database.TryGetEquipment(name, out Equipment charm))
			SelectedCharm = charm;
	}
}
