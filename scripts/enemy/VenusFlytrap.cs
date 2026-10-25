using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class VenusFlytrap : Enemy
{
    public override string Name => "VENUS FLYTRAP";
    public override Vector2 InfoBoxOffset => new(0, -220);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/venus_flytrap.tres");
    protected override Stats Stats => new(250, 125, 15, 5, 1, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["VFTAttack", "VFTDoNothing", "VFTCrunch"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["VFTAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 51)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto crunch;
            case "sad":
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 51)
                    goto doNothing;
                goto crunch;
            case "angry":
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 16)
                    goto doNothing;
                goto crunch;
            default:
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto crunch;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["VFTAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["VFTDoNothing"]);
    crunch:
        return new BattleCommand(this, SelectTarget(), Skills["VFTCrunch"]);
    }
}
