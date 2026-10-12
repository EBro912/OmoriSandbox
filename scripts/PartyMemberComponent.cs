using Godot;
using OmoriSandbox.Actors;
using System;
using System.Collections.Generic;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Editor;

namespace OmoriSandbox;

/// <summary>
/// The component attached to a party member <see cref="Node"/> in the scene.
/// </summary>
public partial class PartyMemberComponent : ActorComponent
{
	private PartyMember PartyMember;
	private StateAnimator StateAnimator;
	private TextureRect SelectedBox;
	private HFlowContainer StateIcons;
	private TextureProgressBar HPBar;
	private TextureProgressBar JuiceBar;
	private Label HPLabel;
	private Label JuiceLabel;
	
	private float DisplayedHP;
	private float DisplayedJuice;
	private int HPRollTarget;
	private int JuiceRollTarget;
	private float HPRollFrames;
	private float JuiceRollFrames;
	private int MaxHP;
	private int MaxJuice;
	private readonly Queue<PendingDamageNumber> PendingDamageNumbers = new();
	private int PopupCount;

	/// <summary>
	/// The <see cref="Actors.PartyMember"/> actor this component is attached to.
	/// </summary>
	public override PartyMember Actor => PartyMember;
	
	private FollowupBubbles FollowupBubbles;
	
	/// <summary>
	/// The on-screen position of the <see cref="Actors.PartyMember"/>.<br/>
	/// See <see cref="BattleManager.GetPartyMemberAtPosition(int)"/> for valid positions.
	/// </summary>
	public int Position { get; private set; }
	/// <summary>
	/// Whether the <see cref="Actors.PartyMember"/> has a followup.
	/// </summary>
	public bool HasFollowup => FollowupBubbles != null;
	/// <summary>
	/// The followup set assigned to the <see cref="Actors.PartyMember"/>, or null when followups are disabled.
	/// </summary>
	internal FollowupSet FollowupSet { get; private set; }

	private Timer HurtTimer = new()
	{
		Autostart = false,
		OneShot = true
	};

	internal bool SetPartyMember(PartyMember partyMember, PackedScene followup, FollowupSet set, BattlePresetActor actor)
	{
		partyMember.Bind(this);
		PartyMember = partyMember;
		AnimatedSprite2D face = GetNode<AnimatedSprite2D>("../Battlecard/Face");
		StateAnimator = GetNode<StateAnimator>("../Battlecard/StateAnimatorComponent");
		if (actor.Emotion is "hurt" or "victory")
			actor.Emotion = "neutral";
		if (!PartyMember.Init(face, actor))
			return false;
		HPLabel = GetNode<Label>("../Battlecard/Health/HealthLabel/");
		HPBar = GetNode<TextureProgressBar>("../Battlecard/Health");
		JuiceLabel = GetNode<Label>("../Battlecard/Juice/JuiceLabel");
		JuiceBar = GetNode<TextureProgressBar>("../Battlecard/Juice");
		SelectedBox = GetNode<TextureRect>("../SelectedCard");
		StateIcons = GetNode<HFlowContainer>("../StateIcons");
		if (actor.Position % 2 == 0)
		{
			StateIcons.Position = new Vector2(0, -115);
			StateIcons.ReverseFill = true;
		}

		MaxHP = PartyMember.CurrentStats.MaxHP;
		MaxJuice = PartyMember.CurrentStats.MaxJuice;
		HPBar.MaxValue = Math.Max(1, MaxHP);
		HPBar.Value = PartyMember.CurrentHP;
		JuiceBar.MaxValue = Math.Max(1, MaxJuice);
		JuiceBar.Value = PartyMember.CurrentJuice;
		DisplayedHP = PartyMember.CurrentHP;
		DisplayedJuice = PartyMember.CurrentJuice;
		HPRollTarget = PartyMember.CurrentHP;
		JuiceRollTarget = PartyMember.CurrentJuice;
		
		HPLabel.Text = $"{Mathf.RoundToInt(DisplayedHP)}/{MaxHP}";
		JuiceLabel.Text = $"{Mathf.RoundToInt(DisplayedJuice)}/{MaxJuice}";

		if (followup != null)
		{
			FollowupBubbles bubbles = followup.Instantiate<FollowupBubbles>();
			bubbles.ApplySet(set, actor.Position);
			bubbles.ZIndex = 3;
			GetParent().AddChild(bubbles);
			FollowupBubbles = bubbles;
			FollowupSet = set;
		}

		Position = actor.Position;

		RefreshCenterPoint();
		PartyMember.OnEmotionChanged += EmotionChanged;
		PartyMember.OnAnimationChanged += AnimationChanged;
		PartyMember.OnDamaged += Damaged;
		PartyMember.OnStatModifierAdded += StatModifiersChanged;
		PartyMember.OnStatModifierRemoved += StatModifiersChanged;
		HurtTimer.Timeout += () => PartyMember.SetHurt(false);
		AddChild(HurtTimer);

		// Init already validated the emotion, delay this call to let everything initialize
		Callable.From(RefreshStateVisuals).CallDeferred();

		return true;
	}

	internal void RefreshCenterPoint()
	{
		PartyMember.CenterPoint = GetParent<Control>().GlobalPosition + new Vector2(57, 82);
	}

	internal override void ShowDamageNumber(int damage, DamageType type, bool critical)
	{
		QueueDamageNumber(damage, type, critical);
	}

	internal void QueueDamageNumber(int damage, DamageType type, bool critical)
	{
		PendingDamageNumbers.Enqueue(new PendingDamageNumber(damage, type, critical));
	}

