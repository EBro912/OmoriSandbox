using Godot;
using System.Threading.Tasks;

using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class Roboheart : Enemy
{
    public override string Name => "ROBOHEART";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/roboheart.tres");
    protected override Stats Stats => new(2500, 1250, 45, 40, 60, 10, 95);
    protected override string[] EquippedSkills => ["RHAttack", "RHDoNothing", "RHLaser", "RHSnack", "RHExplode"];
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["RHAttack"]);
        
        if (IsBelowHP(0.1f))
            return new BattleCommand(this, SelectAllTargets(), Skills["RHExplode"]);

        switch (CurrentEmotion.Id)
        {
            case "happy":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                if (Roll() < 41)
                    goto laser;
                goto snack;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 56)
                    goto nothing;
                if (Roll() < 31)
                    goto laser;
                goto snack;
            case "angry":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 36)
                    goto nothing;
                if (Roll() < 56)
                    goto laser;
                goto snack;
            default:
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 36)
                    goto nothing;
                if (Roll() < 36)
                    goto laser;
                goto snack;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["RHAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["RHDoNothing"]);
    laser:
        return new BattleCommand(this, SelectTarget(), Skills["RHLaser"]);
    snack:
        return new BattleCommand(this, this, Skills["RHSnack"]);
    }

    private int Stage = 0;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0) return;

        if (Stage > 1)
            return;

        if (IsBelowHP(0.75f) && Stage == 0)
        {
            DialogueManager.Instance.QueueMessage(this, "TXkgbGlmZSBpcyBzdW\nZmZXJpbmch");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 1;
        }
        
        if (IsBelowHP(0.25f) && Stage <= 1)
        {
            DialogueManager.Instance.QueueMessage(this, "SGVscC4uLiBtZS4uLgo=");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 2;
        }
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, "[br]V2h5Pw==");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, "Tm8sIEkgZGlkbid0IG1lYW4gdG8h");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}