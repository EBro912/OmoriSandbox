using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// A Stat Modifier that makes the actor immune to all damage.
/// </summary>
public sealed class ImmuneStatModifier : StatModifier
{
    public ImmuneStatModifier(params StatBonus[] bonuses) : base(bonuses) { }

    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking,
        bool isCritical, bool neverMiss)
    {
        if (phase is not DamagePhase.PreApply)
            return;

        damage = 0f;
    }
}