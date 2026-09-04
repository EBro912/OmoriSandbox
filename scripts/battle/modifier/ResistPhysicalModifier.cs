using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used to make an actor immune to physical attacks. Certain Hit (neverMiss = true) attacks still make it through.
/// </summary>
public class ResistPhysicalModifier : StatModifier
{
    public ResistPhysicalModifier(params StatBonus[] bonuses) : base(bonuses) { }

    /// <inheritdoc/>
    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking,
        bool isCritical, bool neverMiss)
    {
        if (phase is not DamagePhase.PreApply)
            return;
        
        if (isAttacking)
            return;
        
        // only block damage if it can miss (physical attack)
        if (!neverMiss)
            damage = 0f;
    }
}