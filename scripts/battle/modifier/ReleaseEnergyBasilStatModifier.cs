using OmoriSandbox.Actors;
using System;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The stat modifier given to party members when Basil's Release Energy is used.
/// </summary>
public sealed class ReleaseEnergyBasilStatModifier : StatModifier
{
    /// <inheritdoc/>
    public ReleaseEnergyBasilStatModifier(params StatBonus[] bonuses) : base(bonuses) {}

    /// <inheritdoc/>
    public override void OnStartOfTurn(Actor actor)
    {
        int heal = (int)Math.Floor(actor.CurrentStats.MaxHP * 0.1);
		int juice = (int)Math.Floor(actor.CurrentStats.MaxJuice * 0.05);
		heal = actor.Heal(heal);
		juice = actor.HealJuice(juice);
		BattleManager.Instance.SpawnDamageNumber(heal, actor, DamageType.Heal);
		BattleManager.Instance.SpawnDamageNumber(juice, actor, DamageType.JuiceGain);
    }
}