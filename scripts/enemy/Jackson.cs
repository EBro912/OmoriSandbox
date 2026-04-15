using Godot;
using System.Threading.Tasks;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class Jackson : Enemy
{
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/jackson.tres");

    public override string Name => "JACKSON";

    protected override Stats Stats => new(45, 75, 10, 1, 1, 10, 100);

    protected override string[] EquippedSkills => ["JKWalkSlowly", "JKAutoKill"];

    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "sad" or "happy" or "angry" or "hurt" or "toast";
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