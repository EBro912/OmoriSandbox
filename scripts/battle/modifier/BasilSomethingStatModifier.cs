namespace OmoriSandbox.Battle.Modifier;

/// <summary>
/// The modifier used in the Basil fight to seal certain skills.
/// </summary>
public sealed class BasilSomethingStatModifier : StatModifier
{
    /// <inheritdoc/>
    public BasilSomethingStatModifier() : base(new StatBonus(StatType.HIT, 1000))
    {
        WithSealedSkills("CalmDown", "Focus", "Persist");
    }
}
