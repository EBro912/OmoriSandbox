using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Wasp : Enemy
{
    public override string Name => "WASP";
    public override Vector2 InfoBoxOffset => new(0, -125);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/wasp.tres");
    protected override Stats Stats => new(10, 5, 4, 2, 100, 10, 95, 5);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["WSPAttack", "WSPDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 51)
            goto attack;
        goto doNothing;
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["WSPAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["WSPDoNothing"]);
    }
}
