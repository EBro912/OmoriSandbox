using System.Collections.Generic;
using Godot;
using Path = System.IO.Path;

namespace OmoriSandbox.Battle;

/// <summary>
/// The three followup directions a set can assign.
/// </summary>
public enum FollowupInput
{
	/// <summary>The top bubble, selected with UP.</summary>
	Up,
	/// <summary>The side bubble, selected with RIGHT or LEFT, depending on orientation.</summary>
	Horizontal,
	/// <summary>The bottom bubble, selected with DOWN.</summary>
	Down
}

/// <summary>
/// A single followup bubble, including the skill it casts, the party slot it targets, and its graphics.
/// </summary>
public readonly struct FollowupEntry
{
	/// <summary>
	/// The party slot (0-3) that must be occupied and alive for this followup to be usable.
	/// </summary>
	public int TargetPosition { get; }

	/// <summary>
	/// The skill cast by this followup.
	/// </summary>
	public string BaseSkillName { get; }

	internal string TexturePath { get; }
	internal Rect2? TextureRegion { get; }
	internal Texture2D Texture { get; }

	internal FollowupEntry(int targetPosition, string baseSkillName, string texturePath, Rect2? textureRegion)
	{
		TargetPosition = targetPosition;
		BaseSkillName = baseSkillName;
		TexturePath = texturePath;
		TextureRegion = textureRegion;
		Texture = null;
	}

	private FollowupEntry(int targetPosition, string baseSkillName, Texture2D texture, Rect2? textureRegion)
	{
		TargetPosition = targetPosition;
		BaseSkillName = baseSkillName;
		TexturePath = null;
		TextureRegion = textureRegion;
		Texture = texture;
	}

	/// <summary>
	/// Creates an entry with a bubble texture loaded from the mods folder.<br/>
	/// Must be a full path from the mods folder, e.g. <c>MyMod/sprites/bubble_up.png</c>.<br/>
	/// Recommended size: 127x99, matching the vanilla bubbles.
	/// </summary>
	/// <param name="targetPosition">The party slot (0-3) that must be alive for this followup.</param>
	/// <param name="baseSkillName">The skill cast by this followup.</param>
	/// <param name="path">The path to the bubble texture, from the mods folder.</param>
	public static FollowupEntry FromModTexture(int targetPosition, string baseSkillName, string path)
	{
		return new FollowupEntry(targetPosition, baseSkillName, LoadModTexture(path), null);
	}

	/// <summary>
	/// Creates an entry using a texture directly, e.g. a region of the vanilla bubble
	/// atlas <c>res://assets/system/ACS_Bubble.png</c>.
	/// </summary>
	/// <param name="targetPosition">The party slot (0-3) that must be alive for this followup.</param>
	/// <param name="baseSkillName">The skill cast by this followup.</param>
	/// <param name="texture">The bubble texture.</param>
	/// <param name="region">An optional region into <paramref name="texture"/>.</param>
	public static FollowupEntry FromTexture(int targetPosition, string baseSkillName, Texture2D texture, Rect2? region = null)
	{
		return new FollowupEntry(targetPosition, baseSkillName, texture, region);
	}

	private static Texture2D LoadModTexture(string path)
	{
		if (string.IsNullOrWhiteSpace(path) || path.Contains("..") || path.Contains("://") || Path.IsPathRooted(path))
		{
			GD.PushError($"Invalid followup bubble path '{path}' (path traversal not allowed)");
			return null;
		}
		if (!FileAccess.FileExists("user://mods/" + path))
		{
			GD.PushError("Failed to find followup bubble at path: user://mods/" + path);
			return null;
		}
		return ImageTexture.CreateFromImage(Image.LoadFromFile("user://mods/" + path));
	}

	/// <summary>
	/// Whether this followup triggers Release Energy.
	/// </summary>
	public bool IsReleaseEnergy => BaseSkillName.StartsWith("ReleaseEnergy");
	internal int Cost => IsReleaseEnergy ? 10 : 3;

	// a mod texture that failed to load leaves both sources null
	internal bool HasTexture => Texture != null || TexturePath != null;

	internal Texture2D ResolveTexture()
	{
		return Texture ?? ResourceLoader.Load<Texture2D>(TexturePath);
	}
}

internal sealed class FollowupSet
{
	public required string Id { get; init; }
	// tracks whether the followup has multiple tiers in order to append to the skill name
	public bool Tiered { get; init; } = true;
	public required IReadOnlyDictionary<FollowupInput, FollowupEntry> Entries { get; init; }
}


internal static class FollowupSets
{
	public const string NoneId = "None";
	private const string BubbleSheet = "res://assets/system/ACS_Bubble.png";
	
