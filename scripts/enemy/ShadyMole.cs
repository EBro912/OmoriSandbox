using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

public class ShadyMole : Enemy
{
    public override string Name => "SHADY MOLE";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/shady_mole.tres");
    protected override Stats Stats => new(1200, 600, 45, 17, 65, 15, 95);
    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "sad" or "happy" or "angry" or "toast";
    }
    protected override string[] EquippedSkills => ["SMAttack", "SMB.E.D.", "SMDynamite"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["SMDynamite"]);
        
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SMAttack"]);
        
        switch (CurrentState)
        {
            case "happy":
                if (Roll() < 26)
                    goto attack;
                if (Roll() < 46)
                    goto bed;
                goto dynamite;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 56)
                    goto bed;
                goto dynamite;
            case "angry":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 26)
                    goto bed;
                goto dynamite;
            default:
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 41)
                    goto bed;
                goto dynamite;
        }
        attack:
        return new BattleCommand(this, SelectTarget(), Skills["SMAttack"]);
        bed:
        return new BattleCommand(this, SelectTarget(), Skills["SMB.E.D."]);
        dynamite:
        return new BattleCommand(this, SelectAllTargets(), Skills["SMDynamite"]);
    }
}