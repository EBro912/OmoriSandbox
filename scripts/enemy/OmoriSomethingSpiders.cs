using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class OmoriSomethingSpiders : Enemy
{
    public override string Name => "SOMETHING";
    public override Vector2 InfoBoxOffset => new(0, -300);
    public override bool BlinksWhenSelected => false;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/omori_something_spiders.tres");
    protected override Stats Stats => new(9000, 4500, 10, 10, 100, 10, 1000);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    protected override string[] EquippedSkills => ["WALAttack", "WALDoNothing", "WALSpiderWebs"];

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
        if (Roll() < 36)
            goto attack;
        if (Roll() < 31)
            goto doNothing;
        goto spiderWebs;
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["WALAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["WALDoNothing"]);
    spiderWebs:
        return new BattleCommand(this, SelectTarget(), Skills["WALSpiderWebs"]);
    }
}
