using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SharkFin : Enemy
{
    public override string Name => "SHARK FIN";
    public override Vector2 InfoBoxOffset => new(0, -150);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/shark_fin.tres");
    protected override Stats Stats => new(425, 145, 43, 32, 31, 25, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["SHFBite", "SHFAttack", "SHFDoNothing", "SHFWorkUp"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SHFBite"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                if (Roll() < 31)
                    goto bite;
                goto workUp;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                if (Roll() < 31)
                    goto bite;
                goto workUp;
            case "angry":
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto bite;
            default:
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                if (Roll() < 51)
                    goto bite;
                goto workUp;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SHFAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["SHFDoNothing"]);
    bite:
        return new BattleCommand(this, SelectTarget(), Skills["SHFBite"]);
    workUp:
        return new BattleCommand(this, this, Skills["SHFWorkUp"]);
    }
}
