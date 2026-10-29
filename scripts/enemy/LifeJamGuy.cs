using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class LifeJamGuy : Enemy
{
    public override string Name => "LIFE JAM GUY";
    public override Vector2 InfoBoxOffset => new(0, -350);
    public override Vector2 DamageNumberOffset => new(0, -75);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/life_jam_guy.tres");
    protected override Stats Stats => new(9999, 0, 1, 999, 1, 10, 95);

    private Stats GetStatsForEmotion()
    {
        return CurrentEmotion.Group?.Id switch
        {
            "happy" => new Stats(9999, 0, 1, 15, 1, 10, 95),
            "sad" => new Stats(9999, 0, 1, 15, 1, 10, 95),
            "angry" => new Stats(9999, 0, 1, 15, 1, 10, 95),
            _ => new Stats(9999, 0, 1, 999, 1, 10, 95)
        };
    }

    public override Stats GetBaseStats()
    {
        return GetStatsForEmotion() + AdjustedStats;
    }

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["LJGDoNothing", "LJGDeathJam"];

    public override BattleCommand ProcessAI()
    {
        return new BattleCommand(this, this, Skills["LJGDoNothing"]);
    }

    private bool DeathJamPending;

    public override Task OnStartOfBattle()
    {
        AddStatModifier("Immune", silent: true);
        AddImmortal();
        return Task.CompletedTask;
    }

    public override async Task ProcessStartOfCommands()
    {
        if (BattleManager.Instance.AbortRequested || BattleManager.Instance.TurnCount != 1)
            return;
        await Wait.Frames(20);
        BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["LJGDeathJam"]);
        DeathJamPending = true;
    }

    public override Task ProcessBattleConditions()
    {
        if (BattleManager.Instance.AbortRequested || !DeathJamPending || BattleManager.Instance.GetCurrentCommand()?.Actor != this)
            return Task.CompletedTask;
        DeathJamPending = false;
        Stun(1);
        ClearResult();
        foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
        {
            member.Stun(1);
            member.ClearResult();
        }
        return Task.CompletedTask;
    }

    public override async Task ProcessEndOfTurn()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        if (BattleManager.Instance.TurnCount == 1)
        {
            DialogueManager.Instance.QueueMessage(this, @"Use [color=#5bd863]LIFE JAM[/color]!\![br]It really works!");
            await DialogueManager.Instance.WaitForDialogue();
            await Wait.Frames(75);
            for (int i = 0; i < 3; i++)
            {
                BattleManager.Instance.AddItem("Life Jam", 1);
                if (i == 0)
                    await Wait.Frames(20);
                DialogueManager.Instance.QueueMessage("You got [color=#5bd863]LIFE JAM[/color]!");
                await DialogueManager.Instance.WaitForDialogue();
                AudioManager.Instance.PlaySFX("SE_NewSkill", 1f, 0.9f);
                await Wait.Frames(20);
            }
        }
        if (BattleManager.Instance.TurnCount < 1 || BattleManager.Instance.GetAllPartyMembers().Exists(x => x.IsToast))
            return;
        await Wait.Frames(20);
        DialogueManager.Instance.QueueMessage(this, @"Enjoy your free samples!\! Goodbye now, haha!");
        await DialogueManager.Instance.WaitForDialogue();
        await Wait.Frames(45);
        await AbortBattle();
    }
}
