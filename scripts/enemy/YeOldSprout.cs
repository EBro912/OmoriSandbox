using Godot;

using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class YeOldSprout : Enemy
{
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/ye_old_sprout.tres");

    public override string Name => "YE OLD SPROUT";

    protected override Stats Stats => new Stats(300, 150, 8, 8, 2, 10, 95);

    protected override string[] EquippedSkills => ["YOSRollOver"];

    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "sad" or "happy" or "angry" or "hurt" or "toast";
    }

    public override BattleCommand ProcessAI()
    {
        return new BattleCommand(this, SelectAllTargets(), Skills["YOSRollOver"]);
    }
}