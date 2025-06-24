public class Sweetheart : Enemy
{
	public override string Name => "SWEETHEART";
	public override string AnimationPath => "res://animations/sweetheart.tres";
	protected override Stats Stats => new(3300, 1650, 30, 25, 40, 10, 90);

	protected override string[] EquippedSkills => ["SHAttack", "SharpInsult", "SwingMace", "Brag"];

	private bool EmotionLocked = false;
	private int Stage = 0;

	public override bool IsStateValid(string state)
	{
		if (EmotionLocked)
			return false;

		return state == "neutral" || state == "sad" || state == "happy"
			|| state == "angry" || state == "hurt" || state == "toast";
	}

	// TODO: handle more boss specific stuff
	public override bool FallsOffScreen => false;

	public override BattleCommand ProcessAI()
	{
		int roll;
		Actor target = BattleManager.Instance.GetRandomAlivePartyMember();
		switch (CurrentState)
		{
			case "manic":
			case "ecstatic":
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 46)
					goto attack;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 46)
					goto insult;
				goto mace;
			case "happy":
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 36)
					goto attack;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 46)
					goto insult;
				goto mace;
			case "sad":
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 36)
					goto attack;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 21)
					goto insult;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 31)
					goto mace;
				goto brag;
			case "angry":
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 51)
					goto attack;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 31)
					goto insult;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 71)
					goto mace;
				goto brag;
			default:
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 41)
					goto attack;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 31)
					goto insult;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 36)
					goto mace;
				goto brag;

		}
	attack:
		return new BattleCommand(this, target, Skills["SHAttack"]);
	insult:
		return new BattleCommand(this, target, Skills["SharpInsult"]);
	mace:
		return new BattleCommand(this, target, Skills["SwingMace"]);
	brag:
		return new BattleCommand(this, target, Skills["Brag"]);
	}



	public override void ProcessBattleConditions()
	{
		if (Stage > 2)
			return;
		if (CurrentHP < 990 && Stage == 2)
		{
			EmotionLocked = false;
			ForceState("manic");
			EmotionLocked = true;
			Stage++;
		}
		if (CurrentHP < 1650 && Stage == 1)
		{
			EmotionLocked = false;
			ForceState("ecstatic");
			EmotionLocked = true;
			Stage++;
		}
		if (CurrentHP < 2640 && Stage == 0)
		{
			ForceState("happy");
			EmotionLocked = true;
			AddStatModifier(Modifier.SweetheartLock, 1, int.MaxValue);
			Stage++;
		}
	}
}
