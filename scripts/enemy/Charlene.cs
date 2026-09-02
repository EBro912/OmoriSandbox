using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Actors;

internal sealed class Charlene : Enemy
{
    // base game calls her CHARLIE
    public override string Name => "CHARLIE";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/charlene.tres");
    protected override Stats Stats => new(300, 100, 10, 40, 10, 10, 95);
    public override Vector2 InfoBoxOffset => new(0, -375);
    protected override string[] EquippedSkills => ["CHAttack", "CHDoNothing"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
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
        int chance = CurrentEmotion.Id switch
        {
            "sad" => 6,
            "angry" => 36,
            _ => 16,
        };
        if (Roll() < chance)
            return new BattleCommand(this, SelectTarget(), Skills["CHAttack"]);
        return new BattleCommand(this, this, Skills["CHDoNothing"]);
    }
}