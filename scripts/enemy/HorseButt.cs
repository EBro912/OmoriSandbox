using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class HorseButt : Enemy
{
    public override string Name => "HORSE BUTT";
    public override Vector2 InfoBoxOffset => CurrentEmotion.Group == null ? new(0, -150) : new(0, -200);
    public override Vector2 DamageNumberOffset => new(0, 40);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/horse_butt.tres");
    protected override Stats Stats => new(260, 105, 25, 20, 30, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["HBTAttack", "HBTDoNothing", "HBTKick", "HBTPrance"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["HBTAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto kick;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 51)
                    goto doNothing;
                if (Roll() < 31)
                    goto kick;
                goto prance;
            case "angry":
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                if (Roll() < 66)
                    goto kick;
                goto prance;
            default:
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                if (Roll() < 36)
                    goto kick;
                goto prance;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["HBTAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["HBTDoNothing"]);
    kick:
        return new BattleCommand(this, SelectTargets(1), Skills["HBTKick"]);
    prance:
        return new BattleCommand(this, this, Skills["HBTPrance"]);
    }
}
