using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal sealed class PlutoExpandedEarth : Enemy
{
    public override string Name => "PLUTO (EXPANDED)";
    public override SpriteFrames Animation =>
        ResourceLoader.Load<SpriteFrames>("res://animations/pluto_expanded.tres");
    protected override Stats Stats => new(10000, 5000, 85, 65, 70, 15, 95);
    protected override string[] EquippedSkills => ["PEAttack", "PESubmissionHold", "PEHeadbutt", "PEDoNothing", "PEEarthsFinale", "PEMeteor"];

    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "hurt" || state == "toast" || state == "sad" || state == "angry" || state == "happy";
    }

    private EnemyComponent Earth;

    public override BattleCommand ProcessAI()
    {
        if (Charging)
        {
            int turns = GetStatModifierTurnsLeft("PlutoCharging");
            // use fake skills to show the charging dialogue
            // the effects are split into func objects to silence the compiler
            Func<Actor, Actor, Task> effect;
            switch (turns)
            {
                case 2:
                    effect = async (self, _) =>
                    {
                        DialogueManager.Instance.QueueMessage("PLUTO", self.CenterPoint,
                            @"I am glad to have met each of you...\! and watch you all grow.");
                        DialogueManager.Instance.QueueMessage("PLUTO", self.CenterPoint,
                            @"I have recognized your strength...\! and will see you as children no longer.");
                        DialogueManager.Instance.QueueMessage("PLUTO continues charging his ultimate attack...");
                        await DialogueManager.Instance.WaitForDialogue();
                    };
                    return new BattleCommand(this, this,
                        new Skill("PlutoDialogue", "PlutoDialogue", SkillTarget.Self, effect, 0));
                case 1:
                    foreach (PartyMember target in SelectAllTargets())
                        target.AddStatModifier("PlutoBuff");
                    effect = async (self, _) =>
                    {
                        DialogueManager.Instance.QueueMessage("PLUTO", self.CenterPoint,
                            @"This fight is mine to win...\! You cannot escape my judgement!");
                        DialogueManager.Instance.QueueMessage("PLUTO finishes charging his ultimate attack!");
                        await DialogueManager.Instance.WaitForDialogue();
                    };
                    return new BattleCommand(this, this,
                        new Skill("PlutoDialogue", "PlutoDialogue", SkillTarget.Self, effect, 0));
                default:
                    Charging = false;
                    effect = async (self, _) =>
                    {
                        DialogueManager.Instance.QueueMessage("PLUTO", self.CenterPoint,
                            @"I hope we meet again in the next life.\! Goodbye.");
                        await DialogueManager.Instance.WaitForDialogue();
                        BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["PEMeteor"]);
                    };
                    return new BattleCommand(this, this,
                        new Skill("PlutoDialogue", "PlutoDialogue", SkillTarget.Self, effect, 0));
             }
        }
        
        IReadOnlyList<PartyMember> party = SelectAllTargets();
        if (party.Any(x => x.HasStatModifier("PlutoBuff")))
            return new BattleCommand(this, party, Skills["PEMeteor"]);
        
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["PEHeadbutt"]);
        
        List<PartyMember> sad = party.Where(x => x.CurrentState is "sad" or "depressed" or "miserable").ToList();
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

    private bool HasSpoken = false;
    private bool HasThrownEarth = false;
    private bool Charging = false;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0)
        {
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"Unbelievable...\! Even at full power...\! I have been bested.");
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"It has been an honor to do battle with you.\! Your victory is well deserved.");
            await DialogueManager.Instance.WaitForDialogue();
            KillEarth();
            return;
        }

        if (CurrentHP < 5000 && !HasThrownEarth)
        {
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"Ah...\! It seems that I have underestimated you once again.");
            await DialogueManager.Instance.WaitForDialogue();
            HasThrownEarth = true;
            if (Earth != null && Earth.Actor.CurrentState is not "toast")
            {
                BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["PEEarthsFinale"]);
                return;
            }
        }
        
        if (CurrentHP < 5000 && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"Very few have pushed me this far...\! and none have left the same.");
            DialogueManager.Instance.QueueMessage("PLUTO", CenterPoint, @"I want nothing more than victory!\! Let me show you my resolve!");
            DialogueManager.Instance.QueueMessage("PLUTO begins charging his ultimate attack!");
            await DialogueManager.Instance.WaitForDialogue();
            AddStatModifier("PlutoCharging");
            AnimationManager.Instance.PlayAnimation(218, this);
            Charging = true;
            HasSpoken = true;
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