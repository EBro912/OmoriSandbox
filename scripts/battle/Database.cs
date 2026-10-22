using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Actors;
using OmoriSandbox.Editor;
using OmoriSandbox.Extensions;
using OmoriSandbox.Menu;
using OmoriSandbox.Modding;
using OmoriSandbox.Battle.Registry;

namespace OmoriSandbox.Battle;

/// <summary>
/// The database where all game related data is stored.
/// </summary>
public class Database
{
	// ordinal comparers so registration and lookups are culture-independent
	// by default, culture-sensitive comparison can treat punctuation-differing ids as equal
	internal static readonly SortedDictionary<string, Func<PartyMember>> PartyMembers = new(StringComparer.Ordinal);
	internal static readonly SortedDictionary<string, Func<Enemy>> Enemies = new(StringComparer.Ordinal);
	internal static readonly Dictionary<string, Skill> Skills = [];
	internal static readonly SortedDictionary<string, Item> Items = new(StringComparer.Ordinal);
	internal static readonly SortedDictionary<string, Equipment> Equipment = new(StringComparer.Ordinal);
	internal static readonly Dictionary<string, Func<StatModifier>> Modifiers = [];
	private static readonly Dictionary<string, Texture2D> StateIcons = [];
	private static readonly Dictionary<string, EmotionGroup> EmotionGroups = [];
	private static readonly Dictionary<string, Emotion> Emotions = [];

	// keep a separate list of emotion ids to easily display in menus
	private static readonly List<string> EmotionOrder = [];
	private static readonly Dictionary<(string Group, int Tier), Emotion> EmotionTiers = [];
	private static readonly Dictionary<string, MenuSkin> MenuSkins = new(StringComparer.OrdinalIgnoreCase);
	private static readonly List<string> MenuSkinOrder = [];
	internal static readonly Dictionary<string, WindowSkin> WindowSkins = new(StringComparer.OrdinalIgnoreCase);
	internal static readonly List<string> WindowSkinOrder = [];

	static Database()
	{
		Init();
	}

	/// <summary>
	/// Tries to get a <see cref="Skill"/> of the given <paramref name="name"/> from the database.
	/// </summary>
	/// <param name="name">The name of the skill to search for.</param>
	/// <param name="skill">The returned skill, if a match is found.</param>
	/// <returns>Whether the skill exists in the database.</returns>
	public static bool TryGetSkill(string name, out Skill skill)
	{
		return Skills.TryGetValue(name, out skill);
	}

	/// <summary>
	/// Whether an enemy with the given id is registered, built in or from a mod.
	/// </summary>
	/// <param name="name">The enemy id, as used in presets and by <c>BattleManager.SummonEnemy</c>.</param>
	/// <returns>True if an enemy is registered under that id.</returns>
	public static bool HasEnemy(string name)
	{
		return name != null && Enemies.ContainsKey(name);
	}

	/// <summary>
	/// Whether a party member with the given id is registered, built in or from a mod.
	/// </summary>
	/// <param name="name">The party member id, as used in presets and by <c>BattleManager.SummonPartyMember</c>.</param>
	/// <returns>True if a party member is registered under that id.</returns>
	public static bool HasPartyMember(string name)
	{
		return name != null && PartyMembers.ContainsKey(name);
	}

	/// <summary>
	/// Whether a stat modifier with the given id is registered, built in or from a mod.
	/// </summary>
	/// <remarks>
	/// This checks the registry. To check whether an actor has a modifier, use <see cref="Actor.HasStatModifier"/>.
	/// </remarks>
	/// <param name="name">The stat modifier id, as used by <c>Actor.AddStatModifier</c>.</param>
	/// <returns>True if a stat modifier is registered under that id.</returns>
	public static bool HasStatModifier(string name)
	{
		return name != null && Modifiers.ContainsKey(name);
	}

	/// <summary>
	/// Tries to get an <see cref="Item"/> of the given <paramref name="name"/> from the database.
	/// </summary>
	/// <param name="name">The name of the item to search for.</param>
	/// <param name="item">The returned item, if a match is found.</param>
	/// <returns>Whether the item exists in the database.</returns>
	public static bool TryGetItem(string name, out Item item)
	{
		return Items.TryGetValue(name, out item);
	}
	

	/// <summary>
	/// Tries to get an <see cref="Equipment"/> of the given <paramref name="name"/> from the database.
	/// </summary>
	/// <param name="name">The name of the equipment to search for.</param>
	/// <param name="equipment">The returned equipment, if a match is found.</param>
	/// <returns>Whether the equipment exists in the database.</returns>
	public static bool TryGetEquipment(string name, out Equipment equipment)
	{
		return Equipment.TryGetValue(name, out equipment);
	}

