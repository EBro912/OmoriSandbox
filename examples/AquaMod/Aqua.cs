using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Modding;

namespace AquaMod;

public class Aqua : Enemy
{
    public override string Name => "AQUA";
    public override SpriteFrames Animation => new SpriteFramesBuilder("AquaMod/sprites/aqua.png", 152, 152)
        .AddAnimation("neutral", 5, 0, 1, 2, 3, 4, 5, 6, 7)
        .AddAnimation("laugh", 5, 8, 9)
        .AddAnimation("hurt", 1, 10)
        .AddAnimation("pose", 1, 11)
        .AddAnimation("spin", 5, 12, 13, 14, 15, 16)
        .AddAnimation("dance", 5, 18, 17, 18, 19)
        .AddAnimation("chain", 5, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29)
        .AddAnimation("toast", 1, 30)
        .AddAnimation("amused", 1, 31)
        .Build();

    public override Vector2 InfoBoxOffset => new(0, -160);

    protected override Stats Stats => new(3060, 0, 16, 0, 20, 10, 40);
    protected override string[] EquippedSkills => ["AQKnifeFan", "AQKnifeChain", "AQKnifeCircle", "AQOmega"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    private int Mercy = 0;
    private bool Attacked = false;
    private int Turn = 0;
    private HashSet<string> UsedActs = [];
    private int Duplicates = 0;
    private bool UsedOmega = false;

    public void AddMercy(string act)
    {
        if (UsedActs.Contains(act)) return;
        AudioManager.Instance.PlaySFX("mercy_add", 1, 2);
        Mercy += 25;
        BattleManager.Instance.SpawnDamageNumber(25, CenterPoint, DamageType.JuiceGain);
    }

    public async Task DoACTDialogue(string act)
    {
        if (!UsedActs.Add(act))
        {
            Duplicates++;
            switch (Duplicates)
            {
                case 1:
                    DialogueManager.Instance.QueueMessage(this, "Didn't we play that old game already?");
                    DialogueManager.Instance.QueueMessage(this, "Let's do something else!");
                    break;
                case 2:
                    DialogueManager.Instance.QueueMessage(this, "What a dull game.");
                    DialogueManager.Instance.QueueMessage(this, "Wouldn't you rather play with something sharp!?");
                    break;
                default:
                    DialogueManager.Instance.QueueMessage(this, @"Hum,\. is that all you can do?");
                    DialogueManager.Instance.QueueMessage(this, @"After all,\. humans are actually a little boring!");
                    break;
            }
            await DialogueManager.Instance.WaitForDialogue();
            return;
        }

        switch (act)
        {
            case "pose":
                DialogueManager.Instance.QueueMessage(this, "Oh, I get it! Posey, posey!");
                DialogueManager.Instance.QueueMessage(this, "Are we part of a team now too!?");
                break;
            case "spin":
                DialogueManager.Instance.QueueMessage(this, @"Hee hee! What's going on?\. Is the whole world revolving!?");
                break;
            case "care":
                DialogueManager.Instance.QueueMessage(this, @"HAHA! What is that!?\. Can humans change their shape so!?");
                await DialogueManager.Instance.WaitForDialogue();
                PlayAnimation("toast");
                DialogueManager.Instance.QueueMessage(this, "...");
                await DialogueManager.Instance.WaitForDialogue();
                PlayAnimation("amused");
                DialogueManager.Instance.QueueMessage(this, "I knew that!!");
                break;
            case "dance":
                DialogueManager.Instance.QueueMessage(this, @"Haha!\. Your feet are making such strange music!");
                break;
        }
        await DialogueManager.Instance.WaitForDialogue();
        DialogueManager.Instance.QueueMessage($"{Name} is now at {Mercy}% mercy.");
        await DialogueManager.Instance.WaitForDialogue();
    }
    
    public override async Task ProcessBattleConditions()
    {
        if (UsedOmega)
        {
            PlayAnimation("laugh");
            DialogueManager.Instance.QueueMessage(this, @"Uee hee hee!! Now, your turn,\. your turn!!");
            DialogueManager.Instance.QueueMessage(this, "Magic, magic, Omega, magic!");
            await DialogueManager.Instance.WaitForDialogue();
            await Wait.Seconds(3);
            PlayAnimation("toast");
            AudioManager.Instance.StopBGM();
            DialogueManager.Instance.QueueMessage(this, "...what?");
            DialogueManager.Instance.QueueMessage(this, "You can't do it!?");
            DialogueManager.Instance.QueueMessage(this, @"Uuu,\. how boring...");
            DialogueManager.Instance.QueueMessage(this, "No more battle!");
            await DialogueManager.Instance.WaitForDialogue();
            CurrentHP = 0;
            return;
        }
        
        if (!Attacked && CurrentHP < 3060)
        {
            AudioManager.Instance.PlaySFX("mercy_add", 1, 2);
            Mercy += 25;
            BattleManager.Instance.SpawnDamageNumber(25, CenterPoint, DamageType.JuiceGain);
            DialogueManager.Instance.QueueMessage(this, @"Uuu, what is this!?\. 'Pain'?\. How funny!!");
            DialogueManager.Instance.QueueMessage(this, @"Did you know?\. You should try some too!");
            await DialogueManager.Instance.WaitForDialogue();
            DialogueManager.Instance.QueueMessage($"AQUA is now at {Mercy}% mercy.");
            await DialogueManager.Instance.WaitForDialogue();
            Attacked = true;
        }

        if (CurrentHP < 612)
        {
            DialogueManager.Instance.QueueMessage(this, @"Uuu, this is fun,\. fun!!!");
            DialogueManager.Instance.QueueMessage("SETH", @"Wh-\.what are you doing!?");
            DialogueManager.Instance.QueueMessage(this, @"Playing a game!\. Do you want to play, too!?");
            DialogueManager.Instance.QueueMessage("SETH", @"What!?\. NO!!\. If you keep fighting, you'll end up...");
            DialogueManager.Instance.QueueMessage(this, @"Uuu? What, what? End up what?\. Is it another type of game...?");
            DialogueManager.Instance.QueueMessage("SETH", "N-No, I'm just worried that...");
            await DialogueManager.Instance.WaitForDialogue();
            PlayAnimation("laugh");
            DialogueManager.Instance.QueueMessage(this, "Don't worry, you can play too!!");
            DialogueManager.Instance.QueueMessage("SETH", "That's not what I'm saying!!! Just give up!! I'm going to retreat, too!");
            await DialogueManager.Instance.WaitForDialogue();
            PlayAnimation("amused");
            DialogueManager.Instance.QueueMessage(this, "Uuu, okay...");
            await DialogueManager.Instance.WaitForDialogue();
            CurrentHP = 0;
            return;
        }

        if (Mercy >= 100)
        {
            DialogueManager.Instance.QueueMessage(this, @"Uee hee hee,\. this is all so much fun!");
            DialogueManager.Instance.QueueMessage(this, @"Here,\. I'll do something,\. and you copy!");
            DialogueManager.Instance.QueueMessage(this, @"My magic,\. my Omega,\. can you follow?");
            await DialogueManager.Instance.WaitForDialogue();
            BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["AQOmega"]);
            UsedOmega = true;
        }
    }

    public override Task ProcessStartOfTurn()
    {
        Turn++;
        return Task.CompletedTask;
    }

    public override BattleCommand ProcessAI()
    {
        return (Turn % 3) switch
        {
            1 => new BattleCommand(this, SelectTarget(), Skills["AQKnifeFan"]),
            2 => new BattleCommand(this, SelectTarget(), Skills["AQKnifeChain"]),
            _ => new BattleCommand(this, SelectTarget(), Skills["AQKnifeCircle"])
        };
    }
}