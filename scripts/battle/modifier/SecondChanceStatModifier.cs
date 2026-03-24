using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used by Plot Armor and Persist to prevent death.
/// </summary>
public sealed class SecondChanceStatModifier : StatModifier
{
    public SecondChanceStatModifier(int turns) : base(turns) { }

    /// <inheritdoc/>
    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking,
        bool isCritical, bool neverMiss)
    {
        if (phase is not DamagePhase.PostApply)
            return;

        if (isAttacking)
            return;

        if (defender.CurrentHP <= 0)
            defender.CurrentHP = 1;
    }
}