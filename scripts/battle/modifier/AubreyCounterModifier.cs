using OmoriSandbox.Actors;
using System.Linq;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used by the Counter skill.
/// </summary>
public sealed class AubreyCounterModifier : StatModifier
{
    public AubreyCounterModifier(int turns) : base(turns) { }

    public override void OverrideDamage(DamagePhase phase, ref float damage, DamageContext context)
    {
        if (phase is not DamagePhase.PostApply || context.IsAttacking || context.Outcome != DamageOutcome.Normal)
            return;
        Respond(context.Attacker, context.Defender);
    }

    /// <inheritdoc/>
    public override void OverrideHeal(DamagePhase phase, ref float amount, HealContext context)
    {
        if (phase is not DamagePhase.PostApply || context.IsHealer)
            return;
        Respond(context.Healer, context.Target);
    }

    private static void Respond(Actor attacker, Actor defender)
    {
        if (attacker is not Enemy)
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