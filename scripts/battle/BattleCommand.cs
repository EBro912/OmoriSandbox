public abstract class BattleCommand
{
	public Actor Actor;
	public Actor Target;
	public bool GoesFirst;
	protected string Message;

	public BattleCommand(Actor actor, Actor target, string message = "", bool goesFirst = false)
	{
		Actor = actor;
		Target = target;
		GoesFirst = goesFirst;
		Message = message;
	}

	public abstract CommandResult Run();

	public string ParseMessage(string message)
	{
		return message.Replace("[actor]", Actor.Name.ToUpper()).Replace("[target]", Target == null ? "" : Target.Name.ToUpper());
	}
}

public struct CommandResult
{
	public bool Hit;
	public bool Critical;
	public int Damage;

	public CommandResult(bool hit, bool critical = false, int damage = 0)
	{
		Hit = hit;
		Critical = critical;
		Damage = damage;
	}
}
