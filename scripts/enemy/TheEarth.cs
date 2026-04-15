using Godot;

using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class TheEarth : Enemy
{
    public override string Name => "THE EARTH";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/earth.tres");
    protected override Stats Stats => new(425, 210, 20, 15, 15, 10, 95);

    protected override string[] EquippedSkills => ["TEAttack", "TEDoNothing", "TECruel", "TEProtect"];

    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "sad" or "happy" or "angry" or "hurt" or "toast";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["TEAttack"]);
        
        if (CurrentHP < 85)
            return new BattleCommand(this, SelectAllTargets(), Skills["TEProtect"]);

        switch (CurrentState)
        {
            case "sad":
                if (Roll() < 51)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto cruel;
            case "happy":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 61)
                    goto nothing;
                goto cruel;
            case "angry":
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 36)
                    goto nothing;
                goto cruel;
            default:
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 36)
                    goto nothing;
                goto cruel;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["TEAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["TEDoNothing"]);
    cruel:
        return new BattleCommand(this, SelectTarget(), Skills["TECruel"]);
    }
}