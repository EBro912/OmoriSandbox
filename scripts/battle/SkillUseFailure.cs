namespace OmoriSandbox.Battle;

/// <summary>
/// The reason why an actor can't use a skill right now, or <see cref="None"/> if it can. See <see cref="Actors.Actor.GetUseFailure(Skill)"/>.
/// </summary>
public enum SkillUseFailure
{
	/// <summary>The skill can be used.</summary>
	None,
	/// <summary>The skill's requirement isn't met, or a stat modifier seals the skill.</summary>
	Requirement,
	/// <summary>The skill can only be used through forced commands.</summary>
	UseDisallowed,
	/// <summary>The skill is cooling down.</summary>
	Cooldown,
	/// <summary>Not enough JUICE.</summary>
	Juice,
	/// <summary>Not enough HP to pay the skill's HP cost.</summary>
	HP,
	/// <summary>Not enough Energy.</summary>
	Energy
}
