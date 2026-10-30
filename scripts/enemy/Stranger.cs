using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Actors;

internal sealed class Stranger : Enemy
{
    private static readonly Vector2 BorderPosition = new(320, 220);
    private static readonly Vector2 BorderScale = new(1.25f, 1.25f);

    private bool EndingPending;

    public override string Name => "STRANGER";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override Vector2 DamageNumberOffset => new(0, -150);
    public override bool InfoBoxCursorAboveBox => true;
    public override bool BlinksWhenSelected => false;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/stranger.tres");
    protected override Stats Stats => new(9999, 9999, 10, 99, 99, 10, 200);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    protected override string[] EquippedSkills => ["STRShadowBasilAttack"];

    public override async Task OnStartOfBattle()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        AnimationManager animation = AnimationManager.Instance;
        await FearPages.BlackStart(255);
        PartyMember standIn = FearPages.Sunny;
        for (int slot = 1; slot <= 5; slot++)
            standIn.UnequipSkill(slot);
        animation.ShowPicture(1, "something_border", BorderPosition, true, BorderScale, 0);
        MoveBorder(45, 450);
        FearPages.AddState(standIn, "PersistentPlotArmor");
        FearPages.AddState(standIn, "Immortal");
        foreach (PartyMember member in BattleManager.Instance.GetAllPartyMembers())
            FearPages.AddState(member, "BasilSomething");
        StorySwitches.BlackSpaceWorld = true;
        FearPages.LearnBrokenSkills(standIn);
        standIn.UnequipSkill(3);
        standIn.SetAttackSkill("SRWBlackSpaceAttack");
        await animation.WaitForTintScreen(0, 0, 0, 0, 180 / 60f);
    }

    public override async Task ProcessEndOfTurn()
    {
        BattleManager battle = BattleManager.Instance;
        if (battle.AbortRequested)
            return;
        switch (battle.TurnCount)
        {
            case 1:
                await BorderPage(80, $@"{FearPages.SunnyName}...\! All those times that I reached out to you...\! Why didn't you answer?");
                break;
            case 2:
                await BorderPage(120, @"You promised me that we'd face this together...\! but you left me all alone.");
                break;
            case 3:
                MoveBorder(140, 450);
                break;
            case 4:
                await BorderPage(180, @"If we can't face it together...\! at least we can be together here.");
                break;
            case 5:
                MoveBorder(200, 450);
                await Wait.Frames(75);
                DialogueManager.Instance.QueueMessage(this, $@"{FearPages.SunnyName}...\! I'll trap you down here with me forever.", font: DialogueManager.FontType.Jagged);
                await DialogueManager.Instance.WaitForDialogue();
                await Wait.Frames(45);
                battle.ForceCommand(this, battle.SmoothPartyTarget(LastTargetIndex), Skills["STRShadowBasilAttack"]);
                EndingPending = true;
                break;
        }
    }

    public override async Task ProcessBattleConditions()
    {
        BattleManager battle = BattleManager.Instance;
        if (battle.AbortRequested)
            return;
        if (!EndingPending || battle.GetCurrentCommand()?.Actor != this)
            return;
        EndingPending = false;
        BattleLogManager.Instance.ClearBattleLog();
        await Wait.Frames(120);
        FadeBGM(0.001f, 4f);
        FadeOutBGS(4f);
        FearPages.RestoreAttack();
        MoveBorder(0, 300);
        await AnimationManager.Instance.WaitForTintScreen(-255, -255, -255, 0, 300 / 60f);
        await FearPages.FadeOutScreen();
        await Wait.Frames(120);
        await FearPages.End(this);
    }

    public override BattleCommand ProcessAI()
    {
        return null;
    }

    private async Task BorderPage(int opacity, string text)
    {
        MoveBorder(opacity, 450);
        await Wait.Frames(20);
        DialogueManager.Instance.QueueMessage(Name, text, font: DialogueManager.FontType.Jagged);
        await DialogueManager.Instance.WaitForDialogue();
    }

    private static void MoveBorder(int opacity, int frames)
    {
        AnimationManager.Instance.MovePicture(1, BorderPosition, BorderScale, opacity, frames);
    }
}
