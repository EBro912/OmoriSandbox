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

		HPBar.MaxValue = PartyMember.MaxHP;
		HPBar.Value = PartyMember.HP;
		JuiceBar.MaxValue = PartyMember.MaxJuice;
		JuiceBar.Value = PartyMember.Juice;

		UpdateHealth();
		UpdateJuice();
	}

	public void UpdateHealth()
	{
		HPBar.Value = PartyMember.HP;
		HPLabel.Text = PartyMember.HP + "/" + PartyMember.MaxHP;
	}

	public void UpdateJuice()
	{
		JuiceBar.Value = PartyMember.Juice;
		JuiceLabel.Text = PartyMember.Juice + "/" + PartyMember.MaxJuice;
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