	/// <summary>
	/// Tries to get the <see cref="Texture2D"/> of the given state icon <paramref name="name"/> from the database.
	/// </summary>
	/// <param name="name">The name of the state icon to search for.</param>
	/// <param name="texture">The returned state icon texture, if a match is found.</param>
	/// <returns>Whether the state icon exists in the database.</returns>
	public static bool TryGetStateIcon(string name, out Texture2D texture)
	{
		return StateIcons.TryGetValue(name, out texture);
	}

	/// <summary>
	/// Tries to get an <see cref="Emotion"/> of the given <paramref name="id"/> from the database.
	/// </summary>
	/// <param name="id">The id of the emotion to search for.</param>
	/// <param name="emotion">The returned emotion, if a match is found.</param>
	/// <returns>Whether the emotion exists in the database.</returns>
	public static bool TryGetEmotion(string id, out Emotion emotion)
	{
		if (id == null)
		{
			emotion = null;
			return false;
		}
		return Emotions.TryGetValue(id, out emotion);
	}

	/// <summary>
	/// Tries to get an <see cref="EmotionGroup"/> of the given <paramref name="id"/> from the database.
	/// </summary>
	/// <param name="id">The id of the group to search for.</param>
	/// <param name="group">The returned group, if a match is found.</param>
	/// <returns>Whether the group exists in the database.</returns>
	public static bool TryGetEmotionGroup(string id, out EmotionGroup group)
	{
		if (id == null)
		{
			group = null;
			return false;
		}
		return EmotionGroups.TryGetValue(id, out group);
	}

	/// <summary>
	/// Tries to get the <see cref="Emotion"/> at the given <paramref name="tier"/> of a group.
	/// Used for emotion escalation (happy -> ecstatic -> manic).
	/// </summary>
	/// <param name="groupId">The id of the group.</param>
	/// <param name="tier">The 1-based tier to look up (base emotion = tier 1).</param>
	/// <param name="emotion">The returned emotion, if a match is found.</param>
	/// <returns>Whether an emotion exists at that tier of the group.</returns>
	public static bool TryGetEmotionByGroupTier(string groupId, int tier, out Emotion emotion)
	{
		if (groupId == null)
		{
			emotion = null;
			return false;
		}
		return EmotionTiers.TryGetValue((groupId, tier), out emotion);
	}

	internal static Emotion NeutralEmotion => Emotions["neutral"];

	/// <summary>
	/// The ids of every registered emotion, in registration order.
	/// </summary>
	public static IEnumerable<string> GetAllEmotionIds()
	{
		return EmotionOrder.ToList();
	}

	// groups that opt into random emotion rolls, in registration order
	private static readonly List<EmotionGroup> RandomEmotionGroups = [];

	internal static IReadOnlyList<EmotionGroup> GetRandomEmotionGroups()
	{
		return RandomEmotionGroups;
	}

	// tracks which mod registered each modded entry, keyed by "kind:id", for collision reporting
	private static readonly Dictionary<string, string> RegistrationOwners = [];

	private static bool TryRegister<T>(IDictionary<string, T> registry, string kind, string id, T value)
	{
		string owner = ModManager.CurrentModName;
		if (!registry.TryAdd(id, value))
		{
			string original = RegistrationOwners.TryGetValue($"{kind}:{id}", out string existing)
				? $"mod \"{existing}\""
				: "the base game";
			string loser = owner != null ? $" from mod \"{owner}\"" : "";
			string fix = owner != null ? " Registration isn't namespaced, so prefix your IDs with your mod's name to keep them unique." : "";
			GD.PrintErr($"{kind} with ID {id}{loser} already exists (registered by {original}), skipping!{fix}");
			return false;
		}
		if (owner != null)
			RegistrationOwners[$"{kind}:{id}"] = owner;
		return true;
	}

	internal static bool RegisterJsonPartyMember(JsonActorMod jsonActor, SpriteFrames builtFrames)
	{
		if (builtFrames == null)
			return false;

		return TryRegister<Func<PartyMember>>(PartyMembers, "PartyMember", jsonActor.Name,
			() => new ModdedPartyMember(jsonActor, builtFrames));
	}

	internal static bool RegisterJsonEnemy(JsonEnemyMod jsonEnemy, SpriteFrames builtFrames)
	{
		if (builtFrames == null)
			return false;

		return TryRegister<Func<Enemy>>(Enemies, "Enemy", jsonEnemy.Name,
			() => new ModdedEnemy(jsonEnemy, builtFrames));
 	}

