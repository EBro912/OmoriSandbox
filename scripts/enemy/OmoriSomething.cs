using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class OmoriSomething : Enemy
{
    public override string Name => "SOMETHING";
    public override Vector2 InfoBoxOffset => new(0, -100);
    public override bool InfoBoxCursorAboveBox => true;
    public override bool BlinksWhenSelected => false;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/omori_something.tres");
    protected override Stats Stats => new(50, 50, 7, 2, 10, 10, 1000);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    protected override string[] EquippedSkills => ["OSMEngulfTrue"];

    public override Task OnSpawned()
    {
        AddStatModifier("Immune", silent: true);
        return Task.CompletedTask;
    }

    public override Task OnStartOfBattle()
    {
        return OmoriSomethingTroop.Begin(this);
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
        goto engulfTrue;
    engulfTrue:
        return new BattleCommand(this, SelectAllTargets(), Skills["OSMEngulfTrue"]);
    }
}
