using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Actors;

internal sealed class HumphreySwarm : Enemy
{
    public override string Name => "HUMPHREY";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/humphrey_swarm.tres");
    protected override Stats Stats => new(9999, 5000, 10, 60, 60, 20, 95);
    protected override string[] EquippedSkills => ["HUSAttack", "HUSAttack2", "HUSAttack3"];
    
    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "sad" or "happy" or "angry" or "toast";
    }

    private int Turn = 0;

    public override BattleCommand ProcessAI()
    {
        Turn++;
        
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectTargets(3), Skills["HUSAttack3"]);
        
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["HUSAttack"]);
        
        switch (CurrentState)
        {
            case "angry":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 21)
                    goto attack2;
                goto attack3;
            case "sad":
                if (Roll() < 41)
                    goto attack;
                goto attack2;
            default:
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 31)
                    goto attack2;
                goto attack3;
        }
        attack:
            return new BattleCommand(this, SelectTarget(), Skills["HUSAttack"]);
        attack2:
            return new BattleCommand(this, SelectTargets(2), Skills["HUSAttack2"]);
        attack3:
            return new BattleCommand(this, SelectTargets(3), Skills["HUSAttack3"]);
    }

    public override async Task OnStartOfBattle()
    {
        AddStatModifier("Immortal");
        DialogueManager.Instance.QueueMessage(this, "[br][wave freq=10.0]Time to feast! Time to feast! Time for you to be deceased![/wave]");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP < 999)
        {
            await ChangePhase();
        }
    }

    public override async Task ProcessEndOfTurn()
    {
        if (Turn >= 5)
        {
            await ChangePhase();
        }
    }

    private async Task ChangePhase()
    {
        DialogueManager.Instance.QueueMessage(this, @"[wave freq=10.0]The final fight as just begun!\| But can you win if we work as one?[/wave]");
        await DialogueManager.Instance.WaitForDialogue();
        await AnimationManager.Instance.WaitForHumphreySwarm();
        EnemyComponent grande = BattleManager.Instance.TransformEnemy(this, "HumphreyGrande");
        grande.Actor.AddStatModifier("Immortal");
        await Task.Delay(2500);
        await AnimationManager.Instance.WaitForTintScreen(ColorsExtension.TransparentBlack, 0.5f);
    }
}