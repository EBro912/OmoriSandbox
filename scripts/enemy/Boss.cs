using Godot;
using System;
using System.Threading.Tasks;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class Boss : Enemy
{
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/boss.tres");

    public override string Name => "BOSS";

    protected override Stats Stats => new Stats(150, 25, 6, 2, 1, 10, 95);

    protected override string[] EquippedSkills => ["BSSAttack", "BSSAttackTwice", "BSSDoNothing", "BSSAttackAll"];
    protected internal override bool ObserveHasMulti => true;

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral" || emotion.Id == "sad" || emotion.Id == "happy" || emotion.Id == "angry";
    }

    public override Vector2 InfoBoxOffset => new(0, -320);

    private int Stage = 0;

    public override BattleCommand ProcessAI()
    {
        if (IsBelowHP(0.15f))
            return new BattleCommand(this, this, Skills["BSSDoNothing"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["BSSAttack"]);
        
        if (Roll() < 31)
            return new BattleCommand(this, SelectTarget(), Skills["BSSAttack"]);

        if (Roll() < 31)
            return new BattleCommand(this, SelectTargets(2), Skills["BSSAttackTwice"]);
        return new BattleCommand(this, this, Skills["BSSDoNothing"]);
    }

    public override async Task OnStartOfBattle()
    {
        AddStatModifier("Immortal", silent: true);
        await Task.CompletedTask;
    }

    public override async Task ProcessBattleConditions()
    {
        if (Stage == 3)
        {
            Stage = 4;
            CurrentHP = Math.Min(CurrentStats.MaxHP, (ImmortalTriggered ? 0 : CurrentHP) + 2);
            RemoveStatModifier("Immortal");
            DialogueManager.Instance.QueueMessage(this, @"HUH!?\! HOW ARE YOU STILL MOVING!?");
            await DialogueManager.Instance.WaitForDialogue();
            return;
        }

        if (Stage > 2)
            return;

        if (IsBelowHP(0.8f) && Stage == 0)
        {
            DialogueManager.Instance.QueueMessage(this, @"[wave freq=10]Hwehwehwe![/wave][br]\!You weaklings!\! You call that an attack!?");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 1;
        }

        if (IsBelowHP(0.4f) && Stage <= 1)
        {
            DialogueManager.Instance.QueueMessage(this, @"Hey, that kinda hurt!\! Hmph!\! This isn't fun anymore.");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 2;
        }
                
        if (IsBelowHP(0.1f) && Stage <= 2)
        {
            DialogueManager.Instance.QueueMessage(this, @"Grr...\![br]Now you've made me ANGRY...");
            DialogueManager.Instance.QueueMessage(this, "It's time for my special move!");
            DialogueManager.Instance.QueueMessage("[center][font_size=52][wave freq=10][shake rate=20]BODY SLAM!!");
            await DialogueManager.Instance.WaitForDialogue();
            SetEmotion("angry", true);
            BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["BSSAttackAll"]);
            Stage = 3;
        }
    }
}