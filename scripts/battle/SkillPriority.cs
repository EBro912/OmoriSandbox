namespace OmoriSandbox.Battle;

/// <summary>
/// The turn priority of a skill in battle.
/// </summary>
public enum SkillPriority
{
    /// <summary>
    /// The skill will always go last.
    /// </summary>
    Last,
    /// <summary>
    /// The skill will be treated normally.
    /// </summary>
    Normal,
    /// <summary>
    /// The skill will always go first.
    /// </summary>
    First
}