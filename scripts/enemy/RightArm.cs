using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class RightArm : Enemy
{
    public override string Name => "RIGHT ARM";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/right_arm.tres");
    protected override string[] EquippedSkills => ["RAAttack", "RAFlex", "RAGrab"];
    protected override Stats Stats => new(175, 75, 12, 5, 5, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["RAAttack"]);
        
        switch (CurrentEmotion.Id)
        {
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 51)
                    goto flex;
                goto grab;
            case "sad":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 31)
                    goto flex;
                goto grab;
            case "happy":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 41)
                    goto flex;
                goto grab;
            default:
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 51)
                    goto flex;
                goto grab;     
        }
        
        attack:
        return new BattleCommand(this, SelectTarget(), Skills["RAAttack"]);
        flex:
        return new BattleCommand(this, this, Skills["RAFlex"]);
        grab:
        return new BattleCommand(this, SelectTarget(), Skills["RAGrab"]);
    }
}