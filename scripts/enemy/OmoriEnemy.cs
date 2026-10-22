using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Actors;

internal sealed class OmoriEnemy : Enemy
{
    public override string Name => "OMORI";
    public override Vector2 InfoBoxOffset => new(0, -217);
    public override bool InfoBoxCursorAboveBox => true;
    public override bool BlinksWhenSelected => false;
    public override Vector2 DamageNumberOffset => new(0, -50);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>($"res://animations/omori_enemy_{Form}.tres");
    protected override Stats Stats => new(255, 0, 54, 52, 256, 10, 1000);
    protected override string[] EquippedSkills => ["OMEAttack", "OMEHackAway", "OMEStab", "OMERedHands", "OMERedHandAttack", "OMEHairAttack", "OMEMariAttack", "OMEFinalAttack"];

    internal int Phase { get; set; } = 1;
    internal int Form => Math.Min(Phase, 6);
    internal int TauntStep { get; set; }
    internal bool SaidSeventy { get; set; }
    internal bool SaidThirty { get; set; }
    internal bool StunArmed { get; set; }
    internal bool ForcedPending { get; set; }
    internal bool StunRanThisTurn { get; set; }

    private static readonly Dictionary<int, string[][]> Taunts = new()
    {
        [3] =
        [
            ["You tell yourself that you don't want to burden others...", @"But the truth is that you're selfish.\! You just don't want people to depend on you."],
            [@"When do you think about others?\! How long are you going to let people take care of you?", @"You say you care but you're a liar.\! You've never done anything for anyone else.", @"You're useless...\| less than useless.\![br]You're sick."],
            ["People like you don't deserve to live."]
        ],
        [4] =
        [
            [@"Your friends are wrong about you.\! The person they love isn't you at all.", "You let them believe in a lie to protect yourself."],
            [@"You're nothing but a liar...\! and when they see the truth...", "They'll hate you as much as you hate yourself."],
            ["If they know the truth, you'll never be able to regain their trust.", "No matter what you do, it will be hopeless.", @"All you'll do is make things worse.\! It would be better to just die."],
            ["It would be better to just die."]
        ],
        [5] =
        [
            [@"You killed MARI.\! She loved you and you killed her."],
            ["HERO loved her and you killed her."],
            ["AUBREY loved her and you killed her."],
            ["KEL loved her and you killed her."],
            ["BASIL loved her and you killed her."],
            ["You loved her and you killed her."]
        ]
    };

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    public override BattleCommand ProcessAI()
    {
        switch (Phase)
        {
            case 1:
                return TrueTurnCount switch
                {
                    2 or 5 => new BattleCommand(this, SelectTarget(), Skills["OMEStab"]),
                    3 or 4 => new BattleCommand(this, SelectAllTargets(), Skills["OMEHackAway"]),
                    _ => new BattleCommand(this, SelectTarget(), Skills["OMEAttack"])
                };
            case 2:
                if (Roll() < 51 && SelectTargetAtOrAboveHP(0.65f) != null)
                    return new BattleCommand(this, SelectTargets(4), Skills["OMERedHands"]);
                return StabHackAwayOrAttack();
            case 3:
                if (Roll() < 51 && SelectTargetAtOrAboveHP(0.65f) is PartyMember redHandTarget)
                    return new BattleCommand(this, redHandTarget, Skills["OMERedHandAttack"]);
                if (Roll() < 41)
                    return new BattleCommand(this, SelectTarget(), Skills["OMEStab"]);
                return new BattleCommand(this, SelectTarget(), Skills["OMEAttack"]);
            case 4:
                if (Roll() < 56 && SelectTargetAtOrAboveHP(0.6f) is PartyMember hairTarget)
                    return new BattleCommand(this, hairTarget, Skills["OMEHairAttack"]);
                return StabHackAwayOrAttack();
            case 5:
                if (Roll() < 51 && SelectTargetAtOrAboveHP(0.65f) is PartyMember mariTarget)
                    return new BattleCommand(this, mariTarget, Skills["OMEMariAttack"]);
                return StabHackAwayOrAttack();
            default:
                return new BattleCommand(this, SelectTarget(), Skills["OMEFinalAttack"]);
        }
    }

    private BattleCommand StabHackAwayOrAttack()
    {
        if (Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["OMEStab"]);
        if (Roll() < 21)
            return new BattleCommand(this, SelectAllTargets(), Skills["OMEHackAway"]);
        return new BattleCommand(this, SelectTarget(), Skills["OMEAttack"]);
    }

    public override Task OnStartOfBattle()
    {
        Phase = 1;
        UpdateSprite();
        BattleManager.Instance.GetSunnyStandIn().AddStatModifier("ReleaseEnergyBasilBonus", silent: true);
        AddImmortal();
        return Task.CompletedTask;
    }

    internal static bool SealsCherish()
    {
        return BattleManager.Instance.GetAllAliveEnemies().Any(x => x is OmoriEnemy { Phase: <= 4 });
    }

    public override Task ProcessStartOfTurn()
    {
        StunRanThisTurn = false;
        if (StunArmed)
        {
            StunAllEnemies();
            StunRanThisTurn = true;
        }
        return Task.CompletedTask;
    }

