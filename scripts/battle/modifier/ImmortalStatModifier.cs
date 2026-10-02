using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used to make a character immortal.
/// </summary>
/// <remarks>
/// On an enemy, it keeps the enemy alive at 0 HP until the modifier is removed. See <see cref="Enemy.AddImmortal"/>.<br/>
/// On a party member, it leaves them at 1 HP instead.
/// </remarks>
public sealed class ImmortalStatModifier : StatModifier
{
    public ImmortalStatModifier(params StatBonus[] bonuses) : base(bonuses) { }

    /// <inheritdoc/>
    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking, bool isCritical, bool neverMiss)
    {
        if (phase is not DamagePhase.PostApply)
            return;

        if (isAttacking)
            return;

        if (defender is PartyMember && defender.CurrentHP <= 0)
            defender.CurrentHP = 1;
    }
}