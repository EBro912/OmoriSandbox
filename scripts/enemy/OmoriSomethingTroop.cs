using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Menu;

namespace OmoriSandbox.Actors;

// handles the enemy swapping and troop event pages of the OmoriSomething fight
internal sealed class OmoriSomethingTroop
{
    private static readonly ConditionalWeakTable<Enemy, OmoriSomethingTroop> Troops = new();

    private static readonly (string Id, Vector2 Offset)[] Roster =
    [
        ("OmoriSomething (Hanging Body)", new Vector2(1f, -2.5f)),
        ("OmoriSomething (Blur)", new Vector2(1f, -2.5f)),
        ("OmoriSomething (Drowning)", new Vector2(1f, -2.5f)),
        ("OmoriSomething", Vector2.Zero),
        ("OmoriSomething (Spiders)", new Vector2(1f, -68.5f)),
        ("OmoriSomething", Vector2.Zero),
        ("OmoriSomething (Heights)", new Vector2(1f, -18.5f)),
        ("OmoriSomething", Vector2.Zero)
    ];

    private const int HostIndex = 7;

    private readonly Enemy[] LiveMembers = new Enemy[Roster.Length];
    private readonly Vector2 Anchor;
    private int LastTurnEnd = -1;

    private OmoriSomethingTroop(Enemy host)
    {
        Host = host;
        Anchor = host.CenterPoint;
        LiveMembers[HostIndex] = host;
        Troops.AddOrUpdate(host, this);
    }

    internal Enemy Host { get; }

    internal bool Phase1 { get; set; }

    internal bool Phase2 { get; set; }

    internal bool Phase3 { get; set; }

    internal bool BsSafe { get; set; }

    internal bool Page2Ran { get; set; }

    internal bool AttackDisabled { get; set; }

    internal int BattleTurn { get; set; }

    private static PartyMember StandIn => BattleManager.Instance.GetSunnyStandIn();

    internal static OmoriSomethingTroop Of(Enemy enemy)
    {
        return enemy != null && Troops.TryGetValue(enemy, out OmoriSomethingTroop troop) ? troop : null;
    }

    internal static async Task Begin(OmoriSomething host)
    {
        OmoriSomethingTroop troop = new(host);
        await FearPages.BlackStart(255);
        PartyMember standIn = StandIn;
        standIn.SeedLearnedSkills("CalmDown", "Focus", "Persist");
        for (int slot = 1; slot <= 4; slot++)
            standIn.UnequipSkill(slot);
        // VANILLA BUG: S1099 "Basil_Battle" is never cleared after the BASIL fight, so Release Stress at 10 ENERGY can soft-lock this fight
        StorySwitches.BasilBattle = true;
        StorySwitches.BlackSpaceWorld = true;
        StorySwitches.FearPhase = 0;
        await troop.Page1();
    }

    internal static Task RunTurnEnd(Enemy member)
    {
        if (BattleManager.Instance.AbortRequested || Of(member) == null)
            return Task.CompletedTask;
        return Of(member).TurnEnd();
    }

    internal static Task RunBattleConditions(Enemy member)
    {
        if (BattleManager.Instance.AbortRequested || Of(member) == null)
            return Task.CompletedTask;
        return FearPages.PlayerDownPage(Of(member).Host);
    }

    internal Enemy Member(int index)
    {
        return index >= 0 && index < LiveMembers.Length ? LiveMembers[index] : null;
    }

    internal Enemy Appear(int index)
    {
        if (index < 0 || index >= LiveMembers.Length)
            return null;
        if (LiveMembers[index] != null)
            return LiveMembers[index];
        BattlePresetEnemy entry = new()
        {
            Name = Roster[index].Id,
            Emotion = "neutral",
            FallsOffScreen = false,
            GrayscaleOnDefeat = false,
            Layer = Host.Layer,
            PreventBattlebackChanges = Host.PreventBattlebackChanges,
            PreventBGMChanges = Host.PreventBGMChanges
        };
        Enemy member = BattleManager.Instance.SummonEnemy(entry, Anchor + Roster[index].Offset)?.Actor;
        if (member == null)
            return null;
        LiveMembers[index] = member;
        Troops.AddOrUpdate(member, this);
        member.SetOpacity(0f);
        member.SetOpacity(1f, 16 / 60f);
        return member;
    }

    internal void Kill(int index)
    {
        Enemy member = Member(index);
        if (member == null || member.IsToast)
            return;
        member.CurrentHP = 0;
        member.SetToast();
        member.ClearStatModifiersOnDeath();
        if (member is OmoriSomethingHeights or OmoriSomethingSpiders or OmoriSomethingDrowning)
            member.SetOpacity(0f);
    }

    private bool IsAppeared(int index)
    {
        return Member(index) is { IsToast: false };
    }

    private Enemy AbortOwner()
    {
        foreach (Enemy member in LiveMembers)
        {
            if (member is { IsToast: false })
                return member;
        }
        return Host;
    }

