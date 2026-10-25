using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Harold : Enemy
{
    public override string Name => "HAROLD";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override Vector2 DamageNumberOffset => new(0, 40);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/harold.tres");
    protected override Stats Stats => new(225, 112, 28, 22, 15, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["HARAttack", "HARDoNothing", "HARProtect", "HARWink"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["HARAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                if (Roll() < 41)
                    goto protect;
                goto wink;
            case "sad":
                if (Roll() < 26)
                    goto attack;
                if (Roll() < 51)
                    goto doNothing;
                if (Roll() < 41)
                    goto protect;
                goto wink;
            case "angry":
                if (Roll() < 81)
                    goto attack;
                if (Roll() < 16)
                    goto doNothing;
                if (Roll() < 21)
                    goto protect;
                goto wink;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                if (Roll() < 31)
                    goto protect;
                goto wink;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["HARAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["HARDoNothing"]);
    protect:
        return new BattleCommand(this, this, Skills["HARProtect"]);
    wink:
        return new BattleCommand(this, SelectTarget(), Skills["HARWink"]);
    }
}
