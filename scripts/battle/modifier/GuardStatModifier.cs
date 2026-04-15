using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used by the Guard skill, as well as any other skill that has a Guard as a side effect.
/// </summary>
public sealed class GuardStatModifier : StatModifier
{
    /// <inheritdoc/>
    public GuardStatModifier(int turns, params StatBonus[] bonuses) : base(turns, bonuses) { }
    
    /// <inheritdoc/>
    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking, bool isCritical, bool neverMiss)
    {
        if (phase is DamagePhase.PreRounding && !isAttacking)
            damage *= 0.5f;
    }
}