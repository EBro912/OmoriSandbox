using System;
using System.Threading.Tasks;

public struct Skill
{
    public string Name;
    public string Description;
    public int Cost;
    public bool GoesFirst;
    public bool Hidden;
    public SkillTarget Target;
    public int AnimationId;
    public Func<Actor, Actor, Skill, Task> Effect;
}

public enum SkillTarget
{
    Self,
    Ally,
    AllAllies,
    Enemy,
    AllEnemies,
    AllyOrEnemy,
    DeadAlly,
    AllDeadAllies
}