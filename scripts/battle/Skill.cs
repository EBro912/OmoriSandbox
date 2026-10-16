using Godot;
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
	/// Whether this skill can't be used outside forced commands. See <see cref="WithUseDisallowed"/>.
	/// </summary>
	public bool UseDisallowed { get; private set; }
	/// <summary>
	/// The ids of the stat modifiers an actor with this skill equipped gets when the battle starts. See <see cref="WithPassiveStatModifier"/>.
	/// </summary>
	public IReadOnlyList<string> PassiveStatModifiers => PassiveModifiers;

	private readonly List<string> PassiveModifiers = [];

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

	/// <summary>
	/// The flat part of this skill's HP cost, ignoring any cost modifiers. See <see cref="WithHPCost"/>.
	/// </summary>
	public int BaseHPCost { get; private set; }

	/// <summary>
	/// The part of this skill's HP cost that scales with the user's max HP, as a fraction. See <see cref="WithHPCost"/>.
	/// </summary>
	public float BaseHPCostRate { get; private set; }

	/// <summary>
	/// The Energy cost of this skill, ignoring any cost modifiers. See <see cref="WithEnergyCost"/>.
	/// </summary>
	public int BaseEnergyCost { get; private set; }

	/// <summary>
	/// The cost of the skill in HP, taking any skill cost changes into account.
	/// </summary>
	/// <param name="actor">The actor to use.</param>
	/// <returns>The HP the actor pays to use this skill.</returns>
	public int HPCost(Actor actor)
	{
		double rate = (double)(decimal)BaseHPCostRate;
		int hp = (int)Math.Floor(Math.Max(0d, BaseHPCost + actor.CurrentStats.MaxHP * rate));
		foreach (StatModifier modifier in actor.StatModifiers.Values)
			modifier.OverrideHPCost(ref hp, actor);
		return hp;
	}

	/// <summary>
	/// The cost of the skill in Energy, taking any skill cost changes into account.
	/// </summary>
	/// <remarks>
	/// Only party members pay Energy costs.
	/// </remarks>
	/// <param name="actor">The actor to use.</param>
	/// <returns>The Energy the party pays when the actor uses this skill.</returns>
	public int EnergyCost(Actor actor)
	{
		int energy = BaseEnergyCost;
		foreach (StatModifier modifier in actor.StatModifiers.Values)
			modifier.OverrideEnergyCost(ref energy, actor);
		return energy;
	}

	/// <summary>
	/// How many turns this skill can't be used after each use. See <see cref="WithCooldown"/>.
	/// </summary>
	public int Cooldown { get; private set; }

	/// <summary>
	/// Makes this skill unusable for the given number of turns after each use.
	/// </summary>
	/// <remarks>
	/// A skill with a cooldown of 2 used on turn 1 can be used again on turn 4. It also can't be used again later in the turn it was used in.
	/// Each actor has its own cooldowns. See <see cref="Actor.GetSkillCooldown(Skill)"/>.
	/// </remarks>
	/// <param name="turns">The number of turns, at least 0.</param>
	public Skill WithCooldown(int turns)
	{
		Cooldown = Math.Max(0, turns);
		return this;
	}

	/// <summary>
	/// Makes this skill cost HP to use.
	/// </summary>
	/// <remarks>
	/// The cost is <paramref name="flat"/> plus <paramref name="maxHPRate"/> times the user's max HP, rounded down.
	/// The user needs more HP than the cost to use the skill, so paying it leaves them with at least 1 HP. A forced use pays it anyway and can knock the user out.
	/// </remarks>
	/// <param name="flat">A fixed amount of HP.</param>
	/// <param name="maxHPRate">A fraction of the user's max HP, e.g. <c>0.2f</c> for 20%.</param>
	public Skill WithHPCost(int flat, float maxHPRate = 0f)
	{
		BaseHPCost = flat;
		BaseHPCostRate = maxHPRate;
		return this;
	}

	/// <summary>
	/// Makes this skill cost Energy to use.
	/// </summary>
	/// <remarks>
	/// Only party members pay it; enemies ignore Energy costs. A follow-up's Energy cost belongs on its follow-up entry, since a follow-up skill with its own Energy cost charges both.
	/// </remarks>
	/// <param name="energy">The Energy the party pays.</param>
	public Skill WithEnergyCost(int energy)
	{
		BaseEnergyCost = energy;
		return this;
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
	/// Gives every actor with this skill equipped the given stat modifier when the battle starts.
	/// </summary>
	/// <remarks>
	/// The modifier is added silently at the start of the battle and of each Boss Rush stage, and when an actor with this skill is summoned.
	/// A tiered modifier starts at tier 1. An actor that already has the modifier only gets its turns refreshed.<br/>
	/// After that it's an ordinary modifier: it counts down if it has turns, and it can be removed like any other.
	/// Use <see cref="StatModifier.WithBypassDeathRemoval"/> or <see cref="StatModifier.WithBypassRecoverAllRemoval"/> to make it last.<br/>
	/// Call this more than once to give several modifiers. Add <see cref="WithUseDisallowed"/> for a skill that can't be selected.
	/// </remarks>
	/// <param name="modifier">The id of the stat modifier to give.</param>
	public Skill WithPassiveStatModifier(string modifier)
	{
		if (!string.IsNullOrWhiteSpace(modifier))
			PassiveModifiers.Add(modifier);
		return this;
	}

	/// <summary>
	/// Makes this skill unusable: it's greyed out in the skill menu, and <see cref="Actor.CanUse(Skill)"/> returns false for it.
	/// </summary>
	/// <remarks>
	/// Enemy AI that checks <c>CanUse</c> skips it, and so do JSON enemies. A command that uses it anyway is refused like a failed requirement, and it can't be used as a follow-up. Forced commands can still use it.
	/// </remarks>
	public Skill WithUseDisallowed()
	{
		UseDisallowed = true;
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

	/// <summary>
	/// The color of this skill's name in the skill menu, or null for the default color. See <see cref="WithMenuColor"/>.
	/// </summary>
	public Color? MenuColor { get; private set; }

	/// <summary>
	/// The color of this skill's name in the skill menu while it can't be used, or null to fade <see cref="MenuColor"/>. See <see cref="WithMenuColor"/>.
	/// </summary>
	public Color? MenuDisabledColor { get; private set; }

	/// <summary>
	/// Sets the color of this skill's name in the skill menu.
	/// </summary>
	/// <remarks>
	/// While the skill can't be used, its name uses <paramref name="disabledColor"/>. Without one, it uses <paramref name="color"/> with the menu's usual disabled transparency.
	/// </remarks>
	/// <param name="color">The name's color while the skill can be used.</param>
	/// <param name="disabledColor">The name's color while the skill can't be used, or null to fade <paramref name="color"/>.</param>
	public Skill WithMenuColor(Color color, Color? disabledColor = null)
	{
		MenuColor = color;
		MenuDisabledColor = disabledColor;
		return this;
	}

	internal Color? MenuFontColor(bool usable)
	{
		if (usable)
			return MenuColor;
		if (MenuDisabledColor != null)
			return MenuDisabledColor;
		if (MenuColor is Color color)
			return new Color(color.R, color.G, color.B, color.A * 160f / 255f);
		return new Color(1f, 1f, 1f, 160f / 255f);
	}
}
