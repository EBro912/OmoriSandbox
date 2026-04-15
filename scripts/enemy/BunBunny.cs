using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class BunBunny : Enemy
{
    public override string Name => "BUN BUNNY";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/bun_bunny.tres");
    protected override Stats Stats => new(400, 200, 35, 35, 30, 10, 95);
    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy" || state == "angry" || state == "hurt" || state == "toast";
    }
    protected override string[] EquippedSkills => ["BBAttack", "BBDoNothing", "BBHide"];
  
    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["BBAttack"]);
        
        switch (CurrentState)
        {
            case "happy":
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto hide;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 51)
                    goto nothing;
                goto hide;
            case "angry":
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                goto hide;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 41)
                    goto nothing;
                goto hide;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["BBAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["BBDoNothing"]);
    hide:
        return new BattleCommand(this, this, Skills["BBHide"]);
    }
}