using System.Linq;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

internal sealed class ImmobileModifier : StatModifier
{
    public ImmobileModifier() { }

    public ImmobileModifier(int turns) : base(turns) { }

    public override void OnAdd(Actor actor) => actor.Stunned = true;

    public override void OnRemove(Actor actor) =>
        actor.Stunned = actor.StatModifiers.Values.Any(m => m is ImmobileModifier or StunnedModifier or CallForFriendDelayModifier);
}
