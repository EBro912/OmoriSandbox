using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The stat modifier that makes skills free under Encore.
/// </summary>
public sealed class EncoreStatModifier : StatModifier
{
    /// <inheritdoc/>
    public EncoreStatModifier(int turns) : base(turns) {}

    /// <inheritdoc/>
    public override void OverrideJuiceCost(ref int juice, Actor actor)
    {
        juice = 0;
    }
}