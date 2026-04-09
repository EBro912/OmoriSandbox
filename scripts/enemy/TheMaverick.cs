using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Actors;

internal sealed class TheMaverick : Enemy
{
    public override string Name => "THE MAVERICK";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/the_maverick.tres");
    protected override Stats Stats => new(375, 125, 13, 5, 18, 15, 95);

    protected override string[] EquippedSkills => ["TMAttack", "TMDoNothing", "TMSmile", "TMTaunt"];

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

    private int Stage = 0;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0) return;

        if (Stage == 0 && CurrentHP < 281)
        {
            DialogueManager.Instance.QueueMessage(this, @"Hmph...\! Not bad...");
            DialogueManager.Instance.QueueMessage(this, "But this fight's just getting started!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 1;
        }
        
        if (Stage == 1 && CurrentHP < 225)
        {
            PartyMember target = BattleManager.Instance.GetPartyMemberAtPosition(2) ?? BattleManager.Instance.GetPartyMember(0);
            DialogueManager.Instance.QueueMessage(this, "Heh, as expected of my rival!");
            DialogueManager.Instance.QueueMessage(this, @"But I must tell you...\! While you were fooling around playing sports...");
            DialogueManager.Instance.QueueMessage(this, "I was honing my techniques just for this moment.");
            DialogueManager.Instance.QueueMessage(this, "You'll never reach my level of skill!");
            DialogueManager.Instance.QueueMessage(this, $"[br]You're going down, {target.Name.ToUpper()}!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 2;
        }
        
        if (Stage == 2 && CurrentHP < 187)
        {
            DialogueManager.Instance.QueueMessage(this, "Ha! Is that all you've got!?");
            DialogueManager.Instance.QueueMessage(this, "I've only been using 10% of my power!");
            DialogueManager.Instance.QueueMessage(this, @"[br]BEHOLD...\! MY FINAL FORM!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 3;
        }
        
        if (Stage == 3 && CurrentHP < 150)
        {
            DialogueManager.Instance.QueueMessage(this, "I bet you're regretting your decision now!");
            DialogueManager.Instance.QueueMessage(this, "I'm just way too cool for you...");
            DialogueManager.Instance.QueueMessage(this, "You're nothing but a loser!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 4;
        }
        
        if (Stage == 4 && CurrentHP < 112)
        {
            DialogueManager.Instance.QueueMessage(this, @"It's only... \!Huff...\! a matter of time before you tire yourselves out!");
            DialogueManager.Instance.QueueMessage(this, "My victory is imminent!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 5;
        }
        
        if (Stage == 5 && CurrentHP < 75)
        {
            DialogueManager.Instance.QueueMessage(this, @"Huff...\! I'll admit...\! I'm impressed...\! but you're still light years away from defeating me!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 6;
        }
        
        if (Stage == 6 && CurrentHP < 37)
        {
            DialogueManager.Instance.QueueMessage(this, @"[shake rate=20]Huff...\! Huff...");
            DialogueManager.Instance.QueueMessage(this, @"No...\![br]This is impossible!\! Improbable!");
            DialogueManager.Instance.QueueMessage(this, "[shake rate=20][br]Absolutely inconceivable!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 7;
        }
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, @"[shake rate=20]Huff...\! Huff...\! Huff...\! Wheeze...");
        DialogueManager.Instance.QueueMessage(this, @"[shake rate=20]Huff...\! Huff...\! Wheeze...");
        DialogueManager.Instance.QueueMessage(this, @"Gah...\! No...\![br]I won't let it end like this!");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnStartOfBattle()
    {
        PartyMember target = BattleManager.Instance.GetPartyMemberAtPosition(2) ?? BattleManager.Instance.GetPartyMember(0);
        DialogueManager.Instance.QueueMessage(this, $@"Oh {target.Name.ToUpper()}, you pitiful fool...\! You don't stand a chance against THE MAVERICK!");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, @"Wait...\! I won?\![br]I mean...");
            DialogueManager.Instance.QueueMessage(this, @"As expected! Haha!\![br]Fear me for I am THE MAVERICK!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 71)
            return new BattleCommand(this, SelectTarget(), Skills["TMAttack"]);
        if (Roll() < 16)
            return new BattleCommand(this, this, Skills["TMDoNothing"]);
        if (Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["TMSmile"]);
        return new BattleCommand(this, SelectTarget(), Skills["TMTaunt"]);
    }
}