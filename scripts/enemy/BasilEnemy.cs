using System;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class BasilEnemy : Enemy
{
    private bool EndingPlayed;

    public override string Name => "BASIL";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override Vector2 DamageNumberOffset => new(0, -150);
    public override bool InfoBoxCursorAboveBox => true;
    public override bool BlinksWhenSelected => false;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/basil_enemy.tres");
    protected override Stats Stats => new(150, 30, 4, 2, 6, 10, 200);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    protected override string[] EquippedSkills => ["BASDoNothing", "BASAttack", "BASSomethingSetUpPlayer", "BASBasilAttackPlayer"];

    public override async Task OnStartOfBattle()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        AnimationManager animation = AnimationManager.Instance;
        await FearPages.BlackStart(0);
        PartyMember standIn = FearPages.Sunny;
        standIn.SeedLearnedSkills("CalmDown", "Focus", "Persist");
        for (int slot = 0; slot <= 4; slot++)
            standIn.UnequipSkill(slot);
        FearPages.LearnBrokenSkills(standIn);
        foreach (PartyMember member in BattleManager.Instance.GetAllPartyMembers())
            FearPages.AddState(member, "BasilSomething");
        foreach (PartyMember member in BattleManager.Instance.GetAllPartyMembers())
            FearPages.AddState(member, "PersistentPlotArmor");
        standIn.ForgetSkill("Overcome");
        StorySwitches.BlackSpaceWorld = true;
        StorySwitches.BasilBattle = true;
        StorySwitches.BasilTroop = true;
        standIn.SetAttackSkill("SRWBasilAttack");
        FearPages.AddState(this, "BasilMarker");
        FearPages.LearnBrokenSkills(standIn);
        await animation.WaitForTintScreen(0, 0, 0, 0, 180 / 60f);
        await CheckEndings();
    }

    public override async Task ProcessStartOfTurn()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        await CheckEndings();
    }

    public override async Task ProcessStartOfCommands()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        switch (BattleManager.Instance.TurnCount)
        {
            case 3:
                // DIALOGUE QUIRK: message 239 has no line break after "BASIL:", so the name shares the first line (the instant drawing isn't ported)
                await TurnPage(FearPages.SunnyName + "...", "[br]There's no way out of this, is there?");
                break;
            case 4:
                await TurnPage($"[br]Tell me, {FearPages.SunnyName}...", "[br]Am I selfish for doing this?");
                break;
            case 5:
                await TurnPage($"[br]{FearPages.SunnyName}...", "[br]Do you hate me?");
                break;
            case 6:
                await Wait.Frames(30);
                await DialogueManager.Instance.ShowMessage(this, @"[br]Please...\! stop it...", DialogueManager.FontType.Jagged);
                await Wait.Frames(10);
                break;
            case 7:
                AddImmortal();
                await TurnPage($"[br]Stop struggling,[br]{FearPages.SunnyName}.", @"[br]I'm...\! I'm so tired...");
                break;
        }
    }

    public override async Task ProcessEndOfTurn()
    {
        BattleManager battle = BattleManager.Instance;
        if (battle.AbortRequested)
            return;
        if (battle.TurnCount >= 1 && FearPages.Sunny.CurrentEmotion.Id is not "stressed" && battle.Energy <= 9)
        {
            await Wait.Frames(40);
            await DialogueManager.Instance.ShowMessage("You want to run.");
            await Wait.Frames(30);
            AudioManager.Instance.PlaySFX("SE_basil_beep", 1f, 0.3f);
            battle.Energy += 1;
        }
        if (battle.TurnCount >= 99)
        {
            battle.ForceCommand(this, SelectAllTargets(), Skills["BASSomethingSetUpPlayer"]);
            return;
        }
        await CheckEndings();
    }

    public override async Task ProcessBattleConditions()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        await CheckEndings();
    }

    public override BattleCommand ProcessAI()
    {
        return TrueTurnCount switch
        {
            0 => null,
            1 => new BattleCommand(this, this, Skills["BASDoNothing"]),
            _ => new BattleCommand(this, SelectTarget(), Skills["BASAttack"])
        };
    }

    private async Task TurnPage(string first, string second)
    {
        await Wait.Frames(30);
        await DialogueManager.Instance.ShowMessage(this, first, DialogueManager.FontType.Jagged);
        await Wait.Frames(60);
        await DialogueManager.Instance.ShowMessage(this, second, DialogueManager.FontType.Jagged);
        await Wait.Frames(40);
    }

    private async Task<bool> CheckEndings()
    {
        if (BattleManager.Instance.AbortRequested || EndingPlayed)
            return false;
        if (IsBelowHP(0.01f))
        {
            await PlayEnding(false);
            return true;
        }
        if (FearPages.Sunny.IsBelowHP(0.02f))
        {
            await PlayEnding(true);
            return true;
        }
        return false;
    }

    private async Task PlayEnding(bool sunnyWornDown)
    {
        EndingPlayed = true;
        BattleLogManager.Instance.ClearBattleLog();
        PartyMember sunny = FearPages.Sunny;
        FearPages.AddState(sunny, "Immortal");
        if (sunnyWornDown && !sunny.IsToast)
            sunny.CurrentHP = Math.Max(sunny.CurrentHP - 10, 1);
        await Wait.Frames(30);
        await DialogueManager.Instance.ShowMessage(this, @"[br][shake rate=60.0 level=20 connected=0]Why!?\! Why won't you stop!?[/shake]", DialogueManager.FontType.Jagged);
        await BasilSomethingEnding(sunny);
        FadeOutBGS(2f);
        FadeBGM(0.001f, 2f);
        AnimationManager animation = AnimationManager.Instance;
        animation.TintScreen(51, -255, -255, 255, 350 / 60f);
        await Wait.Frames(120);
        animation.ShowPicture(100, "black_screen", Vector2.Zero, false, Vector2.One, 0);
        await animation.WaitForMovePicture(100, Vector2.Zero, Vector2.One, 255, 300);
        await FearPages.FadeOutScreen();
        animation.ErasePicture(10);
        if (!sunny.IsToast && sunny.CurrentAnimation == "stabbed")
            sunny.ClearAnimation();
        await FearPages.End(this);
    }

    private async Task BasilSomethingEnding(PartyMember sunny)
    {
        AnimationManager animation = AnimationManager.Instance;
        await Wait.Frames(60);
        AudioManager.Instance.PlaySFX("BA_stab", 1f, 0.9f);
        animation.ShowPicture(7, "black_screen", Vector2.Zero, false, Vector2.One, 255);
        animation.ShowPicture(10, "blind_eye", Vector2.Zero, false, Vector2.One, 255);
        await Wait.Frames(60);
        if (!sunny.IsToast && sunny.Sprite?.SpriteFrames != null && sunny.Sprite.SpriteFrames.HasAnimation("stabbed"))
            sunny.PlayAnimation("stabbed");
        if (!sunny.IsToast)
        {
            sunny.CurrentHP = Math.Max(sunny.CurrentHP - 100, 1);
            sunny.SetEmotion("stressed", true);
            sunny.ClearResult();
        }
        await Wait.Frames(20);
        await FearPages.Flash(Color.Color8(255, 0, 0, 170), 60);
        await Wait.Frames(60);
        await animation.WaitForMovePicture(10, Vector2.Zero, Vector2.One, 255, 1);
        await animation.WaitForMovePicture(7, Vector2.Zero, Vector2.One, 0, 300);
        await Wait.Frames(20);
    }
}