	internal static bool RegisterModdedPartyMember<T>(string id) where T : PartyMember, new()
	{
		return TryRegister<Func<PartyMember>>(PartyMembers, "PartyMember", id, () => new T());
	}

	internal static bool RegisterModdedEnemy<T>(string id) where T : Enemy, new()
	{
		return TryRegister<Func<Enemy>>(Enemies, "Enemy", id, () => new T());
	}

	internal static bool RegisterModdedStatModifier(string id, Func<StatModifier> func)
	{
		return TryRegister(Modifiers, "StatModifier", id, func);
	}

	internal static bool RegisterModdedEmotionGroup(EmotionGroup group)
	{
		if (!TryRegister(EmotionGroups, "EmotionGroup", group.Id, group))
			return false;
		group.Registered = true;
		if (group.IncludedInRandomEmotion)
			RandomEmotionGroups.Add(group);
		return true;
	}

	internal static bool RegisterModdedEmotion(Emotion emotion)
	{
		if (!TryRegister(Emotions, "Emotion", emotion.Id, emotion))
			return false;
		EmotionOrder.Add(emotion.Id);
		LinkEmotion(emotion);
		emotion.Registered = true;
		return true;
	}

	internal static void AddEmotionGroup(EmotionGroup group)
	{
		EmotionGroups.Add(group.Id, group);
		group.Registered = true;
		if (group.IncludedInRandomEmotion)
			RandomEmotionGroups.Add(group);
	}

	internal static void AddEmotion(Emotion emotion)
	{
		Emotions.Add(emotion.Id, emotion);
		EmotionOrder.Add(emotion.Id);
		LinkEmotion(emotion);
		emotion.Registered = true;
	}

	// resolves the emotion's group reference and hooks it into the escalation ladder
	private static void LinkEmotion(Emotion emotion)
	{
		if (emotion.GroupId == null)
			return;
		if (!EmotionGroups.TryGetValue(emotion.GroupId, out EmotionGroup group))
		{
			GD.PrintErr($"Emotion {emotion.Id} references unknown group {emotion.GroupId}! Register the group first.");
			return;
		}
		emotion.Group = group;
		if (!EmotionTiers.TryAdd((emotion.GroupId, emotion.Tier), emotion))
			GD.PrintErr($"Group {emotion.GroupId} already has an emotion at tier {emotion.Tier}; {emotion.Id} will not participate in escalation!");
	}

	internal static bool RegisterModdedSkill(string id, Skill skill)
	{
		return TryRegister(Skills, "Skill", id, skill);
	}

	internal static bool RegisterModdedItem(string id, Item item)
	{
		if (!TryRegister(Items, "Item", id, item))
			return false;
		item.Id = id;
		return true;
	}

	internal static bool RegisterModdedEquipment(string id, Equipment equipment)
	{
		return TryRegister(Equipment, "Equipment", id, equipment);
	}

	// followup sets are stored ordered in FollowupSets (the editor dropdown relies on that
	// order); this map only enforces id uniqueness, seeded lazily with the vanilla sets
	private static readonly Dictionary<string, FollowupSet> FollowupSetsById = [];

	internal static bool RegisterModdedFollowupSet(string id, IReadOnlyDictionary<FollowupInput, FollowupEntry> entries, bool tiered)
	{
		string mod = ModManager.CurrentModName != null ? $" from mod \"{ModManager.CurrentModName}\"" : "";
		if (string.IsNullOrWhiteSpace(id) || id == FollowupSets.NoneId)
		{
			GD.PrintErr($"Invalid followup set ID \"{id}\"{mod}, skipping!");
			return false;
		}
		if (entries == null || entries.Count == 0)
		{
			GD.PrintErr($"Followup set {id}{mod} has no entries, skipping!");
			return false;
		}
		foreach ((FollowupInput input, FollowupEntry entry) in entries)
		{
			if (entry.TargetPosition != FollowupEntry.User && entry.TargetPosition is < 0 or > 3)
			{
				GD.PrintErr($"Followup set {id}{mod}: {input} entry has invalid target position {entry.TargetPosition}, skipping!");
				return false;
			}
			if (string.IsNullOrWhiteSpace(entry.BaseSkillName))
			{
				GD.PrintErr($"Followup set {id}{mod}: {input} entry has no skill name, skipping!");
				return false;
			}
			if (!entry.HasTexture)
			{
				GD.PrintErr($"Followup set {id}{mod}: {input} entry has no usable bubble texture, skipping!");
				return false;
			}
			if (entry.EnergyCost is < 0 or > 10 || entry.MinPartySize is < 0 or > 4)
			{
				GD.PrintErr($"Followup set {id}{mod}: {input} entry has an energy cost outside 0-10 or a party size outside 0-4, skipping!");
				return false;
			}
			if (entry.BaseSkillName.StartsWith("ReleaseEnergy") && !entry.IsReleaseEnergy)
				GD.PushWarning($"Followup set {id}{mod}: {input} entry is named \"{entry.BaseSkillName}\" and costs {entry.EnergyCost} energy and is not a Release Energy! Use WithReleaseEnergy() to mark it as a Release Energy follow-up.");
		}

		if (FollowupSetsById.Count == 0)
			foreach (FollowupSet vanilla in FollowupSets.All)
				FollowupSetsById.Add(vanilla.Id, vanilla);

		FollowupSet set = new()
		{
			Id = id,
			Tiered = tiered,
			Entries = new Dictionary<FollowupInput, FollowupEntry>(entries)
		};
		if (!TryRegister(FollowupSetsById, "FollowupSet", id, set))
			return false;
		FollowupSets.AddModded(set);
		return true;
	}

