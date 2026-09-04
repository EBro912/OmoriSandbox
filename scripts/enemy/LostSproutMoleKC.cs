using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class LostSproutMoleKC : Enemy
{
	public override string Name => "LOST SPROUT MOLE";
	public override Vector2 InfoBoxOffset => new(0, -180);

    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sprout_mole.tres");

    protected override Stats Stats => new(500, 200, 50, 50, 50, 5, 95);
	protected override string[] EquippedSkills => ["LSMAttack", "LSMDoNothing", "LSMRunAround"];
	protected internal override bool ObserveHasMulti => true;

	// vanilla bug: the ANGRY King Crawler LSM has different stats
	private Stats GetStatsForEmotion()
	{
		return CurrentEmotion.Group?.Id == "angry"
			? new Stats(42, 0, 11, 8, 5, 5, 95)
			: new Stats(500, 200, 50, 50, 50, 5, 95);
	}

	public override Stats GetBaseStats()
	{
		return GetStatsForEmotion() + AdjustedStats;
	}

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
				if (Roll() < 41)
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
