using Godot;
using System;

public partial class PartyMemberComponent : Node
{
	private PartyMember PartyMember;
	private StateAnimator StateAnimator;
	private TextureRect SelectedBox;
	private TextureProgressBar HPBar;
	private TextureProgressBar JuiceBar;
	private Label HPLabel;
	private Label JuiceLabel;

	private float DisplayedHP;
	private float DisplayedJuice;
	private const float LerpSpeed = 15f;

	public PartyMemberComponent() { }

	public PartyMember Actor => PartyMember;
	public Node2D FollowupBubbles { get; private set; }
	public int Position { get; private set; }
	public bool HasFollowup => FollowupBubbles != null;

	public void SetPartyMember(PartyMember partyMember, PackedScene followup, int position, string initialState, int level, string weapon)
	{
		PartyMember = partyMember;
		AnimatedSprite2D face = GetNode<AnimatedSprite2D>("../Battlecard/Face");
		StateAnimator = GetNode<StateAnimator>("../Battlecard/StateAnimatorComponent");
		PartyMember.Init(face, initialState, level, weapon);
		HPLabel = GetNode<Label>("../Battlecard/HealthLabel/");
		HPBar = GetNode<TextureProgressBar>("../Battlecard/Health");
		JuiceLabel = GetNode<Label>("../Battlecard/JuiceLabel");
		JuiceBar = GetNode<TextureProgressBar>("../Battlecard/Juice");
		SelectedBox = GetNode<TextureRect>("../SelectedCard");

		HPBar.MaxValue = PartyMember.CurrentHP;
		HPBar.Value = PartyMember.CurrentHP;
		JuiceBar.MaxValue = PartyMember.CurrentJuice;
		JuiceBar.Value = PartyMember.CurrentJuice;
		DisplayedHP = PartyMember.CurrentHP;
		DisplayedJuice = PartyMember.CurrentJuice;

		if (followup != null)
		{
			Node2D bubbles = followup.Instantiate<Node2D>();
			bubbles.Modulate = Colors.Transparent;
			GetParent().AddChild(bubbles);
			FollowupBubbles = bubbles;
		}

		Position = position;

		PartyMember.CenterPoint = GetParent<Control>().GlobalPosition + new Vector2(57, 79);
		PartyMember.OnStateChanged += StateChanged;

		PartyMember.Sprite.Animation = initialState;
		PartyMember.CurrentState = initialState;
		// delay this call to let everything initialize
		StateAnimator.CallDeferred(StateAnimator.MethodName.SetState, initialState);
	}

	private void StateChanged(object sender, EventArgs e)
	{
		StateAnimator.SetState(PartyMember.CurrentState);
	}

	public override void _Process(double delta)
	{
		HPBar.Value = PartyMember.CurrentHP;
		HPLabel.Text = PartyMember.CurrentHP + "/" + HPBar.MaxValue;
		JuiceBar.Value = PartyMember.CurrentJuice;
		JuiceLabel.Text = PartyMember.CurrentJuice + "/" + JuiceBar.MaxValue;
	}

	public bool SelectionBoxVisible
	{
		get { return SelectedBox.Visible; }
		set { SelectedBox.Visible = value; }
	}

	public void FadeInFollowups(int energy)
	{
		Tween tween = CreateTween();
		tween.TweenProperty(FollowupBubbles, "modulate:a", energy > 2 ? 1f : 0.75f, 0.2f);
	}

	public void FadeOutFollowups()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(FollowupBubbles, "modulate:a", 0f, 0.2f);
	}

}
