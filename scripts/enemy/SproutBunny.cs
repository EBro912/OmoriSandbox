using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SproutBunny : Enemy
{
    public override string Name => "SPROUT BUNNY";
    public override Vector2 InfoBoxOffset => new(0, -150);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sprout_bunny.tres");
    protected override Stats Stats => new(810, 405, 75, 65, 35, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["SPRBAttack", "SPRBDoNothing", "SPRBFeed"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SPRBAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto feed;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 51)
                    goto doNothing;
                goto feed;
            case "angry":
                if (Roll() < 51)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                goto feed;
            default:
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto feed;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SPRBAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["SPRBDoNothing"]);
    feed:
        return new BattleCommand(this, SelectEnemy(), Skills["SPRBFeed"]);
    }
}
