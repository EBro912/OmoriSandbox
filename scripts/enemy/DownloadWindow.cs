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
        return emotion.Id is "neutral" or "happy" or "sad" or "angry";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["Crash"]);
            
        return TrueTurnCount switch
        {
            1 or 4 => new BattleCommand(this, this, Skills["DWDoNothing1"]),
            2 or 5 => new BattleCommand(this, this, Skills["DWDoNothing2"]),
            _ => new BattleCommand(this, SelectAllTargets(), Skills["Crash"]),
        };
    }
}