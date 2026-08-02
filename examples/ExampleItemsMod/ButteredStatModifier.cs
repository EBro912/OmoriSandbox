using System;
using OmoriSandbox;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Modifier;

namespace ExampleItemsMod;

public class ButteredStatModifier : StatModifier
{
    // 3 turns of SPD x1.25, applied by the inherited ApplyStats
    public ButteredStatModifier() : base(3, new StatBonus(StatType.SPD, 1.25f)) { }

    // recover 5% HEART at the start of each turn
    public override void OnStartOfTurn(Actor actor)
    {
        int heal = (int)Math.Round(actor.CurrentStats.MaxHP * 0.05f, MidpointRounding.AwayFromZero);
        actor.Heal(heal);
        BattleManager.Instance.SpawnDamageNumber(heal, actor.CenterPoint, DamageType.Heal);
    }

    // incoming attacks "slide off", reducing damage taken by 10%
    // (PreRounding is the same phase Guard uses)
    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender,
        bool isAttacking, bool isCritical, bool neverMiss)
    {
        if (phase == DamagePhase.PreRounding && !isAttacking)
            damage *= 0.9f;
    }
}
