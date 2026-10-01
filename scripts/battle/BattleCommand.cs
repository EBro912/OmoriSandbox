using System.Collections.Generic;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle;

/// <summary>
/// A command that an actor can be given in battle.
/// </summary>
public class BattleCommand
{
    /// <summary>
    /// The <see cref="Actor"/> this command is being given to.
    /// </summary>
    public readonly Actor Actor;

    /// <summary>
    /// The <see cref="Actor"/>s this command is targeting.
    /// </summary>
    public IReadOnlyList<Actor> Targets;

    /// <summary>
    /// The <see cref="BattleAction"/> that is carried out when this command is performed.
    /// </summary>
    public readonly BattleAction Action;

    internal bool Forced { get; init; }

	/// <summary>
	/// Creates a new <see cref="BattleCommand"/> context with a single target.
	/// </summary>
	/// <param name="actor">The <see cref="Actor"/> this command is being given to.</param>
	/// <param name="target">The <see cref="Actor"/> this command is targeting.</param>
	/// <param name="action">The <see cref="BattleAction"/> that is carried out when this command is performed.</param>
    public BattleCommand(Actor actor, Actor target, BattleAction action)
	{
		Actor = actor;
		Targets = [target];
		Action = action;
	}

	/// <summary>
	/// Creates a new <see cref="BattleCommand"/> context with multiple targets.
	/// </summary>
	/// <param name="actor">The <see cref="Actor"/> this command is being given to.</param>
	/// <param name="targets">The <see cref="Actor"/>s this command is targeting.</param>
	/// <param name="action">The <see cref="BattleAction"/> that is carried out when this command is performed.</param>
	public BattleCommand(Actor actor, IReadOnlyList<Actor> targets, BattleAction action)
	{
		Actor = actor;
		Targets = targets;
		Action = action;
	}
}
