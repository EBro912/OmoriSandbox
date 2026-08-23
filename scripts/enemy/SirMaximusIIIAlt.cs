using System;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SirMaximusIIIAlt : Enemy
{
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sir_maximus.tres");

    public override string Name => "SIR MAXIMUS III";
    public override Vector2 InfoBoxOffset => new(0, -200);

    protected override Stats Stats => new(4000, 2000, 75, 100, 75, 15, 95);

    protected override string[] EquippedSkills => ["SMIAttack", "SMIIIDoNothing", "SMIStrikeTwice", "SMIISpin", "SMIIIFlex", "SMUltimateAttack"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
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
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                if (Roll() < 31)
                    goto twice;
                if (Roll() < 41)
                    goto spin;
                goto flex;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 46)
                    goto nothing;
                if (Roll() < 31)
                    goto twice;
                if (Roll() < 21)
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
    
    private bool UltimateAttack = false;

    public override Task OnStartOfBattle()
    {
        AddStatModifier("Immortal");
        return base.OnStartOfBattle();
    }

    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 1 && !UltimateAttack)
        {
            DialogueManager.Instance.QueueMessage(this, @"No... \!I...\![br]I cannot fail now.");
            await DialogueManager.Instance.WaitForDialogue();
            BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["SMUltimateAttack"]);
            UltimateAttack = true;
        }
    }
}