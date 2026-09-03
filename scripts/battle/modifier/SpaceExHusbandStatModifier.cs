using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Editor;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used by Space Ex-Husband to prevent damage while neutral.
/// </summary>
public sealed class SpaceExHusbandStatModifier : StatModifier
{
    /// <inheritdoc/>
    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking, bool isCritical, bool neverMiss)
    {
        if (phase is not DamagePhase.PreApply) 
            return;
        
        if (isAttacking)
            return;

        if (defender.CurrentEmotion.Id is not "neutral") 
            return;
        
        BattleAction action = BattleManager.Instance.GetCurrentCommand()?.Action;

        if (action is Skill skill && skill.Name.StartsWith("Release Energy"))
        {
            if (!SettingsMenuManager.Instance.SpaceExHusbandReleaseEnergy)
                damage = 0f;
            return;
        }
        // certain hit skills can still make it through
        if (!neverMiss)
            damage = 0f;
    }
}
