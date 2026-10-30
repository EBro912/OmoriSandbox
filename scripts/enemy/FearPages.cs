using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Actors;

// common shared behavior in the fear fight's troop events
internal static class FearPages
{
    private static readonly ConditionalWeakTable<Enemy, object> PlayerDownRan = new();

    private static readonly string[] BrokenSkills = ["BrokenCalmDownBS", "BrokenFocusBS", "BrokenPersistBS"];

    internal static PartyMember Sunny => BattleManager.Instance.GetSunnyStandIn();

    internal static string SunnyName => Sunny.Name.ToUpper();

    internal static async Task Voice(string text)
    {
        DialogueManager.Instance.QueueMessage("???", text);
        await DialogueManager.Instance.WaitForDialogue();
    }

    internal static async Task Narrate(string text)
    {
        DialogueManager.Instance.QueueMessage(text);
        await DialogueManager.Instance.WaitForDialogue();
    }

    internal static void DimIn()
    {
        AnimationManager.Instance.ShowPicture(3, "black_screen", Vector2.Zero, false, Vector2.One, 0);
        AnimationManager.Instance.MovePicture(3, Vector2.Zero, Vector2.One, 75, 30);
    }

    internal static async Task DimOut()
    {
        await AnimationManager.Instance.WaitForMovePicture(3, Vector2.Zero, Vector2.One, 0, 30);
        AnimationManager.Instance.ErasePicture(3);
    }

    internal static Task FadeOutScreen()
    {
        return AnimationManager.Instance.WaitForTintOverlay(Colors.Black, 24 / 60f, 22);
    }

    internal static Task FadeInScreen()
    {
        return AnimationManager.Instance.WaitForTintOverlay(ColorsExtension.TransparentBlack, 24 / 60f, 22);
    }

    internal static async Task BlackStart(int gray)
    {
        AnimationManager animation = AnimationManager.Instance;
        animation.TintScreen(-255, -255, -255, gray);
        animation.TintOverlay(Colors.Black, 0f, 22);
        Task sceneFade = FadeInScreen();
        animation.TintScreen(0, 0, 0, 0, 8 / 60f);
        await sceneFade;
    }

    internal static void BlackFlash(int frames)
    {
        AnimationManager.Instance.TintOverlay(Colors.Black);
        AnimationManager.Instance.TintOverlay(ColorsExtension.TransparentBlack, frames / 60f);
    }

    internal static Task FadeOutBGM(Enemy owner, float seconds)
    {
        if (!owner.CanChangeBGM)
            return Task.CompletedTask;
        bool playing = AudioManager.Instance.BGM.Playing;
        Task fade = AudioManager.Instance.WaitForFadeBGMTo(0.001f, seconds);
        return playing ? fade : Task.CompletedTask;
    }

    internal static async Task FadeAway(Enemy owner)
    {
        Task bgm = FadeOutBGM(owner, 5f);
        owner.FadeOutBGS(5f);
        await FadeOutScreen();
        AnimationManager.Instance.ErasePicture(2);
        await Wait.Frames(180);
        await bgm;
    }

    internal static void RestoreAttack()
    {
        PartyMember sunny = Sunny;
        sunny.SetAttackSkill(sunny.EquippedSkills[0]);
    }

    internal static async Task End(Enemy owner)
    {
        if (!await owner.AbortBattle())
            await BattleManager.Instance.RestoreAfterScriptedEnding();
    }

    internal static async Task PlayerDownPage(Enemy owner)
    {
        PartyMember sunny = Sunny;
        if (sunny.CurrentHP > 0 || sunny.HasStatModifier("Immortal") || PlayerDownRan.TryGetValue(owner, out _)
            || !BattleManager.Instance.GetAlivePartyMembers().Exists(member => member != sunny))
            return;
        PlayerDownRan.Add(owner, owner);
        await Wait.Frames(30);
        AnimationManager.Instance.TintScreen(-255, -255, -255, 0, 3f);
        Task tone = Wait.Frames(180);
        if (AudioManager.Instance.BGM.Playing && owner.CanChangeBGM)
        {
            owner.FadeBGM(0.001f, 3f);
            await Wait.Seconds(3f);
        }
        await tone;
        DefeatParty();
    }

    internal static void DefeatParty()
    {
        foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
        {
            member.CurrentHP = 0;
            member.SetToast();
            member.ClearStatModifiersOnDeath();
        }
    }

    internal static void AddState(Actor actor, string key)
    {
        if (actor.IsToast)
            return;
        actor.AddStatModifier(key, silent: true);
        actor.ClearResult();
    }

    internal static void LearnBrokenSkills(PartyMember standIn)
    {
        for (int slot = 0; slot < BrokenSkills.Length; slot++)
        {
            standIn.LearnSkill(BrokenSkills[slot]);
            standIn.EquipSkill(slot, BrokenSkills[slot]);
        }
    }
}