	internal static PartyMember CreatePartyMember(string who)
	{
		if (!PartyMembers.TryGetValue(who, out Func<PartyMember> member))
		{
			GD.PrintErr("Unknown party member: " + who);
			return null;
		}
		return member();
	}

	internal static Enemy CreateEnemy(string who)
	{
		if (!Enemies.TryGetValue(who, out Func<Enemy> enemy))
		{
			GD.PrintErr("Unknown enemy: " + who);
			return null;
		}
		return enemy();
	}

	internal static StatModifier CreateModifier(string what)
	{
		if (!Modifiers.TryGetValue(what, out Func<StatModifier> modifier))
		{
			return null;
		}
		return modifier();
	}

	internal static bool AddStateIcon(string name, Texture2D texture)
	{
		return TryRegister(StateIcons, "State Icon", name, texture);
	}

	/// <summary>
	/// The ids of every registered weapon.
	/// </summary>
	public static IEnumerable<string> GetAllWeaponNames()
	{
		return Equipment.Where(x => !x.Value.IsCharm).Select(x => x.Key);
	}

	/// <summary>
	/// The ids of every registered charm.
	/// </summary>
	public static IEnumerable<string> GetAllCharmNames()
	{
		return Equipment.Where(x => x.Value.IsCharm).Select(x => x.Key);
	}

	/// <summary>
	/// The ids of every registered item.
	/// </summary>
	public static IEnumerable<string> GetAllItemNames()
	{
		return Items.Keys;
	}

	/// <summary>
	/// The ids of every registered party member.
	/// </summary>
	public static IEnumerable<string> GetAllPartyMemberNames()
	{
		return PartyMembers.Keys;
	}

	/// <summary>
	/// The ids of every registered enemy.
	/// </summary>
	public static IEnumerable<string> GetAllEnemyNames()
	{
		return Enemies.Keys;
	}

	/// <summary>
	/// The ids of every registered skill.
	/// </summary>
	public static IEnumerable<string> GetAllSkillNames()
	{
		return Skills.Keys;
	}

	internal const string DefaultMenuSkin = "Dreamworld";

	internal static bool RegisterMenuSkin(string id, MenuSkin skin)
	{
		if (string.IsNullOrWhiteSpace(id) || skin?.CommandsTexture == null || skin.PartyCommandsTexture == null)
		{
			string mod = ModManager.CurrentModName != null ? $" from mod \"{ModManager.CurrentModName}\"" : "";
			GD.PrintErr($"Invalid menu skin \"{id}\"{mod}, skipping!");
			return false;
		}
		if (!TryRegister(MenuSkins, "Menu Skin", id, skin))
			return false;
		MenuSkinOrder.Add(id);
		return true;
	}

	internal static MenuSkin GetMenuSkin(string id)
	{
		return MenuSkins.GetValueOrDefault(id ?? "");
	}

	internal static IReadOnlyList<string> GetAllMenuSkinIds()
	{
		return MenuSkinOrder;
	}

	internal static string ResolveMenuSkinId(string id)
	{
		if (string.IsNullOrEmpty(id))
			return DefaultMenuSkin;
		string known = MenuSkinOrder.FirstOrDefault(x => x.Equals(id, StringComparison.OrdinalIgnoreCase));
		if (known != null)
			return known;
		if (!id.Equals("Auto", StringComparison.OrdinalIgnoreCase))
			GD.PushWarning($"Unknown menu skin \"{id}\", using {DefaultMenuSkin}");
		return DefaultMenuSkin;
	}

