using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class PlutoExpandedEarth : Enemy
{
    public override string Name => "PLUTO (EXPANDED)";
    public override Vector2 InfoBoxOffset => new(0, -235);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation =>
        ResourceLoader.Load<SpriteFrames>("res://animations/pluto_expanded.tres");
    protected override Stats Stats => new(10000, 5000, 85, 65, 70, 15, 95);
    protected override string[] EquippedSkills => ["PEAttack", "PESubmissionHold", "PEHeadbutt", "PEDoNothing", "PEEarthsFinale", "PEMeteor"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "angry" or "happy";
    }

    private EnemyComponent Earth;

    public override BattleCommand ProcessAI()
    {
        IReadOnlyList<PartyMember> party = SelectAllTargets();
        if (party.Any(x => x.HasStatModifier("PlutoBuff")))
            return new BattleCommand(this, party, Skills["PEMeteor"]);
        
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["PEHeadbutt"]);
        
        List<PartyMember> sad = party.Where(x => x.CurrentEmotion.Group?.Id == "sad").ToList();
        if (sad.Count > 0)
            return new BattleCommand(this, sad[GameManager.Instance.Random.RandiRange(0, sad.Count - 1)], Skills["PEHeadbutt"]);
        if (Roll() < 56)
            return new BattleCommand(this, SelectTarget(), Skills["PEAttack"]);
        if (Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["PESubmissionHold"]);
        if (Roll() < 31)
            return new BattleCommand(this, SelectTarget(), Skills["PEDoNothing"]);
        return new BattleCommand(this, SelectTarget(), Skills["PEHeadbutt"]);
    }

    public override async Task OnStartOfBattle()
    {
        Earth = BattleManager.Instance.SummonEnemy("TheEarth (Pluto)", CenterPoint + new Vector2(0, -100), layer: Layer + 1);
        DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"This will be our final fight.\! Show me everything you have.");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task ProcessStartOfCommands()
    {
        if (Charging)
        {
            int turns = GetStatModifierTurnsLeft("PlutoCharging");
            switch (turns)
            {
                case 2:
                    DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint,
                        @"I am glad to have met each of you...\! and watch you all grow.");
                    DialogueManager.Instance.QueueMessage("PLUTO continues charging his ultimate attack...");
                    await DialogueManager.Instance.WaitForDialogue();
                    AnimationManager.Instance.PlayAnimation(218, this);
                    break;
                case 1:
                    DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint,
                        @"I have recognized your strength...\! and will see you as children no longer.");
                    DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint,
                        @"This fight is mine to win...\! You cannot escape my judgement!");
                    DialogueManager.Instance.QueueMessage("PLUTO finishes charging his ultimate attack!");
                    await DialogueManager.Instance.WaitForDialogue();
                    foreach (PartyMember target in SelectAllTargets())
                        target.AddStatModifier("PlutoBuff");
                    break;
                default:
                    DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint,
                        @"I hope we meet again in the next life.\! Goodbye.");
                    await DialogueManager.Instance.WaitForDialogue();
                    Charging = false;
                    Stunned = false;
                    break;
            }
        }
    }

    private bool HasSpoken = false;
    private bool HasThrownEarth = false;
    private bool Charging = false;
    private bool HasMentionedFlex = false;
    private string WhoFlexed;
    public override async Task ProcessBattleConditions()
    {
        if (!HasMentionedFlex && WhoFlexed is null)
        {
            WhoFlexed = SelectAllTargets().FirstOrDefault(x => x.HasStatModifier("Flex"))?.Name;
        }
        
        if (CurrentHP <= 0)
            return;

        if (IsBelowHP(0.5f) && !HasThrownEarth)
        {
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"Ah...\! It seems that I have underestimated you once again.");
            await DialogueManager.Instance.WaitForDialogue();
            HasThrownEarth = true;
            if (Earth != null && !Earth.Actor.IsToast)
            {
                BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["PEEarthsFinale"]);
                return;
            }
        }
        
        if (IsBelowHP(0.5f) && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"Very few have pushed me this far...\! and none have left the same.");
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"I want nothing more than victory!\! Let me show you my resolve!");
            DialogueManager.Instance.QueueMessage("PLUTO begins charging his ultimate attack!");
            await DialogueManager.Instance.WaitForDialogue();
            AddStatModifier("PlutoCharging");
            AnimationManager.Instance.PlayAnimation(218, this);
            Charging = true;
            Stunned = true;
            HasSpoken = true;
        }
    }
    
    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"Unbelievable...\! Even at full power...\! I have been bested.");
        DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"It has been an honor to do battle with you.\! Your victory is well deserved.");
        await DialogueManager.Instance.WaitForDialogue();
        KillEarth();
    }

    public override async Task ProcessEndOfTurn()
    {
        if (!HasMentionedFlex && WhoFlexed != null)
        {
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint,
                $"Impressive progress, young {WhoFlexed.ToUpper()}! Your [color=#6095ff]FLEX[/color] has improved greatly!");
            await DialogueManager.Instance.WaitForDialogue();
            HasMentionedFlex = true;
        }
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"Do not be SAD.\! You were worthy opponents until the end.");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }

    internal void KillEarth()
    {
        if (Earth != null)
            Earth.Actor.CurrentHP = 0;
    }
}