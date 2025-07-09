public class SlimeGirls : Enemy
{
	public override string AnimationPath => "res://animations/slimegirls.tres";

	public override bool FallsOffScreen => false;

	public override string Name => "SLIME GIRLS";

	protected override Stats Stats => new(5700, 1750, 57, 32, 52, 10, 95);

	protected override string[] EquippedSkills => ["ComboAttack", "StrangeGas", "Dynamite", "StingRay", "Swap", "Chainsaw", "SlimeUltimateAttack"];

	public override bool IsStateValid(string state)
	{
		return state == "neutral" || state == "sad" || state == "happy"
			|| state == "angry" || state == "hurt" || state == "toast";
	}

	private int Stage = 0;

	public override BattleCommand ProcessAI()
	{
		int roll;
		Actor target = BattleManager.Instance.GetRandomAlivePartyMember();
		switch (CurrentState)
		{
			case "happy":
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 16)
					goto combo;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 21)
					goto gas;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 16)
					goto dynamite;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 21)
					goto stingray;
				goto chainsaw;
			case "sad":
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 16)
					goto combo;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 16)
					goto gas;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 21)
					goto dynamite;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 16)
					goto stingray;
				goto chainsaw;
			case "angry":
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 21)
					goto combo;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 16)
					goto gas;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 21)
					goto dynamite;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 16)
					goto stingray;
				goto chainsaw;
			default:
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 16)
					goto combo;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 16)
					goto gas;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 16)
					goto dynamite;
				roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 16)
					goto stingray;
				goto chainsaw;
		}

	combo:
		return new BattleCommand(this, target, Skills["ComboAttack"]);
	gas:
		return new BattleCommand(this, target, Skills["StrangeGas"]);
	dynamite:
		return new BattleCommand(this, target, Skills["Dynamite"]);
	stingray:
		return new BattleCommand(this, target, Skills["StingRay"]);
	chainsaw:
		return new BattleCommand(this, target, Skills["Chainsaw"]);
	}

	public override void ProcessBattleConditions()
	{
		if (Stage > 2)
			return;

		if (CurrentHP < 1425 && Stage == 2)
		{
			BattleManager.Instance.ForceCommand(this, null, Skills["SlimeUltimateAttack"]);
			Stage++;
		}

		if (CurrentHP < 2850 && Stage == 1)
		{
			BattleManager.Instance.ForceCommand(this, null, Skills["Swap"]);
			Stage++;
		}

		if (CurrentHP < 4275 && Stage == 0)
		{
			ForceState("angry");
			Stage++;
		}
	}
}
