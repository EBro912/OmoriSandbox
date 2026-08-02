using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class LeftArm : Enemy
{
    public override string Name => "LEFT ARM";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/left_arm.tres");
    protected override string[] EquippedSkills => ["LAAttack", "RAFlex", "LAPoke"];
    protected override Stats Stats => new(175, 75, 12, 5, 5, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral" || emotion.Id == "happy" || emotion.Id == "sad" || emotion.Id == "angry";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["LAAttack"]);
        
        switch (CurrentEmotion.Id)
        {
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 51)
                    goto flex;
                goto poke;
            case "sad":
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 31)
                    goto flex;
                goto poke;
            case "happy":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 36)
                    goto flex;
                goto poke;
            default:
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 41)
                    goto flex;
                goto poke;     
        }
        
        attack:
        return new BattleCommand(this, SelectTarget(), Skills["LAAttack"]);
        flex:
        return new BattleCommand(this, this, Skills["RAFlex"]);
        poke:
        return new BattleCommand(this, SelectTarget(), Skills["LAPoke"]);
    }
}