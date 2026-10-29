using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

internal sealed class HangingModifier : StatModifier
{
    internal int Stage { get; set; }

    internal static void SetStage(Actor actor, int stage)
    {
        if (!actor.HasStatModifier("Hanging"))
            actor.AddStatModifier("Hanging", silent: true);
        if (actor.StatModifiers.TryGetValue("Hanging", out StatModifier modifier) && modifier is HangingModifier hanging)
            hanging.Stage = stage;
    }

    internal static int StageOf(Actor actor)
    {
        return actor.StatModifiers.TryGetValue("Hanging", out StatModifier modifier) && modifier is HangingModifier hanging ? hanging.Stage : 0;
    }
}
