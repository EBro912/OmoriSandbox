using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class PlutoExpandedAlt : Enemy
{
    public override string Name => "PLUTO (EXPANDED)";
    public override SpriteFrames Animation =>
        ResourceLoader.Load<SpriteFrames>("res://animations/pluto_expanded.tres");
    protected override Stats Stats => new(10000, 5000, 85, 65, 70, 15, 95);
    protected override string[] EquippedSkills => ["PEAttack", "PESubmissionHold", "PEHeadbutt", "PEDoNothing", "PEExpandFurther", "PEEarthsFinale"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "angry" or "happy";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["PEAttack"]);
        
        if (CurrentHP < 1000)
            return new BattleCommand(this, SelectAllTargets(), Skills["PEEarthsFinale"]);

        if (Roll() < 31)
            return new BattleCommand(this, SelectTarget(), Skills["PEAttack"]);
        if (Roll() < 31)
            return new BattleCommand(this, SelectTarget(), Skills["PESubmissionHold"]);
        if (Roll() < 31)
            return new BattleCommand(this, this, Skills["PEDoNothing"]);
        if (CurrentHP < 4000)
            return new BattleCommand(this, this, Skills["PEExpandFurther"]);
        return new BattleCommand(this, SelectTarget(), Skills["PEHeadbutt"]);
    }

    public override async Task OnStartOfBattle()
    {
        DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"Behold...\![br]This is my final form.");
        DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"Can you...\! feel the heat?");
        await DialogueManager.Instance.WaitForDialogue();
    }

    private bool HasSpoken = false;
    private bool HasMentionedFlex = false;
    private string WhoFlexed;
    public override async Task ProcessBattleConditions()
    {
        if (!HasMentionedFlex && WhoFlexed is null)
        {
            WhoFlexed = SelectAllTargets().FirstOrDefault(x => x.HasStatModifier("Flex"))?.Name;
        }
        
        if (CurrentHP <= 0)
            return;
        
        if (CurrentHP < 5000 && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"...\! Ah.\! I see.");
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, "You have all gotten stronger.");
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"But...\! so have I.");
            await DialogueManager.Instance.WaitForDialogue();
            BattleManager.Instance.ForceCommand(this, this, Skills["PEExpandFurther"]);
            HasSpoken = true;
        }
    }
    
    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"Hm.\! Well done, children.\![br]You've come a long way.");
        DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"But...\![br]I am not finished yet.");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task ProcessEndOfTurn()
    {
        if (!HasMentionedFlex && WhoFlexed != null)
        {
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint,
                $"Impressive progress, young {WhoFlexed.ToUpper()}! Your [color=#6095ff]FLEX[/color] has improved greatly!");
            await DialogueManager.Instance.WaitForDialogue();
            HasMentionedFlex = true;
        }
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, "I apologize, children.");
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, "You should applaud yourselves for your effort.");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}