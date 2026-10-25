using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class LostSproutMoleVF : Enemy
{
    public override string Name => "LOST SPROUT MOLE";
    public override Vector2 InfoBoxOffset => new(0, -180);
    public override Vector2 DamageNumberOffset => new(0, 30);
    protected internal override bool ObserveHasMulti => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sprout_mole.tres");
    protected override Stats Stats => new(62, 31, 11, 8, 9, 5, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["LSMRunAround", "LSMAttack", "LSMDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectTargets(1), Skills["LSMRunAround"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["LSMAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto runAround;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                goto runAround;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                goto runAround;
            default:
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto runAround;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["LSMAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["LSMDoNothing"]);
    runAround:
        return new BattleCommand(this, SelectTargets(1), Skills["LSMRunAround"]);
    }
}
