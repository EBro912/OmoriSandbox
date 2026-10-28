using OmoriSandbox.Actors;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Battle.Modifier;

internal sealed class PersistentPlotArmorStatModifier : StatModifier
{
    internal bool AnnouncePending;

    private bool PlayedFace;

    public override void OverrideDamage(DamagePhase phase, ref float damage, DamageContext context)
    {
        if (phase is not DamagePhase.PostApply || context.IsAttacking)
            return;
        if (context.Outcome == DamageOutcome.Reflect && !context.IsReflected)
            return;
        Respond(context.Defender);
    }

    public override void OverrideHeal(DamagePhase phase, ref float amount, HealContext context)
    {
        if (phase is DamagePhase.PostApply && !context.IsHealer)
            Respond(context.Target);
    }

    public override void OnStartOfTurn(Actor actor)
    {
        if (!PlayedFace)
            return;
        PlayedFace = false;
        actor.ClearAnimation();
    }

    private void Respond(Actor actor)
    {
        if (actor.CurrentHP > 1)
            return;
        actor.CurrentHP = 1;
        if (actor is not Omori omori || omori.HasUsedPlotArmor)
            return;
        omori.HasUsedPlotArmor = true;
        omori.PlayAnimation("plotarmor", EmotionAsset.PlotArmor);
        PlayedFace = true;
        AnnouncePending = true;
    }
}
