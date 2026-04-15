namespace OmoriSandbox.Battle;

/// <summary>
/// Represents various phases of damage calculation used for damage overrides.
/// </summary>
public enum DamagePhase
{
    /// <summary>
    /// Before emotion advantage is applied.
    /// </summary>
    /// <remarks>
    /// <c>isCritical</c> will always be false during this phase.
    /// </remarks>
    PreEmotion,
    /// <summary>
    /// Before crit damage is applied, if any.
    /// </summary>
    /// <remarks>
    /// <c>isCritical</c> will always be false during this phase.
    /// </remarks>
    PreCrit,
    /// <summary>
    /// Before the flat 1.5 crit damage is applied, if any.
    /// </summary>
    PreFlatCrit,
    /// <summary>
    /// Before damage variance is applied.
    /// </summary>
    PreVariance,
    /// <summary>
    /// Before the first rounding and clamping is applied.
    /// </summary>
    /// <remarks>
    /// Modifiers such as Guard are applied at this stage.
    /// </remarks>
    PreRounding,
    /// <summary>
    /// Before juice damage is applied, if the target is sad.
    /// </summary>
    /// <remarks>
    /// Modifiers such as Flex are applied at this stage.
    /// </remarks>
    PreJuice,
    /// <summary>
    /// Before the final damage is applied to the target. This is also after the second rounding and clamping.
    /// </summary>
    /// <remarks>
    /// Modifiers such as Mr. Jawsum's damage shield are applied at this stage.
    /// </remarks>
    PreApply,
    /// <summary>
    /// After the final damage is applied to the target.
    /// Damage can no longer be overridden here, however any "respond" effects can be applied.
    /// </summary>
    /// <remarks>
    /// Modifiers such as Second Chance, used by Plot Armor and Persist, are applied at this stage.
    /// </remarks>
    PostApply
}