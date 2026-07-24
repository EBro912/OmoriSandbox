using System;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal sealed class Recyclepath : Enemy
{
    public override string Name => "RECYCLEPATH";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>($"res://animations/recyclepath.tres");

    protected override Stats Stats => new(1000, 500, 40, 20, 16, 10, 95);

    protected override string[] EquippedSkills => ["RPathAttack", "RPathGatherTrash", "RPathFlingTrash", "RPathSummon"];

    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "sad" or "happy" or "angry" or "hurt" or "toast";
    }

    private EnemyComponent LeftRecycultist;
    private EnemyComponent RightRecycultist;

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 51)
            return new BattleCommand(this, SelectTarget(), Skills["RPathAttack"]);
        // fling trash can only be used with at least 1 Stockpile stack
        if (HasStatModifier("Stockpile") && Roll() < 61)
            return new BattleCommand(this, SelectTarget(), Skills["RPathFlingTrash"]);
        // we need to check these separately due to the sprite flipping behavior
        if (LeftRecycultist == null || LeftRecycultist.Actor.IsToast)
        {
            LeftRecycultist = BattleManager.Instance.SummonEnemy("RecycultistLeft", CenterPoint + new Vector2(225, 35),
                fallsOffScreen: false, layer: Math.Max(0, Layer - 1));
            return new BattleCommand(this, this, Skills["RPathSummon"]);
        }

        if (RightRecycultist == null || RightRecycultist.Actor.IsToast)
        {
            RightRecycultist = BattleManager.Instance.SummonEnemy("RecycultistRight", CenterPoint + new Vector2(-225, 35),
                fallsOffScreen: false, layer: Math.Max(0, Layer - 1));
            return new BattleCommand(this, this, Skills["RPathSummon"]);
        }

        return new BattleCommand(this, this, Skills["RPathGatherTrash"]);
    }
    
    private bool HasSpoken = false;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0) return;
        
        if (CurrentHP < 500 && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage("THE RECYCLEPATH", CenterPoint, "[br]Oh, HOLY BIN in the sky...");
            DialogueManager.Instance.QueueMessage("THE RECYCLEPATH", CenterPoint, "[br]Please grant me the power to recycle thy enemies!");
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage("THE RECYCLEPATH", CenterPoint, "[br]Oh why, HOLY BIN?");
        DialogueManager.Instance.QueueMessage("THE RECYCLEPATH", CenterPoint, "[br]Have you forsaken us!?");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override Task OnStartOfBattle()
    {
        LeftRecycultist = BattleManager.Instance.SummonEnemy("RecycultistLeft", CenterPoint + new Vector2(225, 35),
            fallsOffScreen: false, layer: Math.Max(0, Layer - 1));
        RightRecycultist = BattleManager.Instance.SummonEnemy("RecycultistRight", CenterPoint + new Vector2(-225, 35),
            fallsOffScreen: false, layer: Math.Max(0, Layer - 1));
        return Task.CompletedTask;
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage("THE RECYCLEPATH", CenterPoint,"[br]Huzzah! We have been blessed with victory!");
            DialogueManager.Instance.QueueMessage("THE RECYCLEPATH", CenterPoint, "[br]All hail the HOLY BIN in the sky!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}