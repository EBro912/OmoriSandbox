using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Actors;

internal sealed class AubreyEnemy : Enemy
{
    public override string Name => "AUBREY";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/aubrey_enemy.tres");
    protected override Stats Stats => new(240, 120, 24, 8, 12, 5, 95);

    protected override string[] EquippedSkills => ["AEAttack", "AEDoNothing", "AEHeadbutt"];

    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy"
              || state == "angry" || state == "hurt" || state == "toast";
    }

    protected override PartyMember SelectTarget()
    {
        if (HasStatModifier("Charm"))
            return (StatModifiers["Charm"] as CharmStatModifier).CharmedBy;
        List<PartyMemberComponent> members = BattleManager.Instance.GetAlivePartyMembers();
        List<PartyMemberComponent> taunting = members.FindAll(x => x.Actor.HasStatModifier("Taunt"));
        if (taunting.Count == 0)
        {
            return members.MaxBy(x => x.Actor.CurrentStats.SPD).Actor;
        }
        return taunting.MaxBy(x => x.Actor.CurrentStats.SPD).Actor;
    }

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 46)
        {
            return new BattleCommand(this, SelectTarget(), Skills["AEAttack"]);
        }
        if (Roll() < 31)
        {
            return new BattleCommand(this, this, Skills["AEDoNothing"]);
        }
        return new BattleCommand(this, SelectTarget(), Skills["AEHeadbutt"]);
    }
}