	internal const string DefaultWindowSkin = "Default";

	internal static bool RegisterWindowSkin(string id, Texture2D texture)
	{
		if (string.IsNullOrWhiteSpace(id) || texture == null || texture.GetWidth() != 192 || texture.GetHeight() != 192 || texture.GetImage() == null)
		{
			string mod = ModManager.CurrentModName != null ? $" from mod \"{ModManager.CurrentModName}\"" : "";
			GD.PrintErr($"Invalid window skin \"{id}\"{mod}: it must be a 192x192 texture, skipping!");
			return false;
		}
		if (!TryRegister(WindowSkins, "Window Skin", id, new WindowSkin(texture, true)))
			return false;
		WindowSkinOrder.Add(id);
		return true;
	}

	internal static WindowSkin GetWindowSkin(string id)
	{
		return WindowSkins.GetValueOrDefault(id ?? "");
	}

	internal static IReadOnlyList<string> GetAllWindowSkinIds()
	{
		return WindowSkinOrder;
	}

	internal static string ResolveWindowSkinId(string id)
	{
		if (string.IsNullOrEmpty(id))
			return DefaultWindowSkin;
		string known = WindowSkinOrder.FirstOrDefault(x => x.Equals(id, StringComparison.OrdinalIgnoreCase));
		if (known != null)
			return known;
		GD.PushWarning($"Unknown window skin \"{id}\", using {DefaultWindowSkin}");
		return DefaultWindowSkin;
	}

	private static void Init()
	{
		UiRegistry.Register();
		CommonSkillsRegistry.Register();
		OmoriRegistry.Register();
		SunnyRegistry.Register();
		BasilRegistry.Register();
		AubreyRegistry.Register();
		KelRegistry.Register();
		HeroRegistry.Register();
		VastForestRegistry.Register();
		OtherworldRegistry.Register();
		JunkyardRegistry.Register();
		PyreflyForestRegistry.Register();
		SweetheartsCastleRegistry.Register();
		LastResortRegistry.Register();
		UnderwaterHighwayRegistry.Register();
		HumphreyRegistry.Register();
		OrangeOasisRegistry.Register();
		SnowglobeMountainRegistry.Register();
		AbyssRegistry.Register();
		BlackspaceRegistry.Register();
		FarawayRegistry.Register();
		ConsolePortRegistry.Register();
		BossRushRegistry.Register();
		EmotionsRegistry.Register();
		ModifiersRegistry.Register();
		ItemsRegistry.Register();
		EquipmentRegistry.Register();

		// have each item keep track of their own Id to make later comparisons easier
		// instead of having to go by name, which causes issues
		foreach (KeyValuePair<string, Item> entry in Items)
			entry.Value.Id = entry.Key;
	}

	internal static void AddBuiltInMenuSkin(string id, MenuSkin skin)
	{
		MenuSkins.Add(id, skin);
		MenuSkinOrder.Add(id);
	}

	internal static void QueueEmotionLine(Actor who, string groupId, bool applied)
	{
		if (applied)
			BattleLogManager.Instance.QueueMessage(who.CurrentEmotion.FeelsMessage(who.Name));
		else
			BattleManager.Instance.ShowEmotionNoEffect(who, groupId);
	}

	internal static void QueueTierOneEmotionLine(Actor who, string groupId, bool applied)
	{
		if (!applied)
			BattleManager.Instance.ShowEmotionNoEffect(who, groupId);
		else if (TryGetEmotionByGroupTier(groupId, 1, out Emotion first))
			BattleLogManager.Instance.QueueMessage(first.FeelsMessage(who.Name));
	}

	internal static EmotionGroup RollRandomEmotionGroup()
	{
		IReadOnlyList<EmotionGroup> pool = GetRandomEmotionGroups();
		return pool.Count == 0 ? null : pool[GameManager.Instance.Random.RandiRange(0, pool.Count - 1)];
	}

	internal static bool AddTierThreeStatModifier(Actor target, string modifier, bool silentAtLimit = false)
	{
		if (target.GetStatModifierTier(modifier) == 3)
		{
			target.RefreshTierStatModifier(modifier, true, silentAtLimit);
			return false;
		}
		target.AddTierStatModifier(modifier, 3);
		return true;
	}

	internal static void AddFixedTierStatModifier(Actor target, string modifier, int tier)
	{
		int held = target.GetStatModifierTier(modifier);
		if (held >= tier)
			target.RefreshTierStatModifier(modifier, held == tier, true);
		else
			target.AddTierStatModifier(modifier, tier, silent: true);
	}

}