	internal Vector2 DamageNumberPosition(int queued, int popupCount)
	{
		return GetParent<Control>().GlobalPosition + new Vector2(56f, 82f - 20f * (float)Math.Floor(queued / 3f) + 40f * popupCount);
	}

	private void RefreshStateVisuals()
	{
		// members spawning with an active animation override show its asset instead of their emotion
		if (PartyMember.CurrentAnimationAsset != null)
			StateAnimator.ShowAsset(PartyMember.CurrentAnimationAsset);
		else
			StateAnimator.SetState(PartyMember.CurrentEmotion.Id);
	}

	private void EmotionChanged(object sender, EventArgs e)
	{
		// while an animation override is active (such as plot armor), only the above-head label follows emotion changes
		if (PartyMember.CurrentAnimation != null)
			StateAnimator.SetStateAtlas(PartyMember.CurrentEmotion.Id);
		else
			StateAnimator.SetState(PartyMember.CurrentEmotion.Id);
	}

	private void AnimationChanged(object sender, EventArgs e)
	{
		// an override without an asset (base PlayAnimation) keeps the battlecard on the emotion
		if (PartyMember.CurrentAnimation != null && PartyMember.CurrentAnimationAsset != null)
			StateAnimator.ShowAsset(PartyMember.CurrentAnimationAsset);
		else
			StateAnimator.SetState(PartyMember.CurrentEmotion.Id);
	}

	private void Damaged(object sender, EventArgs e)
	{
		PartyMember.SetHurt(true);
		HurtTimer.Start(1d);
	}

	private void StatModifiersChanged(object sender, StatModifierEventArgs e)
	{
		if (IsInstanceValid(this))
			UpdateStateIcons();
	}

	public override void _Process(double delta)
	{
		// OMORI uses a queue system for damage numbers spawned on party members
		// the position of where the number spawns is based off of the current numbers queued
		// as well as how many numbers currently exist on the party member
		// enemies do not use this system
		if (PendingDamageNumbers.Count > 0)
		{
			int queued = PendingDamageNumbers.Count;
			PendingDamageNumber number = PendingDamageNumbers.Dequeue();
			BattleManager.Instance.SpawnDamageNumber(number.Damage, DamageNumberPosition(queued, PopupCount), number.Type, number.Critical, party: true);
			PopupCount++;
		}
		else
			PopupCount = 0;

		// keep the cached maxes in sync with stat modifiers that change MaxHP/MaxJuice
		Stats stats = PartyMember.CurrentStats;
		if (MaxHP != stats.MaxHP || MaxJuice != stats.MaxJuice)
		{
			MaxHP = stats.MaxHP;
			MaxJuice = stats.MaxJuice;
			HPBar.MaxValue = Math.Max(1, MaxHP);
			JuiceBar.MaxValue = Math.Max(1, MaxJuice);
			HPLabel.Text = $"{(int)Math.Floor(DisplayedHP + 0.5f)}/{MaxHP}";
			JuiceLabel.Text = $"{(int)Math.Floor(DisplayedJuice + 0.5f)}/{MaxJuice}";
		}
	}

	/// <summary>
	/// Rolls the displayed HP and JUICE toward their current values, one step per physics frame.
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		if (PartyMember == null)
			return;
		if (Roll(ref DisplayedHP, ref HPRollTarget, ref HPRollFrames, PartyMember.CurrentHP))
		{
			HPBar.Value = DisplayedHP;
			HPLabel.Text = $"{(int)Math.Floor(DisplayedHP + 0.5f)}/{MaxHP}";
		}
		if (Roll(ref DisplayedJuice, ref JuiceRollTarget, ref JuiceRollFrames, PartyMember.CurrentJuice))
		{
			JuiceBar.Value = DisplayedJuice;
			JuiceLabel.Text = $"{(int)Math.Floor(DisplayedJuice + 0.5f)}/{MaxJuice}";
		}
	}

	private static bool Roll(ref float current, ref int target, ref float frames, int value)
	{
		if (value != target)
		{
			target = value;
			// VANILLA BUG: a change that lands mid-roll leaves a fractional frame count, so the last step overshoots the new value
			frames = Math.Clamp(Math.Abs(current - target), 0f, 30f);
		}
		if (frames <= 0f)
			return false;
		current = (current * (frames - 1f) + target) / frames;
		frames--;
		return true;
	}

	/// <summary>
	/// Redraws the state icons next to this party member's battlecard from their current <see cref="Actors.Actor.StatModifiers"/>.<br/>
	/// Icons already refresh when a modifier is added or removed; call this after changing what a modifier shows without
	/// adding or removing it, such as a tier change. Does nothing while the Show State Icons setting is off.
	/// </summary>
	public void UpdateStateIcons()
	{
		if (!SettingsMenuManager.Instance.ShowStateIcons)
			return;

		StateIconRenderer.Render(StateIcons, PartyMember);
	}

	internal bool SelectionBoxVisible
	{
		get => SelectedBox.Visible;
		set => SelectedBox.Visible = value;
	}

	internal void FadeInFollowups(HashSet<InputDirection> disabledDirections = null)
	{
		FollowupBubbles.ShowBubbles(disabledDirections);
	}

	internal void FadeOutFollowups()
	{
		FollowupBubbles.HideBubbles();
	}

	internal void FadeOutFollowupsExcept(InputDirection selected)
	{
		FollowupBubbles.HideBubblesExcept(selected);
	}

	private record struct PendingDamageNumber(int Damage, DamageType Type, bool Critical);
}
