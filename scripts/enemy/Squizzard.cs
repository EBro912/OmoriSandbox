using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Squizzard : Enemy
{
    public override string Name => "SQUIZZARD";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override Vector2 DamageNumberOffset => new(0, 30);
    protected internal override bool ObserveHasMulti => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/squizzard.tres");
    protected override Stats Stats => new(550, 175, 42, 25, 15, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["SQZSquidMagic", "SQZAttack", "SQZDoNothing", "SQZSquidWard"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["SQZSquidMagic"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SQZAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                if (Roll() < 51)
                    goto squidWard;
                goto squidMagic;
            case "sad":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 51)
                    goto doNothing;
                if (Roll() < 51)
                    goto squidWard;
                goto squidMagic;
            case "angry":
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                if (Roll() < 51)
                    goto squidWard;
                goto squidMagic;
            default:
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                if (Roll() < 51)
                    goto squidWard;
                goto squidMagic;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SQZAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["SQZDoNothing"]);
    squidWard:
        return new BattleCommand(this, SelectEnemy(), Skills["SQZSquidWard"]);
    squidMagic:
        return new BattleCommand(this, SelectAllTargets(), Skills["SQZSquidMagic"]);
    }
}
