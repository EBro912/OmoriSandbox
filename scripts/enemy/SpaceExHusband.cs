using Godot;
using System.Threading.Tasks;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Animation;
 
namespace OmoriSandbox.Actors;

internal sealed class SpaceExHusband : Enemy
{
    public override string Name => "SPACE EX-HUSBAND";
    public override Vector2 InfoBoxOffset => new(0, -260);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/space_ex_husband.tres");
    protected override Stats Stats => new(6000, 3000, 80, 999, 50, 10, 95);
    protected override string[] EquippedSkills => ["SEHAttack", "SEHLaser", "SEHAngrySong", "SEHAngstySong", "SEHJoyfulSong", "SEHSpinningKick", "SEHBulletHell"];

    private Stats GetStatsForEmotion()
    {
        return CurrentEmotion.Group?.Id switch
        {
            "sad" => new Stats(6000, 3000, 65, 85, 30, 5, 95),
            "happy" => new Stats(6000, 3000, 70, 35, 105, 25, 95),
            "angry" => new Stats(6000, 3000, 90, 15, 50, 10, 95),
            _ => new Stats(6000, 3000, 80, 999, 50, 10, 95)
        };
    }

    public override bool IsEmotionValid(Emotion emotion)
    {
        if (emotion.Id == "neutral")
            return true;

        // the photograph effect is played from OnEmotionChanged
        if (emotion.Id == DesiredEmotion)
            return true;

        // outside a desire, only the next tier of his current group can be applied
        return emotion.Group != null && emotion.Group == CurrentEmotion.Group && emotion.Tier == CurrentEmotion.Tier + 1;
    }

    public override void SetHurt(bool hurt)
    {
        if (CurrentAnimation != null)
            return;

        // unlike other actors, he only shows the hurt animation while neutral (guarding his HEART)
        if (hurt && CurrentEmotion.Id == "neutral")
            Sprite.Animation = "hurt";
        else
            Sprite.Animation = CurrentEmotion.AnimationName;
    }

