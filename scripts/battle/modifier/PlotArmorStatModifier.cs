using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier given when the PartyMember receives a fatal hit and has Plot Armor enabled.
/// </summary>
public sealed class PlotArmorStatModifier : StatModifier
{
    /// <summary>
    /// Whether the "did not succumb" dialogue has been shown.
    /// </summary>
    public bool HasAnnounced = false;

    /// <inheritdoc/>
    public PlotArmorStatModifier() : base() { }

    /// <inheritdoc/>
    public override void OverrideDamage(DamagePhase phase, ref float damage, Actor attacker, Actor defender, bool isAttacking,
        bool isCritical, bool neverMiss)
    {
        if (phase is not DamagePhase.PostApply)
            return;

        if (isAttacking)
            return;

        if (defender.CurrentHP <= 0)
            defender.CurrentHP = 1;
    }

    /// <inheritdoc/>
    public override void OnStartOfTurn(Actor actor)
    {
        actor.RemoveStatModifier("PlotArmor");
        actor.SetState(actor.CurrentState, true);
    }
}
