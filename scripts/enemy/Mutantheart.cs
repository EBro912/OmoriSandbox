using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class Mutantheart : Enemy
{
    public override string Name => "MUTANTHEART";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/mutantheart.tres");
    protected override Stats Stats => new(7000, 3500, 75, 1, 50, 25, 95);

    protected override string[] EquippedSkills => ["MHWink", "MHCry", "MHInsult", "MHInstakill"];
    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy" || state == "angry" || state == "hurt" || state == "toast";
    }

    private static readonly string[] DesireableStates = ["happy", "sad", "angry"];
    private static readonly Dictionary<string, string[]> StateLookup = new()
    {
        { "happy", ["happy", "ecstatic", "manic"] },
        { "sad", ["sad", "depressed", "miserable"] },
        { "angry", ["angry", "enraged", "furious"] },
    };
    private string DesiredState = "";

    public override BattleCommand ProcessAI()
    {
        HasObserveTarget(out PartyMember observe);
        
        switch (CurrentState)
        {
            case "happy":
                return new BattleCommand(this, observe ?? SelectTarget(), Skills["MHWink"]);
            case "sad":
                return new BattleCommand(this, observe ?? SelectTarget(), Skills["MHCry"]);
            case "angry":
                return new BattleCommand(this, observe ?? SelectTarget(), Skills["MHInsult"]);
            default:
                if (observe != null)
                    return new BattleCommand(this, observe, Skills["MHInsult"]);
                if (Roll() < 51)
                    return new BattleCommand(this, SelectTarget(), Skills["MHWink"]);
                if (Roll() < 51)
                    return new BattleCommand(this, SelectTarget(), Skills["MHCry"]);
                return new BattleCommand(this, SelectTarget(), Skills["MHInsult"]);
        }
    }

    private bool HasSpoken = false;

    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0) return;

        if (CurrentHP < 3500 && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage(this, "[font_size=22][wave freq=10.0]Bluh?");
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, "[font_size=22][wave freq=10.0]Bloooohhhh...");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task ProcessStartOfTurn()
    {
        DesiredState = DesireableStates[GameManager.Instance.Random.RandiRange(0, DesireableStates.Length - 1)];
        string message = DesiredState switch
        {
            "happy" => @"[font_size=22][wave freq=10.0]HAPPY...\. please!",
            "sad" => @"[font_size=22][wave freq=10.0]SAD...\. please...",
            "angry" => @"[font_size=22][wave freq=10.0]ANGRY...\. please.",
            _ => @"[font_size=22]I couldn't make up my mind...\. this is a problem!"
        };
        DialogueManager.Instance.QueueMessage(this, message);
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task ProcessEndOfTurn()
    {
        if (CurrentHP <= 0)
            return;
        
        bool failed = false;
        foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
        {
            if (StateLookup[DesiredState].All(state => member.Actor.CurrentState != state))
            {
                failed = true;
                BattleManager.Instance.ForceCommand(this, member.Actor, Skills["MHInstakill"]);
            }
        }
        if (failed)
        {
            DialogueManager.Instance.QueueMessage(this, @"[font_size=22][shake rate=20]Bleh...\| Wrong!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }

    public override async Task OnStartOfBattle()
    {
        DialogueManager.Instance.QueueMessage(this, @"[font_size=22]H...\! Henno...");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, "[wave freq=10.0]Bleh!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }

}