    public override BattleCommand ProcessAI()
    {
        switch (CurrentEmotion.Id)
        {
            case "happy":
            case "ecstatic":
            case "manic":
                if (HasMultiTargetObserve())
                    goto joyful;
                if (HasObserveTarget(out PartyMember observe))
                    return new BattleCommand(this, observe, Skills["SEHLaser"]);
                if (Roll() < 51)
                    goto joyful;
                if (Roll() < 51)
                    goto kick;
                goto laser;
            case "sad":
            case "depressed":
            case "miserable":
                if (HasMultiTargetObserve())
                    goto angsty;
                if (HasObserveTarget(out observe))
                    return new BattleCommand(this, observe, Skills["SEHLaser"]);
                if (Roll() < 51)
                    goto angsty;
                goto laser;
            case "angry":
            case "enraged":
            case "furious":
                if (HasMultiTargetObserve())
                    goto angry;
                if (HasObserveTarget(out observe))
                    return new BattleCommand(this, observe, Skills["SEHLaser"]);
                if (Roll() < 46)
                    goto angry;
                if (Roll() < 46)
                    goto laser;
                goto bullet;
            default:
                if (HasObserveTarget(out observe))
                    return new BattleCommand(this, observe, Skills["SEHLaser"]);
                if (Roll() < 51)
                    goto attack;
                goto laser;

        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SEHAttack"]);
    laser:
        return new BattleCommand(this, SelectTarget(), Skills["SEHLaser"]);
    angry:
        return new BattleCommand(this, SelectAllTargets(), Skills["SEHAngrySong"]);
    angsty:
        return new BattleCommand(this, SelectAllTargets(), Skills["SEHAngstySong"]);
    joyful:
        return new BattleCommand(this, SelectAllTargets(), Skills["SEHJoyfulSong"]);
    kick:
        return new BattleCommand(this, SelectTarget(), Skills["SEHSpinningKick"]);
    bullet:
        return new BattleCommand(this, SelectTargets(4), Skills["SEHBulletHell"]);
    }

    private int TurnCounter = 0;
    private bool TurnTwoLine = false;
    private bool DesiringEmotion = false;
    private string DesiredEmotion = "neutral";
    public override async Task ProcessStartOfTurn()
    {
        TurnCounter++;
        if (DesiringEmotion)
        {
            if (!CheckDesiredEmotion())
            {
                DialogueManager.Instance.QueueMessage(this, @"Sigh...\! No one truly understands the depths of my pain.");
                DialogueManager.Instance.QueueMessage(this, @"If I do not feel...\! then the pain can no longer reach me.");
            }
            await DialogueManager.Instance.WaitForDialogue();
            DesiredEmotion = "neutral";
            DesiringEmotion = false;
            return;
        }
        if (TurnCounter == 3)
        {
            if (!TurnTwoLine)
            {
                DialogueManager.Instance.QueueMessage(this, "All I have left are my memories...");
                DialogueManager.Instance.QueueMessage(this, "But even they cannot make me feel anymore.");
                await DialogueManager.Instance.WaitForDialogue();
                TurnTwoLine = true;
            }

            TurnCounter = 0;

            if (CurrentEmotion.Id != "neutral")
            {
                AnimationManager.Instance.PlayPhotograph();
                SetEmotion("neutral", true);
                await Wait.Milliseconds(1200);
                DialogueManager.Instance.QueueMessage(this, @"Nay! I must guard my HEART.\! I must become one... with the ice...");
                await DialogueManager.Instance.WaitForDialogue();
            }
            else
            {
                DialogueManager.Instance.QueueMessage(this, "Alas! I see a memory before me!");
                await DialogueManager.Instance.WaitForDialogue();
                await ChooseDesiredEmotion();
            }
        }
    }

    private bool CheckDesiredEmotion()
    {
        switch (DesiredEmotion)
        {
            case "sad":
                if (CurrentEmotion.Group?.Id == "sad")
                {
                    DialogueManager.Instance.QueueMessage(this, @"Oh...\! I can't believe she's really gone...");
                    return true;
                }
                break;
            case "happy":
                if (CurrentEmotion.Group?.Id == "happy")
                {
                    DialogueManager.Instance.QueueMessage(this, "I still do think fondly of those times...");
                    return true;
                }
                break;
            case "angry":
                if (CurrentEmotion.Group?.Id == "angry")
                {
                    DialogueManager.Instance.QueueMessage(this, @"GAH!\! HOW DARE SHE TREAT ME THAT WAY!");
                    return true;
                }
                break;
        }
        return false;
    }

    private async Task ChooseDesiredEmotion()
    {
        switch (GameManager.Instance.Random.RandiRange(0, 2))
        {
            case 0:
                DesiredEmotion = "sad";
                switch (GameManager.Instance.Random.RandiRange(0, 2))
                {
                    case 0:
                        DialogueManager.Instance.QueueMessage(this, @"It's me... alone...\! throwing away my [color=#fed966]SPECIAL MIXTAPE[/color]...");
                        break;
                    case 1:
                        DialogueManager.Instance.QueueMessage(this, @"It's me... alone...\! weeping in my king-sized bed...");
                        break;
                    case 2:
                        DialogueManager.Instance.QueueMessage(this, @"It's me... alone...\! holding a picture of my dear SWEETHEART...");
                        break;
                }
                break;
            case 1:
                DesiredEmotion = "happy";
                switch (GameManager.Instance.Random.RandiRange(0, 2))
                {
                    case 0:
                        DialogueManager.Instance.QueueMessage(this, @"It's me... and my SWEETHEART...\! kissing on her glorious stage!");
                        break;
                    case 1:
                        DialogueManager.Instance.QueueMessage(this, @"It's me... and my SWEETHEART...\! staring at the night sky together!");
                        break;
                    case 2:
                        DialogueManager.Instance.QueueMessage(this, @"It's me... and my SWEETHEART...\! gazing into each other's eyes!");
                        break;
                }
                break;
            case 2:
                DesiredEmotion = "angry";
                switch (GameManager.Instance.Random.RandiRange(0, 2))
                {
                    case 0:
                        DialogueManager.Instance.QueueMessage(this, @"It's my SWEETHEART... but she's...\! swinging her mace at me!");
                        break;
                    case 1:
                        DialogueManager.Instance.QueueMessage(this, @"It's my SWEETHEART... but she's...\! in the arms of another man!");
                        break;
                    case 2:
                        DialogueManager.Instance.QueueMessage(this, @"It's my SWEETHEART... but she's...\! throwing my things across the room!");
                        break;
                }
                break;
        }
        DesiringEmotion = true;
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, @"[br]The pain...\! I can feel it...");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, @"I feel nothing...\! I am cold...\! like ice...");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }

    public override async Task OnStartOfBattle()
    {
        AddStatModifier("SpaceExHusbandBlock", silent: true);
        OnEmotionChanged += (_, _) =>
        {
            if (DesiringEmotion && CurrentEmotion.Id == DesiredEmotion)
                AnimationManager.Instance.PlayPhotograph();
        };
        DialogueManager.Instance.QueueMessage(this, @"[br]I feel nothing...\![br]I am cold...\! like ice...");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override Stats GetBaseStats()
    {
        return GetStatsForEmotion() + AdjustedStats;
    }
}
