using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Actors;

internal sealed class AngelAlt : Enemy
{
    public override string Name => "ANGEL";
    public override Vector2 InfoBoxOffset => new(0, -375);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/angel.tres");
    protected override Stats Stats => new(150, 75, 15, 6, 18, 30, 95);

    protected override string[] EquippedSkills => ["ANAttack", "ANDoNothing", "ANQuickAttack", "ANTease"];

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
    
    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, @"Sniff..\! You...\![br]You'll pay for this...");
        await DialogueManager.Instance.WaitForDialogue();
    }

    private bool HasSpoken = false;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0) return;

        if (!HasSpoken && IsBelowHP(0.5f))
        {
            PartyMember target = BattleManager.Instance.GetPartyMemberAtPosition(2) ?? BattleManager.Instance.GetPartyMember(0);
            DialogueManager.Instance.QueueMessage(this, $"Heh. You surprise me, {target.Name.ToUpper()}!");
            DialogueManager.Instance.QueueMessage(this, "You would be a worthy rival for my master!");
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(@"[wave freq=20.0][font_size=40]FWEFWE[font_size=52]FWEFWE!!!");
            DialogueManager.Instance.QueueMessage(this, "My master has taught me well!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 76)
            return new BattleCommand(this, SelectTarget(), Skills["ANAttack"]);
        if (Roll() < 26)
            return new BattleCommand(this, this, Skills["ANDoNothing"]);
        if (Roll() < 46)
            return new BattleCommand(this, SelectTarget(), Skills["ANQuickAttack"]);
        return new BattleCommand(this, SelectTarget(), Skills["ANTease"]);
    }
}