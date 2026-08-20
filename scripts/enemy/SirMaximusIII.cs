using System;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SirMaximusIII : Enemy
{
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sir_maximus.tres");

    public override string Name => "SIR MAXIMUS III";

    protected override Stats Stats => new(1100, 550, 24, 24, 20, 15, 95);

    protected override string[] EquippedSkills => ["SMIAttack", "SMIIIDoNothing", "SMIStrikeTwice", "SMIISpin", "SMIIIFlex", "SMIIIUltimateAttack"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral" || emotion.Id == "sad" || emotion.Id == "happy" || emotion.Id == "angry";
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
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                if (Roll() < 31)
                    goto twice;
                goto spin;
            case "sad":
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 31)
                    goto twice;
                if (Roll() < 36)
                    goto spin;
                goto flex;
            case "angry":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                if (Roll() < 46)
                    goto twice;
                if (Roll() < 41)
                    goto spin;
                goto flex;
            default:
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                if (Roll() < 31)
                    goto twice;
                if (Roll() < 36)
                    goto spin;
                goto flex;
        }
        // reuse skills from SMI and SMII
        attack:
        return new BattleCommand(this, SelectTarget(), Skills["SMIAttack"]);
        nothing:
        return new BattleCommand(this, this, Skills["SMIIIDoNothing"]);
        twice:
        return new BattleCommand(this, SelectTargets(2), Skills["SMIStrikeTwice"]);
        spin:
        return new BattleCommand(this, SelectAllTargets(), Skills["SMIISpin"]);
        flex:
        return new BattleCommand(this, this, Skills["SMIIIFlex"]);
    }

    private bool FirstDialogue = false;
    private bool UltimateAttack = false;
    
    public override async Task ProcessBattleConditions()
    {
        if (IsBelowHP(0.5f) && !FirstDialogue)
        {
            DialogueManager.Instance.QueueMessage(this, "No... I cannot let my father's and his father's deaths be in vain!");
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
            
            Sprite2D ghost2 = new()
            {
                Texture = ResourceLoader.Load<Texture2D>("res://assets/pictures/Maximus.png"),
                Scale = new Vector2(0.75f, 0.75f),
                Modulate = Colors.Transparent,
                GlobalPosition = new Vector2(415, 198),
                Centered = true,
                ZIndex = Layer
            };
            BattleManager.Instance.AddChild(ghost);
            BattleManager.Instance.AddChild(ghost2);
            Tween tween = BattleManager.Instance.CreateTween();
            Tween tween2 = BattleManager.Instance.CreateTween();
            tween.TweenProperty(ghost, "global_position:y", 188, 1.25f);
            tween.Parallel().TweenProperty(ghost, "modulate:a", 1, 1.25f);
            tween2.TweenProperty(ghost2, "global_position:y", 188, 1.25f);
            tween2.Parallel().TweenProperty(ghost2, "modulate:a", 1, 1.25f);
            tween.TweenProperty(ghost, "global_position:y", 198, 1.25f);
            tween2.TweenProperty(ghost2, "global_position:y", 198, 1.25f);
            tween.TweenProperty(ghost, "global_position:y", 188, 1.25f);
            tween.Parallel().TweenProperty(ghost, "modulate:a", 0, 1.25f);
            tween2.TweenProperty(ghost2, "global_position:y", 188, 1.25f);
            tween2.Parallel().TweenProperty(ghost2, "modulate:a", 0, 1.25f);
            tween.TweenInterval(0.25f);
            tween2.TweenInterval(0.25f);
            await BattleManager.Instance.ToSignal(tween, Tween.SignalName.Finished);
            ghost.QueueFree();
            ghost2.QueueFree();
            
            BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["SMIIIUltimateAttack"]);
            UltimateAttack = true;
        }
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, "Father... Grandfather...");
        DialogueManager.Instance.QueueMessage(this, @"I'm sorry...\![br]I have failed you.");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, "Alas, my family has been avenged!");
            DialogueManager.Instance.QueueMessage(this, "This is a glorious day for my people!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}