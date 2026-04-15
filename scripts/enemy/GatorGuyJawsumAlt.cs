using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class GatorGuyJawsumAlt : Enemy
{
    public override string Name => "GATOR GUY";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/gator_guy.tres");
    protected override Stats Stats => new(2200, 85, 100, 60, 70, 10, 95);
    protected override string[] EquippedSkills => ["GGAttack", "GGDoNothing", "GGRoughUp"];
    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "happy" || state == "sad"
               || state == "angry" || state == "hurt" || state == "toast";
    }
    public override BattleCommand ProcessAI()
    {
        switch (CurrentState)
        {
            case "happy":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto rough;
            case "sad":
                if (Roll() < 26)
                    goto attack;
                if (Roll() < 41)
                    goto nothing;
                goto rough;
            case "angry":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                goto rough;
            default:
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                goto rough;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["GGAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["GGDoNothing"]);
    rough:
        return new BattleCommand(this, SelectTarget(), Skills["GGRoughUp"]);
    }
}