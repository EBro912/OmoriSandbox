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

    protected override Stats Stats => new(45, 75, 10, 1, 1, 10, 100);

    protected override string[] EquippedSkills => ["JKWalkSlowly", "JKAutoKill"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
    }

    private int Turn = 0;

    public override BattleCommand ProcessAI()
    {
        Turn++;
        if (Turn % 5 == 0)
            return new BattleCommand(this, SelectAllTargets(), Skills["JKAutoKill"]);
        return new BattleCommand(this, this, Skills["JKWalkSlowly"]);

    }
}