    public override async Task ProcessEndOfTurn()
    {
        if (Phase == 2)
        {
            if (!SaidSeventy && IsBelowHP(0.7f))
            {
                await ShowJagged(@"You've caused so much suffering...\|[br]yet you do nothing.", "And so you've earned nothing in return.");
                SaidSeventy = true;
            }
            if (!SaidThirty && IsBelowHP(0.3f))
            {
                await ShowJagged("Your friends will never forgive you.", @"They'll abandon you like you did them...\! and that's what you deserve.");
                SaidThirty = true;
            }
        }

        await Taunt();

        if (IsBelowHP(0.01f))
        {
            TauntStep = 1;
            await Transition();
        }

        if (StunArmed && !StunRanThisTurn)
        {
            StunAllEnemies();
            StunRanThisTurn = true;
        }

        if (!StunArmed)
            return;
        if (ForcedPending)
        {
            ForceSignatureAttack();
            StunArmed = false;
            ForcedPending = false;
        }
        else
        {
            ForcedPending = true;
        }
    }

    private async Task Taunt()
    {
        if (Phase >= 6)
        {
            await ShowJagged("You should just die.");
            return;
        }
        if (!Taunts.TryGetValue(Phase, out string[][] steps) || TauntStep < 1 || TauntStep > steps.Length)
            return;
        await ShowJagged(steps[TauntStep - 1]);
        // VANILLA BUG: the last taunt never advances the step, so it repeats at every turn end
        if (TauntStep < steps.Length)
        {
            TauntStep++;
        }
        else if (Phase == 5)
        {
            foreach (PartyMember member in BattleManager.Instance.GetAllPartyMembers())
                member.ReplaceSkill("CalmDown", "BrokenCalmDown");
        }
    }

    private static async Task ShowJagged(params string[] lines)
    {
        foreach (string line in lines)
            DialogueManager.Instance.QueueMessage(line, font: DialogueManager.FontType.Jagged);
        await DialogueManager.Instance.WaitForDialogue();
        await Wait.Frames(10);
    }

    private async Task Transition()
    {
        bool leavingFirst = Phase == 1;
        bool stuns = Phase < 7;
        await AnimationManager.Instance.WaitForTintOverlay(Colors.Black, 30 / 60f);
        if (leavingFirst)
            BattleManager.Instance.GetSunnyStandIn().RemoveStatModifier("ReleaseEnergyBasilBonus", true);
        foreach (PartyMember member in BattleManager.Instance.GetAllPartyMembers())
        {
            member.RemoveStatModifier("CherishSeal", true);
            member.ClearResult();
        }
        if (Phase < 7)
        {
            Phase++;
            AddPhaseLayers();
        }
        RecoverAll();
        UpdateSprite();
        await Wait.Frames(45);
        AudioManager.Instance.PlaySFX("BA_ear_ringing", volume: 0.9f);
        _ = AnimationManager.Instance.FadeSuccumb(leavingFirst, 1f, 60 / 60f);
        await Wait.Frames(120);
        DialogueManager.Instance.QueueMessage(Phase <= 4 ? "OMORI did not succumb." : "OMORI will not succumb.");
        await DialogueManager.Instance.WaitForDialogue();
        _ = AnimationManager.Instance.FadeSuccumb(leavingFirst, 0f, 1f);
        await Wait.Frames(60);
        await Wait.Frames(50);
        await AnimationManager.Instance.WaitForTintOverlay(ColorsExtension.TransparentBlack, 30 / 60f);
        if (stuns)
            StunArmed = true;
        AddImmortal();
    }

    private void AddPhaseLayers()
    {
        switch (Phase)
        {
            case 3:
                AddBattlebackLayer(4, "battleback_omori_front");
                AddBattlebackLayer(3, "battleback_omori_back");
                ScrollBattlebackLayer(3, new Vector2(0, -60));
                break;
            case 4:
                AddBattlebackLayer(5, "battleback_omori_form_4");
                RemoveBattlebackLayer(4);
                break;
            case 5:
                AddBattlebackLayer(6, "battleback_omori_form_5");
                RemoveBattlebackLayer(5);
                break;
            case 6:
                AddBattlebackLayer(7, "battleback_omori_form_6");
                RemoveBattlebackLayer(6);
                break;
        }
    }

    private static void StunAllEnemies()
    {
        foreach (Enemy enemy in BattleManager.Instance.GetAllAliveEnemies())
        {
            enemy.AddStatModifier("CallForFriendDelay", silent: true);
            enemy.ClearResult();
        }
    }

    private void ForceSignatureAttack()
    {
        if (Phase == 2)
        {
            BattleManager.Instance.ForceCommand(this, SelectTargets(4), Skills["OMERedHands"]);
            return;
        }
        string skill = Phase switch
        {
            3 => "OMERedHandAttack",
            4 => "OMEHairAttack",
            5 => "OMEMariAttack",
            6 => "OMEFinalAttack",
            _ => null
        };
        if (skill != null)
            BattleManager.Instance.ForceCommand(this, BattleManager.Instance.SmoothPartyTarget(LastTargetIndex), Skills[skill]);
    }

    internal void RestoreForDeathBeat()
    {
        RecoverAll();
        AddImmortal();
        UpdateSprite();
        TauntStep = 0;
        SaidSeventy = false;
        SaidThirty = false;
        StunArmed = false;
        ForcedPending = false;
    }

    internal void RebuildLayersForDeathBeat()
    {
        ClearBattlebackLayers();
        if (Phase >= 6)
            AddBattlebackLayer(7, "battleback_omori_form_6");
        else
            AddBattlebackLayer(6, "battleback_omori_form_5");
        AddBattlebackLayer(3, "battleback_omori_back");
        ScrollBattlebackLayer(3, new Vector2(0, -60));
    }
    
    internal void UpdateSprite()
    {
        // change sprite on phase change
        Sprite.SpriteFrames = Animation;
        Sprite.Animation = "neutral";
        Sprite.Play();
    }
}