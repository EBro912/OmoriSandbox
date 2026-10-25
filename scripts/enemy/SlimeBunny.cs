using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SlimeBunny : Enemy
{
    public override string Name => "SLIME BUNNY";
    public override Vector2 InfoBoxOffset => new(0, -150);
    public override Vector2 DamageNumberOffset => new(0, 30);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/slime_bunny.tres");
    protected override Stats Stats => new(460, 150, 57, 5, 1, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["SLBAttack", "SLBDoNothing", "SLBSticky"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SLBAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto sticky;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 61)
                    goto doNothing;
                goto sticky;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto sticky;
            default:
                if (Roll() < 51)
                    goto attack;
                if (Roll() < 51)
                    goto doNothing;
                goto sticky;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SLBAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["SLBDoNothing"]);
    sticky:
        return new BattleCommand(this, SelectTarget(), Skills["SLBSticky"]);
    }
}
