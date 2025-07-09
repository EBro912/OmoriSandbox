using System.Threading.Tasks;
using System;

/// <summary>
/// A generic action that an actor can perform in a battle
/// </summary>
public class BattleAction
{
	/// <summary>
	/// The name of the action.
	/// </summary>
	public string Name { get; private set; }
	/// <summary>
	/// The description of the action. Displayed in the battle log.
	/// </summary>
	public string Description { get; private set; }
	/// <summary>
	/// What this skill can target. Mainly used for input validation.
	/// </summary>
	public SkillTarget Target { get; private set; }
	/// <summary>
	/// What happens when this action is performed.
	/// </summary>
	public Func<Actor, Actor, Task> Effect { get; protected set; }

	protected BattleAction(string name, string description, SkillTarget target, Func<Actor, Actor, Task> effect)
	{
		Name = name;
		Description = description;
		Target = target;
		Effect = effect;
	}
}
