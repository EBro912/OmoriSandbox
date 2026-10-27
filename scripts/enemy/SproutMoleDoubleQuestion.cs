using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SproutMoleDoubleQuestion : Enemy
{
    public override string Name => "SPROUT MOLE??";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sprout_mole_double_question.tres");
    protected override Stats Stats => new(910, 195, 61, 10, 15, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["SMQQAttack", "SMQQExplode", "SMQQDoNothing", "SMQQStrangeLaser"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SMQQAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (CurrentHP <= 100)
                    goto explode;
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto strangeLaser;
            case "sad":
                if (CurrentHP <= 100)
                    goto explode;
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                goto strangeLaser;
            case "angry":
                if (CurrentHP <= 100)
                    goto explode;
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto strangeLaser;
            default:
                if (CurrentHP <= 100)
                    goto explode;
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto strangeLaser;
        }
    explode:
        return new BattleCommand(this, SelectAllTargets(), Skills["SMQQExplode"]);
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SMQQAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["SMQQDoNothing"]);
    strangeLaser:
        return new BattleCommand(this, SelectTarget(), Skills["SMQQStrangeLaser"]);
    }
}
