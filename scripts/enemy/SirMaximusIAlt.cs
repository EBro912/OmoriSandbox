using System;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SirMaximusIAlt : Enemy
{
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sir_maximus.tres");

    public override string Name => "SIR MAXIMUS";
    public override Vector2 InfoBoxOffset => new(0, -200);

    protected override Stats Stats => new(3000, 1500, 65, 90, 65, 5, 95);

    protected override string[] EquippedSkills => ["SMIAttack", "SMIDoNothing", "SMIStrikeTwice", "SMIUltimateAttackx1", "SMIUltimateAttackx2", "SMIUltimateAttackx3"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectTargets(2), Skills["SMIStrikeTwice"]);
        
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SMIAttack"]);
        
        switch (CurrentEmotion.Id)
        {
            case "happy":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto twice;
            case "sad":
                if (Roll() < 26)
                    goto attack;
                if (Roll() < 51)
                    goto nothing;
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
    
    private bool UltimateAttack = false;

    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0 && !UltimateAttack)
        {
            DialogueManager.Instance.QueueMessage(this, @"No... \!I...\![br]I cannot fail now.");
            await DialogueManager.Instance.WaitForDialogue();
            switch (SelectAllEnemies().Count + 1)
            {
                case 2:
                    BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["SMIUltimateAttackx2"]);
                    break;
                case 1:
                    BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["SMIUltimateAttackx1"]);
                    break;
                default:
                    BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["SMIUltimateAttackx3"]);
                    break;
            }

            UltimateAttack = true;
        }
    }
}