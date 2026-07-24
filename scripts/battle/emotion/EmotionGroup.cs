using Godot;

namespace OmoriSandbox.Battle.Emotions;

/// <summary>
/// A group of emotions that participates in the emotion advantage triangle.<br/>
/// The group's <see cref="Id"/> doubles as its attack "element".
/// </summary>
public sealed class EmotionGroup
{
	/// <summary>
	/// The unique id of the group, and it's "element".
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// The id of the group this group deals bonus damage to.
	/// </summary>
	public string BeatsGroupId { get; private set; }

	/// <summary>
	/// The battle log line shown when an emotion of this group cannot be raised any further.<br/>
	/// Example: <c>"[target] can't get HAPPIER!"</c>.
	/// </summary>
	public string MaxTierMessage { get; private set; }

	/// <summary>
	/// Whether emotions of this group are included in the pool of random emotions.
	/// </summary>
	public bool IncludedInRandomEmotion { get; private set; }
	
	internal bool Registered;

	private bool Frozen()
	{
		if (!Registered)
			return false;
		GD.PushWarning($"EmotionGroup {Id} is already registered and can no longer be modified!");
		return true;
	}

	/// <summary>
	/// Creates a new emotion group.
	/// </summary>
	/// <param name="id">The unique id of the group, and it's "element".</param>
	public EmotionGroup(string id)
	{
		Id = id;
	}

	/// <summary>
	/// Sets the group this group deals bonus damage to.
	/// </summary>
	/// <param name="groupId">The id of the group this group beats.</param>
	public EmotionGroup WithBeatsGroup(string groupId)
	{
		if (Frozen())
			return this;
		BeatsGroupId = groupId;
		return this;
	}

	/// <summary>
	/// Sets the battle log line shown when an emotion of this group cannot be raised any further.
	/// </summary>
	/// <param name="message">The message to show. Supports the <c>[target]</c> placeholder.</param>
	public EmotionGroup WithMaxTierMessage(string message)
	{
		if (Frozen())
			return this;
		MaxTierMessage = message;
		return this;
	}

	/// <summary>
	/// Allows emotions of this group to be rolled by effects that inflict a random emotion.
	/// </summary>
	public EmotionGroup WithRandomEmotion()
	{
		if (Frozen())
			return this;
		IncludedInRandomEmotion = true;
		return this;
	}
}
