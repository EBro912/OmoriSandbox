using Godot;

using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class SesameAlt : Enemy
{
    public override string Name => "SESAME";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sesame.tres");
    protected override Stats Stats => new(1500, 1500, 88, 65, 95, 10, 95);
    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "sad" or "happy" or "angry" or "hurt" or "toast";
    }
    protected override string[] EquippedSkills => ["SESAttack", "SESDoNothing", "SESBreadRoll"];
  
    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["SESBreadRoll"]);
        
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SESAttack"]);
        
        switch (CurrentState)
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