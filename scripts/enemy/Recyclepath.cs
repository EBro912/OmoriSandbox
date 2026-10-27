using System;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Recyclepath : Enemy
{
    public override string Name => "RECYCLEPATH";
    // VANILLA BUG: the emotion entries have no info box offsets, so the box sits at the feet with the finger below
    public override Vector2 InfoBoxOffset => CurrentEmotion.Group == null ? new(15, -230) : Vector2.Zero;
    public override bool InfoBoxCursorAboveBox => CurrentEmotion.Group == null;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/recyclepath.tres");

    protected override Stats Stats => new(1000, 500, 40, 20, 16, 10, 95);

    protected override string[] EquippedSkills => ["RPathAttack", "RPathGatherTrash", "RPathFlingTrash", "RPathSummon"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "ecstatic" or "manic" or "sad" or "depressed" or "miserable" or "angry" or "enraged" or "furious" or "afraid";
    }

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 51)
            return new BattleCommand(this, SelectTarget(), Skills["RPathAttack"]);
        // fling trash can only be used with at least 1 Stockpile stack
        if (HasStatModifier("Stockpile") && Roll() < 31)
            return new BattleCommand(this, SelectTarget(), Skills["RPathFlingTrash"]);
        if (SelectAllEnemies().Count <= 2)
            return new BattleCommand(this, this, Skills["RPathSummon"]);
        return new BattleCommand(this, this, Skills["RPathGatherTrash"]);
    }

    internal void SummonRecycultist()
    {
        // VANILLA BUG: the starting RECYCULTISTS never count as summons, so the one on the left is always summoned first, and the delay hits the starting RECYCULTIST on that side instead of the new one
        ReinforcementSlots.Summon(this, ReinforcementSlots.Recyclepath);
    }
    
    private bool HasSpoken;
    public override async Task ProcessBattleConditions()
    {
        
        if (IsBelowHP(0.5f) && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage("THE RECYCLEPATH", CenterPoint, "[br]Oh, HOLY BIN in the sky...");
            DialogueManager.Instance.QueueMessage("THE RECYCLEPATH", CenterPoint, "[br]Please grant me the power to recycle thy enemies!");
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }

        if (CurrentHP == 0)
            await AbortBattle();
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage("THE RECYCLEPATH", CenterPoint, "[br]Oh why, HOLY BIN?");
        DialogueManager.Instance.QueueMessage("THE RECYCLEPATH", CenterPoint, "[br]Have you forsaken us!?");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override Task OnStartOfBattle()
    {
        AddImmortal();
        ReinforcementSlots.AddTroopMember(this, ReinforcementSlots.Recyclepath, BattleManager.Instance.SummonEnemy("RecycultistRight",
            CenterPoint + new Vector2(-247, 52), fallsOffScreen: false, layer: Math.Max(0, Layer - 1)));
        ReinforcementSlots.AddTroopMember(this, ReinforcementSlots.Recyclepath, BattleManager.Instance.SummonEnemy("RecycultistLeft",
            CenterPoint + new Vector2(233, 52), fallsOffScreen: false, layer: Math.Max(0, Layer - 1)));
        return Task.CompletedTask;
    }

    public override async Task OnEndOfBattle(BattleOutcome outcome)
    {
        if (outcome == BattleOutcome.Defeat)
        {
            DialogueManager.Instance.QueueMessage("THE RECYCLEPATH", CenterPoint,"[br]Huzzah! We have been blessed with victory!");
            DialogueManager.Instance.QueueMessage("THE RECYCLEPATH", CenterPoint, "[br]All hail the HOLY BIN in the sky!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}