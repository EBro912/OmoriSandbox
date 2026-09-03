using System;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

public class SnaleyThree : Enemy
{
    public override string Name => "SNALEY";
    public override Vector2 InfoBoxOffset => new(0, -225);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/snaley.tres");
    protected override Stats Stats => new(2000, 1000, 40, 30, 40, 15, 200);
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id != "afraid" && emotion.Id != "stressed";
    }
    protected override string[] EquippedSkills => ["RabbitAttack", "SNAttack", "SNBeatdown", "SNAttackFollowup", "SNFollowup", "SNReleaseEnergy", "SNMegaphone"];

    private int Turn = 0;
    
    public override BattleCommand ProcessAI()
    {
        if (!PreRolling)
            Turn++;
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["RabbitAttack"]);
        
        if (Turn is 1)
            return new BattleCommand(this, SelectAllTargets(), Skills["SNMegaphone"]);
        if (Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["SNAttack"]);
        if (Roll() < 36)
        {
            PartyMember target = SelectTarget();
            if (!PreRolling)
                BattleManager.Instance.ForceCommand(this, target, Skills["SNFollowup"]);
            return new BattleCommand(this, target, Skills["SNAttackFollowup"]);
        }
        return new BattleCommand(this, SelectTarget(), Skills["SNBeatdown"]);
    }

    public override async Task OnStartOfBattle()
    {
        AddStatModifier("Immortal", silent: true);
        DialogueManager.Instance.QueueMessage(this, @"I'd bet I'm almost as strong as you now!\! You'd better take this seriously!");
        await DialogueManager.Instance.WaitForDialogue();
    }

    private int TurnsEnded = 0;
    private bool ReleasedEnergy = false;
    private bool PendingRelease = false;

    public override async Task ProcessBattleConditions()
    {
        if (PendingRelease)
        {
            PendingRelease = false;
            CurrentHP = Math.Min(CurrentStats.MaxHP, (ImmortalTriggered ? 0 : CurrentHP) + 1);
            RemoveStatModifier("Immortal");
        }
        await Task.CompletedTask;
    }

    public override async Task ProcessEndOfTurn()
    {
        TurnsEnded++;
        if (TurnsEnded == 1)
        {
            DialogueManager.Instance.QueueMessage(this, "Heh! You aren't the only ones who can use EMOTIONS!");
            await DialogueManager.Instance.WaitForDialogue();
            if (CurrentEmotion.Id != "ecstatic" && CurrentEmotion.Id != "manic")
                SetEmotion("happy", true);
            DialogueManager.Instance.QueueMessage("SNALEY is HAPPY!");
            await DialogueManager.Instance.WaitForDialogue();
        }
        
        if (TurnsEnded >= 2 && !ReleasedEnergy && IsBelowHP(0.3f))
        {
            DialogueManager.Instance.QueueMessage(this, "And now it's time for my [wave freq=10.0][color=#6095ff]ULTIMATE SKILL!");
            await DialogueManager.Instance.WaitForDialogue();
            BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["SNReleaseEnergy"]);
            ReleasedEnergy = true;
            PendingRelease = true;
        }
    }
}