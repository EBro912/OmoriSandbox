using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal sealed class SnaleyTwo : Enemy
{
    public override string Name => "SNALEY";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/snaley.tres");
    protected override Stats Stats => new(1500, 750, 35, 25, 35, 10, 95);
    public override bool IsStateValid(string state)
    {
        return state != "afraid" && state != "stressed";
    }
    protected override string[] EquippedSkills => ["SNAttack", "SNDoNothing", "SNBeatdown", "SNAttackFollowup", "SNFollowup"];

    private int Turn = 0;
    
    public override BattleCommand ProcessAI()
    {
        Turn++;
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SNBeatdown"]);
        
        if (Turn is 1)
            return new BattleCommand(this, SelectTarget(), Skills["SNBeatdown"]);
        if (Turn is 2)
        {
            BattleManager.Instance.ForceCommand(this, SelectTarget(), Skills["SNFollowup"]);
            return new BattleCommand(this, SelectTarget(), Skills["SNAttackFollowup"]);
        }
        
        if (Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["SNAttack"]);
        if (Roll() < 36)
        {
            BattleManager.Instance.ForceCommand(this, SelectTarget(), Skills["SNFollowup"]);
            return new BattleCommand(this, SelectTarget(), Skills["SNAttackFollowup"]);
        }
        if (Roll() < 25) 
            return new BattleCommand(this, SelectTarget(), Skills["SNBeatdown"]);
        return new BattleCommand(this, this, Skills["SNDoNothing"]);
    }

    public override async Task OnStartOfBattle()
    {
        DialogueManager.Instance.QueueMessage(this, @"I taught myself some [color=#6095ff]SKILLS[/color] since our last battle!\! You'd better watch out!");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, "Okay, please stop! That's enough!");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task ProcessEndOfTurn()
    {
        if (Turn == 1)
        {
            DialogueManager.Instance.QueueMessage(this, "[wave freq=10.0]Wasn't that cool!?[/wave] I'm awesome!");
            await DialogueManager.Instance.WaitForDialogue();
            DialogueManager.Instance.QueueMessage(this, "I can do [color=#6095ff]FOLLOW-UP SKILLS[/color] too! Watch this!");
            await DialogueManager.Instance.WaitForDialogue();
        }
        else if (Turn == 2)
        {
            if (CurrentState != "ecstatic" && CurrentState != "manic")
                SetState("happy", true);
            DialogueManager.Instance.QueueMessage(this, @"How was that!?\! One of these days, I'll be as strong as you!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}