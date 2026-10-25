using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Lucius : Enemy
{
    public override string Name => "LUCIUS";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override Vector2 DamageNumberOffset => new(0, 40);
    protected internal override bool ObserveHasMulti => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/lucius.tres");
    protected override Stats Stats => new(170, 75, 22, 12, 10, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["LUCMisfireMagic", "LUCAttack", "LUCDoNothing", "LUCFireMagic"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["LUCMisfireMagic"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["LUCAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 51)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                if (Roll() < 51)
                    goto fireMagic;
                goto misfireMagic;
            case "sad":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                if (Roll() < 21)
                    goto fireMagic;
                goto misfireMagic;
            case "angry":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 11)
                    goto doNothing;
                if (Roll() < 66)
                    goto fireMagic;
                goto misfireMagic;
            default:
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                if (Roll() < 56)
                    goto fireMagic;
                goto misfireMagic;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["LUCAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["LUCDoNothing"]);
    fireMagic:
        return new BattleCommand(this, SelectAllTargets(), Skills["LUCFireMagic"]);
    misfireMagic:
        return new BattleCommand(this, SelectAllTargets(), Skills["LUCMisfireMagic"]);
    }
}
