using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class BigStrongTree : Enemy
{
    public override string Name => "BIG STRONG TREE";
    public override Vector2 InfoBoxOffset => new(0, -300);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/big_strong_tree.tres");
    protected override Stats Stats => new(1000, 500, 999, 999, 1, 999, 100);
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral";
    }

    protected override string[] EquippedSkills => ["BSTDoNothing"];

    public override BattleCommand ProcessAI()
    {
        return new BattleCommand(this, this, Skills["BSTDoNothing"]);
    }
}