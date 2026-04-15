using Godot;
using System.Threading.Tasks;

using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class SpaceExBoyfriend : Enemy
{
    public override string Name => "SPACE EX-BOYFRIEND";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/space_ex_boyfriend.tres");
    protected override Stats Stats => new(1350, 750, 15, 16, 25, 10, 95);
    protected override string[] EquippedSkills => ["SEBAttack", "SEBDoNothing", "AngstySong", "AngrySong", "SpaceLaser", "BulletHell"];
    public override bool IsStateValid(string state)
    {
        if (state == "toast")
            return true;

        if (EmotionLocked)
            return false;

        return state is "neutral" or "sad" or "happy" or "angry" or "hurt";
    }

    private bool EmotionLocked = false;
    private int Stage = 0;
    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SEBAttack"]);
        
        switch (CurrentState)
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

        if (CurrentHP < 1013 && Stage == 0)
        {
            DialogueManager.Instance.QueueMessage(this, "[br]My rage cannot be contained...[br]You cannot placate me!");
            await DialogueManager.Instance.WaitForDialogue();
            ForceState("SpaceExAngry", "angry");
            DialogueManager.Instance.QueueMessage("SPACE EX-BOYFRIEND became ANGRY!");
            DialogueManager.Instance.QueueMessage("SPACE EX-BOYFRIEND can no longer be HAPPY or SAD!");
            await DialogueManager.Instance.WaitForDialogue();
            EmotionLocked = true;
            Stage = 1;
        }
        
        if (CurrentHP < 675 && Stage <= 1)
        {
            EmotionLocked = false;
            DialogueManager.Instance.QueueMessage(this, @"[br]Gah!\! How are you still moving!?");
            DialogueManager.Instance.QueueMessage(this, @"[br]I...\! I won't let you defeat me!");
            await DialogueManager.Instance.WaitForDialogue();
            ForceState("SpaceExEnraged", "enraged");
            DialogueManager.Instance.QueueMessage("SPACE EX-BOYFRIEND became ENRAGED!");
            await DialogueManager.Instance.WaitForDialogue();
            EmotionLocked = true;
            Stage = 2;
        }
        
        if (CurrentHP < 338 && Stage <= 2)
        {
            EmotionLocked = false;
            DialogueManager.Instance.QueueMessage(this, "[br]Out of my way, earthly scum!");
            DialogueManager.Instance.QueueMessage(this, "[br]This is your last chance!");
            await DialogueManager.Instance.WaitForDialogue();
            ForceState("SpaceExFurious", "furious");
            DialogueManager.Instance.QueueMessage("SPACE EX-BOYFRIEND became FURIOUS!");
            await DialogueManager.Instance.WaitForDialogue();
            EmotionLocked = true;
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