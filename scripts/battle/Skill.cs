using OmoriSandbox.Actors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Battle;

/// <summary>
/// A skill, including regular attacks
/// </summary>
public class Skill : BattleAction
{
	/// <summary>
	/// The base cost of the skill in Juice, ignoring any cost modifiers.
	/// </summary>
	/// <remarks>
	/// Use <see cref="Cost(Actor)"/> for an accurate juice calculation.
	/// </remarks>
	public int BaseCost { get; private set; }
	/// <summary>
	/// Whether this skill is hidden in the skill menu.
	/// </summary>
	public bool Hidden { get; private set; }
	/// <summary>
	/// Whether this skill causes the followup bubbles to appear when used by a <see cref="PartyMember"/>.
	/// </summary>
	public bool ShowFollowups { get; private set; }

	/// <summary>
	/// Whether the given <param name="actor"> meets the requirements to use this skill.</param>
	/// </summary>
	/// <remarks>
	/// By default, all skills have the requirement that the user's emotion does not block actions (afraid/stressed).
	/// Juice cost is an inherent requirement for all skills and cannot be modified.
	/// </remarks>
	/// <param name="actor">The actor to check.</param>
	/// <returns>True if the actor can use this skill at the given moment.</returns>
	public bool MeetsRequirements(Actor actor) => Requirement(actor);

	/// <summary>
	/// The cost of the skill in Juice, taking any skill cost reductions into account.
	/// </summary>
	/// <param name="actor">The actor to use.</param>
	/// <returns>The cost of the skill in Juice after any cost reductions are applied.</returns>
	public int Cost(Actor actor)
	{
		int cost = BaseCost;
		foreach (StatModifier modifier in actor.StatModifiers.Values)
		{
			modifier.OverrideJuiceCost(ref cost, actor);
		}

		return cost;
	}
	
	private Func<Actor, bool> Requirement = actor => !actor.CurrentEmotion.BlocksActions;

	/// <summary>
	/// The battle log message shown when this skill's requirement fails at execution time
	/// for a reason other than the actor being afraid/stressed. When null, nothing is shown.
	/// Supports the [actor] tag.
	/// </summary>
	public string RequirementFailureMessage { get; private set; }

	/// <summary>
	/// Sets a custom battle log message for when this skill's requirement fails at execution
	/// time. The default afraid/stressed failure keeps its own message regardless.
	/// </summary>
	public Skill WithRequirementFailureMessage(string message)
	{
		RequirementFailureMessage = message;
		return this;
	}

	/// <summary>
	/// Creates a new single-target skill. Must be registered via <see cref="Modding.Mod.RegisterSkill(string, Skill)"/> to appear in-game.
	/// </summary>
	/// <param name="name">The name of the skill.</param>
	/// <param name="description">The description of the skill.<br/>You can use the [actor] tag to place the actor's name in the description.</param>
	/// <param name="target">What this skill targets. Mainly used for visual targeting.</param>
	/// <param name="effect">The code that runs when this skill is used.</param>
	/// <param name="cost">How much juice this skill costs to use.</param>
	/// <param name="hidden">Whether this skill should show up in the actor's skill list.</param>
	/// <param name="priority">The skill's speed, added to the user's SPD for turn order.</param>
	/// <param name="showFollowups">Whether this skill causes the followup bubbles to appear.</param>
	/// <param name="setup">Optional code that runs before the skill applies, like an RPG Maker <c>setup action</c>.</param>
	public Skill(string name, string description, SkillTarget target, Func<Actor, Actor, Task> effect, int cost, bool hidden = false, int priority = 0, bool showFollowups = false, Func<Actor, Actor, Task> setup = null)
		: base(name, description, target, priority, effect, setup)
	{
		BaseCost = cost;
		Hidden = hidden;
		ShowFollowups = showFollowups;
	}
	
	/// <summary>
	/// Creates a new multi-target skill. Must be registered via <see cref="Modding.Mod.RegisterSkill(string, Skill)"/> to appear in-game.
	/// </summary>
	/// <param name="name">The name of the skill.</param>
	/// <param name="description">The description of the skill.<br/>You can use the [actor] tag to place the actor's name in the description.</param>
	/// <param name="target">What this skill targets. Mainly used for visual targeting.</param>
	/// <param name="effect">The code that runs when this skill is used.</param>
	/// <param name="cost">How much juice this skill costs to use.</param>
	/// <param name="hidden">Whether this skill should show up in the actor's skill list.</param>
	/// <param name="priority">The skill's speed, added to the user's SPD for turn order.</param>
	/// /// <param name="showFollowups">Whether this skill causes the followup bubbles to appear.</param>
	/// <param name="setup">Optional code that runs before the skill applies, like an RPG Maker <c>setup action</c>.</param>
	public Skill(string name, string description, SkillTarget target, Func<Actor, IReadOnlyList<Actor>, Task> effect, int cost, bool hidden = false, int priority = 0, bool showFollowups = false, Func<Actor, IReadOnlyList<Actor>, Task> setup = null)
		: base(name, description, target, priority, effect, setup)
	{
		BaseCost = cost;
		Hidden = hidden;
		ShowFollowups = showFollowups;
	}

	/// <summary>
	/// Adds a custom requirement for this skill to be used.
	/// If the actor does not meet the requirements to use the skill, the skill will be greyed out in the menu.
	/// </summary>
	/// <remarks>
	/// By default, all skills have the requirement that the user is not afraid or stressed.
	/// By adding a custom requirement, you must also re-add those checks in yourself if necessary.<br/>
	/// A party member's basic attack (their first skill) should use <c>WithCustomRequirement(_ =&gt; true)</c> so they can still attack while AFRAID or STRESSED OUT, like the base game's party members.<br/>
	/// Juice cost is an inherent requirement for all skills and cannot be modified.
	/// </remarks>
	public Skill WithCustomRequirement(Func<Actor, bool> requirement)
	{
		Requirement = requirement;
		return this;
	}

	/// <summary>
	/// How many random targets a <see cref="SkillTarget.XRandomEnemies"/> skill hits. Defaults to 1. See <see cref="WithRandomTargets"/>.
	/// </summary>
	public int RandomTargetCount { get; private set; } = 1;

	/// <summary>
	/// Sets how many random targets a <see cref="SkillTarget.XRandomEnemies"/> skill hits. The same target can be picked more than once.
	/// </summary>
	/// <param name="count">The number of targets, at least 1.</param>
	public Skill WithRandomTargets(int count)
	{
		RandomTargetCount = Math.Max(1, count);
		return this;
	}

	/// <summary>
	/// Sets what this skill records in <see cref="Actor.LastTarget"/> when it starts.
	/// </summary>
	/// <param name="record">What to record. Defaults to <see cref="LastTargetRecord.Target"/>.</param>
	public Skill WithLastTarget(LastTargetRecord record)
	{
		LastTargetRecord = record;
		return this;
	}

	/// <summary>
	/// Makes tier step-ups on this skill's targets follow the first target's next tier, like many of the base game's multi-target skills.
	/// </summary>
	/// <remarks>
	/// The "Per-Target Buff Tiers" setting turns this off.
	/// </remarks>
	public Skill WithTiersFromFirstTarget()
	{
		TiersFromFirstTarget = true;
		return this;
	}
}
