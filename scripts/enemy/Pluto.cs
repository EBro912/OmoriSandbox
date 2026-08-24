using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Pluto : Enemy
{
    public override string Name => "???";
    public override Vector2 InfoBoxOffset => new(0, -250);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/pluto.tres");
    protected override string[] EquippedSkills => ["PLDoNothing", "PLBrag", "PLHeadbutt", "PLExpand"];
    protected override Stats Stats => new(300, 150, 12, 10, 4, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral" || emotion.Id == "happy" || emotion.Id == "sad" || emotion.Id == "angry";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["PLHeadbutt"]);
        
        switch (CurrentEmotion.Id)
        {
            case "angry":
                if (Roll() < 31)
                    goto nothing;
                if (Roll() < 81)
                    goto headbutt;
                goto brag;
            case "sad":
                if (Roll() < 81)
                    goto nothing;
                if (Roll() < 31)
                    goto headbutt;
                goto brag;
            case "happy":
                if (Roll() < 31)
                    goto nothing;
                goto headbutt;
            default:
                if (Roll() < 41)
                    goto nothing;
                if (Roll() < 61)
                    goto headbutt;
                goto brag;
        }
        nothing:
        return new BattleCommand(this, this, Skills["PLDoNothing"]);
        headbutt:
        return new BattleCommand(this, SelectTarget(), Skills["PLHeadbutt"]);
        brag:
        return new BattleCommand(this, this, Skills["PLBrag"]);
    }

    private EnemyComponent LeftArm;
    private EnemyComponent RightArm;
    public override Task OnStartOfBattle()
    {
        LeftArm = BattleManager.Instance.SummonEnemy("LeftArm", CenterPoint - new Vector2(-140, 50), layer: Layer + 1);
        RightArm = BattleManager.Instance.SummonEnemy("RightArm", CenterPoint - new Vector2(140, 50), layer: Layer + 1);
        return Task.CompletedTask;
    }

    private bool HasExpanded = false;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0) return;

        if (IsBelowHP(0.5f) && !HasExpanded)
        {
            DialogueManager.Instance.QueueMessage(this, "[br][wave freq=20.0]GWAH[font_size=40]AHAHAH[font_size=52]AHAHA!!!");
            DialogueManager.Instance.QueueMessage(this, "[br]What a splendid show of force!");
            await DialogueManager.Instance.WaitForDialogue();
            BattleManager.Instance.ForceCommand(this, this, Skills["PLExpand"]);
            HasExpanded = true;
        }
        
    }

    public override Task OnDefeat()
    {
        if (LeftArm != null)
            LeftArm.Actor.CurrentHP = 0;
        if (RightArm != null)
            RightArm.Actor.CurrentHP = 0;
        return Task.CompletedTask;
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, @"Hmph...\![br]You kids fought well...\! but you lack training.");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}