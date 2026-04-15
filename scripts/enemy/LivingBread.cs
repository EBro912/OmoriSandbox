using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class LivingBread : Enemy
{
    public override string Name => "LIVING BREAD";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/living_bread.tres");
    protected override Stats Stats => new(250, 75, 45, 15, 5, 10, 95);
    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy" || state == "angry" || state == "hurt" || state == "toast";
    }
    protected override string[] EquippedSkills => ["LBAttack", "LBDoNothing", "LBBite"];
  
    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["LBBite"]);
        
        switch (CurrentState)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 46)
                    goto nothing;
                goto bite;
            case "sad":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 56)
                    goto nothing;
                goto bite;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 16)
                    goto nothing;
                goto bite;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 41)
                    goto nothing;
                goto bite;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["LBAttack"]);
    nothing:
        // living bread do nothing skill dialogue has a target
        return new BattleCommand(this, SelectTarget(), Skills["LBDoNothing"]);
    bite:
        return new BattleCommand(this, SelectTarget(), Skills["LBBite"]);
    }
}