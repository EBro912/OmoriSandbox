using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class MilkshakeBunny : Enemy
{
    public override string Name => "MILKSHAKE BUNNY";
    public override Vector2 InfoBoxOffset => new(0, -175);
    public override Vector2 DamageNumberOffset => new(0, 40);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/milkshake_bunny.tres");
    protected override Stats Stats => new(220, 110, 15, 7, 10, 20, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["MSBShake", "MSBAttack", "MSBDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["MSBShake"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["MSBAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto shake;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 41)
                    goto doNothing;
                goto shake;
            case "angry":
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto shake;
            default:
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto shake;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["MSBAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["MSBDoNothing"]);
    shake:
        return new BattleCommand(this, SelectAllTargets(), Skills["MSBShake"]);
    }
}
