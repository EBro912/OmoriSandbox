using System;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal sealed class KingCrawler : Enemy
{
    public override string Name => "KING CRAWLER";
    public override SpriteFrames Animation =>
        ResourceLoader.Load<SpriteFrames>("res://animations/king_crawler.tres");
    protected override Stats Stats => new(730, 250, 25, 10, 18, 10, 200);
    protected override string[] EquippedSkills => ["KCAttack", "KCDoNothing", "KCCrunch", "KCRam", "KCEat", "KCRecover"];
    
    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "hurt" or "toast" or "sad" or "angry" or "happy";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["KCRam"]);
        
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["KCAttack"]);
        
        if (CurrentState == "angry")
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
        if (CurrentHP < 365 && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage(this, "[br][shake rate=20][font_size=12]Ssssssssssssssssssss...");
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }
    }

    private EnemyComponent SproutMole;
    private bool AteSproutMoleLastTurn = false;
    
    public override async Task ProcessEndOfTurn()
    {
        if (AteSproutMoleLastTurn)
        {
            AteSproutMoleLastTurn = false;
            return;
        }
        
        if (SproutMole == null || SproutMole.Actor.CurrentState == "toast")
        {
            SproutMole =
                BattleManager.Instance.SummonEnemy("LostSproutMole", CenterPoint - new Vector2(100, 0), layer: Layer + 1);
            DialogueManager.Instance.QueueMessage("A SPROUT MOLE appears!");
            await DialogueManager.Instance.WaitForDialogue();
        }
        else
        {
            DialogueManager.Instance.QueueMessage("KING CRAWLER eats a SPROUT MOLE!");
            await DialogueManager.Instance.WaitForDialogue();
            BattleManager.Instance.ForceCommand(this, SproutMole.Actor, Skills["KCEat"]);
            BattleManager.Instance.ForceCommand(this, this, Skills["KCRecover"]);
            AteSproutMoleLastTurn = true;
        }
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