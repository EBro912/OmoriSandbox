public class LostSproutMole : Enemy
{
	public override string Name => "LOST SPROUT MOLE";

	public override string AnimationPath => "res://animations/sprout_mole.tres";

	protected override Stats Stats => new(170, 75, 22, 10, 13, 5, 95);
	protected override string[] EquippedSkills => ["LSMAttack", "LSMDoNothing", "LSMRunAround"];

	public override bool IsStateValid(string state)
	{
		return state == "neutral" || state == "sad" || state == "happy" || state == "angry" || state == "hurt" || state == "toast";
	}
    public override bool FallsOffScreen => true;
    public override BattleCommand ProcessAI()
	{
		int roll;
		Actor target = BattleManager.Instance.GetRandomAlivePartyMember();
		switch (CurrentState)
		{
			case "happy":
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 36)
					goto attack;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 36)
					goto nothing;
				goto run;
			case "sad":
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 31)
					goto attack;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 56)
					goto nothing;
				goto run;
			case "angry":
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 61)
					goto attack;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 21)
					goto nothing;
				goto run;
			default:
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 56)
					goto attack;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 36)
					goto nothing;
				goto run;

		}
		attack:
		return new BattleCommand(this, target, Skills["LSMAttack"]);
		nothing:
		return new BattleCommand(this, target, Skills["LSMDoNothing"]);
		run:
		return new BattleCommand(this, target, Skills["LSMRunAround"]);
	}
}
