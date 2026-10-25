using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class GatorGuy : Enemy
{
    public override string Name => "GATOR GUY";
    public override Vector2 InfoBoxOffset => CurrentEmotion.Group == null ? new(0, -250) : new(-10, -275);
    public override Vector2 DamageNumberOffset => new(-20, -160);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/gator_guy.tres");
    protected override Stats Stats => new(500, 150, 30, 13, 20, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["GGAttack", "GGDoNothing", "GGRoughUp"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["GGAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto roughUp;
            case "sad":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 51)
                    goto doNothing;
                goto roughUp;
            case "angry":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 16)
                    goto doNothing;
                goto roughUp;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto roughUp;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["GGAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["GGDoNothing"]);
    roughUp:
        return new BattleCommand(this, SelectTarget(), Skills["GGRoughUp"]);
    }
}
