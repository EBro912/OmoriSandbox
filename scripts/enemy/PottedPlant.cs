using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class PottedPlant : Enemy
{
    public override string Name => "POTTED PLANT";
    public override Vector2 InfoBoxOffset => new(0, -225);
    public override Vector2 DamageNumberOffset => new(0, 20);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/potted_plant.tres");
    protected override Stats Stats => new(200, 400, 20, 25, 10, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["PPTrip", "PPExplode", "PPAttack", "PPDoNothing", "BGBScuttle"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["PPTrip"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (IsBelowHP(0.2f))
                    goto explode;
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                goto trip;
            case "sad":
                if (IsBelowHP(0.2f))
                    goto explode;
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 66)
                    goto doNothing;
                goto trip;
            case "angry":
                if (IsBelowHP(0.2f))
                    goto explode;
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 16)
                    goto doNothing;
                goto scuttle;
            default:
                if (IsBelowHP(0.2f))
                    goto explode;
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                goto trip;
        }
    explode:
        return new BattleCommand(this, SelectAllTargets(), Skills["PPExplode"]);
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["PPAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["PPDoNothing"]);
    trip:
        return new BattleCommand(this, SelectTarget(), Skills["PPTrip"]);
    scuttle:
        return new BattleCommand(this, this, Skills["BGBScuttle"]);
    }
}
