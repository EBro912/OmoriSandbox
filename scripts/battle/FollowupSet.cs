using System;
using System.Collections.Generic;
using Godot;
using OmoriSandbox.Actors;
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
/// A single followup bubble, including the skill it casts, the party slot it targets, its energy cost and requirements, and its graphics.
/// </summary>
public readonly struct FollowupEntry
{
	/// <summary>
	/// A <see cref="TargetPosition"/> standing for the party member using the followup set, whatever slot it is in.
	/// </summary>
	public const int User = -1;

	/// <summary>
	/// The party slot (0-3) that must be occupied and alive for this followup to be usable, or <see cref="User"/>.
	/// </summary>
	public int TargetPosition { get; }

	/// <summary>
	/// The skill cast by this followup.
	/// </summary>
	public string BaseSkillName { get; }

	/// <summary>
	/// The energy this followup needs, and spends when it is used. Defaults to 3.
	/// </summary>
	public int EnergyCost { get; private init; }

	/// <summary>
	/// The fewest members the party must have for this followup to be usable. Defaults to 0.
	/// </summary>
	public int MinPartySize { get; private init; }

	/// <summary>
	/// Whether every party member must be alive for this followup to be usable.
	/// </summary>
	public bool RequiresAllAlive { get; private init; }

	/// <summary>
	/// Whether this followup is a Release Energy, which shakes its bubble while usable.
	/// </summary>
	/// <remarks>
	/// The "Release Energy with Any Living Party" setting ignores <see cref="MinPartySize"/> for these entries.
	/// </remarks>
	public bool IsReleaseEnergy { get; private init; }

	/// <summary>
	/// Whether the battle's followup tier (1-3) is appended to the skill name. Null will follow the set.
	/// </summary>
	public bool? Tiered { get; private init; }

	/// <summary>
	/// An extra check ran on the party member using the followup. Null means no extra check.
	/// </summary>
	public Func<PartyMember, bool> Requirement { get; private init; }

	private string TexturePath { get; }
	internal Rect2? TextureRegion { get; }
	private Texture2D Texture { get; }

	internal FollowupEntry(int targetPosition, string baseSkillName, string texturePath, Rect2? textureRegion)
	{
		TargetPosition = targetPosition;
		BaseSkillName = baseSkillName;
		TexturePath = texturePath;
		TextureRegion = textureRegion;
		Texture = null;
		EnergyCost = 3;
	}

	private FollowupEntry(int targetPosition, string baseSkillName, Texture2D texture, Rect2? textureRegion)
	{
		TargetPosition = targetPosition;
		BaseSkillName = baseSkillName;
		TexturePath = null;
		TextureRegion = textureRegion;
		Texture = texture;
		EnergyCost = 3;
	}

	/// <summary>
	/// Creates an entry with a bubble texture loaded from the mods folder.<br/>
	/// Must be a full path from the mods folder, e.g. <c>MyMod/sprites/bubble_up.png</c>.<br/>
	/// Recommended size: 127x99, matching the vanilla bubbles.
	/// </summary>
	/// <param name="targetPosition">The party slot (0-3) that must be alive for this followup, or <see cref="User"/>.</param>
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
	/// <param name="targetPosition">The party slot (0-3) that must be alive for this followup, or <see cref="User"/>.</param>
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
	/// Sets the energy cost (0-10) of the entry.
	/// </summary>
	/// <param name="cost">The energy needed and spent.</param>
	public FollowupEntry WithEnergyCost(int cost) => this with { EnergyCost = cost };

	/// <summary>
	/// Sets the required alive party members (0-4) for the entry.
	/// </summary>
	/// <param name="size">The fewest party members.</param>
	public FollowupEntry WithMinPartySize(int size) => this with { MinPartySize = size };

	/// <summary>
	/// Marks the entry as requiring every party member alive, regardless of party size.
	/// </summary>
	public FollowupEntry WithAllAlive() => this with { RequiresAllAlive = true };

	/// <summary>
	/// Marks this entry as a Release Energy entry. See <see cref="IsReleaseEnergy"/>.
	/// </summary>
	public FollowupEntry WithReleaseEnergy() => this with { IsReleaseEnergy = true };

	/// <summary>
	/// Marks whether the tier number is appended to the skill name.
	/// </summary>
	/// <param name="tiered">Whether the tier (1-3) is appended to the skill name.</param>
	public FollowupEntry WithTiered(bool tiered) => this with { Tiered = tiered };

	/// <summary>
	/// Adds a custom requirement for the followup to be used.
	/// </summary>
	/// <param name="requirement">Returns true if the followup can be used, given the party member using it.</param>
	public FollowupEntry WithRequirement(Func<PartyMember, bool> requirement) => this with { Requirement = requirement };

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
				{ FollowupInput.Up, new FollowupEntry(FollowupEntry.User, "AttackAgain", BubbleSheet, new Rect2(0, -1, 127, 99)) },
				{ FollowupInput.Horizontal, new FollowupEntry(FollowupEntry.User, "Trip", BubbleSheet, new Rect2(127, 0, 127, 99)) },
				{ FollowupInput.Down, new FollowupEntry(FollowupEntry.User, "ReleaseEnergy", BubbleSheet, new Rect2(258, 0, 127, 99))
					.WithEnergyCost(10).WithMinPartySize(4).WithAllAlive().WithReleaseEnergy() },
			},
		},
		new()
		{
			Id = "Omori (Basil RE)",
			Entries = new Dictionary<FollowupInput, FollowupEntry>
			{
				{ FollowupInput.Up, new FollowupEntry(FollowupEntry.User, "AttackAgain", BubbleSheet, new Rect2(0, -1, 127, 99)) },
				{ FollowupInput.Horizontal, new FollowupEntry(FollowupEntry.User, "Trip", BubbleSheet, new Rect2(127, 0, 127, 99)) },
				{ FollowupInput.Down, new FollowupEntry(FollowupEntry.User, "ReleaseEnergyBasil", BubbleSheet, new Rect2(258, 0, 127, 99))
					.WithEnergyCost(10).WithMinPartySize(2).WithAllAlive().WithReleaseEnergy().WithTiered(false) },
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
				// PassToHero reads the position 1 member's ATK, so that slot must be filled
				{ FollowupInput.Up, new FollowupEntry(3, "PassToHero", BubbleSheet, new Rect2(256, 95, 127, 99))
					.WithRequirement(_ => BattleManager.Instance.GetPartyMemberAtPosition(1) != null) },
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
		string id;
		if (!string.IsNullOrEmpty(actor.FollowupSet))
		{
			id = actor.FollowupSet == NoneId || Get(actor.FollowupSet) != null
				? actor.FollowupSet
				: NoneId;
		}
		else if (actor.FollowupsDisabled)
			id = NoneId;
		else if (preset.BasilFollowups && actor.Position == 2)
			id = "Basil";
		else
			id = DefaultIdForPosition(actor.Position);

		// the legacy checkbox swapped Omori's ReleaseEnergy for the Basil variant
		if (id == "Omori" && preset.BasilReleaseEnergy)
			id = "Omori (Basil RE)";
		return id;
	}
	
	internal static string SkillNameFor(FollowupEntry entry, FollowupSet set, int tier)
	{
		return (entry.Tiered ?? set.Tiered) ? entry.BaseSkillName + tier : entry.BaseSkillName;
	}

	internal static void WarnMissingSkills(FollowupSet set, BattlePreset preset)
	{
		foreach (FollowupEntry entry in set.Entries.Values)
		{
			string name = SkillNameFor(entry, set, preset.FollowupTier);
			if (!Database.TryGetSkill(name, out _))
				GD.PushWarning($"Followup set \"{set.Id}\": skill \"{name}\" is not registered, followup will do nothing");
		}
	}
}
