using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class ReverseMermaid : Enemy
{
    public override string Name => "REVERSE MERMAID";
    public override Vector2 InfoBoxOffset => new(0, -250);
    public override Vector2 DamageNumberOffset => CurrentEmotion.Group == null ? new(0, -50) : Vector2.Zero;
    protected internal override bool ObserveHasMulti => true;
    public override bool Mirrored => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/reverse_mermaid.tres");
    protected override Stats Stats => new(460, 165, 40, 8, 50, 12, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["RVMRunAround", "RVMAttack", "RVMDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectTargets(2), Skills["RVMRunAround"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["RVMAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto runAround;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                goto runAround;
            case "angry":
                if (Roll() < 71)
                    goto attack;
                if (Roll() < 31)
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
        return new BattleCommand(this, SelectTarget(), Skills["RVMAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["RVMDoNothing"]);
    runAround:
        return new BattleCommand(this, SelectTargets(2), Skills["RVMRunAround"]);
    }
}
