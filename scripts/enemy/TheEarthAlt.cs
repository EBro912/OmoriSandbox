using Godot;

using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class TheEarthAlt : Enemy
{
    public override string Name => "THE EARTH";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/earth.tres");
    protected override Stats Stats => new(5000, 2500, 70, 70, 85, 10, 95);

    protected override string[] EquippedSkills => ["TEACruel", "TEAProtect"];

    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "sad" or "happy" or "angry" or "hurt" or "toast";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out _))
            return new BattleCommand(this, SelectAllTargets(), Skills["TEAProtect"]);
        
        switch (CurrentState)
        {
            case "sad":
                if (Roll() < 51)
                    goto cruel;
                goto protect;
            case "happy":
                if (Roll() < 61)
                    goto cruel;
                goto protect;
            case "angry":
                if (Roll() < 31)
                    goto cruel;
                goto protect;
            default:
                if (Roll() < 56)
                    goto cruel;
                goto protect;
        }
    cruel:
        return new BattleCommand(this, SelectAllTargets(), Skills["TEACruel"]); 
    protect:
        return new BattleCommand(this, SelectAllTargets(), Skills["TEAProtect"]);
    }
}