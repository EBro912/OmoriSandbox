using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Actors;

internal sealed class AngelAlt : Enemy
{
    public override string Name => "ANGEL";
    public override Vector2 InfoBoxOffset => new(0, -375);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/angel.tres");
    protected override Stats Stats => new(150, 75, 15, 6, 18, 30, 95);

    protected override string[] EquippedSkills => ["ANAttack", "ANDoNothing", "ANQuickAttack", "ANTease"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry" or "afraid";
    }

    protected override PartyMember SelectTarget()
    {
        if (HasStatModifier("Charm"))
            return (StatModifiers["Charm"] as CharmStatModifier).CharmedBy;
        List<PartyMember> members = BattleManager.Instance.GetAlivePartyMembers();
        List<PartyMember> taunting = members.FindAll(x => x.HasStatModifier("Taunt"));
        if (taunting.Count == 0)
        {
            return members.MaxBy(x => x.CurrentStats.SPD);
        }
        return taunting.MaxBy(x => x.CurrentStats.SPD);
    }
    
    public override Task OnStartOfBattle()
    {
        AddImmortal();
        return Task.CompletedTask;
    }

    public override Task ProcessBattleConditions()
    {
        if (CurrentHP == 0)
            RemoveImmortal();
        return Task.CompletedTask;
    }

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 76)
            return new BattleCommand(this, SelectTarget(), Skills["ANAttack"]);
        if (Roll() < 26)
            return new BattleCommand(this, this, Skills["ANDoNothing"]);
        if (Roll() < 46)
            return new BattleCommand(this, SelectTarget(), Skills["ANQuickAttack"]);
        return new BattleCommand(this, SelectTarget(), Skills["ANTease"]);
    }
}