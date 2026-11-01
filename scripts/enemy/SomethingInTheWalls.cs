using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SomethingInTheWalls : Enemy
{
    public override string Name => "SOMETHING";
    public override Vector2 InfoBoxOffset => new(0, -125);
    public override bool BlinksWhenSelected => false;
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/fear_of_spiders.tres");
    protected override Stats Stats => new(9000, 4500, 10, 10, 100, 10, 1000);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    protected override string[] EquippedSkills => ["WALSpiderWebs", "WALDoNothing", "WALAttack"];

    public override BattleCommand ProcessAI()
    {
        return TrueTurnCount switch
        {
            1 or 5 or 9 => new BattleCommand(this, SelectTarget(), Skills["WALSpiderWebs"]),
            2 or 6 or 7 => new BattleCommand(this, this, Skills["WALDoNothing"]),
            3 or 4 or 8 or >= 10 => new BattleCommand(this, SelectTarget(), Skills["WALAttack"]),
            _ => null
        };
    }

    public override async Task OnStartOfBattle()
    {
        AnimationManager.Instance.TintScreen(-255, -255, -255, 0);
        PartyMember sunny = FearPages.Sunny;
        sunny.SeedLearnedSkills("CalmDown");
        sunny.EquipSkill(0, "CalmDown");
        for (int slot = 1; slot <= 4; slot++)
            sunny.UnequipSkill(slot);
        AnimationManager.Instance.ShowPicture(2, "dark_overlay 2", Vector2.Zero, false, Vector2.One, 180);
        sunny.LearnSkill("CalmDown");
        sunny.EquipSkill(0, "CalmDown");
        sunny.SetAttackSkill("SRWFearAttack");
        StorySwitches.HeightsDone = true;
        StorySwitches.SpidersDone = false;
        StorySwitches.DrowningDone = false;
        StorySwitches.BlackSpaceWorld = true;
        AddStatModifier("Immune", silent: true);
        await Wait.Frames(1);
        await Wait.Frames(60);
        await AnimationManager.Instance.WaitForTintScreen(0, 0, 0, 0, 3f);
        await Wait.Frames(60);
        sunny.EquipSkill(0, "CalmDown");
    }

    public override async Task ProcessEndOfTurn()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        int turn = BattleManager.Instance.TurnCount;
        PartyMember sunny = FearPages.Sunny;
        if (turn >= 1 && sunny.HasStatModifier("FocusText"))
        {
            await FearPages.FadeOutScreen();
            FadeBattlebackLayer(1, 1f, 60);
            await Wait.Frames(20);
            FearPages.RestoreAttack();
            await FearPages.End(this);
            return;
        }
        if (turn == 3)
        {
            AnimationManager.Instance.MovePicture(2, Vector2.Zero, Vector2.One, 200, 30);
            FearPages.DimIn();
            await Wait.Frames(25);
            await DialogueManager.Instance.ShowMessage("???", $"This is getting nowhere fast... huh, {FearPages.SunnyName}?");
            await Wait.Frames(25);
            AnimationManager.Instance.MovePicture(2, Vector2.Zero, Vector2.One, 160, 30);
            await FearPages.DimOut();
        }
        if (turn == 5)
        {
            sunny.LearnSkill("Focus");
            FearPages.DimIn();
            await Wait.Frames(25);
            await DialogueManager.Instance.ShowMessage("???", "It seems like there's a lot going on...");
            await DialogueManager.Instance.ShowMessage("???", @"You need to block out the little things...\! and figure out what's important!");
            await Wait.Frames(45);
            await DialogueManager.Instance.ShowMessage("You clear your mind... and remember how to FOCUS.");
            sunny.EquipSkill(1, "Focus");
            await Wait.Frames(25);
            await FearPages.DimOut();
        }
        // VANILLA BUG: SOMETHING takes no damage, so this turn-9 hint never shows
        if (turn == 9 && IsBelowHP(0.99f))
        {
            FearPages.DimIn();
            await Wait.Frames(25);
            await DialogueManager.Instance.ShowMessage("???", @"It's hard to FOCUS when you're all riled up.\! Remember to take a deep breath...");
            await Wait.Frames(25);
            await FearPages.DimOut();
        }
        await FearPages.PlayerDownPage(this);
    }

    public override async Task ProcessBattleConditions()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        await FearPages.PlayerDownPage(this);
    }
}
