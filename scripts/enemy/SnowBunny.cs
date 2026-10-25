using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SnowBunny : Enemy
{
    public override string Name => "SNOW BUNNY";
    public override Vector2 InfoBoxOffset => new(0, -150);
    public override Vector2 DamageNumberOffset => new(0, 30);
    protected internal override bool ObserveHasMulti => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/snow_bunny.tres");
    protected override Stats Stats => new(650, 200, 62, 35, 40, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["SNBSmallSnowStorm", "SNBAttack", "SNBDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["SNBSmallSnowStorm"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SNBAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto smallSnowStorm;
            case "sad":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 61)
                    goto doNothing;
                goto smallSnowStorm;
            case "angry":
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 16)
                    goto doNothing;
                goto smallSnowStorm;
            default:
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 41)
                    goto doNothing;
                goto smallSnowStorm;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SNBAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["SNBDoNothing"]);
    smallSnowStorm:
        return new BattleCommand(this, SelectAllTargets(), Skills["SNBSmallSnowStorm"]);
    }
}
