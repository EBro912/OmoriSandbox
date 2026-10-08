namespace OmoriSandbox.Battle;

/// <summary>
/// What happens to a hit from <c>BattleManager.Damage</c> or <c>BattleManager.DamageJuice</c>. See <see cref="DamageContext.Outcome"/>.
/// </summary>
public enum DamageOutcome
{
    /// <summary>
    /// The defender takes the hit.
    /// </summary>
    Normal,
    /// <summary>
    /// The defender recovers the hit's amount instead of taking it.
    /// </summary>
    Absorb,
    /// <summary>
    /// The attacker takes the hit instead of the defender.
    /// </summary>
    Reflect
}
