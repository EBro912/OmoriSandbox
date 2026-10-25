using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Mixtape : Enemy
{
    public override string Name => "MIXTAPE";
    public override Vector2 InfoBoxOffset => new(0, -150);
    public override Vector2 DamageNumberOffset => new(0, 30);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/mixtape.tres");
    protected override Stats Stats => new(120, 60, 14, 6, 16, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["MXTAttack", "MXTDoNothing", "MXTTangle"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["MXTAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto tangle;
            case "sad":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                goto tangle;
            case "angry":
                if (Roll() < 81)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto tangle;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto tangle;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["MXTAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["MXTDoNothing"]);
    tangle:
        return new BattleCommand(this, SelectTargetWithHighest(StatType.SPD), Skills["MXTTangle"]);
    }
}
