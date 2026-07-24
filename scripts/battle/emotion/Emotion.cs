using Godot;
using System.Collections.Generic;

namespace OmoriSandbox.Battle.Emotions;

/// <summary>
/// A single emotion definition, including the identity, group and tier, stat bonuses, battle traits, and assets.<br/>
/// Mods can register new emotions with <c>Mod.RegisterEmotion</c>.
/// </summary>
public sealed class Emotion
{
	/// <summary>
	/// The unique id of the emotion.
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// The name shown in battle messages ("NAME feels HAPPY!"). Defaults to the uppercased <see cref="Id"/>.
	/// </summary>
	public string DisplayName { get; private set; }

	/// <summary>
	/// The sprite animation played while feeling this emotion. Defaults to <see cref="Id"/>.
	/// </summary>
	public string AnimationName { get; private set; }

	/// <summary>
	/// The id of the group this emotion belongs to, or null for group-less emotions (neutral, afraid, stressed).
	/// </summary>
	public string GroupId { get; private set; }

	/// <summary>
	/// The resolved <see cref="EmotionGroup"/>. Set by the database when the emotion is registered.
	/// </summary>
	public EmotionGroup Group { get; internal set; }

	/// <summary>
	/// The 0-based tier within the group (happy = 0, ecstatic = 1, manic = 2). 0 for group-less emotions.
	/// </summary>
	public int Tier { get; private set; }

	/// <summary>
	/// The stat bonuses this emotion grants while felt.
	/// </summary>
	public StatBonus[] StatBonuses { get; private set; } = [];

	/// <summary>
	/// Whether this emotion prevents the actor from using most skills, such as afraid and stressed.
	/// </summary>
	public bool BlocksActions { get; private set; }

	/// <summary>
	/// The fraction of incoming damage converted to juice loss instead of HP loss.
	/// </summary>
	public float JuiceBleedFraction { get; private set; }

	/// <summary>
	/// Defensive damage multipliers against specific attack elements by element id.<br/>
	/// </summary>
	public IReadOnlyDictionary<string, float> DefensiveRateOverrides => Rates;
	private readonly Dictionary<string, float> Rates = [];

	/// <summary>
	/// The label/portrait assets shown for this emotion.
	/// </summary>
	public EmotionAsset Asset { get; private set; }
	
	internal bool Registered;

	private bool Frozen()
	{
		if (!Registered)
			return false;
		GD.PushWarning($"Emotion {Id} is already registered and can no longer be modified!");
		return true;
	}

	/// <summary>
	/// Creates a new emotion definition.
	/// </summary>
	/// <param name="id">The unique id of the emotion.</param>
	public Emotion(string id)
	{
		Id = id;
		DisplayName = id.ToUpper();
		AnimationName = id;
	}

	/// <summary>
	/// Sets the name shown in battle messages.
	/// </summary>
	public Emotion WithDisplayName(string displayName)
	{
		if (Frozen())
			return this;
		DisplayName = displayName;
		return this;
	}

	/// <summary>
	/// Sets the sprite animation played while feeling this emotion.
	/// </summary>
	public Emotion WithAnimationName(string animationName)
	{
		if (Frozen())
			return this;
		AnimationName = animationName;
		return this;
	}

	/// <summary>
	/// Places this emotion in a group at the given tier, hooking it into the advantage triangle and escalation ladder.
	/// </summary>
	/// <param name="groupId">The id of the group. Must be registered before this emotion.</param>
	/// <param name="tier">The 0-based tier within the group.</param>
	public Emotion WithGroup(string groupId, int tier)
	{
		if (Frozen())
			return this;
		GroupId = groupId;
		Tier = tier;
		return this;
	}

	/// <summary>
	/// Sets the stat bonuses this emotion grants.
	/// </summary>
	public Emotion WithStatBonuses(params StatBonus[] bonuses)
	{
		if (Frozen())
			return this;
		StatBonuses = bonuses;
		return this;
	}

	/// <summary>
	/// Makes this emotion prevent the actor from using most skills, like afraid and stressed.
	/// </summary>
	public Emotion WithBlocksActions()
	{
		if (Frozen())
			return this;
		BlocksActions = true;
		return this;
	}

	/// <summary>
	/// Sets the fraction of incoming damage converted to juice loss instead of HP loss.
	/// </summary>
	public Emotion WithJuiceBleed(float fraction)
	{
		if (Frozen())
			return this;
		JuiceBleedFraction = fraction;
		return this;
	}

	/// <summary>
	/// Sets a defensive damage multiplier against a specific attack element. See <see cref="DefensiveRateOverrides"/>.
	/// </summary>
	public Emotion WithDefensiveRate(string elementId, float multiplier)
	{
		if (Frozen())
			return this;
		Rates[elementId] = multiplier;
		return this;
	}

	/// <summary>
	/// Sets the label/portrait assets shown for this emotion.
	/// </summary>
	public Emotion WithAsset(EmotionAsset asset)
	{
		if (Frozen())
			return this;
		Asset = asset;
		return this;
	}
}
