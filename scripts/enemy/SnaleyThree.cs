using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

public class SnaleyThree : Enemy
{
    public override string Name => "SNALEY";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/snaley.tres");
    protected override Stats Stats => new(2000, 1000, 40, 30, 40, 15, 200);
    public override bool IsStateValid(string state)
    {
        return state != "afraid" && state != "stressed";
    }
    protected override string[] EquippedSkills => ["RabbitAttack", "SNDoNothing", "SNAttackFollowup", "SNFollowup", "SNReleaseEnergy", "SNMegaphone"];

    private int Turn = 0;
    
    public override BattleCommand ProcessAI()
    {
        Turn++;
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["RabbitAttack"]);
        
        if (Turn is 1)
            return new BattleCommand(this, SelectAllTargets(), Skills["SNMegaphone"]);
        if (Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["RabbitAttack"]);
        if (Roll() < 36)
        {
            BattleManager.Instance.ForceCommand(this, SelectTarget(), Skills["SNFollowup"]);
            return new BattleCommand(this, SelectTarget(), Skills["SNAttackFollowup"]);
        }
        return new BattleCommand(this, this, Skills["SNDoNothing"]);
    }

    public override async Task OnStartOfBattle()
    {
        DialogueManager.Instance.QueueMessage(this, @"I'd bet I'm almost as strong as you now!\! You'd better take this seriously!");
        await DialogueManager.Instance.WaitForDialogue();
    }

    private bool ReleasedEnergy = false;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP < 600 && !ReleasedEnergy)
        {
            DialogueManager.Instance.QueueMessage(this, "And now it's time for my [wave freq=10.0][color=#6095ff]ULTIMATE SKILL!");
            await DialogueManager.Instance.WaitForDialogue();
            BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["SNReleaseEnergy"]);
            ReleasedEnergy = true;
        }
    }

    public override async Task ProcessEndOfTurn()
    {
        if (Turn == 1)
        {
            DialogueManager.Instance.QueueMessage(this, "Heh! You aren't the only ones who can use EMOTIONS!");
            await DialogueManager.Instance.WaitForDialogue();
            if (CurrentState != "ecstatic" && CurrentState != "manic")
                SetState("happy", true);
            DialogueManager.Instance.QueueMessage("SNALEY is HAPPY!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}