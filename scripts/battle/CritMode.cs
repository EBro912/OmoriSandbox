namespace OmoriSandbox.Battle;

/// <summary>
/// Whether a damage call can be a critical hit. See <see cref="DamageOptions.Crit"/>.
/// </summary>
public enum CritMode
{
    /// <summary>
    /// Rolls with the attacker's LUCK stat.
    /// </summary>
    Roll,
    /// <summary>
    /// Always a critical hit.
    /// </summary>
    Always,
    /// <summary>
    /// Never a critical hit.
    /// </summary>
    Never
}
