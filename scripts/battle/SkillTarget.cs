namespace OmoriSandbox.Battle;

/// <summary>
/// What a <see cref="Skill"/> can target. Mainly used for visual targeting.
/// </summary>
public enum SkillTarget
{
#pragma warning disable CS1591
    Self,
    Ally,
    AllyNotSelf,
    AllAllies,
    Enemy,
    XRandomEnemies,
    AllEnemies,
    AllyOrEnemy,
    DeadAlly,
    AllDeadAllies
#pragma warning restore CS1591
}
