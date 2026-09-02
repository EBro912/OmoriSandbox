using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SnaleyOne : Enemy
{
    public override string Name => "SNALEY";
    public override Vector2 InfoBoxOffset => new(0, -225);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/snaley.tres");
    protected override Stats Stats => new(1000, 500, 30, 10, 30, 10, 85);
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id != "afraid" && emotion.Id != "stressed";
    }
    protected override string[] EquippedSkills => ["SNAttack", "SNDoNothing"];

    private int Turn = 0;
    
    public override BattleCommand ProcessAI()
    {
        Turn++;
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SNAttack"]);
        
        if (Turn is 3 or 4)
            return new BattleCommand(this, this, Skills["SNDoNothing"]);
        return new BattleCommand(this, SelectTarget(), Skills["SNAttack"]);
    }

    public override async Task OnStartOfBattle()
    {
        DialogueManager.Instance.QueueMessage(this, @"Wow!\! My first battle!\! Here I come!");
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
            DialogueManager.Instance.QueueMessage(this, @"Battles are harder than they look...\! I gotta try harder!");
            await DialogueManager.Instance.WaitForDialogue();
        }
        else if (Turn == 2)
        {
            if (CurrentEmotion.Id != "depressed" && CurrentEmotion.Id != "miserable")
                SetEmotion("sad", true);
            DialogueManager.Instance.QueueMessage(this, @"Sigh...\! I don't know if I'm cut out for this...");
            await DialogueManager.Instance.WaitForDialogue();
            DialogueManager.Instance.QueueMessage("SNALEY is SAD...");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}