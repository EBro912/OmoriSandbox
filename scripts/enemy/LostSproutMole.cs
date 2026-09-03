using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class LostSproutMole : Enemy
{
	public override string Name => "LOST SPROUT MOLE";
	public override Vector2 InfoBoxOffset => new(0, -180);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sprout_mole.tres");

    protected override Stats Stats => new(170, 75, 22, 10, 13, 5, 95);
	protected override string[] EquippedSkills => ["LSMAttack", "LSMDoNothing", "LSMRunAround"];
	protected internal override bool ObserveHasMulti => true;

	public override bool IsEmotionValid(Emotion emotion)
	{
		return emotion.Id is "neutral" or "sad" or "happy" or "angry";
	}
    public override BattleCommand ProcessAI()
    {
	    if (HasMultiTargetObserve())
		    return new BattleCommand(this, SelectTargets(1), Skills["LSMRunAround"]);
	    
	    if (HasObserveTarget(out PartyMember observe))
		    return new BattleCommand(this, observe, Skills["LSMAttack"]);
		
		switch (CurrentEmotion.Id)
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
				if (Roll() < 66)
					goto nothing;
				goto run;
			case "angry":
				if (Roll() < 51)
					goto attack;
				if (Roll() < 21)
					goto nothing;
				goto run;
			default:
				if (Roll() < 66)
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
