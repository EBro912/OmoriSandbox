using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Menu;

namespace OmoriSandbox.Actors;

internal sealed class Mari : Enemy
{
    public override string Name => "MARI";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override Vector2 DamageNumberOffset => new(0, -150);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/mari.tres");
    protected override Stats Stats => new(2300, 1500, 10, 143, 10, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["MARMariNothing", "MARPicnicBasket"];

    public override Task OnStartOfBattle()
    {
        AddImmortal();
        PartyMember omori = BattleManager.Instance.GetPartyMemberAtPosition(0);
        if (omori != null)
        {
            omori.AddStatModifier("Immortal", silent: true);
            omori.ClearResult();
        }
        return Task.CompletedTask;
    }

    private bool PageThreeRan;

    public override async Task ProcessEndOfTurn()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        if (PageThreeRan || !IsBelowHP(0.2f))
            return;
        PageThreeRan = true;
        await Transition();
    }

    private async Task Transition()
    {
        PartyMember omori = BattleManager.Instance.GetPartyMemberAtPosition(0);
        PartyMember basil = BattleManager.Instance.GetPartyMemberAtPosition(2);
        Enemy hanging = null;
        try
        {
            AnimationManager.Instance.SetGlitch(true);
            AudioManager.Instance.PlaySFX("SE_bs_record_scratch", 1f, 0.9f);
            StopBGM();
            ChangeBGS("boss_something_heartbeat", 0.35f);
            if (omori != null)
            {
                omori.AddStatModifier("OmoriAfraid", silent: true);
                omori.ClearResult();
            }
            if (basil != null)
            {
                basil.SetEmotion("afraid", silent: true);
                basil.AddStatModifier("BasilImmobile", silent: true);
                basil.ClearResult();
            }
            if (omori != null)
            {
                omori.EquipSkill(0, "BrokenCalmDown");
                omori.EquipSkill(1, "BrokenFocus");
                omori.EquipSkill(2, "BrokenPersist");
                omori.UnequipSkill(3);
            }
            await Wait.Frames(1);
            AddBattlebackLayer(3, "battleback_black_screen");
            StorySwitches.BlackSpaceWorld = true;
            MenuManager.Instance.ApplyBattleSkin("Blackspace", true);
            MenuManager.Instance.ShowButtons();
            hanging = BattleManager.Instance.TransformEnemy(this, "HangingBody (Mari)", offset: new Vector2(0, -27));
            await AnimationManager.Instance.WaitForTintScreen(0, 0, 0, 0, 1 / 60f);
        }
        finally
        {
            AnimationManager.Instance.SetGlitch(false);
        }
        if (hanging is HangingBodyMari body)
        {
            body.TroopStep = 1;
            await body.RunCounterPage();
        }
    }

    public override BattleCommand ProcessAI()
    {
        if (TrueTurnCount is 0 or > 6)
            return new BattleCommand(this, SelectTarget(), Skills["MARMariNothing"]);
        
        return new BattleCommand(this, SelectTarget(), TrueTurnCount % 2 == 0 ? Skills["MARPicnicBasket"] : Skills["MARMariNothing"]);
    }
}
