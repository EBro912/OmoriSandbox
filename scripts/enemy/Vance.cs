using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Actors;

internal sealed class Vance : Enemy
{
    public override string Name => "VANCE";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/vance.tres");
    protected override Stats Stats => new(145, 72, 13, 10, 7, 10, 95);

    protected override string[] EquippedSkills => ["VAAttack", "VADoNothing", "VACandy", "VATease"];

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

    private bool HasSpoken = false;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0) return;

        if (!HasSpoken && CurrentHP < 72)
        {
            DialogueManager.Instance.QueueMessage(this, @"Ouch...\! That hurts.");
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }
    }
    
    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, @"Dang...\! All I wanted was some taffy.");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, @"So... uh...\! Can we get that taffy now?");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 66)
            return new BattleCommand(this, SelectTarget(), Skills["VAAttack"]);
        if (Roll() < 26)
            return new BattleCommand(this, this, Skills["VADoNothing"]);
        if (Roll() < 46)
            return new BattleCommand(this, SelectAllTargets(), Skills["VACandy"]);
        return new BattleCommand(this, SelectTarget(), Skills["VATease"]);
    }
}