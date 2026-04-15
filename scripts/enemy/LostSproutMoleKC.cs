using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class LostSproutMoleKC : Enemy
{
	public override string Name => "LOST SPROUT MOLE";

    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sprout_mole.tres");

    protected override Stats Stats => new(500, 200, 50, 50, 50, 5, 95);
	protected override string[] EquippedSkills => ["LSMAttack", "LSMDoNothing", "LSMRunAround"];

	public override bool IsStateValid(string state)
	{
		return state == "neutral" || state == "sad" || state == "happy" || state == "angry" || state == "hurt" || state == "toast";
	}
    public override BattleCommand ProcessAI()
	{
		if (HasMultiTargetObserve())
			return new BattleCommand(this, SelectTargets(1), Skills["LSMRunAround"]);
	    
		if (HasObserveTarget(out PartyMember observe))
			return new BattleCommand(this, observe, Skills["LSMAttack"]);
		
		switch (CurrentState)
		{
			case "happy":
				if (Roll() < 36)
					goto attack;
				if (Roll() < 36)
					goto nothing;
				goto run;
			case "sad":
				if (Roll() < 31)
					goto attack;
				if (Roll() < 56)
					goto nothing;
				goto run;
			case "angry":
				if (Roll() < 51)
					goto attack;
				if (Roll() < 21)
					goto nothing;
				goto run;
			default:
				if (Roll() < 51)
					goto attack;
				if (Roll() < 36)
					goto nothing;
				goto run;

		}
	attack:
		return new BattleCommand(this, SelectTarget(), Skills["LSMAttack"]);
	nothing:
		return new BattleCommand(this, this, Skills["LSMDoNothing"]);
	run:
		return new BattleCommand(this, SelectTargets(1), Skills["LSMRunAround"]);
	}
}
