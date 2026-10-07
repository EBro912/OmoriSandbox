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
	/// The action's speed, like RPG Maker's skill and item speed: turn order sorts by the user's SPD plus this value.
	/// </summary>
	public int Priority {get; private set;}
	/// <summary>
	/// What happens when this action is performed.
	/// </summary>
	public Func<Actor, IReadOnlyList<Actor>, Task> Effect { get; }
	/// <summary>
	/// Optional code that runs before this action applies, like an RPG Maker <c>setup action</c>.<br/>
	/// </summary>
	public Func<Actor, IReadOnlyList<Actor>, Task> Setup { get; }
	/// <summary>
	/// What this action records in <see cref="Actor.LastTarget"/> when it starts. Defaults to <see cref="LastTargetRecord.Target"/>.
	/// </summary>
	public LastTargetRecord LastTargetRecord { get; protected set; }
	/// <summary>
	/// Whether tier step-ups on this action's targets follow the first target. Set with <see cref="Skill.WithTiersFromFirstTarget"/> or <see cref="Item.WithTiersFromFirstTarget"/>.
	/// </summary>
	public bool TiersFromFirstTarget { get; protected set; }

	protected BattleAction(string name, string description, SkillTarget target, int priority, Func<Actor, IReadOnlyList<Actor>, Task> effect,
		Func<Actor, IReadOnlyList<Actor>, Task> setup = null)
	{
		Name = name;
		Description = description;
		Target = target;
		Effect = effect;
		Setup = setup;
		Priority = priority;
	}

	protected BattleAction(string name, string description, SkillTarget target, int priority,
		Func<Actor, Actor, Task> effect, Func<Actor, Actor, Task> setup = null)
		: this(name, description, target, priority, async (self, targets) =>
		{
			if (targets.Count != 1)
			{
				GD.PrintErr($"Skill {name} with single target effect cannot have more than one target.");
				return;
			}
			await effect(self, targets[0]);
		}, setup == null ? null : async (self, targets) =>
		{
			if (targets.Count == 1)
				await setup(self, targets[0]);
		}){}
}
