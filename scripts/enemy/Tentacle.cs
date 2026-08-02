using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Tentacle : Enemy
{
    public override string Name => "TENTACLE";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/tentacle.tres");
    protected override Stats Stats => new(1200, 600, 72, 50, 110, 25, 95);
    protected override string[] EquippedSkills => ["TENAttack", "TENWeaken", "TENGrab", "TENGoop"];
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["TENAttack"]);
        
        switch (CurrentEmotion.Id)
        {
            case "angry":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 21)
                    goto weaken;
                if (Roll() < 36)
                    goto grab;
                goto goop;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 26)
                    goto weaken;
                if (Roll() < 46)
                    goto grab;
                goto goop;
            case "happy":
                if (Roll() < 26)
                    goto attack;
                if (Roll() < 36)
                    goto weaken;
                if (Roll() < 36)
                    goto grab;
                goto goop;   
            default:
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 31)
                    goto weaken;
                if (Roll() < 31)
                    goto grab;
                goto goop;    
        }
        
        attack:
        return new BattleCommand(this, SelectTarget(), Skills["TENAttack"]);
        weaken:
        return new BattleCommand(this, SelectTarget(), Skills["TENWeaken"]);
        grab:
        return new BattleCommand(this, SelectTarget(), Skills["TENGrab"]);
        goop:
        return new BattleCommand(this, SelectTarget(), Skills["TENGoop"]);
    }
}