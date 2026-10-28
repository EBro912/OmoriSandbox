using System.Linq;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

internal sealed class StunnedModifier : StatModifier
{
    public StunnedModifier() : base(1) { }

    public override void OnAdd(Actor actor) => actor.Stunned = true;

    public override void OnRemove(Actor actor) => actor.Stunned = actor.StatModifiers.Values.Any(m => m is ImmobileModifier);
}
