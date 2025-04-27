using Godot;

public partial class PartyMemberComponent : Node
{
	private PartyMember PartyMember;
	private StateAnimator StateAnimator;
	private TextureRect SelectedBox;
	private TextureProgressBar HPBar;
	private TextureProgressBar JuiceBar;
	private Label HPLabel;
	private Label JuiceLabel;
	public PartyMemberComponent() { }

	public PartyMember Actor => PartyMember;

	public void SetPartyMember(PartyMember partyMember, string initialState = "neutral", int level = 1)
	{
		PartyMember = partyMember;
		AnimatedSprite2D face = GetNode<AnimatedSprite2D>("../Battlecard/Face");
		StateAnimator = GetNode<StateAnimator>("../Battlecard/StateAnimatorComponent");
		PartyMember.Init(face, initialState, level);
		HPLabel = GetNode<Label>("../Battlecard/HealthLabel/");
		HPBar = GetNode<TextureProgressBar>("../Battlecard/Health");
		JuiceLabel = GetNode<Label>("../Battlecard/JuiceLabel");
		JuiceBar = GetNode<TextureProgressBar>("../Battlecard/Juice");
		SelectedBox = GetNode<TextureRect>("../SelectedCard");

		HPBar.MaxValue = PartyMember.BaseStats.HP;
		HPBar.Value = PartyMember.CurrentHP;
		JuiceBar.MaxValue = PartyMember.BaseStats.Juice;
		JuiceBar.Value = PartyMember.CurrentJuice;
	}

	public override void _Process(double delta)
	{
		HPBar.Value = PartyMember.CurrentHP;
		HPLabel.Text = PartyMember.CurrentHP + "/" + PartyMember.BaseStats.HP;
		JuiceBar.Value = PartyMember.CurrentJuice;
		JuiceLabel.Text = PartyMember.CurrentJuice + "/" + PartyMember.BaseStats.Juice;
	}

	public bool SelectionBoxVisible
	{
		get { return SelectedBox.Visible; }
		set { SelectedBox.Visible = value; }
	}

	public bool SetState(string state)
	{
		if (PartyMember.IsStateValid(state))
		{
			PartyMember.SetState(state);
			StateAnimator.SetState(state);
			return true;
		}
		return false;
	}

	public string GetState()
	{
		return PartyMember.CurrentState;
	}

}
