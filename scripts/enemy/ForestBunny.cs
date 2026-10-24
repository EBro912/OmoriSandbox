using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class ForestBunny : Enemy
{
    public override string Name => "FOREST BUNNY";
    public override Vector2 InfoBoxOffset => new(0, -150);
    public override Vector2 DamageNumberOffset => new(0, 30);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/forest_bunny.tres");
    protected override Stats Stats => new(85, 42, 10, 2, 10, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["FBAttack", "FBDoNothing", "FBBeCute", "FBSadEyes"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["FBAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto beCute;
            case "sad":
                if (Roll() < 21)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                goto sadEyes;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                goto beCute;
            default:
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 41)
                    goto doNothing;
                goto beCute;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["FBAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["FBDoNothing"]);
    beCute:
        return new BattleCommand(this, SelectTarget(), Skills["FBBeCute"]);
    sadEyes:
        return new BattleCommand(this, SelectTarget(), Skills["FBSadEyes"]);
    }
}
