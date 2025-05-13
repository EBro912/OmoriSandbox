public class BattleCommand
{
	public Actor Actor;
	public Actor Target;
	public string Message;
	public Skill Skill;

	public BattleCommand(Actor actor, Actor target, Skill skill, string message = "")
	{
		Actor = actor;
		Target = target;
		Skill = skill;
		Message = message;
	}
}
