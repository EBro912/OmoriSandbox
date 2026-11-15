namespace OmoriSandbox.Menu;

/// <summary>
/// The state that the battle menu can be in.
/// </summary>
public enum MenuState
{
    /// <summary>
    /// No menu is open.
    /// </summary>
    None,
    /// <summary>
    /// The party (FIGHT/RUN) menu.
    /// </summary>
    Party,
    /// <summary>
    /// The battle (ATTACK/SKILL/SNACK/TOY) menu.
    /// </summary>
    Battle,
    /// <summary>
    /// The SKILL menu.
    /// </summary>
    Skill,
    /// <summary>
    /// The SNACK menu.
    /// </summary>
    Snack,
    /// <summary>
    /// The TOY menu.
    /// </summary>
    Toy
}