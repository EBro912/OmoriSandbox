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
	private float TargetHP;
	private float TargetJuice;

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
		TargetHP = PartyMember.CurrentHP;
		DisplayedJuice = PartyMember.CurrentJuice;
		TargetJuice = PartyMember.CurrentJuice;

		if (followup != null)
		{
            FollowupBubbles bubbles = followup.Instantiate<FollowupBubbles>();
			GetParent().AddChild(bubbles);
			FollowupBubbles = bubbles;
		}

		Position = actor.Position;

		PartyMember.CenterPoint = GetParent<Control>().GlobalPosition + new Vector2(57, 79);
		PartyMember.OnStateChanged += StateChanged;
		PartyMember.OnHPChanged += HPChanged;
		PartyMember.OnJuiceChanged += JuiceChanged;
		PartyMember.OnDamaged += Damaged;
		HurtTimer.Timeout += () => PartyMember.SetHurt(false);
		AddChild(HurtTimer);

		return true;
	}

	private void StateChanged(object sender, EventArgs e)
	{
		// avoid updating the background during plot armor
		if (PartyMember.HasStatModifier("PlotArmor"))
			StateAnimator.SetStateAtlas(PartyMember.CurrentState);
		else
			StateAnimator.SetState(PartyMember.CurrentState);
	}

	private void HPChanged(object sender, EventArgs e)
	{
		TargetHP = PartyMember.CurrentHP;
	}

	private void JuiceChanged(object sender, EventArgs e)
	{
		TargetJuice = PartyMember.CurrentJuice;
	}

	private void Damaged(object sender, EventArgs e)
	{
		PartyMember.SetHurt(true);
		HurtTimer.Start(1d);
	}

	public override void _Process(double delta)
	{
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
		
		// this may need to be optimized, not the best practice to fully replace nodes
		foreach (Node child in StateIcons.GetChildren())
			child.Free();
		
		foreach (StatModifier modifier in PartyMember.StatModifiers.Values)
		{
			StateIcon[] icons = modifier.GetStateIcons();
			foreach (StateIcon icon in icons)
			{
				string tooltip = modifier.TurnsLeft > -1 ? icon.Description + "\nTurns Left: " + modifier.TurnsLeft : icon.Description;
				if (Database.TryGetStateIcon(icon.AssetName, out Texture2D texture))
				{
					TextureRect rect = new()
					{
						Texture = texture,
						TooltipText = tooltip
					};
					StateIcons.AddChild(rect);
				}
				else
				{
					GD.PrintErr("Unknown state icon texture: " + icon.AssetName);
				}
			}
		}
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
