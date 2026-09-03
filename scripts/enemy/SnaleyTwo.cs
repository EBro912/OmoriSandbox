using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SnaleyTwo : Enemy
{
    public override string Name => "SNALEY";
    public override Vector2 InfoBoxOffset => new(0, -225);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/snaley.tres");
    protected override Stats Stats => new(1500, 750, 35, 25, 35, 10, 95);
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id != "afraid" && emotion.Id != "stressed";
    }
    protected override string[] EquippedSkills => ["SNAttack", "SNDoNothing", "SNBeatdown", "SNAttackFollowup", "SNFollowup"];

    private int Turn = 0;
    
    public override BattleCommand ProcessAI()
    {
        if (!PreRolling)
            Turn++;
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SNBeatdown"]);
        
        if (Turn is 1)
            return new BattleCommand(this, SelectTarget(), Skills["SNBeatdown"]);
        if (Turn is 2)
        {
            PartyMember target = SelectTarget();
            if (!PreRolling)
                BattleManager.Instance.ForceCommand(this, target, Skills["SNFollowup"]);
            return new BattleCommand(this, target, Skills["SNAttackFollowup"]);
        }
        
        if (Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["SNAttack"]);
        if (Roll() < 36)
        {
            PartyMember target = SelectTarget();
            if (!PreRolling)
                BattleManager.Instance.ForceCommand(this, target, Skills["SNFollowup"]);
            return new BattleCommand(this, target, Skills["SNAttackFollowup"]);
        }
        if (Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["SNBeatdown"]);
        return new BattleCommand(this, this, Skills["SNDoNothing"]);
    }

    public override async Task OnStartOfBattle()
    {
        DialogueManager.Instance.QueueMessage(this, @"I taught myself some [color=#6095ff]SKILLS[/color] since our last battle!\! You'd better watch out!");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, "Okay, please stop! That's enough!");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task ProcessEndOfTurn()
    {
        if (Turn == 1)
        {
            DialogueManager.Instance.QueueMessage(this, "[wave freq=10.0]Wasn't that cool!?[/wave] I'm awesome!");
            await DialogueManager.Instance.WaitForDialogue();
            DialogueManager.Instance.QueueMessage(this, "I can do [color=#6095ff]FOLLOW-UP SKILLS[/color] too! Watch this!");
            await DialogueManager.Instance.WaitForDialogue();
        }
        else if (Turn == 2)
        {
            if (CurrentEmotion.Id != "ecstatic" && CurrentEmotion.Id != "manic")
                SetEmotion("happy", true);
            DialogueManager.Instance.QueueMessage(this, @"How was that!?\! One of these days, I'll be as strong as you!");
            await DialogueManager.Instance.WaitForDialogue();
            AudioManager.Instance.PlaySFX("GEN_shine", 1f, 0.9f);
            DialogueManager.Instance.QueueMessage("SNALEY is HAPPY!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}