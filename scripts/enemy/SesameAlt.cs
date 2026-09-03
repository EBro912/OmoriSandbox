using Godot;

using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class SesameAlt : Enemy
{
    public override string Name => "SESAME";
    public override Vector2 InfoBoxOffset => new(0, -175);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sesame.tres");
    protected override Stats Stats => new(1000, 1000, 77, 43, 122, 20, 95);
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
    }
    protected override string[] EquippedSkills => ["SESAttack", "SESDoNothing", "SESBreadRoll"];
    internal override bool ObserveHasMulti => true;
  
    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["SESBreadRoll"]);
        
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SESAttack"]);
        
        switch (CurrentEmotion.Id)
        {
            case "happy":
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 36)
                    goto nothing;
                goto roll;
            case "sad":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 51)
                    goto nothing;
                goto roll;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 21)
                    goto nothing;
                goto roll;
            default:
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 41)
                    goto nothing;
                goto roll;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SESAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["SESDoNothing"]);
    roll:
        return new BattleCommand(this, SelectAllTargets(), Skills["SESBreadRoll"]);
    }
}