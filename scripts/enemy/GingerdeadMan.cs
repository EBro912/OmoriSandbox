using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class GingerdeadMan : Enemy
{
    public override string Name => "GINGERDEAD MAN";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/gingerdead_man.tres");
    protected override Stats Stats => new(220, 225, 21, 3, 10, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["GDMAttack", "GDMDoNothing", "GDMBiteArm"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["GDMAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto biteArm;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 66)
                    goto doNothing;
                goto biteArm;
            case "angry":
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto biteArm;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto biteArm;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["GDMAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["GDMDoNothing"]);
    biteArm:
        return new BattleCommand(this, SelectTarget(), Skills["GDMBiteArm"]);
    }
}
