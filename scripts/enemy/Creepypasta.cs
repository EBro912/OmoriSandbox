using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class Creepypasta : Enemy
{
    public override string Name => "CREEPYPASTA";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/creepypasta.tres");
    protected override Stats Stats => new(300, 150, 50, 1, 90, 10, 95);
    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy" || state == "angry" || state == "hurt" || state == "toast";
    }
    protected override string[] EquippedSkills => ["CPAttack", "CPDoNothing", "CPScare"];
  
    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["CPAttack"]);
        
        if (CurrentHP < 60)
            goto scare;

        switch (CurrentState)
        {
            case "happy":
                if (Roll() < 66)
                    goto attack;
                goto nothing;
            case "sad":
                if (Roll() < 41)
                    goto attack;
                goto nothing;
            case "angry":
                if (Roll() < 86)
                    goto attack;
                goto nothing;
            default:
                if (Roll() < 76)
                    goto attack;
                goto nothing;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["CPAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["CPDoNothing"]);
    scare:
        return new BattleCommand(this, SelectAllTargets(), Skills["CPScare"]);
    }
}