using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SirMaximusIII : Enemy
{
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sir_maximus.tres");

    public override string Name => "SIR MAXIMUS III";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override Vector2 DamageNumberOffset => new(0, 40);

    protected override Stats Stats => new(1100, 550, 24, 24, 20, 15, 95);

    protected override string[] EquippedSkills => ["SMIAttack", "SMIIIDoNothing", "SMIStrikeTwice", "SMIISpin", "SMIIIFlex", "SMIIIUltimateAttack"];
    protected internal override bool ObserveHasMulti => true;

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral" || emotion.Id == "sad" || emotion.Id == "happy" || emotion.Id == "angry" || emotion.Id == "afraid";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["SMIISpin"]);
        
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SMIAttack"]);
        
        switch (CurrentEmotion.Id)
        {
            case "happy":
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                if (Roll() < 31)
                    goto twice;
                goto spin;
            case "sad":
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 31)
                    goto twice;
                if (Roll() < 31)
                    goto spin;
                goto flex;
            case "angry":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                if (Roll() < 46)
                    goto twice;
                if (Roll() < 41)
                    goto spin;
                goto flex;
            default:
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                if (Roll() < 31)
                    goto twice;
                if (Roll() < 36)
                    goto spin;
                goto flex;
        }
        // reuse skills from SMI and SMII
        attack:
        return new BattleCommand(this, SelectTarget(), Skills["SMIAttack"]);
        nothing:
        return new BattleCommand(this, this, Skills["SMIIIDoNothing"]);
        twice:
        return new BattleCommand(this, SelectTargets(2), Skills["SMIStrikeTwice"]);
        spin:
        return new BattleCommand(this, SelectAllTargets(), Skills["SMIISpin"]);
        flex:
        return new BattleCommand(this, this, Skills["SMIIIFlex"]);
    }

    public override Task OnStartOfBattle()
    {
        AddImmortal();
        return Task.CompletedTask;
    }

    private bool FirstDialogue;
    private bool UltimateAttack;
    
    public override async Task ProcessBattleConditions()
    {
        if (IsBelowHP(0.5f) && !FirstDialogue)
        {
            DialogueManager.Instance.QueueMessage(this, "No... I cannot let my father's and his father's deaths be in vain!");
            await DialogueManager.Instance.WaitForDialogue();
            FirstDialogue = true;
        }
        
        if (IsBelowHP(0.2f) && !UltimateAttack)
        {
            await Task.WhenAll(SirMaximusII.ShowGhost(3, 415), SirMaximusII.ShowGhost(2, 190));
            BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["SMIIIUltimateAttack"]);
            UltimateAttack = true;
        }

        if (CurrentHP == 0)
            RemoveImmortal(afterForcedCommands: true);
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, "Father... Grandfather...");
        DialogueManager.Instance.QueueMessage(this, @"I'm sorry...\![br]I have failed you.");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(BattleOutcome outcome)
    {
        if (outcome == BattleOutcome.Defeat)
        {
            DialogueManager.Instance.QueueMessage(this, "Alas, my family has been avenged!");
            DialogueManager.Instance.QueueMessage(this, "This is a glorious day for my people!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}