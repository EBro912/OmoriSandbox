using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal sealed class KidsKiteAlt : Enemy
{
    public override string Name => "KID'S KITE";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/kids_kite.tres");
    protected override Stats Stats => new(3000, 1500, 100, 35, 40, 10, 95);
    protected override string[] EquippedSkills => ["KSKAttack", "KSKDoNothing", "KSKFly"];

    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "sad" or "happy" or "angry" or "hurt" or "toast";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["KSKAttack"]);
        
        switch (CurrentState)
        {
            case "angry":
                if (Roll() < 51)
                    goto fly;
                if (Roll() < 51)
                    goto attack;
                goto nothing;
            case "sad":
                if (Roll() < 21)
                    goto fly;
                if (Roll() < 31)
                    goto attack;
                goto nothing;
            case "happy":
                if (Roll() < 31)
                    goto fly;
                if (Roll() < 31)
                    goto attack;
                goto nothing;
            default:
                if (Roll() < 26)
                    goto fly;
                if (Roll() < 46)
                    goto attack;
                goto nothing;
        }
        fly:
        return new BattleCommand(this, this, Skills["KSKFly"]);
        attack:
        return new BattleCommand(this, SelectTarget(), Skills["KSKAttack"]);
        nothing:
        return new BattleCommand(this, this, Skills["KSKDoNothing"]);
    }
}