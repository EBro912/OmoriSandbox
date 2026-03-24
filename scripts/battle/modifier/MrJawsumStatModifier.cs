using System;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used by Mr. Jawsum to have his Gator Guys take the hit.
/// </summary>
public sealed class MrJawsumStatModifier : StatModifier
{
    /// <inheritdoc/>
    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking, bool isCritical, bool neverMiss)
    {
        if (phase is not DamagePhase.PreApply) 
            return;
        
        if (isAttacking)
            return;

        if (defender is not MrJawsum jawsum)
            return;

        if (jawsum.GatorGuys.Count == 0)
            return;

        int shared = (int)Math.Round(damage / jawsum.GatorGuys.Count);
        for (int i = jawsum.GatorGuys.Count - 1; i >= 0; i--)
        {
            EnemyComponent gator = jawsum.GatorGuys[i];
            gator.Actor.Damage(shared);
            BattleManager.Instance.SpawnDamageNumber(shared, gator.Actor.CenterPoint);
            AnimationManager.Instance.PlayAnimation(123, gator.Actor);
            BattleLogManager.Instance.QueueMessage(attacker, gator.Actor, "[target] takes " + shared + " damage!");
            if (gator.Actor.CurrentHP <= 0)
            {
                jawsum.GatorGuys.RemoveAt(i);
            }
        }

        damage = 0f;
    }
}