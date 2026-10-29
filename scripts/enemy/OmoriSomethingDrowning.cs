using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class OmoriSomethingDrowning : Enemy
{
    public override string Name => "SOMETHING";
    public override Vector2 InfoBoxOffset => new(0, -300);
    public override bool BlinksWhenSelected => false;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/omori_something_drowning.tres");
    protected override Stats Stats => new(9000, 4500, 10, 10, 900, 10, 1000);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    protected override string[] EquippedSkills => ["OSDRReserved", "OSDROmorisSomething", "OSDRAttack", "OSDRDoNothing"];

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

    // VANILLA BUG: its AI only covers turns 1 to 7 and the troop can't show it before the end of turn 7, so it never acts
    public override BattleCommand ProcessAI()
    {
        if (TrueTurnCount == 1)
            goto reserved;
        if (TrueTurnCount == 2)
            goto reserved;
        if (TrueTurnCount == 3)
            goto omoriSSomething;
        if (TrueTurnCount == 4)
            goto attack;
        if (TrueTurnCount == 5)
            goto doNothing;
        if (TrueTurnCount == 6)
            goto omoriSSomething;
        if (TrueTurnCount == 7)
            goto doNothing;
        return null;
    reserved:
        return new BattleCommand(this, SelectTarget(), Skills["OSDRReserved"]);
    omoriSSomething:
        return new BattleCommand(this, SelectTarget(), Skills["OSDROmorisSomething"]);
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["OSDRAttack"]);
    doNothing:
        return new BattleCommand(this, SelectTarget(), Skills["OSDRDoNothing"]);
    }
}
