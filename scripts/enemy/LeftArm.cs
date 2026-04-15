using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal sealed class LeftArm : Enemy
{
    public override string Name => "LEFT ARM";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/left_arm.tres");
    protected override string[] EquippedSkills => ["LAAttack", "RAFlex", "LAPoke"];
    protected override Stats Stats => new(175, 75, 12, 5, 5, 10, 95);

    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "happy" || state == "sad"
               || state == "angry" || state == "hurt" || state == "toast";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["LAAttack"]);
        
        switch (CurrentState)
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