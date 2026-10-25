using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class DialUp : Enemy
{
    public override string Name => "DIAL-UP";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override Vector2 DamageNumberOffset => new(0, 30);
    protected internal override bool ObserveHasMulti => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/dial_up.tres");
    protected override Stats Stats => new(65, 130, 14, 22, 1, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["DUSlowDownAll", "DUAttack", "DUDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["DUSlowDownAll"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["DUAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                goto slowDownAll;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 61)
                    goto doNothing;
                goto slowDownAll;
            case "angry":
                if (Roll() < 81)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto slowDownAll;
            default:
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 41)
                    goto doNothing;
                goto slowDownAll;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["DUAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["DUDoNothing"]);
    slowDownAll:
        return new BattleCommand(this, SelectAllTargets(), Skills["DUSlowDownAll"]);
    }
}
