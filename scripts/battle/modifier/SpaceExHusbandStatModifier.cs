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

        if (defender.CurrentState is not "neutral") 
            return;
        
        BattleCommand command = BattleManager.Instance.GetCurrentCommand();
        // recreate the Omori bug where items and certain skills can deal damage to him in neutral
        if (command.Action is Skill skill)
        {
            if (SettingsMenuManager.Instance.SpaceExHusbandReleaseEnergy &&
                skill.Name.StartsWith("Release Energy"))
                return;

            if (skill.Name != "TRICK" &&
                !skill.Name.StartsWith("Pass To Aubrey") &&
                skill.Name != "FLOWER CROWN" &&
                skill.Name != "Vent")
                damage = 0f;
        }
    }
}
