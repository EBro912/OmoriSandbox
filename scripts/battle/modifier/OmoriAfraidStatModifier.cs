using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

internal sealed class OmoriAfraidStatModifier : StatModifier
{
    public OmoriAfraidStatModifier()
    {
        WithCategories("EMOTION", "AFRAID");
    }

    public override void OnAdd(Actor actor) => actor.SetEmotionForced("afraid");

    public override void OnRemove(Actor actor)
    {
        if (actor.CurrentEmotion.Id == "afraid")
            actor.SetEmotionForced("neutral");
    }
}
