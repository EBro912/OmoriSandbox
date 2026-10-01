using OmoriSandbox.Actors;
using System.Linq;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used by the Counter skill.
/// </summary>
public sealed class AubreyCounterModifier : StatModifier
{
    public AubreyCounterModifier(int turns) : base(turns) { }

    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking, bool isCritical, bool neverMiss)
    {
        if (phase is not DamagePhase.PostApply || isAttacking || attacker is not Enemy)
            return;

        BattleCommand command = BattleManager.Instance.GetCurrentCommand();
        if (command?.Action is not Skill skill || command.Actor != attacker)
            return;
        if (skill.Target != SkillTarget.Enemy && !(skill.Target == SkillTarget.XRandomEnemies && command.Targets.Count == 1))
            return;

        if (defender.CurrentHP > 0 && !defender.IsToast)
        {
            if (!BattleManager.Instance.HasForcedCommandsSinceCommandStart && Database.TryGetSkill("CounterAttack", out Skill s))
                BattleManager.Instance.QueueForceCommand(defender, attacker, s, false);
        }
        else
        {
            BattleManager.Instance.DropForcedCommandsSinceCommandStart();
            defender.RemoveStatModifier("AubreyCounter");
        }
    }
}