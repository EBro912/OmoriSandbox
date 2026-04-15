using Godot;
using System.Threading.Tasks;
using OmoriSandbox.Battle;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Actors;
internal sealed class FearOfHeights : Enemy
{
    public override string Name => "SOMETHING";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/fear_of_heights.tres");
    protected override Stats Stats => new(6000, 4000, 120, 100, 80, 10, 95);
    protected override string[] EquippedSkills => ["FOHAttack", "FOHDoNothing", "FOHGrab", "FOHHands", "FOHShove"];

    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "hurt" || state == "toast";
    }

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 31)
            return new BattleCommand(this, SelectTarget(), Skills["FOHAttack"]);
        if (Roll() < 16)
            return new BattleCommand(this, SelectTarget(), Skills["FOHDoNothing"]);
        if (Roll() < 26)
            return new BattleCommand(this, SelectAllTargets(), Skills["FOHGrab"]);
        if (Roll() < 31)
            return new BattleCommand(this, this, Skills["FOHHands"]);
        else
            return new BattleCommand(this, SelectTarget(), Skills["FOHShove"]);
    }

    private int TurnsLeft = 11;
    public override async Task ProcessStartOfTurn()
    {
        TurnsLeft--;
        if (TurnsLeft == 0)
        {
            AnimationManager.Instance.InitShake(new Shake(9, 3, 60));
            foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
            {
                // fear of heights bypasses plot armor
                member.Actor.CurrentHP = 0;
                member.Actor.SetState("toast", true);
            }
            await Wait.Milliseconds(1500);
            DialogueManager.Instance.QueueMessage("You hit the ground.");
            await DialogueManager.Instance.WaitForDialogue();
            BattleManager.Instance.CheckBattleOver();
            return;
        }
        DialogueManager.Instance.QueueMessage(TurnsLeft + $" turn{(TurnsLeft == 1 ? "" : 's')} left.");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnStartOfBattle()
    {
        DialogueManager.Instance.QueueMessage("You are falling.");
        await DialogueManager.Instance.WaitForDialogue();
    }
}