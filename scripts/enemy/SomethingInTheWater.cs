using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SomethingInTheWater : Enemy
{
    public override string Name => "SOMETHING";
    public override Vector2 InfoBoxOffset => new(0, -350);
    public override bool BlinksWhenSelected => false;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>($"res://animations/fear_of_drowning_{Form}.tres");
    protected override Stats Stats => new(9000, 4500, 10, 10, 900, 10, 1000);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    protected override string[] EquippedSkills => ["WTRDoNothing", "WTRAttack", "WTRDragDown"];

    public override BattleCommand ProcessAI()
    {
        return TrueTurnCount switch
        {
            >= 1 and <= 5 => new BattleCommand(this, SelectTarget(), Skills["WTRDoNothing"]),
            >= 6 => new BattleCommand(this, SelectTarget(), Skills["WTRAttack"]),
            _ => null
        };
    }

    private static bool DrowningFirstAttack;
    private static int FearEffect;
    private static bool PersistGranted;
    private bool DragDownPending;

    internal int Form => HasStatModifier("Drowning3") ? 3 : HasStatModifier("Drowning2") ? 2 : 1;

    public override async Task OnStartOfBattle()
    {
        DrowningFirstAttack = false;
        FearEffect = 0;
        PersistGranted = false;
        AnimationManager.Instance.TintScreen(-255, -255, -255, 255);
        AddStatModifier("Immune", silent: true);
        PartyMember sunny = FearPages.Sunny;
        sunny.SeedLearnedSkills("CalmDown", "Focus");
        for (int slot = 0; slot <= 4; slot++)
            sunny.UnequipSkill(slot);
        AnimationManager.Instance.TintScreen(-255, -255, -255, 255, 1 / 60f);
        AnimationManager.Instance.ShowPicture(2, "dark_overlay 2", Vector2.Zero, false, Vector2.One, 0);
        AnimationManager.Instance.MovePicture(2, Vector2.Zero, Vector2.One, 200, 200);
        sunny.LearnSkill("CalmDown");
        sunny.EquipSkill(0, "CalmDown");
        sunny.LearnSkill("Focus");
        sunny.EquipSkill(1, "Focus");
        await Wait.Frames(30);
        sunny.SetAttackSkill("SRWFearAttack");
        await Wait.Frames(10);
        AnimationManager.Instance.TintScreen(-255, -255, -255, 0, 1f);
        await Wait.Frames(60);
        StorySwitches.HeightsDone = true;
        StorySwitches.SpidersDone = true;
        StorySwitches.DrowningDone = false;
        StorySwitches.BlackSpaceWorld = true;
        sunny.AddStatModifier("Immobile", 2, silent: true);
        sunny.ClearResult();
        sunny.AddStatModifier("Drowning3", silent: true);
        sunny.ClearResult();
        AnimationManager.Instance.InitShake(new Shake(2, 6, 15));
        AudioManager.Instance.PlaySFX("BA_angry_stage_2", 0.5f, 0.05f);
        FearPages.BlackFlash(60);
        BattleManager.Instance.ForceCommand(this, BattleManager.Instance.SmoothPartyTarget(LastTargetIndex), Skills["WTRDragDown"]);
        DragDownPending = true;
    }

    public override async Task ProcessStartOfCommands()
    {
        if (BattleManager.Instance.AbortRequested || BattleManager.Instance.TurnCount != 1)
            return;
        await Wait.Frames(120);
        await FearPages.FadeInScreen();
        await AnimationManager.Instance.WaitForTintScreen(0, 0, 0, 0, 3f);
    }

    public override async Task ProcessEndOfTurn()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        int turn = BattleManager.Instance.TurnCount;
        PartyMember sunny = FearPages.Sunny;
        if (turn >= 1)
        {
            if (sunny.HasStatModifier("Drowning3"))
            {
                if (sunny.HasStatModifier("PersistCheck"))
                {
                    FearPages.BlackFlash(60);
                    Shift(sunny, null, "PersistCheck");
                    AnimationManager.Instance.MovePicture(2, Vector2.Zero, Vector2.One, 160, 120);
                }
                ChangeBattlebackLayer(3, "battleback_bs_fear_drowning_3");
                AddBattlebackLayer(3, "battleback_bs_fear_drowning_3");
                Shift(this, "Drowning3", "Drowning1", "Drowning2");
            }
            if (sunny.HasStatModifier("Drowning2"))
            {
                if (sunny.HasStatModifier("PersistCheck"))
                {
                    await AnimationManager.Instance.WaitForTintScreen(-255, -255, -255, 0, 1f);
                    Shift(sunny, null, "PersistCheck");
                    AnimationManager.Instance.MovePicture(2, Vector2.Zero, Vector2.One, 90, 120);
                    await Wait.Frames(60);
                    AnimationManager.Instance.TintScreen(0, 0, 0, 0, 150 / 60f);
                }
                ChangeBattlebackLayer(3, "battleback_bs_fear_drowning_2");
                AddBattlebackLayer(3, "battleback_bs_fear_drowning_2");
                Shift(this, "Drowning2", "Drowning1", "Drowning3");
                // VANILLA BUG: (Check) Persist was removed above, so this line never shows
                if (sunny.HasStatModifier("PersistCheck"))
                {
                    await Wait.Frames(50);
                    await FearPages.Narrate("You push through the darkness and swim higher...");
                }
            }
            if (sunny.HasStatModifier("Drowning1"))
            {
                if (sunny.HasStatModifier("PersistCheck"))
                {
                    await AnimationManager.Instance.WaitForTintScreen(-255, -255, -255, 0, 1f);
                    Shift(sunny, null, "PersistCheck");
                    AnimationManager.Instance.MovePicture(2, Vector2.Zero, Vector2.One, 40, 120);
                    await Wait.Frames(60);
                    AnimationManager.Instance.TintScreen(0, 0, 0, 0, 150 / 60f);
                }
                RemoveBattlebackLayer(3);
                Shift(this, "Drowning1", "Drowning3", "Drowning2");
                // VANILLA BUG: (Check) Persist was removed above, so this line never shows
                if (sunny.HasStatModifier("PersistCheck"))
                {
                    await Wait.Frames(50);
                    await FearPages.Narrate("You push through the darkness and swim higher...");
                }
            }
            if (sunny.HasStatModifier("DrowningFinish"))
            {
                await FearPages.FadeAway(this);
                StorySwitches.DrowningDone = true;
                FearPages.RestoreAttack();
                await FearPages.End(this);
                return;
            }
        }
        if (turn == 3)
            await FearPages.Voice(@"Steady your heartbeat...\![br]Don't be afraid.\! It's not as scary as you think.");
        if (turn >= 4 && sunny.CurrentEmotion.Id != "afraid" && !PersistGranted)
        {
            sunny.LearnSkill("Persist");
            sunny.EquipSkill(2, "Persist");
            await FearPages.Voice(@"You have to keep going.\![br]Don't give up.\! No matter how impossible it seems...");
            await Wait.Frames(45);
            await FearPages.Narrate("You reach out to the voice... and remember how to PERSIST.");
            PersistGranted = true;
            await Wait.Frames(25);
        }
        await FearPages.PlayerDownPage(this);
    }

    public override async Task ProcessBattleConditions()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        if (DragDownPending && BattleManager.Instance.GetCurrentCommand()?.Actor == this)
        {
            DragDownPending = false;
            ChangeBattlebackLayer(3, "battleback_bs_fear_drowning_3");
            Shift(this, "Drowning3", "Drowning1", "Drowning2");
            AddStatModifier("Immobile", 2, silent: true);
            ClearResult();
        }
        await FearPages.PlayerDownPage(this);
    }

    internal async Task DragDownEvent()
    {
        PartyMember sunny = FearPages.Sunny;
        string level;
        if (!DrowningFirstAttack)
        {
            DrowningFirstAttack = true;
            level = "Drowning3";
        }
        else
        {
            if (FearEffect <= 3)
                FearEffect++;
            level = sunny.HasStatModifier("Drowning3") ? "Drowning3"
                : sunny.HasStatModifier("Drowning2") ? "Drowning2"
                : sunny.HasStatModifier("Drowning1") ? "Drowning1"
                : null;
        }
        switch (level)
        {
            case "Drowning3":
                ChangeBattlebackLayer(3, "battleback_bs_fear_drowning_3");
                AddBattlebackLayer(3, "battleback_bs_fear_drowning_3");
                Shift(this, "Drowning3", "Drowning1", "Drowning2");
                if (FearEffect == 3)
                    FearPages.BlackFlash(60);
                Shift(sunny, "Drowning3", "Drowning1", "Drowning2");
                break;
            case "Drowning2":
                ChangeBattlebackLayer(3, "battleback_bs_fear_drowning_2");
                AddBattlebackLayer(3, "battleback_bs_fear_drowning_2");
                Shift(this, "Drowning2", "Drowning1", "Drowning3");
                if (FearEffect == 2)
                    FearPages.BlackFlash(60);
                Shift(sunny, "Drowning2", "Drowning1", "Drowning3");
                break;
            case "Drowning1":
                RemoveBattlebackLayer(3);
                Shift(this, "Drowning1", "Drowning3", "Drowning2");
                if (FearEffect == 1)
                    FearPages.BlackFlash(60);
                Shift(sunny, "Drowning1", "Drowning2", "Drowning3");
                break;
            default:
                RemoveBattlebackLayer(3);
                Shift(sunny, null, "Drowning1", "Drowning2", "Drowning3");
                break;
        }
        await Wait.Frames(30);
        await FearPages.Narrate("SOMETHING drags you down...");
    }

    private void Shift(Actor actor, string add, params string[] remove)
    {
        foreach (string modifier in remove)
        {
            actor.RemoveStatModifier(modifier);
            actor.ClearResult();
        }
        if (add != null)
        {
            actor.AddStatModifier(add, silent: true);
            actor.ClearResult();
        }
        if (actor == this)
            UpdateForm();
    }

    private void UpdateForm()
    {
        SpriteFrames frames = Animation;
        if (Sprite.SpriteFrames == frames)
            return;
        Sprite.SpriteFrames = frames;
        Sprite.Animation = "neutral";
        Sprite.Play();
    }
}
