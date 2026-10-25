using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class BugBunny : Enemy
{
    public override string Name => "BUG BUNNY";
    public override Vector2 InfoBoxOffset => CurrentEmotion.Group == null ? new(0, -175) : new(0, -150);
    public override Vector2 DamageNumberOffset => new(0, 20);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/bug_bunny.tres");
    protected override Stats Stats => new(145, 62, 25, 5, 25, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["BGBAttack", "BGBDoNothing", "BGBSuddenJump", "BGBScuttle"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["BGBAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto suddenJump;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 66)
                    goto doNothing;
                if (Roll() < 31)
                    goto suddenJump;
                goto scuttle;
            case "angry":
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 16)
                    goto doNothing;
                if (Roll() < 66)
                    goto suddenJump;
                goto scuttle;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                if (Roll() < 51)
                    goto suddenJump;
                goto scuttle;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["BGBAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["BGBDoNothing"]);
    suddenJump:
        return new BattleCommand(this, SelectTarget(), Skills["BGBSuddenJump"]);
    scuttle:
        return new BattleCommand(this, this, Skills["BGBScuttle"]);
    }
}
