using System;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SirMaximusIIAlt : Enemy
{
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sir_maximus.tres");

    public override string Name => "SIR MAXIMUS II";
    public override Vector2 InfoBoxOffset => new(0, -200);

    protected override Stats Stats => new(3500, 2000, 70, 95, 70, 10, 95);

    protected override string[] EquippedSkills => ["SMIAttack", "SMIIDoNothing", "SMIStrikeTwice", "SMIISpin", "SMUltimateAttack"];
    internal override bool ObserveHasMulti => true;

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
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                if (Roll() < 36)
                    goto twice;
                goto spin;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 56)
                    goto nothing;
                if (Roll() < 26)
                    goto twice;
                goto spin;
            case "angry":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                if (Roll() < 41)
                    goto twice;
                goto spin;
            default:
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                if (Roll() < 36)
                    goto twice;
                goto spin;
        }
        // reuse skills from SMI
        attack:
        return new BattleCommand(this, SelectTarget(), Skills["SMIAttack"]);
        nothing:
        return new BattleCommand(this, this, Skills["SMIIDoNothing"]);
        twice:
        return new BattleCommand(this, SelectTargets(2), Skills["SMIStrikeTwice"]);
        spin:
        return new BattleCommand(this, SelectAllTargets(), Skills["SMIISpin"]);
    }


    private bool UltimateAttack = false;

    public override Task OnStartOfBattle()
    {
        AddStatModifier("Immortal");
        return base.OnStartOfBattle();
    }

    public override async Task ProcessBattleConditions()
    {
        if (ImmortalTriggered && !UltimateAttack)
        {
            DialogueManager.Instance.QueueMessage("SIR MAXIMUS", CenterPoint, @"No... \!I...\![br]I cannot fail now.");
            await DialogueManager.Instance.WaitForDialogue();
            BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["SMUltimateAttack"]);
            UltimateAttack = true;
        }
    }
}