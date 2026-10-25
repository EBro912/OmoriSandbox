using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SproutMoleQuestion : Enemy
{
    public override string Name => "SPROUT MOLE?";
    public override Vector2 InfoBoxOffset => new(0, -50);
    public override bool InfoBoxCursorAboveBox => true;
    protected internal override bool ObserveHasMulti => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sprout_mole_question.tres");
    protected override Stats Stats => new(180, 90, 23, 7, 24, 13, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["SMQRunAround", "SMQAttack", "SMQDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectTargets(2), Skills["SMQRunAround"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SMQAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto runAround;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 66)
                    goto doNothing;
                goto runAround;
            case "angry":
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 16)
                    goto doNothing;
                goto runAround;
            default:
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto runAround;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SMQAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["SMQDoNothing"]);
    runAround:
        return new BattleCommand(this, SelectTargets(2), Skills["SMQRunAround"]);
    }
}
