using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// Vanilla state 29 CALL FOR FRIEND DELAY, used to prevent summoned enemies from acting immediately.
/// </summary>
/// <remarks>
/// This is broken for most vanilla bosses due to the way the Common Events work.
/// </remarks>
internal sealed class CallForFriendDelayModifier : StatModifier
{
    public CallForFriendDelayModifier() : base(1) { }

    /// <inheritdoc/>
    public override void OnAdd(Actor actor) => actor.Stunned = true;

    /// <inheritdoc/>
    public override void OnRemove(Actor actor) => actor.Stunned = false;
}
