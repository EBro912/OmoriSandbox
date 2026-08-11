using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Actors;

internal sealed class TheHooligans : Enemy
{
    public override string Name => "THE HOOLIGANS";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/the_hooligans.tres");
    protected override Stats Stats => new(500, 250, 25, 22, 22, 25, 95);

    protected override string[] EquippedSkills => ["HOAngelAttack", "HOMaverickCharm", "HOKimHeadbutt", "HOVanceCandy", "HOGroupAttack"];

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

    private int Stage = 0;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0) return;

        if (Stage == 0 && IsBelowHP(0.75f))
        {
            DialogueManager.Instance.QueueMessage("ANGEL", CenterPoint, "My master and I have been training for this moment...");
            DialogueManager.Instance.QueueMessage("THE MAVERICK", CenterPoint, "You won't make fools out of us ever again!");
            DialogueManager.Instance.QueueMessage("VANCE", CenterPoint, "KIM... Are you ready to rumble?");
            DialogueManager.Instance.QueueMessage("KIM", CenterPoint, @"You know it, VANCE!\! These nerds have got it coming to 'em!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 1;
        }

        if (Stage == 1 && IsBelowHP(0.5f))
        {
            DialogueManager.Instance.QueueMessage("THE MAVERICK", CenterPoint, "ANGEL, remember our training! Make weakness your strength!");
            DialogueManager.Instance.QueueMessage("ANGEL", CenterPoint, "Yes, master![br]I won't let you down!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 2;
        }
        
        if (Stage == 2 && IsBelowHP(0.25f))
        {
            DialogueManager.Instance.QueueMessage("VANCE", CenterPoint, "KIM... are you okay?");
            DialogueManager.Instance.QueueMessage("KIM", CenterPoint, @"Huff...\! Huff...\! Heh!\! Don't worry, VANCE... I'm not done yet!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 3;
        }
    }
    
    public override async Task OnDefeat()
    {
        BattleCommand command = BattleManager.Instance.GetCurrentCommand();
        if (command.Action is Item item && item.Name == "PEPPER SPRAY")
        {
            DialogueManager.Instance.QueueMessage("CHARLIE", CenterPoint, "!!!");
            DialogueManager.Instance.QueueMessage("ANGEL", CenterPoint, @"[shake rate=20]AUGH!! MY EYES!\! MASTER! I CAN'T SEE!!");
            DialogueManager.Instance.QueueMessage("THE MAVERICK", CenterPoint, @"[shake rate=20]Huff...\! Wheeze...\! What trickery is this!?");
            DialogueManager.Instance.QueueMessage("VANCE", CenterPoint, "Ouch... That hurts.");
            DialogueManager.Instance.QueueMessage("KIM", CenterPoint, @"WHAT THE HECK IS THIS!?\! [color=#ff9233]PEPPER SPRAY[/color]?\! REALLY!?");
            DialogueManager.Instance.QueueMessage("AUBREY", CenterPoint, @"Gah...\! You two are the worst...");
            await DialogueManager.Instance.WaitForDialogue();
            return;
        }

        DialogueManager.Instance.QueueMessage("THE MAVERICK", CenterPoint, @"Huff...\! Huff...\! Is this real life?");
        DialogueManager.Instance.QueueMessage("ANGEL", CenterPoint, @"How...\! How is this possible!?");
        DialogueManager.Instance.QueueMessage("KIM", CenterPoint, "I can't believe we lost...");
        DialogueManager.Instance.QueueMessage("VANCE", CenterPoint, @"KIM... I'm hungry. \![br]Can we go now?");
        DialogueManager.Instance.QueueMessage("AUBREY", CenterPoint, "... ... ...");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage("THE MAVERICK", CenterPoint, @"Huff...\! Huff...\![br]Is...\!Is this real life?");
            DialogueManager.Instance.QueueMessage("ANGEL", CenterPoint, "We won, master! We won!");
            DialogueManager.Instance.QueueMessage("KIM", CenterPoint, "Serves you right, nerds.");
            DialogueManager.Instance.QueueMessage("VANCE", CenterPoint, @"KIM... I'm hungry... Let's go get some food.");
            DialogueManager.Instance.QueueMessage("AUBREY", CenterPoint, "Heh.");
            PartyMember targetOne = BattleManager.Instance.GetPartyMemberAtPosition(0) ?? BattleManager.Instance.GetPartyMember(0);
            PartyMember targetTwo = BattleManager.Instance.GetPartyMemberAtPosition(2) ?? BattleManager.Instance.GetPartyMember(0);
            DialogueManager.Instance.QueueMessage("AUBREY", CenterPoint, $@"{targetTwo.Name.ToUpper()}...\! {targetOne.Name.ToUpper()}...\![br]Get the heck out of here.");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }

    public override BattleCommand ProcessAI()
    {
        if (IsBelowHP(0.152f))
            return new BattleCommand(this, SelectTargets(4), Skills["HOGroupAttack"]);
        if (Roll() < 46)
            return new BattleCommand(this, SelectTarget(), Skills["HOAngelAttack"]);
        if (Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["HOMaverickCharm"]);
        if (Roll() < 46)
            return new BattleCommand(this, SelectTarget(), Skills["HOKimHeadbutt"]);
        return new BattleCommand(this, SelectAllTargets(), Skills["HOVanceCandy"]);
    }
}