using System;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SirMaximusII : Enemy
{
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sir_maximus.tres");

    public override string Name => "SIR MAXIMUS II";
    public override Vector2 InfoBoxOffset => new(0, -200);

    protected override Stats Stats => new(750, 375, 20, 20, 10, 10, 95);

    protected override string[] EquippedSkills => ["SMIAttack", "SMIIDoNothing", "SMIStrikeTwice", "SMIISpin", "SMIIUltimateAttack"];
    protected internal override bool ObserveHasMulti => true;

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
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 36)
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

    private bool FirstDialogue = false;
    private bool UltimateAttack = false;
    
    public override async Task ProcessBattleConditions()
    {
        if (IsBelowHP(0.5f) && !FirstDialogue)
        {
            DialogueManager.Instance.QueueMessage(this, "No... I cannot let my father's death be in vain!");
            await DialogueManager.Instance.WaitForDialogue();
            FirstDialogue = true;
        }
        
        if (IsBelowHP(0.2f) && !UltimateAttack)
        {
            Sprite2D ghost = new()
            {
                Texture = ResourceLoader.Load<Texture2D>("res://assets/pictures/Maximus.png"),
                Scale = new Vector2(0.75f, 0.75f),
                Modulate = Colors.Transparent,
                GlobalPosition = new Vector2(190, 198),
                Centered = true,
                ZIndex = Layer
            };
            BattleManager.Instance.AddChild(ghost);
            Tween tween = BattleManager.Instance.CreateTween();
            tween.TweenProperty(ghost, "global_position:y", 188, 1.25f);
            tween.Parallel().TweenProperty(ghost, "modulate:a", 1, 1.25f);
            tween.TweenProperty(ghost, "global_position:y", 198, 1.25f);
            tween.TweenProperty(ghost, "global_position:y", 188, 1.25f);
            tween.Parallel().TweenProperty(ghost, "modulate:a", 0, 1.25f);
            tween.TweenInterval(0.25f);
            await BattleManager.Instance.ToSignal(tween, Tween.SignalName.Finished);
            ghost.QueueFree();
            
            BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["SMIIUltimateAttack"]);
            UltimateAttack = true;
        }
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, "Father...[br]Forgive me.");
        DialogueManager.Instance.QueueMessage(this, @"I'm sorry...\![br]I have failed you.");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, "Alas, my father has been avenged!");
            DialogueManager.Instance.QueueMessage(this, "This is a glorious day for my people!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}