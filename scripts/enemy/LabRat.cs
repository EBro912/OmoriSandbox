using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class LabRat : Enemy
{
    public override string Name => "LAB RAT";
    public override Vector2 InfoBoxOffset => new(25, -250);
    protected internal override bool ObserveHasMulti => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/lab_rat.tres");
    protected override Stats Stats => new(300, 110, 53, 17, 70, 15, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["LRHappyGas", "LRAttack", "LRDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["LRHappyGas"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["LRAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 76)
                    goto attack;
                goto doNothing;
            case "sad":
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 41)
                    goto doNothing;
                goto happyGas;
            case "angry":
                if (Roll() < 71)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                goto happyGas;
            default:
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto happyGas;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["LRAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["LRDoNothing"]);
    happyGas:
        return new BattleCommand(this, SelectAllTargets(), Skills["LRHappyGas"]);
    }
}
