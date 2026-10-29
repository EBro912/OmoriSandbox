using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class OmoriSomethingHeights : Enemy
{
    public override string Name => "SOMETHING";
    public override Vector2 InfoBoxOffset => new(0, -225);
    public override bool InfoBoxCursorAboveBox => true;
    public override bool BlinksWhenSelected => false;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/omori_something_heights.tres");
    protected override Stats Stats => new(9000, 4500, 10, 10, 10, 10, 1000);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    protected override string[] EquippedSkills => ["DRKDoNothing1"];

    public override Task OnSpawned()
    {
        AddStatModifier("Immune", silent: true);
        return Task.CompletedTask;
    }

    public override Task ProcessEndOfTurn()
    {
        return OmoriSomethingTroop.RunTurnEnd(this);
    }

    public override Task ProcessBattleConditions()
    {
        return OmoriSomethingTroop.RunBattleConditions(this);
    }

    public override BattleCommand ProcessAI()
    {
        goto doNothing1;
    doNothing1:
        return new BattleCommand(this, SelectTarget(), Skills["DRKDoNothing1"]);
    }
}
