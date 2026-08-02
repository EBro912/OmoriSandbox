using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class KiteKidAlt : Enemy
{
    public override string Name => "KITE KID";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/kite_kid.tres");
    protected override Stats Stats => new(8000, 4000, 84, 68, 70, 10, 95);
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
        KidsKite = BattleManager.Instance.SummonEnemy("KidsKite (Epilogue)", CenterPoint - new Vector2(125, 0), layer: Layer + 1);
        DialogueManager.Instance.QueueMessage(this, "We are one with the wind!");
        DialogueManager.Instance.QueueMessage(this, "As long as it blows, we are unbeatable!");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task ProcessEndOfTurn()
    {
        int wind = GameManager.Instance.Random.RandiRange(0, 2);
        IReadOnlyList<Enemy> enemies = SelectAllEnemies();
        switch (wind)
        {
            case 0:
                DialogueManager.Instance.QueueMessage("The wind is raging.");
                await DialogueManager.Instance.WaitForDialogue();
                foreach (Enemy enemy in enemies)
                {
                    enemy.RemoveStatModifier("AttackUp");
                    enemy.RemoveStatModifier("DefenseUp");
                    enemy.RemoveStatModifier("SpeedDown");
                    enemy.RemoveStatModifier("SpeedUp");
                    enemy.AddTierStatModifier("AttackUp", 3);
                    enemy.AddTierStatModifier("DefenseUp", 3);
                    enemy.AddTierStatModifier("SpeedUp", 3);
                    AnimationManager.Instance.PlayAnimation(218, enemy);
                }
                break;
            case 1:
                DialogueManager.Instance.QueueMessage("The wind is steady.");
                await DialogueManager.Instance.WaitForDialogue();
                foreach (Enemy enemy in enemies)
                {
                    enemy.RemoveStatModifier("AttackUp");
                    enemy.RemoveStatModifier("DefenseUp");
                    enemy.RemoveStatModifier("SpeedDown");
                    enemy.RemoveStatModifier("SpeedUp");
                    enemy.AddTierStatModifier("AttackUp", 2);
                    enemy.AddTierStatModifier("DefenseUp", 2);
                    enemy.AddTierStatModifier("SpeedDown", 2);
                    AnimationManager.Instance.PlayAnimation(218, enemy);
                }
                break;
            case 2:
                DialogueManager.Instance.QueueMessage("The wind is weak.");
                await DialogueManager.Instance.WaitForDialogue();
                foreach (Enemy enemy in enemies)
                {
                    enemy.RemoveStatModifier("AttackUp");
                    enemy.RemoveStatModifier("DefenseUp");
                    enemy.RemoveStatModifier("SpeedDown");
                    enemy.RemoveStatModifier("SpeedUp");
                    AnimationManager.Instance.PlayAnimation(219, enemy);
                }
                break;
        }
    }

    private bool HasSpoken = false;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0)
            return;

        if (CurrentHP < 2000 && !HasSpoken)
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