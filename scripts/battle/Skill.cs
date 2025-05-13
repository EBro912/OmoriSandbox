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
    public string Animation;
    public Func<Actor, Actor, Skill, Task> Effect;
}

public enum SkillTarget
{
    Ally,
    AllAllies,
    Enemy,
    AllEnemies,
    AllyOrEnemy,
    DeadAlly,
    AllDeadAllies
}