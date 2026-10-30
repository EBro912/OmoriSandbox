using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class KingCrawler : Enemy
{
    public override string Name => "KING CRAWLER";
    public override Vector2 InfoBoxOffset => new(0, -350);
    public override Vector2 DamageNumberOffset => new(0, -150);
    public override SpriteFrames Animation =>
        ResourceLoader.Load<SpriteFrames>("res://animations/king_crawler.tres");
    protected override Stats Stats => new(730, 250, 25, 10, 18, 10, 200);
    protected override string[] EquippedSkills => ["KCAttack", "KCDoNothing", "KCCrunch", "KCRam", "KCEat", "KCRecover"];
    protected internal override bool ObserveHasMulti => true;
    
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "angry" or "happy" or "afraid";
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

    private bool HasSpoken;
    public override async Task ProcessBattleConditions()
    {
        if (IsBelowHP(0.5f) && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage(this, "[br][shake rate=60.0 level=20 connected=0][font_size=23]Ssssssssssssssssssss...");
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }
    }

    // vanilla bug: reinforcements are never removed from the troop list, so a dead mole keeps its index, causing the
    // CallForFriendDelay in ReinforcementSlots.Summon to fail
    private readonly List<Enemy> Moles = [];

    private static bool Alive(Enemy e) =>
        e?.Component != null && !e.IsToast;

    public override async Task ProcessEndOfTurn()
    {
        // eat every living mole among the first seven in troop order at turn end
        HashSet<Enemy> eaten = [];
        foreach (Enemy mole in Moles.Take(7).Where(Alive))
        {
            DialogueManager.Instance.QueueMessage("KING CRAWLER eats a SPROUT MOLE!");
            await DialogueManager.Instance.WaitForDialogue();
            BattleManager.Instance.ForceCommand(this, mole, Skills["KCEat"]);
            BattleManager.Instance.ForceCommand(this, this, Skills["KCRecover"]);
            eaten.Add(mole);
        }
        // the forced eat resolves before page 4 in vanilla, so moles queued for eating count as dead below
        bool Living(Enemy e) => Alive(e) && !eaten.Contains(e);

        // page 4: summon unless troop index 2 is alive
        if (BattleManager.Instance.TurnCount % 4 != 1)
            return;
        if (Moles.Count >= 2 && Living(Moles[1]))
            return;
        DialogueManager.Instance.QueueMessage("A SPROUT MOLE appears!");
        await DialogueManager.Instance.WaitForDialogue();

        Enemy summoned = ReinforcementSlots.Summon(this, ReinforcementSlots.KingCrawler);
        if (summoned != null)
            Moles.Add(summoned);
    }

    public override Task OnDefeat()
    {
        foreach (Enemy mole in Moles.Where(Alive))
            mole.CurrentHP = 0;
        return Task.CompletedTask;
    }

    public override async Task OnEndOfBattle(BattleOutcome outcome)
    {
        if (outcome == BattleOutcome.Defeat)
        {
            DialogueManager.Instance.QueueMessage(this, "[br][shake rate=60.0 level=20 connected=0][font_size=23]KISHKISHKISHKISHKISH!!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}