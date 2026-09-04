using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Actors;

internal sealed class AubreyEnemy : Enemy
{
    public override string Name => "AUBREY";
    public override Vector2 InfoBoxOffset => new(25, -200);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/aubrey_enemy.tres");
    protected override Stats Stats => new(240, 120, 24, 8, 12, 5, 95);

    protected override string[] EquippedSkills => ["AEAttack", "AEDoNothing", "AEHeadbutt"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral" || emotion.Id == "sad" || emotion.Id == "happy" || emotion.Id == "angry";
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

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 46)
        {
            return new BattleCommand(this, SelectTarget(), Skills["AEAttack"]);
        }
        if (Roll() < 31)
        {
            return new BattleCommand(this, this, Skills["AEDoNothing"]);
        }
        return new BattleCommand(this, SelectTarget(), Skills["AEHeadbutt"]);
    }

    private int Stage = 0;
    private int Turn = 0;
    
    private static readonly string[] ChurchGoerLines =
    [
        @"[font_size=22]Look at her clothing...\| It is completely inappropriate for church...",
        @"[font_size=22]I can't believe she would bring a weapon in here...\| How uncivilized...",
        @"[font_size=22]What do these delinquents think they're doing?\| This is a place of worship!",
        @"[font_size=22]Someone needs to stop them...\| Where are their parents?",
        @"[font_size=22]That girl is a threat to this neighborhood.\| There's no hope for sinners like her!",
        @"[font_size=22]I always thought she would be trouble...\| The pastor should have kicked her out a long time ago.",
        @"[font_size=22]Children these days have no respect...\|[br]I hope my kids don't turn out like her.",
    ];

    public override async Task ProcessBattleConditions()
    {
        if (IsBelowHP(0.7f) && Stage == 0)
        {
            DialogueManager.Instance.QueueMessage(this, "[br]Why are you here?");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 1;
        }
        if (IsBelowHP(0.25f) && Stage <= 1)
        {
            DialogueManager.Instance.QueueMessage(this, @"[br]Why...\! Why now?");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 2;
        }
    }

    public override async Task ProcessEndOfTurn()
    {
        Turn++;
        if (Turn > ChurchGoerLines.Length)
            return;
        DialogueManager.Instance.QueueMessage(ChurchGoerLines[Turn - 1]);
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, "[br]Ugh...");
        DialogueManager.Instance.QueueMessage(this, @"[br]Forget it...\![br]You two aren't worth my time.");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, "[br]Hmph.");
            DialogueManager.Instance.QueueMessage(this, "[br]Serves you right, KEL.");
            DialogueManager.Instance.QueueMessage(this, @"[br]Now...\! leave me alone.");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}