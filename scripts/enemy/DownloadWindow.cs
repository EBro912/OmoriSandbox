using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class DownloadWindow : Enemy
{
    public override string Name => "DOWNLOAD WINDOW";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/download_window.tres");
    protected override Stats Stats => new(600, 210, 10, 5, 1, 10, 95);
    protected override string[] EquippedSkills => ["Crash", "DWDoNothing1", "DWDoNothing2"];
    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "happy" || state == "sad"
               || state == "angry" || state == "hurt" || state == "toast";
    }

    private int TurnCounter = 0;
    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["Crash"]);
            
        TurnCounter++;
        return TurnCounter switch
        {
            1 or 4 => new BattleCommand(this, this, Skills["DWDoNothing1"]),
            2 or 5 => new BattleCommand(this, this, Skills["DWDoNothing2"]),
            _ => new BattleCommand(this, SelectAllTargets(), Skills["Crash"]),
        };
    }
}