using System;
using System.Threading.Tasks;

public class Skill : BattleAction
{
	public int Cost;
	public bool GoesFirst;
	public bool Hidden;
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
