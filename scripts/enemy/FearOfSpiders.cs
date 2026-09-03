using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class FearOfSpiders : Enemy
{
    public override string Name => "SOMETHING";
    public override Vector2 InfoBoxOffset => new(0, -125);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/fear_of_spiders.tres");
    protected override Stats Stats => new(7500, 3000, 115, 35, 110, 30, 95);
    protected override string[] EquippedSkills => ["FOSAttack", "FOSDoNothing", "FOSSpinWeb", "FOSAttackAll"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral";
    }

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["FOSAttack"]);
        if (Roll() < 16)
            return new BattleCommand(this, this, Skills["FOSDoNothing"]);
        if (Roll() < 21)
            return new BattleCommand(this, SelectTarget(), Skills["FOSSpinWeb"]);
        return new BattleCommand(this, SelectAllTargets(), Skills["FOSAttackAll"]);
    }
}