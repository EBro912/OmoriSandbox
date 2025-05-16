public class BattleCommand
{
	public Actor Actor;
	public Actor Target;
	public Skill Skill;

	public BattleCommand(Actor actor, Actor target, Skill skill)
	{
		Actor = actor;
		Target = target;
		Skill = skill;
	}
}
