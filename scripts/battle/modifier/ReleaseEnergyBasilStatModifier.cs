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
        int heal = (int)Math.Round(actor.CurrentStats.MaxHP * 0.1f, MidpointRounding.AwayFromZero);
		int juice = (int)Math.Round(actor.CurrentStats.MaxJuice * 0.05f, MidpointRounding.AwayFromZero);
		actor.Heal(heal);
		actor.HealJuice(juice);
		BattleManager.Instance.SpawnDamageNumber(heal, actor.CenterPoint, DamageType.Heal);
		BattleManager.Instance.SpawnDamageNumber(juice, actor.CenterPoint, DamageType.JuiceGain);
    }
}