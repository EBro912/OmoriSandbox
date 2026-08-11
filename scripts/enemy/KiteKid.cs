using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class KiteKid : Enemy
{
    public override string Name => "KITE KID";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/kite_kid.tres");
    protected override Stats Stats => new(750, 375, 24, 15, 25, 10, 95);
    protected override string[] EquippedSkills => ["KKAttack", "KKBrag"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["KKAttack"]);
        
        if (CurrentEmotion.Id == "happy" || Roll() < 76)
            return new BattleCommand(this, SelectTarget(), Skills["KKAttack"]);
        
        return new BattleCommand(this, this, Skills["KKBrag"]);
    }

    private EnemyComponent KidsKite;

    public override async Task OnStartOfBattle()
    {
        KidsKite = BattleManager.Instance.SummonEnemy("KidsKite", CenterPoint - new Vector2(125, 0), layer: Layer + 1);
    }

    public override async Task ProcessEndOfTurn()
    {
        if (KidsKite.Actor.IsToast)
        {
            KidsKite = BattleManager.Instance.SummonEnemy("KidsKite", CenterPoint - new Vector2(125, 0), layer: Layer + 1);
            AudioManager.Instance.PlaySFX("BA_Repair", 1f, 0.9f);
            DialogueManager.Instance.QueueMessage("KITE KID repairs KID'S KITE.");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }

    private bool HasSpoken = false;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0)
            return;

        if (IsBelowHP(0.251f) && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage(this, "No... This can't be...");
            DialogueManager.Instance.QueueMessage(this, @"The wind...\![br]It's getting weaker!");
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, "But me and my kite have an unbreakable bond...");
        DialogueManager.Instance.QueueMessage(this, "How could we lose?");
        await DialogueManager.Instance.WaitForDialogue();
        if (KidsKite != null && !KidsKite.Actor.IsToast)
            KidsKite.Actor.CurrentHP = 0;
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, @"Haha! As the wind predicted!\! Me and my kite are unbeatable.");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}