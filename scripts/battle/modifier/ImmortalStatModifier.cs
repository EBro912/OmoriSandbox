using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used to make a character immortal.
/// </summary>
public sealed class ImmortalStatModifier : StatModifier
{
    public ImmortalStatModifier(params StatBonus[] bonuses) : base(bonuses) { }

    /// <summary>
    /// Whether a lethal hit has already been absorbed.
    /// </summary>
    public bool Triggered { get; private set; }

    /// <inheritdoc/>
    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking, bool isCritical, bool neverMiss)
    {
        if (phase is not DamagePhase.PostApply)
            return;

        if (isAttacking)
            return;

        if (defender.CurrentHP <= 0)
        {
            defender.CurrentHP = 1;
            Triggered = true;
        }
    }
}