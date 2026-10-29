using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Actors;

internal sealed class OmoriSomethingHangingBody : Enemy
{
    public override string Name => "HANGING BODY";
    public override Vector2 InfoBoxOffset => new(0, -300);
    public override bool BlinksWhenSelected => false;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/omori_something_hanging_body.tres");
    protected override Stats Stats => new(9999, 5000, 10, 999, 10, 10, 1000);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    protected override string[] EquippedSkills => ["OSHBNothing1", "OSHBNothing2", "OSHBNothing3", "OSHBNothing10", "OSHBNothing11", "OSHBNothing12"];

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
        (int Stage, string Skill)[] lines =
            [(1, "OSHBNothing1"), (2, "OSHBNothing2"), (3, "OSHBNothing3"), (10, "OSHBNothing10"), (11, "OSHBNothing11"), (12, "OSHBNothing12")];
        List<PartyMember> members = BattleManager.Instance.GetAlivePartyMembers();
        foreach ((int stage, string skill) in lines)
        {
            List<PartyMember> holders = members.FindAll(member => HangingModifier.StageOf(member) == stage);
            if (holders.Count > 0)
                return new BattleCommand(this, holders[GameManager.Instance.Random.RandiRange(0, holders.Count - 1)], Skills[skill]);
        }
        return null;
    }
}
