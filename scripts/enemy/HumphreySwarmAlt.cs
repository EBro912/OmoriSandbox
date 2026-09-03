using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Actors;

internal sealed class HumphreySwarmAlt : Enemy
{
    public override string Name => "HUMPHREY";
    public override Vector2 InfoBoxOffset => new(0, -350);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/humphrey_swarm.tres");
    protected override Stats Stats => new(9999, 5000, 10, 150, 65, 20, 95);
    protected override string[] EquippedSkills => ["HUSAttack", "HUSAttack2", "HUSAttack3"];
    protected internal override bool ObserveHasMulti => true;
    
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
    }

    private int Turn = 0;
    // prevent race condition between two triggers
    private bool HasTransformed = false;
    
    public override BattleCommand ProcessAI()
    {
        if (!PreRolling)
            Turn++;
        
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectTargets(3), Skills["HUSAttack3"]);
        
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["HUSAttack"]);
        
        switch (CurrentEmotion.Id)
        {
            case "angry":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 21)
                    goto attack2;
                goto attack3;
            case "sad":
                if (Roll() < 41)
                    goto attack;
                goto attack2;
            default:
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 31)
                    goto attack2;
                goto attack3;
        }
        attack:
            return new BattleCommand(this, SelectTarget(), Skills["HUSAttack"]);
        attack2:
            return new BattleCommand(this, SelectTargets(2), Skills["HUSAttack2"]);
        attack3:
            return new BattleCommand(this, SelectTargets(3), Skills["HUSAttack3"]);
    }

    public override async Task OnStartOfBattle()
    {
        DialogueManager.Instance.QueueMessage(this, "[br][wave freq=10.0]Time to feast! Time to feast! Time for you to be deceased![/wave]");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task ProcessBattleConditions()
    {
        if (IsBelowHP(0.1f) && !HasTransformed)
        {
            HasTransformed = true;
            await ChangePhase();
        }
    }

    public override async Task ProcessEndOfTurn()
    {
        if (Turn >= 5 && !HasTransformed)
        {
            HasTransformed = true;
            await ChangePhase();
        }
    }

    private async Task ChangePhase()
    {
        DialogueManager.Instance.QueueMessage(this, @"[wave freq=10.0]The final fight has just begun!\| But can you win if we work as one?[/wave]");
        await DialogueManager.Instance.WaitForDialogue();
        await AnimationManager.Instance.WaitForHumphreySwarm();
        BattleManager.Instance.TransformEnemy(this, "HumphreyGrande (Boss Rush)");
        await Wait.Milliseconds(2500);
        await AnimationManager.Instance.WaitForTintScreen(ColorsExtension.TransparentBlack, 0.5f);
    }
}