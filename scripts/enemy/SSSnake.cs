using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SSSnake : Enemy
{
    public override string Name => "S.S. SNAKE";
    public override Vector2 InfoBoxOffset => CurrentEmotion.Group == null ? new(0, -170) : new(0, -175);
    public override Vector2 DamageNumberOffset => new(0, 20);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/ss_snake.tres");
    protected override Stats Stats => new(620, 310, 60, 25, 75, 25, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["SSSAttack", "SSSDoNothing", "SSSSlither"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SSSAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 71)
                    goto attack;
                goto doNothing;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 41)
                    goto doNothing;
                goto slither;
            case "angry":
                if (Roll() < 51)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto slither;
            default:
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto slither;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SSSAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["SSSDoNothing"]);
    slither:
        return new BattleCommand(this, this, Skills["SSSSlither"]);
    }
}
