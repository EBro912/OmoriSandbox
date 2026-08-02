using OmoriSandbox.Actors;
using System.Linq;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used by the Counter skill.
/// </summary>
public sealed class AubreyCounterModifier : StatModifier
{
    public AubreyCounterModifier(int turns) : base(turns) { }

    private bool HasCounteredThisTurn = false;
    
    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking, bool isCritical, bool neverMiss)
    {
        if (phase is DamagePhase.PreApply)
        {
            if (isAttacking)
            {
                HasCounteredThisTurn = false;
                return;
            }
        }
        
        if (phase is DamagePhase.PostApply)
        {
            if (HasCounteredThisTurn)
                return;

            BattleCommand command = BattleManager.Instance.GetCurrentCommand();

            if (attacker is Enemy && command.Action is Skill skill && skill.Target == SkillTarget.Enemy)
            {
                if (Database.TryGetSkill("CounterAttack", out Skill s))
                {
                    HasCounteredThisTurn = true;
                    BattleManager.Instance.ForceCommand(defender, attacker, s);
                }
            }
        }
    }
}