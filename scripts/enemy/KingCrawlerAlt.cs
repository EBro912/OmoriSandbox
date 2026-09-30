using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class KingCrawlerAlt : Enemy
{
    public override string Name => "KING CRAWLER";
    public override Vector2 InfoBoxOffset => new(0, -350);
    public override Vector2 DamageNumberOffset => new(0, -150);
    public override SpriteFrames Animation =>
        ResourceLoader.Load<SpriteFrames>("res://animations/king_crawler.tres");
    protected override Stats Stats => new(6200, 2500, 90, 60, 100, 10, 200);
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
        if (IsBelowHP(0.5f) && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage(this, "[br][shake rate=20][font_size=12]Ssssssssssssssssssss...");
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }
    }

    // vanilla bug: reinforcements are never removed from the troop list, so a dead mole keeps its index, causing the
    // CallForFriendDelay below to fail
    private readonly List<Enemy> Moles = [];
    private readonly List<int> MoleSlots = [];   // reinforcement slot used by each summon

    private static bool Alive(Enemy e) =>
        e?.Component != null && !e.IsToast && e.CurrentHP > 0;

    public override async Task ProcessEndOfTurn()
    {
        // eat every living mole in troop order at turn end
        HashSet<Enemy> eaten = [];
        foreach (Enemy mole in Moles.Where(Alive))
        {
            DialogueManager.Instance.QueueMessage("KING CRAWLER eats a SPROUT MOLE!");
            await DialogueManager.Instance.WaitForDialogue();
            BattleManager.Instance.ForceCommand(this, mole, Skills["KCEat"]);
            BattleManager.Instance.ForceCommand(this, this, Skills["KCRecover"]);
            eaten.Add(mole);
        }
        // the forced eat resolves before page 5 in vanilla, so moles queued for eating count as dead below
        bool Living(Enemy e) => Alive(e) && !eaten.Contains(e);

        // page 5: summon unless troop index 2 is alive
        if (BattleManager.Instance.TurnCount % 4 != 1)
            return;
        if (Moles.Count >= 2 && Living(Moles[1]))
            return;
        DialogueManager.Instance.QueueMessage("A SPROUT MOLE appears!");
        await DialogueManager.Instance.WaitForDialogue();

        // first reinforcement slot without a living mole, then stun the troop index that slot's branch names
        int slot = !Moles.Where((m, i) => MoleSlots[i] == 2).Any(Living) ? 2
            : !Moles.Where((m, i) => MoleSlots[i] == 3).Any(Living) ? 3 : 0;
        if (slot == 0)
            return;
        Enemy summoned = BattleManager.Instance.SummonEnemy("LostSproutMole (King Crawler)", CenterPoint - new Vector2(100, 0), layer: Layer + 1);
        Moles.Add(summoned);
        MoleSlots.Add(slot);
        Enemy stunned = Moles[slot - 2];   // troop index slot-1 == Moles[slot-2]
        if (Living(stunned))
        {
            stunned.AddStatModifier("CallForFriendDelay", silent: true);
            stunned.ClearResult();
        }
    }

    public override Task OnDefeat()
    {
        foreach (Enemy mole in Moles.Where(Alive))
            mole.CurrentHP = 0;
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