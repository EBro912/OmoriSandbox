using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Godot;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle;

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
	/// What this action can target. Mainly used for input validation.
	/// </summary>
	public SkillTarget Target { get; private set; }
	/// <summary>
	/// The priority of the action when calculating turn order.
	/// </summary>
	public SkillPriority Priority {get; private set;}
	/// <summary>
	/// What happens when this action is performed.
	/// </summary>
	public Func<Actor, IReadOnlyList<Actor>, Task> Effect { get; }

	protected BattleAction(string name, string description, SkillTarget target, SkillPriority priority, Func<Actor, IReadOnlyList<Actor>, Task> effect)
	{
		Name = name;
		Description = description;
		Target = target;
		Effect = effect;
		Priority = priority;
	}

	protected BattleAction(string name, string description, SkillTarget target, SkillPriority priority, Func<Actor, Actor, Task> effect)
		: this(name, description, target, priority, async (self, targets) =>
		{
			if (targets.Count != 1)
			{
				GD.PrintErr($"Skill {name} with single target effect cannot have more than one target.");
				return;
			}

			await effect(self, targets[0]);
		}){}
}
