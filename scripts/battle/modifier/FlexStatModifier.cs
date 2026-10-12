using System;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used by the Flex skill.
/// </summary>
public sealed class FlexStatModifier : StatModifier
{
    /// <inheritdoc/>
    public FlexStatModifier(params StatBonus[] bonuses) : base(bonuses) { }
    
    /// <inheritdoc/>
    public override void OverrideDamage(DamagePhase phase, ref float damage, DamageContext context)
    {
        if (phase is DamagePhase.PreJuice && context.IsAttacking && !context.NeverMiss && !context.IsJuice && damage > 0)
        {
            damage = (float)Math.Ceiling(damage * 2.5f);
            context.Attacker.RemoveStatModifier("Flex");
        }
    }
}