    private async Task TurnEnd()
    {
        int turn = BattleManager.Instance.TurnCount;
        if (LastTurnEnd == turn)
            return;
        LastTurnEnd = turn;
        if (turn >= 1 && turn % 2 == 0 && Phase1 && !Page2Ran)
            await Page2();
        if (IsAppeared(6) && StandIn.HasStatModifier("CalmDownText"))
            await Page3();
        if (Phase2 && BattleTurn >= 2)
            await Page4();
        if (IsAppeared(4) && StandIn.HasStatModifier("FocusText"))
            await Page5();
        if (Phase3 && BattleTurn >= 2)
            await Page6();
        if (IsAppeared(2) && StandIn.HasStatModifier("PersistText"))
            await Page7();
        if (IsAppeared(1) && StandIn.HasStatModifier("FocusText"))
            await Page8();
        if (IsAppeared(0) && !BsSafe)
            await Page9();
        if (await Page10())
            return;
        Page11();
        await FearPages.PlayerDownPage(Host);
        if (AttackDisabled && !StandIn.HasStatModifier("Hanging"))
        {
            BattleManager.Instance.SetMenuOptionEnabled(BattleMenuOption.Attack, true);
            AttackDisabled = false;
        }
    }

    private async Task Page1()
    {
        PartyMember standIn = StandIn;
        Host.ChangeBGS("boss_something_heartbeat", 1f, 0.5f);
        StorySwitches.HeightsDone = true;
        StorySwitches.SpidersDone = true;
        StorySwitches.DrowningDone = true;
        standIn.LearnSkill("CalmDown");
        standIn.EquipSkill(0, "CalmDown");
        standIn.ForgetSkill("Focus");
        standIn.UnequipSkill(1);
        standIn.ForgetSkill("Persist");
        standIn.UnequipSkill(2);
        standIn.SetAttackSkill("SRWBlackSpaceAttack");
        await AnimationManager.Instance.WaitForTintScreen(0, 0, 0, 0, 180 / 60f);
        Phase1 = true;
    }

    private async Task Page2()
    {
        PartyMember standIn = StandIn;
        AnimationManager animation = AnimationManager.Instance;
        await FearPages.FadeOutScreen();
        FearPages.RemoveState(standIn, "CalmDownText");
        Kill(7);
        Appear(6);
        ChangeFear(standIn);
        animation.ShowPicture(2, "dark_overlay 2", Vector2.Zero, false, Vector2.One, 0);
        animation.MovePicture(2, Vector2.Zero, Vector2.One, 190, 90);
        Host.AddBattlebackLayer(3, "battleback_bs_fear_heights");
        await Wait.Frames(90);
        await FearPages.FadeInScreen();
        StorySwitches.FearPhase = 1;
        BattleTurn = 0;
        Phase1 = false;
        Page2Ran = true;
    }

    private async Task Page3()
    {
        PartyMember standIn = StandIn;
        await FearPages.FadeOutScreen();
        AnimationManager.Instance.ErasePicture(2);
        Kill(6);
        Appear(5);
        FearPages.RemoveState(standIn, "CalmDownText");
        Host.RemoveBattlebackLayer(3);
        await Wait.Frames(90);
        await FearPages.FadeInScreen();
        StorySwitches.FearPhase = 0;
        BattleTurn = 0;
        Phase2 = true;
        RaiseEnergy();
    }

    private async Task Page4()
    {
        PartyMember standIn = StandIn;
        AnimationManager animation = AnimationManager.Instance;
        await FearPages.FadeOutScreen();
        animation.ShowPicture(2, "dark_overlay 2", Vector2.Zero, false, Vector2.One, 0);
        animation.MovePicture(2, Vector2.Zero, Vector2.One, 190, 90);
        animation.ShowPicture(89, "something_FearofSpiders", new Vector2(320f, -1200f), true, new Vector2(1.5f, 1.5f), 255);
        animation.MovePicture(89, new Vector2(320f, 1250f), new Vector2(1.5f, 1.5f), 255, 30);
        await Wait.Frames(10);
        Kill(5);
        Host.AddBattlebackLayer(4, "battleback_bs_fear_spiders");
        ChangeFear(standIn);
        standIn.LearnSkill("Focus");
        standIn.EquipSkill(1, "Focus");
        await Wait.Frames(45);
        Appear(4);
        await Wait.Frames(90);
        await FearPages.FadeInScreen();
        StorySwitches.FearPhase = 2;
        animation.ErasePicture(89);
        BattleTurn = 0;
        Phase2 = false;
    }

    private async Task Page5()
    {
        PartyMember standIn = StandIn;
        await FearPages.FadeOutScreen();
        AnimationManager.Instance.ErasePicture(2);
        Kill(4);
        Appear(3);
        FearPages.RemoveState(standIn, "FocusText");
        Host.RemoveBattlebackLayer(4);
        await Wait.Frames(90);
        await FearPages.FadeInScreen();
        standIn.ForgetSkill("Focus");
        // VANILLA BUG: page 5 passes FOCUS's id as unequipSkill's re-learn flag, so FOCUS is re-learned and stays equipped until the drowning phase
        standIn.UnequipSkill(1, relearn: true);
        StorySwitches.FearPhase = 0;
        RaiseEnergy();
        Phase3 = true;
    }