	private static readonly List<FollowupSet> Sets =
	[
		new()
		{
			Id = "Omori",
			Entries = new Dictionary<FollowupInput, FollowupEntry>
			{
				{ FollowupInput.Up, new FollowupEntry(0, "AttackAgain", BubbleSheet, new Rect2(0, -1, 127, 99)) },
				{ FollowupInput.Horizontal, new FollowupEntry(0, "Trip", BubbleSheet, new Rect2(127, 0, 127, 99)) },
				{ FollowupInput.Down, new FollowupEntry(0, "ReleaseEnergy", BubbleSheet, new Rect2(258, 0, 127, 99)) },
			},
		},
		new()
		{
			Id = "Aubrey",
			Entries = new Dictionary<FollowupInput, FollowupEntry>
			{
				{ FollowupInput.Up, new FollowupEntry(3, "LookAtHero", BubbleSheet, new Rect2(512.51f, -1, 127, 99)) },
				{ FollowupInput.Horizontal, new FollowupEntry(2, "LookAtKel", BubbleSheet, new Rect2(640, 0, 127, 99)) },
				{ FollowupInput.Down, new FollowupEntry(0, "LookAtOmori", BubbleSheet, new Rect2(768, 0, 127, 99)) },
			},
		},
		new()
		{
			Id = "Kel",
			Entries = new Dictionary<FollowupInput, FollowupEntry>
			{
				{ FollowupInput.Up, new FollowupEntry(3, "PassToHero", BubbleSheet, new Rect2(256, 95, 127, 99)) },
				{ FollowupInput.Horizontal, new FollowupEntry(1, "PassToAubrey", BubbleSheet, new Rect2(129, 95, 127, 99)) },
				{ FollowupInput.Down, new FollowupEntry(0, "PassToOmori", BubbleSheet, new Rect2(2, 96, 127, 99)) },
			},
		},
		new()
		{
			Id = "Hero",
			Entries = new Dictionary<FollowupInput, FollowupEntry>
			{
				{ FollowupInput.Up, new FollowupEntry(1, "CallAubrey", BubbleSheet, new Rect2(512.51f, 97, 127, 99)) },
				{ FollowupInput.Horizontal, new FollowupEntry(0, "CallOmori", BubbleSheet, new Rect2(384, 95, 127, 99)) },
				{ FollowupInput.Down, new FollowupEntry(2, "CallKel", BubbleSheet, new Rect2(642, 97, 127, 99)) },
			},
		},
		new()
		{
			Id = "Basil",
			Tiered = false,
			Entries = new Dictionary<FollowupInput, FollowupEntry>
			{
				{ FollowupInput.Up, new FollowupEntry(0, "Vent", "res://assets/system/ACS_Bubble_Vent.png", null) },
				{ FollowupInput.Horizontal, new FollowupEntry(0, "Mull", "res://assets/system/ACS_Bubble_Mull.png", null) },
				{ FollowupInput.Down, new FollowupEntry(0, "Comfort", "res://assets/system/ACS_Bubble_Comfort.png", null) },
			},
		},
	];

	public static IReadOnlyList<FollowupSet> All => Sets;

	internal static void AddModded(FollowupSet set) => Sets.Add(set);

	private static bool IsRightSide(int position) => position >= 2;

	public static InputDirection DirectionFor(FollowupInput input, int position)
	{
		return input switch
		{
			FollowupInput.Up => InputDirection.Up,
			FollowupInput.Down => InputDirection.Down,
			_ => IsRightSide(position) ? InputDirection.Left : InputDirection.Right
		};
	}

	public static FollowupInput? InputFor(InputDirection direction, int position)
	{
		return direction switch
		{
			InputDirection.Up => FollowupInput.Up,
			InputDirection.Down => FollowupInput.Down,
			InputDirection.Left when IsRightSide(position) => FollowupInput.Horizontal,
			InputDirection.Right when !IsRightSide(position) => FollowupInput.Horizontal,
			_ => null
		};
	}

	public static FollowupSet Get(string id)
	{
		if (string.IsNullOrEmpty(id) || id == NoneId)
			return null;

		foreach (FollowupSet set in All)
		{
			if (set.Id == id)
				return set;
		}

		GD.PushWarning($"Unknown followup set \"{id}\", disabling followups");
		return null;
	}

	public static string DefaultIdForPosition(int position)
	{
		return position switch
		{
			0 => "Omori",
			1 => "Aubrey",
			2 => "Kel",
			3 => "Hero",
			_ => NoneId
		};
	}

	// helper to resolve older presets that predate the followup changes
	public static string ResolveId(BattlePreset preset, BattlePresetActor actor)
	{
		if (!string.IsNullOrEmpty(actor.FollowupSet))
		{
			if (actor.FollowupSet == NoneId || Get(actor.FollowupSet) != null)
				return actor.FollowupSet;
			return NoneId;
		}

		if (actor.FollowupsDisabled)
			return NoneId;
		if (preset.BasilFollowups && actor.Position == 2)
			return "Basil";
		return DefaultIdForPosition(actor.Position);
	}
	
	internal static void WarnMissingSkills(FollowupSet set, BattlePreset preset)
	{
		foreach (FollowupEntry entry in set.Entries.Values)
		{
			string name = entry.BaseSkillName;
			if (entry.IsReleaseEnergy)
				name += preset.BasilReleaseEnergy ? "Basil" : preset.FollowupTier.ToString();
			else if (set.Tiered)
				name += preset.FollowupTier;
			if (!Database.TryGetSkill(name, out _))
				GD.PushWarning($"Followup set \"{set.Id}\": skill \"{name}\" is not registered, followup will do nothing");
		}
	}
}
