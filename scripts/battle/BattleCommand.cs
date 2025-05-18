public class BattleCommand
{
	public Actor Actor;
	public Actor Target;
	public BattleAction Action;

	public BattleCommand(Actor actor, Actor target, BattleAction action)
	{
		Actor = actor;
		Target = target;
		Action = action;
	}
}
