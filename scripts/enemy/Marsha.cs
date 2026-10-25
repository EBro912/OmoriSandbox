using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Marsha : Enemy
{
    public override string Name => "MARSHA";
    public override Vector2 InfoBoxOffset => new(-40, -200);
    public override Vector2 DamageNumberOffset => new(-30, 20);
    protected internal override bool ObserveHasMulti => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/marsha.tres");
    protected override Stats Stats => new(310, 155, 23, 4, 10, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["MARSpin", "MARAttack", "MARDoNothing", "MARChop"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["MARSpin"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["MARAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                if (Roll() < 56)
                    goto spin;
                goto chop;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 61)
                    goto doNothing;
                if (Roll() < 31)
                    goto spin;
                goto chop;
            case "angry":
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 11)
                    goto doNothing;
                if (Roll() < 66)
                    goto spin;
                goto chop;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                if (Roll() < 51)
                    goto spin;
                goto chop;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["MARAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["MARDoNothing"]);
    spin:
        return new BattleCommand(this, SelectAllTargets(), Skills["MARSpin"]);
    chop:
        return new BattleCommand(this, SelectTarget(), Skills["MARChop"]);
    }
}
