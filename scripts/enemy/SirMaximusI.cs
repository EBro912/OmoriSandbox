using System;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal sealed class SirMaximusI : Enemy
{
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sir_maximus.tres");

    public override string Name => "SIR MAXIMUS";

    protected override Stats Stats => new(600, 300, 17, 15, 5, 5, 95);

    protected override string[] EquippedSkills => ["SMIAttack", "SMIDoNothing", "SMIStrikeTwice", "SMIUltimateAttack"];

    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "sad" or "happy" or "angry" or "toast";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectTargets(2), Skills["SMIStrikeTwice"]);
        
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SMIAttack"]);
        
        switch (CurrentState)
        {
            case "happy":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto twice;
            case "sad":
                if (Roll() < 41)
                    goto attack;
                goto twice;
            case "angry":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 21)
                    goto nothing;
                goto twice;
            default:
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                goto twice;
        }
        attack:
        return new BattleCommand(this, SelectTarget(), Skills["SMIAttack"]);
        nothing:
        return new BattleCommand(this, this, Skills["SMIDoNothing"]);
        twice:
        return new BattleCommand(this, SelectTargets(2), Skills["SMIStrikeTwice"]);
    }

    private bool FirstDialogue = false;
    private bool UltimateAttack = false;
    
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP < 120 && !UltimateAttack)
        {
            DialogueManager.Instance.QueueMessage(this, "Behold! My family has spent generations perfecting this technique...");
            DialogueManager.Instance.QueueMessage(this, "[br]This is my ultimate attack!");
            await DialogueManager.Instance.WaitForDialogue();
            BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["SMIUltimateAttack"]);
            UltimateAttack = true;
            return;
        }
        
        if (CurrentHP < 300 && !FirstDialogue)
        {
            DialogueManager.Instance.QueueMessage(this, @"No... \!I...\![br]I cannot fail now.");
            DialogueManager.Instance.QueueMessage(this, "My son needs me!");
            await DialogueManager.Instance.WaitForDialogue();
            FirstDialogue = true;
        }
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, "Our family's fighting technique has been undefeated for generations!");
            DialogueManager.Instance.QueueMessage(this, "You were foolish to challenge me!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}