    private async Task Page6()
    {
        PartyMember standIn = StandIn;
        AnimationManager animation = AnimationManager.Instance;
        await FearPages.FadeOutScreen();
        Kill(3);
        Appear(2);
        animation.TintScreen(-255, -255, -255, 0, 360 / 60f);
        animation.ShowPicture(2, "dark_overlay 2", Vector2.Zero, false, Vector2.One, 190);
        ChangeFear(standIn);
        Host.AddBattlebackLayer(5, "battleback_bs_fear_drowning_3");
        standIn.LearnSkill("Persist");
        standIn.EquipSkill(2, "Persist");
        Appear(2);
        await Wait.Frames(90);
        await FearPages.FadeInScreen();
        animation.TintScreen(0, 0, 0, 0, 90 / 60f);
        StorySwitches.FearPhase = 3;
        BattleTurn = 0;
        Phase3 = false;
    }

    private async Task Page7()
    {
        PartyMember standIn = StandIn;
        await FearPages.FadeOutScreen();
        AnimationManager.Instance.ErasePicture(2);
        Kill(2);
        Appear(1);
        Host.RemoveBattlebackLayer(5);
        FearPages.RemoveState(standIn, "PersistText");
        foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
        {
            member.SetEmotion("afraid", true);
            member.ClearResult();
        }
        await Wait.Frames(60);
        await FearPages.FadeInScreen();
        standIn.LearnSkill("Focus");
        standIn.EquipSkill(1, "Focus");
        standIn.ForgetSkill("Persist");
        standIn.UnequipSkill(2);
        standIn.UnequipSkill(3);
        standIn.UnequipSkill(4);
        standIn.UnequipSkill(5);
        StorySwitches.FearPhase = 0;
        RaiseEnergy();
    }

    private async Task Page8()
    {
        PartyMember standIn = StandIn;
        Kill(1);
        Appear(0);
        HangingModifier.SetStage(standIn, 1);
        standIn.ClearResult();
        BattleManager.Instance.SetMenuOptionEnabled(BattleMenuOption.Attack, false);
        AttackDisabled = true;
        BattleManager.Instance.BossDisabledAttack = true;
        FearPages.RemoveState(standIn, "FocusText");
        BattleManager.Instance.Energy = 0;
        await Wait.Frames(60);
        standIn.ForgetSkill("CalmDown");
        standIn.UnequipSkill(0);
        standIn.ForgetSkill("Focus");
        standIn.UnequipSkill(1);
        standIn.UnequipSkill(2);
        standIn.LearnSkill("Persist");
        standIn.EquipSkill(0, "Persist");
    }

    private async Task Page9()
    {
        AnimationManager animation = AnimationManager.Instance;
        animation.ShowPicture(2, "dark_overlay 2", Vector2.Zero, false, Vector2.One, 0);
        animation.MovePicture(2, Vector2.Zero, Vector2.One, 190, 90);
        await Wait.Frames(15);
        await DialogueManager.Instance.ShowMessage("You feel like your life is in danger.");
        BsSafe = true;
    }

    private async Task<bool> Page10()
    {
        PartyMember standIn = StandIn;
        if (standIn.HasStatModifier("HangingEndFight"))
        {
            Host.FadeOutBGS(1f);
            FearPages.RestoreAttack();
            Host.RemoveBattlebackLayer(5);
            await Wait.Frames(240);
            standIn.LearnSkill("CalmDown");
            standIn.EquipSkill(0, "CalmDown");
            standIn.LearnSkill("Focus");
            standIn.EquipSkill(1, "Focus");
            standIn.LearnSkill("Persist");
            standIn.EquipSkill(2, "Persist");
            standIn.ForgetSkill("Overcome");
            standIn.UnequipSkill(3);
            BsSafe = false;
            await FearPages.End(AbortOwner());
            return true;
        }
        int stage = HangingModifier.StageOf(standIn);
        if (stage == 13)
        {
            standIn.UnequipSkill(0);
            standIn.UnequipSkill(1);
            standIn.UnequipSkill(2);
            await Wait.Frames(2);
            standIn.ForgetSkill("CalmDown");
            await Wait.Frames(2);
            standIn.LearnSkill("Overcome");
            standIn.EquipSkill(3, "Overcome");
            await Wait.Frames(2);
        }
        else if (stage == 11)
        {
            standIn.ForgetSkill("Persist");
            standIn.UnequipSkill(2);
            await Wait.Frames(2);
            standIn.LearnSkill("CalmDown");
            standIn.EquipSkill(0, "CalmDown");
            await Wait.Frames(2);
        }
        return false;
    }

    private void Page11()
    {
        if (Phase1)
            BattleTurn++;
        if (Phase2)
            BattleTurn++;
        if (Phase3)
            BattleTurn++;
    }

    private static void ChangeFear(PartyMember standIn)
    {
        if (!standIn.IsToast && standIn.SetEmotion("afraid", true))
            BattleLogManager.Instance.QueueMessage(standIn, "[actor] feels AFRAID...");
    }

    private static void RaiseEnergy()
    {
        BattleManager.Instance.Energy = Math.Min(BattleManager.Instance.Energy + 3, 10);
    }
}
