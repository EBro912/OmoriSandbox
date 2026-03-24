using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used by the Flex skill.
/// </summary>
public sealed class FlexStatModifier : StatModifier
{
    /// <inheritdoc/>
    public FlexStatModifier(params StatBonus[] bonuses) : base(bonuses) { }
    
    /// <inheritdoc/>
    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking, bool isCritical, bool neverMiss)
    {
        if (phase is DamagePhase.PreJuice && isAttacking && !neverMiss)
        {
            damage *= 2.5f;
        }
        
        if (phase is DamagePhase.PreApply && isAttacking && damage > 0 && !neverMiss)
        {
            attacker.RemoveStatModifier("Flex");
        }
    }
}