using System;
using System.Collections.Generic;
using System.Linq;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier that makes other enemies take the hit, such as Mr. Jawsum's Gator Guys.
/// </summary>
public sealed class MinionBarrierModifier : StatModifier
{
    /// <inheritdoc/>
    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking, bool isCritical, bool neverMiss)
    {
        if (phase is not DamagePhase.PreApply) 
            return;
        
        if (isAttacking)
            return;

        List<Enemy> allEnemies = BattleManager.Instance.GetAllEnemies();
        // if there's only one enemy alive, there's no one to share the damage with
        if (allEnemies.Count == 1)
            return;
        
        int shared = (int)Math.Ceiling(damage / allEnemies.Count - 1);
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy == defender)
                continue;
            enemy.Damage(shared);
            BattleManager.Instance.SpawnDamageNumber(shared, enemy.CenterPoint);
            AnimationManager.Instance.PlayAnimation(123, enemy);
            BattleLogManager.Instance.QueueMessage(attacker, enemy, "[target] takes " + shared + " damage!");
        }

        damage = 0f;
    }
}