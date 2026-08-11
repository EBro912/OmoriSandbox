using System;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The reason a <see cref="StatModifier"/> was removed from an actor.
/// </summary>
public enum StatModifierRemovalReason
{
    /// <summary>Removed directly, e.g. by a skill or by the modifier itself.</summary>
    Manual,
    /// <summary>The modifier's turn counter ran out at the end of a turn.</summary>
    Expired,
    /// <summary>Removed as part of clearing all of an actor's modifiers, e.g. when the actor becomes toast.</summary>
    Cleared,
    /// <summary>Canceled out or overpowered by its counterpart under the Combined Buffs/Debuffs setting.</summary>
    Combined
}

/// <summary>
/// Event arguments for <see cref="Actors.Actor.OnStatModifierAdded"/>.
/// </summary>
public class StatModifierEventArgs : EventArgs
{
    /// <summary>
    /// The id the modifier is registered under, e.g. "AttackUp".
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The modifier instance held by the actor.
    /// </summary>
    public StatModifier Modifier { get; }

    internal StatModifierEventArgs(string id, StatModifier modifier)
    {
        Id = id;
        Modifier = modifier;
    }
}

/// <summary>
/// Event arguments for <see cref="Actors.Actor.OnStatModifierRemoved"/>.
/// </summary>
public sealed class StatModifierRemovedEventArgs : StatModifierEventArgs
{
    /// <summary>
    /// Why the modifier was removed.
    /// </summary>
    public StatModifierRemovalReason Reason { get; }

    internal StatModifierRemovedEventArgs(string id, StatModifier modifier, StatModifierRemovalReason reason)
        : base(id, modifier)
    {
        Reason = reason;
    }
}
