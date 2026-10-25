using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SnowPile : Enemy
{
    public override string Name => "SNOW PILE";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/snow_pile.tres");
    protected override Stats Stats => new(500, 235, 57, 70, 1, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["SNPAttack", "SNPDoNothing", "SNPEngulf", "SNPMoreSnow"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SNPAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                if (Roll() < 56)
                    goto engulf;
                goto moreSnow;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 61)
                    goto doNothing;
                if (Roll() < 31)
                    goto engulf;
                goto moreSnow;
            case "angry":
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 16)
                    goto doNothing;
                if (Roll() < 46)
                    goto engulf;
                goto moreSnow;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                if (Roll() < 46)
                    goto engulf;
                goto moreSnow;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SNPAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["SNPDoNothing"]);
    engulf:
        return new BattleCommand(this, SelectTarget(), Skills["SNPEngulf"]);
    moreSnow:
        return new BattleCommand(this, this, Skills["SNPMoreSnow"]);
    }
}
