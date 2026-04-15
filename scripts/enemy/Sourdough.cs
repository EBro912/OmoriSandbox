using Godot;

using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class Sourdough : Enemy
{
    public override string Name => "SOURDOUGH";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sourdough.tres");
    protected override Stats Stats => new(363, 93, 55, 33, 49, 10, 95);
    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy" || state == "angry" || state == "hurt" || state == "toast";
    }
    protected override string[] EquippedSkills => ["SDAttack", "SDDoNothing", "SDBadWord"];
  
    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SDBadWord"]);
        
        switch (CurrentState)
        {
            case "happy":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto badword;
            case "sad":
                if (Roll() < 21)
                    goto attack;
                if (Roll() < 56)
                    goto nothing;
                goto badword;
            case "angry":
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                goto badword;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                goto badword;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SDAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["SDDoNothing"]);
    badword:
        return new BattleCommand(this, SelectTarget(), Skills["SDBadWord"]);
    }
}