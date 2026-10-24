using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SpaceBunny : Enemy
{
    public override string Name => "SPACE BUNNY";
    public override Vector2 InfoBoxOffset => new(0, -180);
    public override Vector2 DamageNumberOffset => new(0, 30);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/space_bunny.tres");
    protected override Stats Stats => new(105, 50, 12, 7, 10, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["SPBAttack", "SPBDoNothing", "SPBBunnyBeam"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SPBAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto bunnyBeam;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                goto bunnyBeam;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 16)
                    goto doNothing;
                goto bunnyBeam;
            default:
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto bunnyBeam;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SPBAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["SPBDoNothing"]);
    bunnyBeam:
        return new BattleCommand(this, SelectTarget(), Skills["SPBBunnyBeam"]);
    }
}
