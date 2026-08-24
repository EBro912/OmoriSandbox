using Godot;

using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class YeOldSprout : Enemy
{
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/ye_old_sprout.tres");

    public override string Name => "YE OLD SPROUT";
    public override Vector2 InfoBoxOffset => new(0, -250);

    protected override Stats Stats => new Stats(300, 150, 8, 8, 2, 10, 95);

    protected override string[] EquippedSkills => ["YOSRollOver"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
    }

    public override BattleCommand ProcessAI()
    {
        return new BattleCommand(this, SelectAllTargets(), Skills["YOSRollOver"]);
    }
}