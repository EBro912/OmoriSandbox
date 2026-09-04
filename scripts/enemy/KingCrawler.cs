using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class KingCrawler : Enemy
{
    public override string Name => "KING CRAWLER";
    public override Vector2 InfoBoxOffset => new(0, -350);
    public override SpriteFrames Animation =>
        ResourceLoader.Load<SpriteFrames>("res://animations/king_crawler.tres");
    protected override Stats Stats => new(730, 250, 25, 10, 18, 10, 200);
    protected override string[] EquippedSkills => ["KCAttack", "KCDoNothing", "KCCrunch", "KCRam", "KCEat", "KCRecover"];
    protected internal override bool ObserveHasMulti => true;
    
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "angry" or "happy";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["KCRam"]);
        
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["KCAttack"]);
        
        if (CurrentEmotion.Id == "angry")
        {
            if (Roll() < 41)
                return new BattleCommand(this, SelectTarget(), Skills["KCAttack"]);
            if (Roll() < 31)
                return new BattleCommand(this, SelectTarget(), Skills["KCCrunch"]);
            return new BattleCommand(this, SelectAllTargets(), Skills["KCRam"]);
        }

        if (Roll() < 41)
            return new BattleCommand(this, SelectTarget(), Skills["KCAttack"]);
        if (Roll() < 26)
            return new BattleCommand(this, this, Skills["KCDoNothing"]);
        if (Roll() < 31)
            return new BattleCommand(this, SelectTarget(), Skills["KCCrunch"]);
        return new BattleCommand(this, SelectAllTargets(), Skills["KCRam"]);
    }

    private bool HasSpoken = false;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP > 0 && IsBelowHP(0.5f) && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage(this, "[br][shake rate=20][font_size=20]Ssssssssssssssssssss...");
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }
    }

    // vanilla bug: reinforcements are never removed from the troop list, so a dead mole keeps its index, causing the
    // CallForFriendDelay below to fail
    private readonly List<EnemyComponent> Moles = [];
    private readonly List<int> MoleSlots = [];   // reinforcement slot used by each summon
    private int Turn = 0;

    private static bool Alive(EnemyComponent e) =>
        GodotObject.IsInstanceValid(e) && !e.Actor.IsToast && e.Actor.CurrentHP > 0;

    public override async Task ProcessEndOfTurn()
    {
        Turn++;
        // eat every living mole in troop order at turn end
        HashSet<EnemyComponent> eaten = [];
        foreach (EnemyComponent mole in Moles.Where(Alive))
        {
            DialogueManager.Instance.QueueMessage("KING CRAWLER eats a SPROUT MOLE!");
            await DialogueManager.Instance.WaitForDialogue();
            BattleManager.Instance.ForceCommand(this, mole.Actor, Skills["KCEat"]);
            BattleManager.Instance.ForceCommand(this, this, Skills["KCRecover"]);
            eaten.Add(mole);
        }
        // the forced eat resolves before page 4 in vanilla, so moles queued for eating count as dead below
        bool Living(EnemyComponent e) => Alive(e) && !eaten.Contains(e);

        // page 4: summon unless troop index 2 is alive
        if (Turn % 4 != 1)
            return;
        if (Moles.Count >= 2 && Living(Moles[1]))
            return;
        DialogueManager.Instance.QueueMessage("A SPROUT MOLE appears!");
        await DialogueManager.Instance.WaitForDialogue();

        // first reinforcement slot without a living mole, then stun the troop index that slot's branch names
        int slot = Moles.Where((m, i) => MoleSlots[i] == 2).Any(Living) 
            ? Moles.Where((m, i) => MoleSlots[i] == 3).Any(Living) ? 0 : 3
            : 2;
        if (slot == 0)
            return;
        EnemyComponent summoned = BattleManager.Instance.SummonEnemy("LostSproutMole", CenterPoint - new Vector2(100, 0), layer: Layer + 1);
        Moles.Add(summoned);
        MoleSlots.Add(slot);
        EnemyComponent stunned = Moles[slot - 2];   // troop index slot-1 == Moles[slot-2]
        if (Living(stunned))
            stunned.Actor.AddStatModifier("CallForFriendDelay", silent: true);
    }

    public override Task OnDefeat()
    {
        foreach (EnemyComponent mole in Moles.Where(Alive))
            mole.Actor.CurrentHP = 0;
        return Task.CompletedTask;
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, "[br][shake rate=20][font_size=12]KISHKISHKISHKISHKISH!!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}