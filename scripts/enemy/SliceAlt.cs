using Godot;

using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class SliceAlt : Enemy
{
    public override string Name => "SLICE";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/slice.tres");
    protected override Stats Stats => new(1500, 1500, 88, 65, 95, 10, 95);
    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy" || state == "angry" || state == "hurt" || state == "toast";
    }
    protected override string[] EquippedSkills => ["SLAttack", "SLDoNothing", "SLRile"];
  
    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SLAttack"]);
        
        switch (CurrentState)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto rile;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 51)
                    goto nothing;
                goto rile;
            case "angry":
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                goto rile;
            default:
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 41)
                    goto nothing;
                goto rile;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SLAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["SLDoNothing"]);
    rile:
        return new BattleCommand(this, SelectAllEnemies(), Skills["SLRile"]);
    }
}