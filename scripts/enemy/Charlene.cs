using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Actors;

internal sealed class Charlene : Enemy
{
    public override string Name => "CHARLENE";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/charlene.tres");
    protected override Stats Stats => new(300, 100, 10, 40, 10, 10, 95);

    protected override string[] EquippedSkills => ["CHAttack", "CHDoNothing"];

    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "sad" or "happy" or "angry" or "hurt" or "toast";
    }

    protected override PartyMember SelectTarget()
    {
        if (HasStatModifier("Charm"))
            return (StatModifiers["Charm"] as CharmStatModifier).CharmedBy;
        List<PartyMemberComponent> members = BattleManager.Instance.GetAlivePartyMembers();
        List<PartyMemberComponent> taunting = members.FindAll(x => x.Actor.HasStatModifier("Taunt"));
        if (taunting.Count == 0)
        {
            return members.MaxBy(x => x.Actor.CurrentStats.SPD).Actor;
        }
        return taunting.MaxBy(x => x.Actor.CurrentStats.SPD).Actor;
    }

    public override Task OnStartOfBattle()
    {
        AddStatModifier("Immune");
        return Task.CompletedTask;
    }

    public override async Task ProcessBattleConditions()
    {
        if (SelectAllEnemies().Count == 1)
        {
            DialogueManager.Instance.QueueMessage("CHARLIE", CenterPoint, "...");
            DialogueManager.Instance.QueueMessage("CHARLIE stopped fighting.");
            await DialogueManager.Instance.WaitForDialogue();
            CurrentHP = 0;
        }
    }

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 16)
            return new BattleCommand(this, SelectTarget(), Skills["CHAttack"]);
        return new BattleCommand(this, this, Skills["CHDoNothing"]);
    }
}