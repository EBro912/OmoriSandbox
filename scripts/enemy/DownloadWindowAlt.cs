using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class DownloadWindowAlt : Enemy
{
    public override string Name => "DOWNLOAD WINDOW";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/download_window.tres");
    protected override Stats Stats => new(6000, 3000, 10, 65, 1, 10, 95);
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
        
        TurnCounter++;
        return TurnCounter switch
        {
            1 or 4 => new BattleCommand(this, this, Skills["DWDoNothing1"]),
            2 or 5 => new BattleCommand(this, this, Skills["DWDoNothing2"]),
            _ => new BattleCommand(this, SelectAllTargets(), Skills["Crash"]),
        };
    }
}