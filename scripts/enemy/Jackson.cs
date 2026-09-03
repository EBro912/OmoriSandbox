using Godot;
using System.Threading.Tasks;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class Jackson : Enemy
{
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/jackson.tres");

    public override string Name => "JACKSON";
    public override Vector2 InfoBoxOffset => new(6, -162);
    public override bool InfoBoxCursorAboveBox => true;

    protected override Stats Stats => new(45, 75, 10, 1, 10, 10, 100);

    protected override string[] EquippedSkills => ["JKWalkSlowly", "JKAutoKill", "Idle"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
    }

    private int Turn = 0;

    public override BattleCommand ProcessAI()
    {
        if (!PreRolling)
            Turn++;
        if (Turn == 5)
            return new BattleCommand(this, SelectAllTargets(), Skills["JKAutoKill"]);
        if (Turn > 5)
            return new BattleCommand(this, this, Skills["Idle"]);
        return new BattleCommand(this, this, Skills["JKWalkSlowly"]);

    }
}