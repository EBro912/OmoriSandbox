using System;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

public sealed class SalesTagStatModifier : StatModifier
{
    /// <inheritdoc/>
    public SalesTagStatModifier(params StatBonus[] bonuses) : base(bonuses) {}

    public override void OverrideJuiceCost(ref int juice, Actor actor)
    {
        juice = (int)Math.Floor(juice / 2d);
    }
}