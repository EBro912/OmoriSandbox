using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SomethingInTheDark : Enemy
{
    public override string Name => "SOMETHING";
    public override Vector2 InfoBoxOffset => new(0, -225);
    public override Vector2 DamageNumberOffset => new(0, -200);
    public override bool BlinksWhenSelected => false;
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/fear_of_heights.tres");
    protected override Stats Stats => new(9000, 4500, 10, 10, 10, 10, 1000);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    protected override string[] EquippedSkills => ["DRKSilent", "DRKDarkAttack", "DRKDoNothing2", "DRKDoNothing1", "DRKFinalAttack"];

    public override BattleCommand ProcessAI()
    {
        List<PartyMember> calm = BattleManager.Instance.GetAlivePartyMembers().FindAll(x => x.HasStatModifier("CalmDownText"));
        if (calm.Count > 0)
            return new BattleCommand(this, calm[GameManager.Instance.Random.RandiRange(0, calm.Count - 1)], Skills["DRKSilent"]);
        return TrueTurnCount switch
        {
            1 or 2 or 3 or 6 or 7 or 8 => new BattleCommand(this, SelectTarget(), Skills["DRKDarkAttack"]),
            4 or 5 or 9 or 10 => new BattleCommand(this, SelectTarget(), Skills["DRKDoNothing2"]),
            _ => new BattleCommand(this, SelectTarget(), Skills["DRKDoNothing1"])
        };
    }

    private bool FinalAttackPending;

    public override async Task OnStartOfBattle()
    {
        AnimationManager.Instance.TintScreen(-255, -255, -255, 0);
        AddStatModifier("Immune", silent: true);
        AddBattlebackLayer(3, "battleback_bs_fear_heights");
        FearPages.Sunny.SetAttackSkill("SRWFearAttack");
        StorySwitches.HeightsDone = true;
        StorySwitches.SpidersDone = false;
        StorySwitches.DrowningDone = false;
        StorySwitches.BlackSpaceWorld = true;
        AnimationManager.Instance.ShowPicture(2, "dark_overlay 2", new Vector2(320, 240), true, new Vector2(1.25f, 1.25f), 160);
        await AnimationManager.Instance.WaitForTintScreen(0, 0, 0, 0, 180 / 60f);
    }

    public override async Task ProcessStartOfCommands()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        if (BattleManager.Instance.TurnCount == 1)
            FadeBattlebackLayer(3, 0f, 2400);
        if (BattleManager.Instance.TurnCount != 9)
            return;
        FearPages.DimIn();
        await Wait.Frames(25);
        await FearPages.Voice($@"Remember, {FearPages.SunnyName}...\! All you need to do is CALM DOWN...");
        await Wait.Frames(25);
        await FearPages.DimOut();
    }

    public override async Task ProcessEndOfTurn()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        int turn = BattleManager.Instance.TurnCount;
        PartyMember sunny = FearPages.Sunny;
        if (turn == 3)
        {
            AnimationManager.Instance.MovePicture(2, new Vector2(320, 240), new Vector2(1.25f, 1.25f), 200, 30);
            FearPages.DimIn();
            await Wait.Frames(25);
            await FearPages.Voice($"{FearPages.SunnyName}...");
            await Wait.Frames(25);
            AnimationManager.Instance.MovePicture(2, new Vector2(320, 240), new Vector2(1.25f, 1.25f), 160, 30);
            await FearPages.DimOut();
        }
        if (turn == 5)
        {
            await Wait.Frames(25);
            sunny.LearnSkill("CalmDown");
            FearPages.DimIn();
            await Wait.Frames(25);
            await FearPages.Voice(@"Take a deep breath...\![br]Don't be afraid.\! It's not as scary as you think.");
            await Wait.Frames(45);
            await FearPages.Narrate("You steady yourself... and remember how to CALM DOWN.");
            sunny.EquipSkill(0, "CalmDown");
            await Wait.Frames(25);
            await FearPages.DimOut();
        }
        if (turn >= 5 && sunny.HasStatModifier("CalmDownText"))
        {
            await FearPages.FadeAway(this);
            StorySwitches.HeightsDone = true;
            FearPages.RestoreAttack();
            await FearPages.End(this);
            return;
        }
        if (turn == 10)
        {
            await Wait.Frames(45);
            BattleManager.Instance.ForceCommand(this, BattleManager.Instance.SmoothPartyTarget(LastTargetIndex), Skills["DRKFinalAttack"]);
            FinalAttackPending = true;
        }
        await FearPages.PlayerDownPage(this);
    }

    public override async Task ProcessBattleConditions()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        if (FinalAttackPending && BattleManager.Instance.GetCurrentCommand()?.Actor == this)
        {
            FinalAttackPending = false;
            await Wait.Frames(60);
            FadeOutBGS(4f);
            FadeBGM(0.001f, 4f);
            AnimationManager.Instance.ShowPicture(1, "black_screen", Vector2.Zero, false, Vector2.One, 0);
            await AnimationManager.Instance.WaitForMovePicture(1, Vector2.Zero, Vector2.One, 255, 120);
            await Wait.Frames(180);
            await FearPages.FadeOutScreen();
            AnimationManager.Instance.ErasePicture(2);
            FearPages.DefeatParty();
        }
        await FearPages.PlayerDownPage(this);
    }
}
