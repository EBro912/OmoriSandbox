using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Therese : Enemy
{
    public override string Name => "THERESE";
    public override Vector2 InfoBoxOffset => new(-25, -200);
    public override Vector2 DamageNumberOffset => new(-30, 30);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/therese.tres");
    protected override Stats Stats => new(195, 90, 22, 15, 25, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["THRAttack", "THRDoNothing", "THRSnipe", "THRInsult", "THRDoubleShot"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["THRAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto doubleShot;
                if (Roll() < 31)
                    goto doNothing;
                if (HasTargetWithEmotion("angry", out PartyMember angry))
                    return new BattleCommand(this, angry, Skills["THRSnipe"]);
                goto insult;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                if (HasTargetWithEmotion("angry", out PartyMember angry2))
                    return new BattleCommand(this, angry2, Skills["THRSnipe"]);
                goto insult;
            case "angry":
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                if (HasTargetWithEmotion("angry", out PartyMember angry3))
                    return new BattleCommand(this, angry3, Skills["THRSnipe"]);
                goto insult;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                if (HasTargetWithEmotion("angry", out PartyMember angry4))
                    return new BattleCommand(this, angry4, Skills["THRSnipe"]);
                goto insult;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["THRAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["THRDoNothing"]);
    insult:
        return new BattleCommand(this, SelectTarget(), Skills["THRInsult"]);
    doubleShot:
        return new BattleCommand(this, SelectTarget(), Skills["THRDoubleShot"]);
    }
}
