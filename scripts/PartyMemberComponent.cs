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
public partial class PartyMemberComponent : Node
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

    /// <summary>
    /// The <see cref="Actors.PartyMember"/> actor this component is attached to.
    /// </summary>
    public PartyMember Actor => PartyMember;
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

    private Timer HurtTimer = new()
    {
	    Autostart = false,
	    OneShot = true
    };

    internal bool SetPartyMember(PartyMember partyMember, PackedScene followup, BattlePresetActor actor)
	{
		PartyMember = partyMember;
		AnimatedSprite2D face = GetNode<AnimatedSprite2D>("../Battlecard/Face");
		StateAnimator = GetNode<StateAnimator>("../Battlecard/StateAnimatorComponent");
		if (actor.Emotion is "hurt" or "victory")
			actor.Emotion = "neutral";
		if (!PartyMember.Init(face, actor))
			return false;
		HPLabel = GetNode<Label>("../Battlecard/HealthLabel/");
		HPBar = GetNode<TextureProgressBar>("../Battlecard/Health");
		JuiceLabel = GetNode<Label>("../Battlecard/JuiceLabel");
		JuiceBar = GetNode<TextureProgressBar>("../Battlecard/Juice");
		SelectedBox = GetNode<TextureRect>("../SelectedCard");
		StateIcons = GetNode<HFlowContainer>("../StateIcons");
		if (actor.Position % 2 == 0)
		{
			StateIcons.Position = new Vector2(0, -115);
			StateIcons.ReverseFill = true;
		}

		HPBar.MaxValue = PartyMember.CurrentStats.MaxHP;
		HPBar.Value = PartyMember.CurrentHP;
		JuiceBar.MaxValue = PartyMember.CurrentStats.MaxJuice;
		JuiceBar.Value = PartyMember.CurrentJuice;
		DisplayedHP = PartyMember.CurrentHP;
		DisplayedJuice = PartyMember.CurrentJuice;
		
		HPLabel.Text = $"{Mathf.RoundToInt(DisplayedHP)}/{HPBar.MaxValue}";
		JuiceLabel.Text = $"{Mathf.RoundToInt(DisplayedJuice)}/{JuiceBar.MaxValue}";

		if (followup != null)
		{
            FollowupBubbles bubbles = followup.Instantiate<FollowupBubbles>();
			GetParent().AddChild(bubbles);
			FollowupBubbles = bubbles;
		}

		Position = actor.Position;

		PartyMember.CenterPoint = GetParent<Control>().GlobalPosition + new Vector2(57, 79);
		PartyMember.OnStateChanged += StateChanged;
		PartyMember.OnAnimationChanged += AnimationChanged;
		PartyMember.OnDamaged += Damaged;
		HurtTimer.Timeout += () => PartyMember.SetHurt(false);
		AddChild(HurtTimer);

		// Init already validated the emotion, delay this call to let everything initialize
		StateAnimator.CallDeferred(StateAnimator.MethodName.SetState, PartyMember.CurrentState);

		return true;
	}

	private void StateChanged(object sender, EventArgs e)
	{
		// while an animation override is active (such as plot armor), only the above-head label follows emotion changes
		if (PartyMember.CurrentAnimation != null)
			StateAnimator.SetStateAtlas(PartyMember.CurrentState);
		else
			StateAnimator.SetState(PartyMember.CurrentState);
	}

	private void AnimationChanged(object sender, EventArgs e)
	{
		if (PartyMember.CurrentAnimation != null)
		{
			if (PartyMember.CurrentAnimationAsset != null)
				StateAnimator.ShowAsset(PartyMember.CurrentAnimationAsset);
		}
		else
		{
			StateAnimator.SetState(PartyMember.CurrentState);
		}
	}

	private void Damaged(object sender, EventArgs e)
	{
		PartyMember.SetHurt(true);
		HurtTimer.Start(1d);
	}

	public override void _Process(double delta)
	{
		// nothing to animate once the displayed values have settled
		// ReSharper disable twice CompareOfFloatsByEqualityOperator
		if (DisplayedHP == PartyMember.CurrentHP && DisplayedJuice == PartyMember.CurrentJuice)
			return;

		float dt = (float)delta;

		DisplayedHP = Mathf.MoveToward(DisplayedHP, PartyMember.CurrentHP, dt * ((float)HPBar.MaxValue / 0.5f));
		DisplayedJuice = Mathf.MoveToward(DisplayedJuice, PartyMember.CurrentJuice, dt * ((float)JuiceBar.MaxValue / 0.5f));

		HPBar.Value = DisplayedHP;
		JuiceBar.Value = DisplayedJuice;

		HPLabel.Text = $"{Mathf.RoundToInt(DisplayedHP)}/{HPBar.MaxValue}";
		JuiceLabel.Text = $"{Mathf.RoundToInt(DisplayedJuice)}/{JuiceBar.MaxValue}";
	}

	internal void UpdateStateIcons()
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

}
