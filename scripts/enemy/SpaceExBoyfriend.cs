using Godot;
using System.Threading.Tasks;

using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class SpaceExBoyfriend : Enemy
{
    public override string Name => "SPACE EX-BOYFRIEND";
    public override Vector2 InfoBoxOffset => new(25, -400);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/space_ex_boyfriend.tres");
    protected override Stats Stats => new(1350, 750, 15, 16, 25, 10, 95);
    protected override string[] EquippedSkills => ["SEBAttack", "SEBDoNothing", "AngstySong", "AngrySong", "SpaceLaser", "BulletHell"];
    public override bool IsEmotionValid(Emotion emotion)
    {
        if (IsEmotionLocked)
            return false;

        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
    }

    // Space Ex-Boyfriend's locked emotions use slightly different stats than the generic angry line
    protected override StatBonus[] GetEmotionStatBonuses(Emotion emotion)
    {
        if (IsEmotionLocked && emotion.Group?.Id == "angry")
        {
            return emotion.Tier switch
            {
                1 => [new StatBonus(StatType.ATK, 1.25f), new StatBonus(StatType.DEF, 0.9f)],
                2 => [new StatBonus(StatType.ATK, 1.5f), new StatBonus(StatType.DEF, 0.5f)],
                3 => [new StatBonus(StatType.ATK, 2f), new StatBonus(StatType.DEF, 0.3f)],
                _ => base.GetEmotionStatBonuses(emotion)
            };
        }
        return base.GetEmotionStatBonuses(emotion);
    }

    private int Stage = 0;
    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SEBAttack"]);
        
        switch (CurrentEmotion.Id)
        {
            case "furious":
                if (Roll() < 36)
                    goto attack;
                goto bullet;
            case "enraged":
            case "angry":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 21)
                    goto nothing;
                if (Roll() < 21)
                    goto angsty;
                if (Roll() < 31)
                    goto angry;
                goto laser;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 21)
                    goto nothing;
                if (Roll() < 41)
                    goto angsty;
                if (Roll() < 21)
                    goto angry;
                goto laser;
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 21)
                    goto nothing;
                if (Roll() < 21)
                    goto angsty;
                if (Roll() < 21)
                    goto angry;
                goto laser;
            default:
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                if (Roll() < 31)
                    goto angsty;
                if (Roll() < 31)
                    goto angry;
                goto laser;

        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SEBAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["SEBDoNothing"]);
    angsty:
        return new BattleCommand(this, SelectTarget(), Skills["AngstySong"]);
    angry:
        return new BattleCommand(this, SelectAllTargets(), Skills["AngrySong"]);
    laser:
        return new BattleCommand(this, SelectTarget(), Skills["SpaceLaser"]);
    bullet:
        return new BattleCommand(this, SelectTargets(4), Skills["BulletHell"]);
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, @"[br]Ugh...\! my heart...");
        DialogueManager.Instance.QueueMessage(this, @"[br]It...\! hurts...");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0) return;

        if (Stage > 2)
            return;

        if (IsBelowHP(0.7504f) && Stage == 0)
        {
            DialogueManager.Instance.QueueMessage(this, "[br]My rage cannot be contained...[br]You cannot placate me!");
            await DialogueManager.Instance.WaitForDialogue();
            SetEmotionForced("angry");
            DialogueManager.Instance.QueueMessage("SPACE EX-BOYFRIEND became ANGRY!");
            DialogueManager.Instance.QueueMessage("SPACE EX-BOYFRIEND can no longer be HAPPY or SAD!");
            await DialogueManager.Instance.WaitForDialogue();
            LockEmotion("angry");
            Stage = 1;
        }
        
        if (IsBelowHP(0.5f) && Stage <= 1)
        {
            UnlockEmotion();
            DialogueManager.Instance.QueueMessage(this, @"[br]Gah!\! How are you still moving!?");
            DialogueManager.Instance.QueueMessage(this, @"[br]I...\! I won't let you defeat me!");
            await DialogueManager.Instance.WaitForDialogue();
            SetEmotionForced("enraged");
            DialogueManager.Instance.QueueMessage("SPACE EX-BOYFRIEND became ENRAGED!");
            await DialogueManager.Instance.WaitForDialogue();
            LockEmotion("angry");
            Stage = 2;
        }
        
        if (IsBelowHP(0.2504f) && Stage <= 2)
        {
            UnlockEmotion();
            DialogueManager.Instance.QueueMessage(this, "[br]Out of my way, earthly scum!");
            DialogueManager.Instance.QueueMessage(this, "[br]This is your last chance!");
            await DialogueManager.Instance.WaitForDialogue();
            SetEmotionForced("furious");
            DialogueManager.Instance.QueueMessage("SPACE EX-BOYFRIEND became FURIOUS!");
            await DialogueManager.Instance.WaitForDialogue();
            LockEmotion("angry");
            Stage = 3;
        }
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, "[br]You should have thought twice before challenging me.");
            DialogueManager.Instance.QueueMessage(this, "[br]You are nothing but earthly scum!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}