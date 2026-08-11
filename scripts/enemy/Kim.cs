using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Actors;

internal sealed class Kim : Enemy
{
    public override string Name => "KIM";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/kim.tres");
    protected override Stats Stats => new(130, 65, 20, 3, 10, 5, 95);

    protected override string[] EquippedSkills => ["KMAttack", "KMDoNothing", "KMSmash", "KMTaunt"];

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

        if (!HasSpoken && IsBelowHP(0.5f))
        {
            DialogueManager.Instance.QueueMessage(this, "Your face annoys me!");
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, @"Grumble... Grumble...\![br]You're... still nerds...");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnStartOfBattle()
    {
        DialogueManager.Instance.QueueMessage(this, @"I'll show you that size isn't everything!\! I'm not about to lose to nerds like you!");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, "Heh... You guys never stood a chance.");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 56)
            return new BattleCommand(this, SelectTarget(), Skills["KMAttack"]);
        if (Roll() < 26)
            return new BattleCommand(this, this, Skills["KMDoNothing"]);
        if (Roll() < 66)
            return new BattleCommand(this, SelectTarget(), Skills["KMSmash"]);
        return new BattleCommand(this, SelectTarget(), Skills["KMTaunt"]);
    }
}