using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class DownloadWindow : Enemy
{
    public override string Name => "DOWNLOAD WINDOW";
    public override Vector2 InfoBoxOffset => new(0, -175);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/download_window.tres");
    protected override Stats Stats => new(600, 210, 10, 5, 1, 10, 95);
    protected override string[] EquippedSkills => ["Crash", "DWDoNothing1", "DWDoNothing2"];
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral" || emotion.Id == "happy" || emotion.Id == "sad" || emotion.Id == "angry";
    }

    private int TurnCounter = 0;
    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["Crash"]);
            
        if (!PreRolling)
            TurnCounter++;
        // the pre-roll must pick the same schedule entry as the real roll without advancing the counter
        return (PreRolling ? TurnCounter + 1 : TurnCounter) switch
        {
            1 or 4 => new BattleCommand(this, this, Skills["DWDoNothing1"]),
            2 or 5 => new BattleCommand(this, this, Skills["DWDoNothing2"]),
            _ => new BattleCommand(this, SelectAllTargets(), Skills["Crash"]),
        };
    }
}