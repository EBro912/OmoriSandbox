using System.Collections.Generic;
using Godot;

namespace OmoriSandbox.Battle;

internal enum FollowupInput
{
	Up,
	Horizontal,
	Down
}

internal readonly record struct FollowupEntry(int TargetPosition, string BaseSkillName, string TexturePath, Rect2? TextureRegion)
{
	public bool IsReleaseEnergy => BaseSkillName.StartsWith("ReleaseEnergy");
	public int Cost => IsReleaseEnergy ? 10 : 3;
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

	public static readonly IReadOnlyList<FollowupSet> All =
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
}
