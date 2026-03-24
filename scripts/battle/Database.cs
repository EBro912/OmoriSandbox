using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Actors;
using OmoriSandbox.Editor;
using OmoriSandbox.Extensions;
using OmoriSandbox.Modding;

namespace OmoriSandbox.Battle;

/// <summary>
/// The database where all game related data is stored.
/// </summary>
public class Database
{
	private static readonly SortedDictionary<string, Func<PartyMember>> PartyMembers = [];
	private static readonly SortedDictionary<string, Func<Enemy>> Enemies = [];
	private static readonly Dictionary<string, Skill> Skills = [];
	private static readonly SortedDictionary<string, Item> Items = [];
	private static readonly SortedDictionary<string, Weapon> Weapons = [];
	private static readonly SortedDictionary<string, Charm> Charms = [];
	private static readonly Dictionary<string, Func<StatModifier>> Modifiers = [];

	static Database()
	{
		Init();
	}

	/// <summary>
	/// Tries to get a <see cref="Skill"/> of the given <paramref name="name"/> from the database.
	/// </summary>
	/// <param name="name">The name of the skill to search for.</param>
	/// <param name="skill">The returned skill, if a match is found.</param>
	/// <returns>Whether or not the skill exists in the database.</returns>
	public static bool TryGetSkill(string name, out Skill skill)
	{
		return Skills.TryGetValue(name, out skill);
	}

	/// <summary>
	/// Tries to get an <see cref="Item"/> of the given <paramref name="name"/> from the database.
	/// </summary>
	/// <param name="name">The name of the item to search for.</param>
	/// <param name="item">The returned item, if a match is found.</param>
	/// <returns>Whether or not the item exists in the database.</returns>
	public static bool TryGetItem(string name, out Item item)
	{
		return Items.TryGetValue(name, out item);
	}

	/// <summary>
	/// Tries to get a <see cref="Weapon"/> of the given <paramref name="name"/> from the database.
	/// </summary>
	/// <param name="name">The name of the weapon to search for.</param>
	/// <param name="weapon">The returned weapon, if a match is found.</param>
	/// <returns>Whether or not the weapon exists in the database.</returns>
	public static bool TryGetWeapon(string name, out Weapon weapon)
	{
		return Weapons.TryGetValue(name, out weapon);
	}

	/// <summary>
	/// Tries to get a <see cref="Charm"/> of the given <paramref name="name"/> from the database.
	/// </summary>
	/// <param name="name">The name of the charm to search for.</param>
	/// <param name="charm">The returned charm, if a match is found.</param>
	/// <returns>Whether or not the charm exists in the database.</returns>
	public static bool TryGetCharm(string name, out Charm charm)
	{
		return Charms.TryGetValue(name, out charm);
	}

	internal static void RegisterJsonPartyMember(JsonActorMod jsonActor, SpriteFrames builtFrames)
	{
		if (builtFrames == null)
			return;

		if (PartyMembers.ContainsKey(jsonActor.Name))
		{
			GD.PrintErr("Party member with name " +  jsonActor.Name + " already exists!");
			return;
		}
		PartyMembers[jsonActor.Name] = () => new ModdedPartyMember(jsonActor, builtFrames);
	}

	internal static void RegisterJsonEnemy(JsonEnemyMod jsonEnemy, SpriteFrames builtFrames)
	{
		if (builtFrames == null)
			return;

		if (Enemies.ContainsKey(jsonEnemy.Name))
		{
			GD.PrintErr("Enemy with name " + jsonEnemy.Name + " already exists!");
			return;
		}
		Enemies[jsonEnemy.Name] = () => new ModdedEnemy(jsonEnemy, builtFrames);
 	}

	internal static void RegisterModdedPartyMember<T>(string id) where T : PartyMember, new()
	{
		if (!PartyMembers.TryAdd(id, () => new T()))
		{
			GD.PrintErr("PartyMember with ID " + id + " already exists!");
		}
	}

	internal static void RegisterModdedEnemy<T>(string id) where T : Enemy, new()
	{
		if (!Enemies.TryAdd(id, () => new T()))
		{
			GD.PrintErr("Enemy with ID " + id + " already exists!");
		}
	}

	internal static void RegisterModdedStatModifier(string id, Func<StatModifier> func)
	{
		if (!Modifiers.TryAdd(id, func))
		{
			GD.PrintErr("StatModifier with ID " + id + " already exists!");
		}
	}

	internal static void RegisterModdedSkill(string id, Skill skill)
	{
		if (!Skills.TryAdd(id, skill))
		{
			GD.PrintErr("Skill with ID " + id + " already exists!");
		}
	}

	internal static void RegisterModdedItem(string id, Item item)
	{
		if (!Items.TryAdd(id, item))
		{
			GD.PrintErr("Item with ID " + id + " already exists!");
		}
	}

	internal static void RegisterModdedWeapon(string id, Weapon weapon)
	{
		if (!Weapons.TryAdd(id, weapon))
		{
			GD.PrintErr("Weapon with ID " + id + " already exists!");
		}
	}

	internal static void RegisterModdedCharm(string id, Charm charm)
	{
		if (!Charms.TryAdd(id, charm))
		{
			GD.PrintErr("Charm with ID " + id + " already exists!");
		}
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

	internal static IEnumerable<string> GetAllWeaponNames()
	{
		return Weapons.Keys;
	}

	internal static IEnumerable<string> GetAllCharmNames()
	{
		return Charms.Keys;
	}

	internal static IEnumerable<string> GetAllItemNames()
	{
		return Items.Keys;
	}

	internal static IEnumerable<string> GetAllPartyMemberNames()
	{
		return PartyMembers.Keys;
	}

	internal static IEnumerable<string> GetAllEnemyNames()
	{
		return Enemies.Keys;
	}

	internal static IEnumerable<string> GetAllSkillNames()
	{
		return Skills.Keys;
	}

	private static void Init()
	{
		#region PARTY MEMBERS
		PartyMembers.Add("Omori", () => new Omori());
		PartyMembers.Add("Aubrey", () => new Aubrey());
		PartyMembers.Add("Hero", () => new Hero());
		PartyMembers.Add("Kel", () => new Kel());
		PartyMembers.Add("AubreyRW", () => new AubreyRW());
		PartyMembers.Add("KelRW", () => new KelRW());
		PartyMembers.Add("HeroRW", () => new HeroRW());
		PartyMembers.Add("Sunny", () => new Sunny());
		PartyMembers.Add("Sunny (Alt)", () => new SunnyAlt());
		PartyMembers.Add("Basil", () => new Basil());
		#endregion

		#region ENEMIES
		Enemies.Add("LostSproutMole", () => new LostSproutMole());
		Enemies.Add("LostSproutMole (King Crawler)", () => new LostSproutMoleKC());
		Enemies.Add("ForestBunny?", () => new ForestBunnyQuestion());
		Enemies.Add("Sweetheart", () => new Sweetheart());
		Enemies.Add("Sweetheart (Boss Rush)", () => new SweetheartAlt());
		Enemies.Add("SlimeGirls", () => new SlimeGirls());
		Enemies.Add("SlimeGirls (Boss Rush)", () => new SlimeGirlsAlt());
		Enemies.Add("AubreyEnemy", () => new AubreyEnemy());
		Enemies.Add("BigStrongTree", () => new BigStrongTree());
		Enemies.Add("DownloadWindow", () => new DownloadWindow());
		Enemies.Add("DownloadWindow (Boss Rush)", () => new DownloadWindowAlt());
		Enemies.Add("SpaceExBoyfriend", () => new SpaceExBoyfriend());
		Enemies.Add("SpaceExBoyfriend (Boss Rush)", () => new SpaceExBoyfriendAlt());
		Enemies.Add("GatorGuyJawsum", () => new GatorGuyJawsum());
		Enemies.Add("GatorGuyJawsum (Boss Rush)", () => new GatorGuyJawsumAlt());
		Enemies.Add("MrJawsum", () => new MrJawsum());
		Enemies.Add("MrJawsum (Boss Rush)", () => new MrJawsumAlt());
		Enemies.Add("FearOfSpiders", () => new FearOfSpiders());
		Enemies.Add("UnbreadTwins", () => new UnbreadTwins());
		Enemies.Add("UnbreadTwins (Boss Rush)", () => new UnbreadTwinsAlt());
		Enemies.Add("BunBunny", () => new BunBunny());
		Enemies.Add("Creepypasta", () => new Creepypasta());
		Enemies.Add("Slice", () => new Slice());
		Enemies.Add("Slice (Boss Rush)", () => new SliceAlt());
		Enemies.Add("Sourdough", () => new Sourdough());
		Enemies.Add("Sourdough (Boss Rush)", () => new SourdoughAlt());
		Enemies.Add("Sesame", () => new Sesame());
		Enemies.Add("Sesame (Boss Rush)", () => new SesameAlt());
		Enemies.Add("LivingBread", () => new LivingBread());
		Enemies.Add("Boss", () => new Boss());
		Enemies.Add("YeOldSprout", () => new YeOldSprout());
		Enemies.Add("YeOldSprout (Boss Rush)", () => new YeOldSproutAlt());
		Enemies.Add("Mutantheart", () => new Mutantheart());
		Enemies.Add("NefariousChip", () => new NefariousChip());
		Enemies.Add("TheEarth", () => new TheEarth());
		Enemies.Add("TheEarth (Pluto)", () => new TheEarthAlt());
		Enemies.Add("Perfectheart", () => new Perfectheart());
		Enemies.Add("Roboheart", () => new Roboheart());
		Enemies.Add("FearOfHeights", () => new FearOfHeights());
		Enemies.Add("SpaceExHusband", () => new SpaceExHusband());
		Enemies.Add("SirMaximusI", () => new SirMaximusI());
		Enemies.Add("SirMaximusI (Boss Rush)", () => new SirMaximusIAlt());
		Enemies.Add("SirMaximusII", () => new SirMaximusII());
		Enemies.Add("SirMaximusII (Boss Rush)", () => new SirMaximusIIAlt());
		Enemies.Add("SirMaximusIII", () => new SirMaximusIII());
		Enemies.Add("SirMaximusIII (Boss Rush)", () => new SirMaximusIIIAlt());
		Enemies.Add("FearOfDrowning", () => new FearOfDrowning());
		Enemies.Add("PlutoExpanded", () => new PlutoExpanded());
		Enemies.Add("PlutoExpanded (Boss Rush)", () => new PlutoExpandedAlt());
		Enemies.Add("PlutoExpandedAndEarth", () => new PlutoExpandedEarth());
		Enemies.Add("KingCrawler", () => new KingCrawler());
		Enemies.Add("KingCrawler (Boss Rush)", () => new KingCrawlerAlt());
		Enemies.Add("KiteKid", () => new KiteKid());
		Enemies.Add("KiteKid (Epilogue)", () => new KiteKidAlt());
		Enemies.Add("KidsKite", () => new KidsKite());
		Enemies.Add("KidsKite (Epilogue)", () => new KidsKiteAlt());
		Enemies.Add("Pluto", () => new Pluto());
		Enemies.Add("LeftArm", () => new LeftArm());
		Enemies.Add("RightArm", () => new RightArm());
		Enemies.Add("Abbi", () => new Abbi());
		Enemies.Add("Tentacle", () => new Tentacle());
		Enemies.Add("RecycultistLeft", () => new Recycultist(true));
		Enemies.Add("RecycultistRight", () => new Recycultist(false));
		Enemies.Add("Recyclepath", () => new Recyclepath());
		Enemies.Add("AubreyBoss", () => new AubreyBoss());
		Enemies.Add("KelBoss", () => new KelBoss());
		Enemies.Add("HeroBoss", () => new HeroBoss());
		Enemies.Add("BossmanHero", () => new BossmanHero());
		Enemies.Add("GatorGuyHero", () => new GatorGuyHero());
		Enemies.Add("Snaley 1", () => new SnaleyOne());
		Enemies.Add("Snaley 2", () => new SnaleyTwo());
		Enemies.Add("Snaley 3", () => new SnaleyThree());
		Enemies.Add("ShadyMole", () => new ShadyMole());
		Enemies.Add("HumphreySwarm", () => new HumphreySwarm());
		Enemies.Add("HumphreyGrande", () => new HumphreyGrande());
		Enemies.Add("HumphreyFace", () => new HumphreyFace());
		Enemies.Add("Angel", () => new Angel());
		Enemies.Add("Angel (Two Days Left)", () => new AngelAlt());
		Enemies.Add("Charlene", () => new Charlene());
		Enemies.Add("TheMaverick", () => new TheMaverick());
		Enemies.Add("Kim", () => new Kim());
		Enemies.Add("Vance", () => new Vance());
		Enemies.Add("TheHooligans", () => new TheHooligans());
		Enemies.Add("Jackson", () => new Jackson());
		#endregion

		#region SKILLS
		Skills["Guard"] = new Skill(
			name: "GUARD",
			description: "Acts first, reducing damage taken for 1 turn.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] guards.");
				await AnimationManager.Instance.WaitForAnimation(115, self);
				self.AddStatModifier("Guard");
			},
			priority: SkillPriority.First
			// guard can always be used regardless of emotion
		).WithCustomRequirement((_) => true);

		// OMORI //
		Skills["OAttack"] = new Skill(
			name: "OAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForAnimation(3, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true,
			showFollowups: true
		);

		Skills["SadPoem"] = new Skill(
			name: "SAD POEM",
			description: "Inflicts SAD on a friend or foe.\nCost: 5",
			target: SkillTarget.AllyOrEnemy,
			cost: 5,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] reads a sad poem.");
				await AnimationManager.Instance.WaitForAnimation(5, self);
				MakeSad(target);
			}
		);
		Skills["LuckySlice"] = new Skill(
			name: "LUCKY SLICE",
			description: "Acts first. An attack that's stronger\nwhen [actor] is HAPPY. Cost: 15",
			target: SkillTarget.Enemy,
			cost: 15,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(8, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] lunges at [target]!");
				if (self.CurrentState is "happy" or "ecstatic" or "manic")
					BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK + self.CurrentStats.LCK) * 2f - target.CurrentStats.DEF, false);
				else
					BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK + self.CurrentStats.LCK) * 1.5f - target.CurrentStats.DEF, false);
			},
			priority: SkillPriority.First
		);
		Skills["Stab"] = new Skill(
			name: "STAB",
			description: "Always deals a critical hit.\nIgnores DEFENSE when [actor] is sad. Cost: 13",
			target: SkillTarget.Enemy,
			cost: 13,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(9, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] stabs [target].");
				if (self.CurrentState is "sad" or "depressed" or "miserable")
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f, false, guaranteeCrit: true);
				else
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF, false, guaranteeCrit: true);
			}
		);

		Skills["Trick"] = new Skill(
			name: "TRICK",
			description: "Deals damage. If the foe is HAPPY, greatly\nreduce its SPEED. Cost: 20",
			target: SkillTarget.Enemy,
			cost: 20,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(10, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] tricks [target].");
				if (target.CurrentState is "happy" or "ecstatic" or "manic")
				{
					AnimationManager.Instance.PlayAnimation(219, target);
					target.AddTierStatModifier("SpeedDown", 3);
				}
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF);
				await Task.Delay(334);
			}
		);
		
		Skills["Observe"] = new Skill(
			name: "OBSERVE",
			description: "Predicts who a foe will target next turn.\nCost: 0",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self,target,"[actor] focuses their vision and observes\n[target]!");
				AnimationManager.Instance.PlayAnimation(4, target);
				await Task.Delay(1000);
				List<PartyMemberComponent> members = BattleManager.Instance.GetAlivePartyMembers();
				PartyMemberComponent taunting = members.FirstOrDefault(x => x.Actor.HasStatModifier("Taunt"));
				if (taunting != null)
				{
					await AnimationManager.Instance.WaitForAnimation(4, taunting.Actor);
					if (target is Enemy e)
						e.ObserveTarget = taunting.Actor;
					BattleLogManager.Instance.QueueMessage(target, taunting.Actor,"[actor] has their eyes on\n[target]!");
					return;
				}

				bool multi = GameManager.Instance.Random.RandiRange(1, 2) == 1;
				if (multi)
				{
					// vanilla omori technically stops after the 4th attempt
					// maybe add a toggle for this?
					Enemy enemy = BattleManager.Instance.GetAllEnemies().FirstOrDefault(x => x.HasMultiTargetSkill);
					if (enemy != null)
					{
						enemy.ObserveMultiTarget = true;
						BattleLogManager.Instance.QueueMessage(enemy, "[actor] has their eyes on\neveryone!");
						foreach (PartyMemberComponent m in members)
							AnimationManager.Instance.PlayAnimation(4, m.Actor);
						await Task.Delay(1000);
						return;
					}
				}
				
				PartyMember member = members[GameManager.Instance.Random.RandiRange(0, members.Count - 1)].Actor;
				BattleLogManager.Instance.QueueMessage(target, member,"[actor] has their eyes on\n[target]!");
				await AnimationManager.Instance.WaitForAnimation(4, member);
				if (target is Enemy en)
					en.ObserveTarget = member;
			},
			priority: SkillPriority.Last
		);

		Skills["HackAway"] = new Skill(
			name: "HACK AWAY",
			description: "Attacks 3 times, hitting random foes.\nCost: 30",
			target: SkillTarget.AllEnemies,
			cost: 30,
			effect: async (self, targets) =>
			{
				await AnimationManager.Instance.WaitForScreenAnimation(6, targets[0] is Enemy);
				BattleLogManager.Instance.QueueMessage(self, "[actor] slashes wildly!");
				List<Actor> randomTargets = [];
				for (int i = 0; i < 3; i++)
				{
					randomTargets.Add(targets[GameManager.Instance.Random.RandiRange(0, targets.Count - 1)]);
				}
				foreach (Actor enemy in randomTargets)
				{
					Actor target = enemy;
					if (target.CurrentHP == 0)
						target = randomTargets.FirstOrDefault(x => x.CurrentHP > 0, target);
					BattleManager.Instance.Damage(self, target, () =>
					{
						if (self.CurrentState is "angry" or "enraged" or "furious")
						{
							return self.CurrentStats.ATK * 2.25f - target.CurrentStats.DEF;
						}
						return self.CurrentStats.ATK * 2f - target.CurrentStats.DEF;
					}, false);
				}
			}
		);

		Skills["PainfulTruth"] = new Skill(
			name: "PAINFUL TRUTH",
			description: "Deals damage to a foe. [actor] and the foe\nbecome SAD. Cost: 10",
			target: SkillTarget.Enemy,
			cost: 10,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(5, self);
				AnimationManager.Instance.PlayAnimation(19, target);

				MakeSad(self);
				MakeSad(target);

				await Task.Delay(1000);

				BattleLogManager.Instance.QueueMessage(self, target, "[actor] whispers something\nto [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["Mock"] = new Skill(
			name: "MOCK",
			description: "Deals damage. If the foe is ANGRY, greatly\nreduce it's ATTACK. Cost: 20",
			target: SkillTarget.Enemy,
			cost: 20,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(12, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] mocks [target].");
				if (target.CurrentState is "angry" or "enraged" or "furious")
				{
					AnimationManager.Instance.PlayAnimation(219, target);
					target.AddTierStatModifier("AttackDown", 3);
				}
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, false);
				await Task.Delay(334);
			}
		);

		Skills["Shun"] = new Skill(
			name: "SHUN",
			description: "Deals damage. If the foe is SAD, greatly\nreduce its DEFENSE. Cost: 20",
			target: SkillTarget.Enemy,
			cost: 20,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(11, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] shuns [target].");
				if (target.CurrentState is "sad" or "depressed" or "miserable")
				{
					AnimationManager.Instance.PlayAnimation(219, target);
					target.AddTierStatModifier("DefenseDown", 3);
				}
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, false);
				await Task.Delay(334);
			}
		);

		Skills["Stare"] = new Skill(
			name: "STARE",
			description: "Reduces all of a foe's STATS.\nCost: 45",
			target: SkillTarget.Enemy,
			cost: 45,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(18, target);
				await Task.Delay(1660);
				AnimationManager.Instance.PlayAnimation(219, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] stares at [target].");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] feels uncomfortable.");
				target.AddStatModifier("AttackDown");
				target.AddStatModifier("DefenseDown");
				target.AddStatModifier("SpeedDown");
				await Task.Delay(334);
			}
		);

		Skills["Exploit"] = new Skill(
			name: "EXPLOIT",
			description: "Deals extra damage to a HAPPY, SAD, or\nANGRY foe. Cost: 30",
			target: SkillTarget.Enemy,
			cost: 30,
			effect: async (self, target) =>
			{
				switch (target.CurrentState)
				{
					case "happy" or "ecstatic" or "manic":
						await AnimationManager.Instance.WaitForAnimation(10, target);
						break;
					case "sad" or "depressed" or "miserable":
						await AnimationManager.Instance.WaitForAnimation(11, target);
						break;
					case "angry" or "enraged" or "furious":
						await AnimationManager.Instance.WaitForAnimation(12, target);
						break;
					default:
						await AnimationManager.Instance.WaitForAnimation(123, target);
						break;
				}
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] exploits [target]'s EMOTIONS!");
				if (target.CurrentState != "neutral")
				{
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3.5f - target.CurrentStats.DEF, false);
				}
				else
				{
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF, false);
				}
			}
		);

		Skills["FinalStrike"] = new Skill(
			name: "FINAL STRIKE",
			description: "Strikes all foes. Deals more damage if [actor]\nhas a higher stage of EMOTION. Cost: 50",
			target: SkillTarget.AllEnemies,
			cost: 50,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] releases his ultimate\nattack!");
				await AnimationManager.Instance.WaitForScreenAnimation(13, targets[0] is Enemy);
				float multiplier = self.CurrentState switch
				{
					"manic" or "miserable" or "furious" => 6f,
					"ecstatic" or "depressed" or "enraged" => 5f,
					"happy" or "sad" or "angry" => 4f,
					_ => 3f
				};
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy, () => self.CurrentStats.ATK * multiplier - enemy.CurrentStats.DEF, false);
				}
			}
		);

		Skills["RedHands"] = new Skill(
			name: "RED HANDS",
			description: "Deals big damage 4 times.\nCost: 75",
			target: SkillTarget.Enemy,
			cost: 75,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForRedHands();
				for (int i = 0; i < 4; i++)
				{
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, false);
				}
			}
		);

		Skills["Vertigo"] = new Skill(
			name: "VERTIGO",
			description: "Deals damage to all foes based on user's\nSPEED and greatly reduces their ATTACK.",
			target: SkillTarget.AllEnemies,
			cost: 45,
			effect: async (self, targets) =>
			{
				AudioManager.Instance.PlaySFX("SE_bs_scare4", 0.5f, 0.9f);
				await AnimationManager.Instance.WaitForOmoriSpecialAnimation(
						"res://assets/pictures/dark_overlay.png",
						"res://assets/pictures/fear_hands_effect.png"
					);
				BattleLogManager.Instance.QueueMessage(self,"[actor] throws the foes off balance!");
				BattleLogManager.Instance.QueueMessage("All foes' ATTACK fell!");
				foreach (Actor enemy in targets)
				{
					enemy.AddTierStatModifier("AttackDown", 3, silent: true);
					AnimationManager.Instance.PlayAnimation(219, enemy);
					if (SettingsMenuManager.Instance.VertigoUsesAtk)
						BattleManager.Instance.Damage(self, enemy, () => self.CurrentStats.ATK * 3f - enemy.CurrentStats.DEF, false);
					else
						BattleManager.Instance.Damage(self, enemy, () => self.CurrentStats.SPD * 3f - enemy.CurrentStats.DEF, false);
				}
			}
		);

		Skills["Cripple"] = new Skill(
			name: "CRIPPLE",
			description: "Deals big damage to all foes and\ngreatly reduces their SPEED.",
			target: SkillTarget.AllEnemies,
			cost: 45,
			effect: async (self, targets) =>
			{
				AudioManager.Instance.PlaySFX("SE_something_ALT");
				await AnimationManager.Instance.WaitForOmoriSpecialAnimation(
						"res://assets/pictures/dark_overlay.png",
						"res://assets/pictures/fear_spiders_effect.png"
					);
				BattleLogManager.Instance.QueueMessage(self, "[actor] cripples the foes!");
				BattleLogManager.Instance.QueueMessage("All foes' SPEED fell!");
				foreach (Actor enemy in targets)
				{
					enemy.AddTierStatModifier("SpeedDown", 3, silent: true);
					AnimationManager.Instance.PlayAnimation(219, enemy);
					BattleManager.Instance.Damage(self, enemy, () => self.CurrentStats.ATK * 3.5f - enemy.CurrentStats.DEF, false);
				}
			}
		);

		Skills["Suffocate"] = new Skill(
		   name: "SUFFOCATE",
		   description: "Deals 400 damage to all foes and\ngreatly reduces their DEFENSE.",
		   target: SkillTarget.AllEnemies,
		   cost: 45,
		   effect: async (self, targets) =>
		   {
			   AudioManager.Instance.PlaySFX("SE_reverse_swell", 0.8f, 0.9f);
			   await AnimationManager.Instance.WaitForOmoriSpecialAnimation(
					   "res://assets/pictures/dark_overlay.png",
					   "res://assets/pictures/fear_hair.png"
				   );
			   BattleLogManager.Instance.QueueMessage(self, "[actor] suffocates the foes!");
			   BattleLogManager.Instance.QueueMessage("All foes feel a shortness of breath.");
			   BattleLogManager.Instance.QueueMessage("All foes' DEFENSE fell!");
			   foreach (Actor enemy in targets)
			   {
				   AnimationManager.Instance.PlayAnimation(219, enemy);
				   BattleManager.Instance.Damage(self, enemy, () => 400, false, 0f, neverCrit: true);
				   enemy.AddTierStatModifier("DefenseDown", 3, silent: true);
			   }
		   }
		);

		Skills["AttackAgain1"] = new Skill(
			name: "Attack Again 1",
			description: "Omori Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] readies his blade.");
				await Task.Delay(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForAnimation(3, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks again!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["AttackAgain2"] = new Skill(
			name: "Attack Again 2",
			description: "Omori Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] readies his blade.");
				await Task.Delay(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForAnimation(3, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks again!");
				BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK * 2) + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["AttackAgain3"] = new Skill(
			name: "Attack Again 3",
			description: "Omori Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] readies his blade.");
				await Task.Delay(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForAnimation(290, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks again!");
				BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK * 2) + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
				await Task.Delay(500);
				BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK * 2) + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["Trip1"] = new Skill(
			name: "Trip 1",
			description: "Omori Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] walks forward.");
				await Task.Delay(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForAnimation(14, target);
				AnimationManager.Instance.PlayAnimation(219, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] trips [target]!");
				target.AddStatModifier("SpeedDown");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["Trip2"] = new Skill(
			name: "Trip 2",
			description: "Omori Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] walks forward.");
				await Task.Delay(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForAnimation(14, target);
				AnimationManager.Instance.PlayAnimation(219, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] trips [target]!");
				target.AddTierStatModifier("SpeedDown", 2);
				target.SetState("sad");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["Trip3"] = new Skill(
			name: "Trip 3",
			description: "Omori Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] walks forward.");
				await Task.Delay(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForAnimation(14, target);
				AnimationManager.Instance.PlayAnimation(219, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] trips [target]!");
				target.AddTierStatModifier("SpeedDown", 3);
				target.SetState("sad");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["ReleaseEnergy1"] = new Skill(
			name: "Release Energy 1",
			description: "Omori Followup",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] and friends come together and\nuse their ultimate attack!");
				foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
				{
					AnimationManager.Instance.PlayAnimation(243, member.Actor);
				}
				await AnimationManager.Instance.WaitForReleaseEnergy();
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(15, true);
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy, () => 300, true, 0f, false, true);
				}
				foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
				{
					member.Actor.AddStatModifier("ReleaseEnergy");
				}
			},
			hidden: true
		);

		Skills["ReleaseEnergy2"] = new Skill(
			name: "Release Energy 2",
			description: "Omori Followup",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] and friends come together and\nuse their ultimate attack!");
				foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
				{
					AnimationManager.Instance.PlayAnimation(243, member.Actor);
				}
				await AnimationManager.Instance.WaitForReleaseEnergy();
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(15, true);
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy, () => 600, true, 0f, false, true);
				}
				foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
				{
					member.Actor.AddStatModifier("ReleaseEnergy");
				}
			},
			hidden: true
		);

		Skills["ReleaseEnergy3"] = new Skill(
			name: "Release Energy 3",
			description: "Omori Followup",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] and friends come together and\nuse their ultimate attack!");
				foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
				{
					AnimationManager.Instance.PlayAnimation(243, member.Actor);
				}
				await AnimationManager.Instance.WaitForReleaseEnergy();
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(15, true);
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy, () => 1000, true, 0f, false, true);
				}
				foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
				{
					member.Actor.AddStatModifier("ReleaseEnergy");
				}
			},
			hidden: true
		);

		// SUNNY

		Skills["SRWAttack"] = new Skill(
			name: "Attack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForAnimation(108, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		);
		
		Skills["SRWAltAttack"] = new Skill(
			name: "Attack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForAnimation(21, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		);

		Skills["CalmDown"] = new Skill(
			name: "CALM DOWN",
			description: "Removes EMOTIONS and heals some HEART.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.FadeBGMTo(0.1f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] calms down.");
				AnimationManager.Instance.PlayScreenAnimation(104, false);
				await Task.Delay(2500);
				target.Heal((int)Math.Round(target.BaseStats.MaxHP * 0.5, MidpointRounding.AwayFromZero));
				target.SetState("neutral", true);
				AudioManager.Instance.FadeBGMTo(1f);
			},
			priority: SkillPriority.First
			// calm down can be used while afraid
		).WithCustomRequirement((actor) => actor.CurrentState is not "stressed");
		
		Skills["Focus"] = new Skill(
			name: "FOCUS",
			description: "[actor]'s next attack deals more damage.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.FadeBGMTo(0.1f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] focuses.");
				AnimationManager.Instance.PlayScreenAnimation(105, false);
				await Task.Delay(2500);
				target.AddStatModifier("Flex");
				AudioManager.Instance.FadeBGMTo(1f);
			},
			priority: SkillPriority.First
		);
		
		Skills["Persist"] = new Skill(
			name: "PERSIST",
			description: "HEART cannot reach 0 for 1 turn.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.FadeBGMTo(0.1f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] persists.");
				AnimationManager.Instance.PlayScreenAnimation(106, false);
				await Task.Delay(2500);
				target.Heal(20);
				target.AddStatModifier("SecondChance");
				AudioManager.Instance.FadeBGMTo(1f);
			},
			priority: SkillPriority.First
		);
		
		
		Skills["Allegro"] = new Skill(
			name: "ALLEGRO",
			description: "Attacks 3 times. \nCost: 30",
			target: SkillTarget.Enemy,
			cost: 19,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(103, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] strikes three times.");
				for (int i = 0; i < 3; i++)
				{
					BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.15f + self.CurrentStats.ATK - target.CurrentStats.DEF, false);
				}
			}
		);
		
		Skills["Encore"] = new Skill(
			name: "ENCORE",
			description: "Your JUICE will not fall for 3 turns.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] gathered themself...");
				target.AddStatModifier("Encore");
				await AnimationManager.Instance.WaitForEncore();
			}
		).WithCustomRequirement(actor => !actor.HasStatModifier("Encore") && actor.CurrentState is not ("afraid" or "stressed"));
		
		Skills["Cherish"] = new Skill(
			name: "CHERISH",
			description: "Heal your wounds and come back stronger.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] steadies their breathing.");
				AnimationManager.Instance.PlayCherish();
				await Task.Delay(2000);
				if (target.HasStatModifier("Encore"))
				{
					target.RemoveStatModifier("Encore");
					foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
					{
						member.Actor.Heal(300);
						member.Actor.HealJuice(30);
					}
				}
				else
				{
					target.Heal(target.CurrentStats.MaxHP);
					target.HealJuice(target.CurrentStats.MaxJuice);
				}
				target.AddTierStatModifier("AttackUp", silent: true);
				target.AddTierStatModifier("DefenseUp", silent: true);
				target.AddTierStatModifier("SpeedUp", silent: true);
			}
		);

		// BASIL //
		Skills["BAttack"] = new Skill(
			name: "BAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForAnimation(142, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true,
			showFollowups: true
		);

		Skills["BodySlam"] = new Skill(
			name: "BODY SLAM",
			description: "Deals damage that increases with more ENERGY.\nCost: 40",
			target: SkillTarget.Enemy,
			cost: 40,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] body slams [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 + (BattleManager.Instance.Energy * self.Level) - target.CurrentStats.DEF, false);
			}
		);

		Skills["Cheer"] = new Skill(
			name: "CHEER",
			description: "Heals all friends JUICE by 20%. Grealtly increases\na STAT if [actor] is feeling an EMOTION. Cost: 80",
			target: SkillTarget.AllAllies,
			cost: 80,
			effect: async (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(340, false);
				await Task.Delay(1000);
				BattleLogManager.Instance.QueueMessage(self, "[actor] cheers!");
				foreach (Actor member in targets)
				{
					BattleManager.Instance.HealJuice(self, member, () => member.CurrentStats.MaxJuice * 0.2f);
					string modifier = member.CurrentState switch
					{
						"happy" or "ecstatic" or "manic" => "SpeedUp",
						"sad" or "depressed" or "miserable" => "DefenseUp",
						"angry" or "enraged" or "furious" => "AttackUp",
						_ => null
					};
					if (modifier != null)
					{
						member.AddTierStatModifier(modifier, 3);
						AnimationManager.Instance.PlayAnimation(214, member);
					}
				}
			}
		);

		Skills["Photograph"] = new Skill(
			name: "PHOTOGRAPH",
			description: "Acts first, reducing HIT RATE for all foes for 1\nturn. All foes target [actor] for 1 turn. Cost: 50",
			target: SkillTarget.AllEnemies,
			cost: 50,
			effect: async (self, targets) =>
			{
				AudioManager.Instance.PlaySFX("SYS_tag1", volume: 0.9f);
				AnimationManager.Instance.PlayPhotograph();
				await Task.Delay(500);
				self.AddStatModifier("Taunt");
				foreach (Actor enemy in targets)
				{
					AnimationManager.Instance.PlayAnimation(219, enemy);
					enemy.AddStatModifier("PhotographHitRateDown");
				}
				BattleLogManager.Instance.QueueMessage(self, "[actor] takes a picture.");
				BattleLogManager.Instance.QueueMessage("The foe's HIT RATE fell!");
			},
			priority: SkillPriority.First
		);

		Skills["HerbalRemedy"] = new Skill(
			name: "HERBAL REMEDY",
			description: "Heals a friend for 75% of their HEART. Also\nincreases ENERGY by 1. Cost: 35",
			target: SkillTarget.Ally,
			cost: 35,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(341, false);
				await Task.Delay(1000);
				AnimationManager.Instance.PlayAnimation(342, target);
				await Task.Delay(1000);
				AnimationManager.Instance.PlayAnimation(212, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] brings out a remedy.");
				BattleManager.Instance.Heal(self, target, () => target.CurrentStats.MaxHP * 0.75f);
				BattleManager.Instance.AddEnergy(1);
			}
		);

		Skills["Tulip"] = new Skill(
			name: "TULIP",
			description: "Deals damage to all foes based on [first]'s\nSTATS. Cost: 40",
			target: SkillTarget.AllEnemies,
			cost: 40,
			effect: async (self, targets) =>
			{
				AudioManager.Instance.PlaySFX("GEN_shine", 0.5f, 0.9f);
				await AnimationManager.Instance.WaitForBasilSpecialAnimation("res://assets/pictures/border_tulip.png", 326);
				PartyMember first = BattleManager.Instance.GetPartyMember(0);
				BattleLogManager.Instance.QueueMessage(self, "[actor] plants a TULIP.");
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(first, enemy, () => (first.CurrentStats.ATK + first.CurrentStats.DEF + first.CurrentStats.SPD + (first.CurrentStats.LCK * 5)) - enemy.CurrentStats.DEF, false);
				}
			}
		);

		Skills["Gladiolus"] = new Skill(
			name: "GLADIOLUS",
			description: "Deals big damage that ignores DEFENSE.\nAlways hits right in the HEART. Cost: 40",
			target: SkillTarget.Enemy,
			cost: 40,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("GEN_shine", 0.5f, 0.9f);
				await AnimationManager.Instance.WaitForBasilSpecialAnimation("res://assets/pictures/border_gladiolus.png", 290);
				BattleLogManager.Instance.QueueMessage(self, "[actor] plants a GLADIOLUS.");
				BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 4; }, false, 0.1f, true);
			}
		);

		Skills["Cactus"] = new Skill(
			name: "CACTUS",
			description: "Deals damage based on DEFENSE and HEART\ninstead of ATTACK. Cost: 40",
			target: SkillTarget.Enemy,
			cost: 40,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("GEN_shine", 0.5f, 0.9f);
				await AnimationManager.Instance.WaitForBasilSpecialAnimation("res://assets/pictures/border_cactus.png", 124);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] plants a CACTUS.");
				BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.DEF * 2) + self.CurrentHP - target.CurrentStats.DEF, false, 0.1f);
			}
		);

		Skills["Rose"] = new Skill(
			name: "ROSE",
			description: "Acts first, reducing all foes' ATTACK. Heals\nall friends for 40% of their HEART. Cost: 50",
			target: SkillTarget.AllEnemies,
			cost: 50,
			priority: SkillPriority.First,
			effect: async (self, targets) =>
			{
				AudioManager.Instance.PlaySFX("GEN_shine", 0.5f, 0.9f);
				await AnimationManager.Instance.WaitForBasilSpecialAnimation("res://assets/pictures/border_rose.png", 335);
				BattleLogManager.Instance.QueueMessage(self, "[actor] plants a ROSE.");
				foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
				{
					AnimationManager.Instance.PlayAnimation(212, member.Actor);
					int heal = (int)Math.Round(self.CurrentStats.MaxHP * 0.4f, MidpointRounding.AwayFromZero);
					member.Actor.Heal(heal);
					BattleManager.Instance.SpawnDamageNumber(heal, member.Actor.CenterPoint, DamageType.Heal);
				}
				await Task.Delay(500);
				foreach (Actor enemy in targets)
				{
					AnimationManager.Instance.PlayAnimation(219, enemy);
					enemy.AddStatModifier("AttackDown", silent: true);
				}
			}
		);

		Skills["FlowerCrown"] = new Skill(
			name: "FLOWER CROWN",
			description: "Deals big damage 4 times.\nCost: 75",
			target: SkillTarget.Enemy,
			cost: 75,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForFlowerCrown();
				BattleLogManager.Instance.QueueMessage(self, "[actor] makes a FLOWER CROWN.");
				for (int i = 0; i < 4; i++)
				{
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF);
				}
			}
		);

		Skills["Vent"] = new Skill(
			name: "Vent",
			description: "Basil Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(142, target);
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForAnimation(3, target);
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] and [target] vent their ANGER!");
				BattleManager.Instance.Damage(self, target, () => (first.CurrentStats.ATK * 1.5f) + (self.CurrentStats.ATK * 1.5f) - target.CurrentStats.DEF, true, 0.1f);
				MakeAngry(first);
				MakeAngry(self);
			}
		);

		Skills["Mull"] = new Skill(
			name: "Mull",
			description: "Basil Followup",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] and [target] mull over SAD thoughts.");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(213, member);
					BattleManager.Instance.HealJuice(self, member, () => member.CurrentStats.MaxJuice * 0.25f);
				}
				// only character 1 and basil become sad
				MakeSad(first);
				MakeSad(self);
				await Task.CompletedTask;
			}
		);

		Skills["Comfort"] = new Skill(
			name: "Comfort",
			description: "Basil Followup",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(212, member);
					BattleManager.Instance.Heal(self, member, () => member.CurrentStats.MaxJuice * 0.25f);
				}
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] and [target] comfort each other.");
				// only character 1 and basil become happy
				MakeHappy(first);
				MakeHappy(self);
				await Task.CompletedTask;
			}
		);

		Skills["ReleaseEnergyBasil"] = new Skill(
			name: "Release Energy Basil",
			description: "Omori Followup",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] and friends come together and\nuse their ultimate attack!");
				foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
				{
					AnimationManager.Instance.PlayAnimation(243, member.Actor);
				}
				await AnimationManager.Instance.WaitForReleaseEnergyBasil();
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForRedHands();
				await AnimationManager.Instance.WaitForFlowerCrown();
				await AnimationManager.Instance.WaitForScreenAnimation(344, true);
				foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
				{
					AnimationManager.Instance.PlayAnimation(212, member.Actor);
					member.Actor.Heal(member.Actor.CurrentStats.MaxHP);
					member.Actor.HealJuice(member.Actor.CurrentStats.MaxJuice);
				}
				await Task.Delay(1000);
				AnimationManager.Instance.PlayPhotograph();
				foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
				{
					AnimationManager.Instance.PlayAnimation(214, member.Actor);
					if (member.Actor.HasStatModifier("ReleaseEnergyBasil"))
						member.Actor.AddStatModifier("ReleaseEnergyBasilBonus", silent: true);
					else
						member.Actor.AddStatModifier("ReleaseEnergyBasil", silent: true);
				}
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy, () => 1000, true, 0f, false, true);
				}
			},
			hidden: true
		);



		// AUBREY //
		Skills["AAttack"] = new Skill(
			name: "AAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForAnimation(28, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true,
			showFollowups: true
		);
		Skills["PepTalk"] = new Skill(
			name: "PEP TALK",
			description: "Makes a friend or foe HAPPY.\nCost: 5",
			target: SkillTarget.AllyOrEnemy,
			cost: 5,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] cheers on [target]!");
				await AnimationManager.Instance.WaitForScreenAnimation(29, target is Enemy);
				MakeHappy(target);
			}
		);
		Skills["Headbutt"] = new Skill(
			name: "HEADBUTT",
			description: "Deals big damage, but [actor] also takes damage.\nStronger when [actor] is ANGRY. Cost: 5",
			target: SkillTarget.Enemy,
			cost: 5,
			effect: async (self, target) =>
			{
				double neededHp = Math.Floor(self.CurrentStats.MaxHP * 0.2);
				if (self.CurrentHP < neededHp)
				{
					BattleLogManager.Instance.QueueMessage(self, target, "[actor] does not have enough HP!");
					// refund juice
					self.CurrentJuice += Skills["Headbutt"].Cost(self);
					return;
				}
				AnimationManager.Instance.PlayScreenAnimation(30, target is Enemy);
				await Task.Delay(1500);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] headbutts [target]!");
				if (self.CurrentState is "angry" or "enraged")
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, false);
				else
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF, false);
				self.CurrentHP = (int)Math.Max(1f, self.CurrentHP - Math.Floor(self.CurrentStats.MaxHP * 0.2));
			}
		).WithCustomRequirement(actor =>
		{
			double neededHp = Math.Floor(actor.CurrentStats.MaxHP * 0.2d);
			return actor.CurrentHP >= neededHp;
		});

		Skills["Counter"] = new Skill(
			name: "COUNTER",
			description: "All foes target [actor] for 1 turn.\nIf [actor] is attacked, she attacks. Cost: 5",
			target: SkillTarget.Self,
			cost: 5,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_protect", volume: 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] readies her bat!");
				target.AddStatModifier("Taunt");
				target.AddStatModifier("AubreyCounter");
				await Task.CompletedTask;
			},
			priority: SkillPriority.First
		);
		
		Skills["CounterAttack"] = new Skill(
			name: "CounterAttack",
			description: "Counter Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(28, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] swings back!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["PowerHit"] = new Skill(
			name: "POWER HIT",
			description: "An attack that ignore's a foe's DEFENSE,\nthen reduces the foe's DEFENSE. Cost: 20",
			target: SkillTarget.Enemy,
			cost: 20,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(31, target);
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForAnimation(219, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] smashes [target]!");
				target.AddStatModifier("DefenseDown");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f, false);
			}
		);

		Skills["Twirl"] = new Skill(
			name: "TWIRL",
			description: "[actor] attacks a foe and becomes HAPPY.\nCost: 10",
			target: SkillTarget.Enemy,
			cost: 10,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				AnimationManager.Instance.PlayAnimation(45, target);
				await Task.Delay(500);
				AnimationManager.Instance.PlayAnimation(28, target);
				await Task.Delay(500);
				int damage = BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK * 2f + self.CurrentStats.LCK) - target.CurrentStats.DEF, false);
				if (damage > -1)
				{
					MakeHappy(self);
				}

			}
		);

		Skills["MoodWrecker"] = new Skill(
			name: "MOOD WRECKER",
			description: "A swing that doesn't miss. Deals extra damage to\nHAPPY foes. Cost: 10",
			target: SkillTarget.Enemy,
			cost: 10,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(46, target);
				await Task.Delay(500);
				if (target.CurrentState is "happy" or "ecstatic" or "manic")
				{
					// very nice
					if (target.CurrentState is "ecstatic" or "manic")
						await AnimationManager.Instance.WaitForAnimation(279, target);
					else
						await AnimationManager.Instance.WaitForAnimation(278, target);
					BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF);
				}
				else
				{
					BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2.25f - target.CurrentStats.DEF);
				}
			}
		);

		Skills["TeamSpirit"] = new Skill(
			name: "TEAM SPIRIT",
			description: "Makes [actor] and a friend HAPPY.\nCost: 10",
			target: SkillTarget.AllyNotSelf,
			cost: 10,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] cheers on [target]!");
				AnimationManager.Instance.PlayAnimation(49, self);
				await Task.Delay(500);
				AnimationManager.Instance.PlayScreenAnimation(29, target is Enemy);
				MakeHappy(target);
				MakeHappy(self);
			}
		);

		Skills["WindUpThrow"] = new Skill(
			name: "WIND-UP THROW",
			description: "Damages all foes. Deals more damage the less\nenemies there are. Cost: 20",
			target: SkillTarget.AllEnemies,
			cost: 20,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] throws her weapon!");
				await AnimationManager.Instance.WaitForScreenAnimation(33, targets[0] is Enemy);
				int enemies = targets.Count;
				foreach (Actor enemy in targets)
				{
					switch (enemies)
					{
						case 1:
							BattleManager.Instance.Damage(self, enemy, () => self.CurrentStats.ATK * 3f - enemy.CurrentStats.DEF, false);
							break;
						case 2:
							BattleManager.Instance.Damage(self, enemy, () => self.CurrentStats.ATK * 2.5f - enemy.CurrentStats.DEF, false);
							break;
						default:
							BattleManager.Instance.Damage(self, enemy, () => self.CurrentStats.ATK * 2f - enemy.CurrentStats.DEF, false);
							break;
					}
				}
			}
		);

		Skills["Mash"] = new Skill(
			name: "MASH",
			description: "If this skill defeats a foe, recover 100% JUICE.\nCost: 15",
			target: SkillTarget.Enemy,
			cost: 15,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(28, target);
				await Task.Delay(500);
				AnimationManager.Instance.PlayAnimation(213, target);
				await Task.Delay(500);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF, false);
				if (target.CurrentHP == 0)
				{
					AnimationManager.Instance.PlayAnimation(213, self);
					self.HealJuice(self.CurrentStats.MaxJuice);
					BattleManager.Instance.SpawnDamageNumber(self.CurrentStats.MaxJuice, target.CenterPoint, DamageType.JuiceGain);
				}
			}
		);

		Skills["Beatdown"] = new Skill(
			name: "BEATDOWN",
			description: "Attacks a foe 3 times.\nCost: 30",
			target: SkillTarget.Enemy,
			cost: 30,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] furiously attacks!");
				await AnimationManager.Instance.WaitForAnimation(17, target);
				for (int i = 0; i < 3; i++)
				{
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false);
					await Task.Delay(1000);
				}
			}
		);

		Skills["LastResort"] = new Skill(
			name: "LAST RESORT",
			description: "Deals damage based on [actor]'s HEART,\nbut [actor] becomes TOAST. Cost: 50",
			target: SkillTarget.Enemy,
			cost: 50,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(34, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] strikes [target]\nwith all her strength!");
				int damage = BattleManager.Instance.Damage(self, target, () => self.CurrentHP * 4f, false);
				if (damage > -1)
				{
					BattleManager.Instance.SpawnDamageNumber(self.CurrentHP, self.CenterPoint);
					self.Damage(self.CurrentHP);
				}
			}
		);

		Skills["LookAtOmori1"] = new Skill(
			name: "Look At Omori 1",
			description: "Aubrey Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(35, false);
				await AnimationManager.Instance.WaitForAnimation(28, target);
				BattleLogManager.Instance.QueueMessage(self, other, "[target] didn't notice [actor], so\n[actor] attacks again!");
				BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK * 2 + self.CurrentStats.LCK) - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["LookAtOmori2"] = new Skill(
			name: "Look At Omori 2",
			description: "Aubrey Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(36, false);
				await AnimationManager.Instance.WaitForAnimation(28, target);
				BattleLogManager.Instance.QueueMessage(self, other, "[target] still didn't notice [actor], so\n[actor] attacks harder!");
				BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK * 3 + self.CurrentStats.LCK) - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["LookAtOmori3"] = new Skill(
			name: "Look At Omori 3",
			description: "Aubrey Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(37, false);
				await AnimationManager.Instance.WaitForAnimation(44, target);
				BattleLogManager.Instance.QueueMessage(self, other, "[target] finally notices [actor]!");
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] swings her bat in happiness!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 + self.CurrentStats.LCK, false);
			},
			hidden: true
		);

		Skills["LookAtKel1"] = new Skill(
			name: "Look At Kel 1",
			description: "Aubrey Followup",
			target: SkillTarget.Self,
			cost: 0,
			effect: async(Actor self, Actor target) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(2);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(38, false);
				await Task.Delay(2000);
				BattleLogManager.Instance.QueueMessage(self, other, "[target] eggs [actor] on!");
				MakeAngry(self);
			},
			hidden: true
		);

		Skills["LookAtKel2"] = new Skill(
			name: "Look At Kel 2",
			description: "Aubrey Followup",
			target: SkillTarget.Self,
			cost: 0,
			effect: async(Actor self, Actor target) =>
		   {
			   PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(2);
			   BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
			   await Task.Delay(1000);
			   AnimationManager.Instance.PlayScreenAnimation(39, false);
			   await Task.Delay(2000);
			   BattleLogManager.Instance.QueueMessage(self, other, "[target] eggs [actor] on!");
			   self.AddStatModifier("AttackUp", silent: true);
			   BattleLogManager.Instance.QueueMessage(self, other, "[target] and [actor]'s ATTACK ROSE!");
			   AnimationManager.Instance.PlayAnimation(214, self);
			   AnimationManager.Instance.PlayAnimation(214, other);
			   MakeAngry(self);
			   MakeAngry(other);
		   },
		   hidden: true
	   );

		Skills["LookAtKel3"] = new Skill(
		  name: "Look At Kel 3",
		  description: "Aubrey Followup",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (Actor self, Actor target) =>
		  {
			  PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(2);
			  BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
			  await Task.Delay(1000);
			  AnimationManager.Instance.PlayScreenAnimation(40, false);
			  await Task.Delay(2000);
			  BattleLogManager.Instance.QueueMessage(self, other, "[target] eggs [actor] on!");
			  self.AddTierStatModifier("AttackUp", 3, silent: true);
			  BattleLogManager.Instance.QueueMessage(self, other, "[target] and [actor]'s ATTACK ROSE!");
			  AnimationManager.Instance.PlayAnimation(214, self);
			  AnimationManager.Instance.PlayAnimation(214, other);
			  self.SetState("enraged");
			  other.SetState("enraged");
		  },
		  hidden: true
	  );

		Skills["LookAtHero1"] = new Skill(
			name: "Look At Hero 1",
			description: "Aubrey Followup",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor self, Actor target) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(3);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(41, false);
				await Task.Delay(2000);
				AnimationManager.Instance.PlayAnimation(214, self);
				await Task.Delay(1000);
				BattleLogManager.Instance.QueueMessage(self, other, "[target] tells [actor] to focus!");
				self.AddStatModifier("DefenseUp");
				MakeHappy(self);
			},
			hidden: true
		);

		Skills["LookAtHero2"] = new Skill(
			name: "Look At Hero 2",
			description: "Aubrey Followup",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor self, Actor target) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(3);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(42, false);
				await Task.Delay(2000);
				AnimationManager.Instance.PlayAnimation(214, self);
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForAnimation(212, self);
				int heal = (int)Math.Round(self.CurrentStats.MaxHP * 0.25f, MidpointRounding.AwayFromZero);
				BattleLogManager.Instance.QueueMessage(self, other, "[target] cheers [actor]!");
				self.Heal(heal);
				self.AddTierStatModifier("DefenseUp", 2);
				MakeHappy(self);
			},
			hidden: true
		);

		Skills["LookAtHero3"] = new Skill(
			name: "Look At Hero 3",
			description: "Aubrey Followup",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor self, Actor target) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(3);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(43, false);
				await Task.Delay(2000);
				AnimationManager.Instance.PlayAnimation(214, self);
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForAnimation(212, self);
				int heal = (int)Math.Round(self.CurrentStats.MaxHP * 0.75f, MidpointRounding.AwayFromZero);
				int juice = (int)Math.Round(self.CurrentStats.MaxJuice * 0.5f, MidpointRounding.AwayFromZero);
				BattleLogManager.Instance.QueueMessage(self, other, "[target] cheers [actor]!");
				self.Heal(heal);
				self.HealJuice(juice);
				self.AddTierStatModifier("DefenseUp", 3);
				self.SetState("ecstatic");
			},
			hidden: true
		);


		Skills["ARWAttack"] = new Skill(
			name: "ARWAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(48, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		);

		Skills["Homerun"] = new Skill(
			name: "HOMERUN",
			description: "Has a chance to instantly defeat a\nfoe. [actor] also takes damage. Cost: 25",
			target: SkillTarget.Enemy,
			cost: 25,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(32, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] hits a home run!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 4f - target.CurrentStats.DEF, neverCrit: true);
				int roll = GameManager.Instance.Random.RandiRange(0, 100);
				if (roll < 11)
				{
					target.CurrentHP = 0;
				}
				self.CurrentHP = Math.Max(0, (int)Math.Round(self.CurrentHP - self.BaseStats.MaxHP * 0.2f, MidpointRounding.AwayFromZero));
			}
		);

		// KEL //
		Skills["KAttack"] = new Skill(
			name: "KAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForAnimation(54, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true,
			showFollowups: true
		);
		Skills["Annoy"] = new Skill(
			name: "ANNOY",
			description: "Makes a friend or foe ANGRY.\nCost: 5",
			target: SkillTarget.AllyOrEnemy,
			cost: 5,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] annoys [target]!");
				await AnimationManager.Instance.WaitForScreenAnimation(55, target is Enemy);
				MakeAngry(target);
			}
		);
		Skills["Rebound"] = new Skill(
			name: "REBOUND",
			description: "Deals damage to all foes.\nCost: 15",
			target: SkillTarget.AllEnemies,
			cost: 15,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor]'s ball bounces everywhere!");
				await AnimationManager.Instance.WaitForScreenAnimation(56, targets[0] is Enemy);
				foreach (Actor enemy in targets)
					BattleManager.Instance.Damage(self, enemy, () => self.CurrentStats.ATK * 2.5f - enemy.CurrentStats.DEF, false);
			}
		);

		Skills["RunNGun"] = new Skill(
			name: "RUN 'N GUN",
			description: "[actor] does an attack based on his SPEED\ninstead of his ATTACK. Cost: 15",
			target: SkillTarget.Enemy,
			cost: 15,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(72, self);
				await Task.Delay(500);
				AnimationManager.Instance.PlayAnimation(54, target);
				await Task.Delay(500);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.SPD * 1.5f - target.CurrentStats.DEF, false);
			}
		);

		Skills["CantCatchMe"] = new Skill(
			name: "CAN'T CATCH ME",
			description: "Attracts attention and reduces all foes'\nHIT RATE for the turn. Cost: 50",
			target: SkillTarget.AllEnemies,
			cost: 50,
			effect: async (self, targets) =>
			{
				AudioManager.Instance.PlaySFX("BA_dodge", volume: 0.9f);
				BattleLogManager.Instance.QueueMessage(self, "[actor] starts taunting all the foes!");
				BattleLogManager.Instance.QueueMessage("All foes' HIT RATE fell for the turn!");
				self.AddStatModifier("Taunt");
				foreach (Actor enemy in targets)
					enemy.AddStatModifier("HitRateDown");
				await Task.CompletedTask;
			},
			priority: SkillPriority.First
		);

		Skills["Curveball"] = new Skill(
			name: "CURVEBALL",
			description: "Makes a foe feel a random EMOTION. Deals\nextra damage to foes with EMOTION. Cost: 20",
			target: SkillTarget.Enemy,
			cost: 20,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(73, target is Enemy);
				await Task.Delay(1000);
				AnimationManager.Instance.PlayAnimation(67, target);
				await Task.Delay(500);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws a curveball...");
				int damage;
				if (target.CurrentState != "neutral")
					damage = BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, false);
				else
					damage = BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false);
				if (damage > -1)
				{
					BattleManager.Instance.RandomEmotion(target);
				}
			}
		);

		Skills["Ricochet"] = new Skill(
			name: "RICOCHET",
			description: "Deals damage to a foe 3 times.\nCost: 30",
			target: SkillTarget.Enemy,
			cost: 30,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] does a fancy ball trick!");
				await AnimationManager.Instance.WaitForScreenAnimation(58, target is Enemy);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0.3f);
				await Task.Delay(1000);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0.3f);
				await Task.Delay(1000);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0.3f);
			}
		);

		Skills["Megaphone"] = new Skill(
			name: "MEGAPHONE",
			description: "Makes all friends ANGRY.\nCost: 45",
			target: SkillTarget.AllAllies,
			cost: 45,
			effect: async (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(74, targets[0] is Enemy);
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(55, targets[0] is Enemy);
				BattleLogManager.Instance.QueueMessage(self, "[actor] runs around and annoys everyone!");
				foreach (Actor member in targets)
				{
					MakeAngry(member);
				}
			}
		);

		Skills["Rally"] = new Skill(
			name: "RALLY",
			description: "[actor] becomes HAPPY. [actor]'s friends recover\nsome ENERGY and JUICE. Cost: 50",
			target: SkillTarget.AllAllies,
			cost: 50,
			effect: async (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(61, targets[0] is Enemy);
				BattleLogManager.Instance.QueueMessage(self, "[actor] gets everyone pumped up!");
				MakeHappy(self);
				BattleLogManager.Instance.QueueMessage("Everyone gains ENERGY!");
				BattleManager.Instance.AddEnergy(4);
				foreach (Actor member in targets.Where(member => member != self))
				{
					AnimationManager.Instance.PlayAnimation(213, member);
					int rounded = (int)Math.Round(member.CurrentStats.MaxJuice * 0.3f, MidpointRounding.AwayFromZero);
					member.HealJuice(rounded);
					BattleLogManager.Instance.QueueMessage(self, member, $"[target] recovered {rounded} JUICE!");
				}
				await Task.Delay(500);
			}
		);

		Skills["Comeback"] = new Skill(
			name: "COMEBACK",
			description: "Makes [actor] HAPPY. If SAD was removed,\n[actor] gains FLEX. Cost: 25",
			target: SkillTarget.Self,
			cost: 25,
			effect: async (_, target) =>
			{
				if (target.CurrentState is "sad" or "depressed" or "miserable")
				{
					AnimationManager.Instance.PlayAnimation(76, target);
					await Task.Delay(1000);
					target.AddStatModifier("Flex");
					AnimationManager.Instance.PlayAnimation(214, target);
				}
				else
				{
					AnimationManager.Instance.PlayAnimation(75, target);
				}
				MakeHappy(target);
			}
		);

		Skills["Tickle"] = new Skill(
			name: "TICKLE",
			description: "All attacks on a foe will hit right\nin the HEART for the turn. Cost: 55",
			target: SkillTarget.Enemy,
			cost: 55,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] tickles [target]!");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] let their guard down!");
				target.AddStatModifier("Tickle");
				await Task.CompletedTask;
			}
		);

		Skills["JuiceMe"] = new Skill(
			name: "JUICE ME",
			description: "Heals a lot of JUICE to a friend, but\nalso hurts the friend. Cost: 10",
			target: SkillTarget.Ally,
			cost: 10,
			effect: async (self, target) =>
			{
				if (self is PartyMember member)
				{
					string weapon = member.Weapon.Name;
					BattleLogManager.Instance.QueueMessage(self, target,
						"[actor] passes the " + weapon + " to [target]!");
				}
				else
				{
					BattleLogManager.Instance.QueueMessage(self, target,
						"[actor] passes to [target]!");
				}

				AnimationManager.Instance.PlayAnimation(123, target);
				int rounded = (int)Math.Round(target.CurrentStats.MaxJuice * 0.3f, MidpointRounding.AwayFromZero);
				target.HealJuice(rounded);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {rounded} JUICE!");
				// can juice me miss???
				BattleManager.Instance.Damage(self, target, () => target.CurrentHP * .25f, true, 0f, neverCrit: true);
				await Task.CompletedTask;
			}
		);

		Skills["Snowball"] = new Skill(
			name: "SNOWBALL",
			description: "Makes a foe SAD.\nAlso deals big damage to SAD foes. Cost: 20",
			target: SkillTarget.Enemy,
			cost: 20,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(60, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws a snowball at [target]!");
				if (target.CurrentState is "sad" or "depressed" or "miserable")
				{
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, false);
				}
				else
				{
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF, false);
					MakeSad(target);
				}
			}
		);

		Skills["Flex"] = new Skill(
			name: "FLEX",
			description: "[actor] deals more damage next turn and increases\nHIT RATE for his next attack. Cost: 10",
			target: SkillTarget.Self,
			cost: 10,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(57, true);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] flexes and feels his best!");
				BattleLogManager.Instance.QueueMessage(self, target, "[actor]'s HIT RATE rose!");
				self.AddStatModifier("Flex");
				await Task.CompletedTask;
			}
		);

		Skills["KRWAttack"] = new Skill(
			name: "KRWAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(77, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		);

		Skills["Encourage"] = new Skill(
			name: "ENCOURAGE",
			description: "[actor] encourages [first].\nRaises their attack. No cost.",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] gives some encouragement!");
				PartyMember first = BattleManager.Instance.GetPartyMember(0);
				AnimationManager.Instance.PlayAnimation(214, first);
				await Task.Delay(1000);
				first.AddTierStatModifier("AttackUp", 3);
			}
		);
		Skills["PassToOmori1"] = new Skill(
			name: "Pass To Omori 1",
			description: "Kel Followup",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (Actor self, Actor _) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] passes to [target].");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(62, false);
				await Task.Delay(1000);
				BattleLogManager.Instance.QueueMessage(self, first, "[target] wasn't looking and gets bopped!");
				BattleManager.Instance.Damage(self, first, () => 1, true, 0f, false, true);
				first.SetState("sad");
			},
			hidden: true
		);
		Skills["PassToOmori2"] = new Skill(
			name: "Pass To Omori 2",
			description: "Kel Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] passes to [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(63, false);
				BattleLogManager.Instance.QueueMessage(self, first, "[target] catches [actor]'s ball!");
				BattleLogManager.Instance.QueueMessage(first, target, "[actor] throws the ball at\n[target]!");
				BattleManager.Instance.Damage(self, target, () => (first.CurrentStats.ATK * 1.5f) + (self.CurrentStats.ATK * 1.5f) - target.CurrentStats.DEF, false);
				first.SetState("happy");
			},
			hidden: true
		);
		Skills["PassToOmori3"] = new Skill(
		   name: "Pass To Omori 3",
		   description: "Kel Followup",
		   target: SkillTarget.Enemy,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
			   BattleLogManager.Instance.QueueMessage(self, first, "[actor] passes to [target].");
			   await Task.Delay(1000);
			   await AnimationManager.Instance.WaitForScreenAnimation(64, false);
			   BattleLogManager.Instance.QueueMessage(self, first, "[target] catches [actor]'s ball!");
			   BattleLogManager.Instance.QueueMessage(first, target, "[actor] throws the ball at\n[target]!");
			   BattleManager.Instance.Damage(self, target, () => (first.CurrentStats.ATK * 2f) + (self.CurrentStats.ATK * 2f) - target.CurrentStats.DEF, false);
			   first.SetState("ecstatic");
		   },
		   hidden: true
	   );
		Skills["PassToAubrey1"] = new Skill(
			name: "Pass To Aubrey 1",
			description: "Kel Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] passes to [target].");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(65, true);
				await Task.Delay(2000);
				await AnimationManager.Instance.WaitForAnimation(66, target);
				BattleLogManager.Instance.QueueMessage(self, second, "[target] knocks the ball out of the park!");
				BattleManager.Instance.Damage(self, target, () => second.CurrentStats.ATK + self.CurrentStats.ATK - target.CurrentStats.DEF);
			},
			hidden: true
		);
		Skills["PassToAubrey2"] = new Skill(
			name: "Pass To Aubrey 2",
			description: "Kel Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] passes to [target].");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(65, true);
				await Task.Delay(2000);
				await AnimationManager.Instance.WaitForAnimation(67, target);
				BattleLogManager.Instance.QueueMessage(self, second, "[target] knocks the ball out of the park!");
				BattleManager.Instance.Damage(self, target, () => (second.CurrentStats.ATK * 2f) + self.CurrentStats.ATK - target.CurrentStats.DEF);
			},
			hidden: true
		);
		Skills["PassToAubrey3"] = new Skill(
			name: "Pass To Aubrey 3",
			description: "Kel Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] passes to [target].");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(79, true);
				await Task.Delay(2000);
				await AnimationManager.Instance.WaitForAnimation(68, target);
				BattleLogManager.Instance.QueueMessage(self, second, "[target] knocks the ball out of the park!");
				BattleManager.Instance.Damage(self, target, () => (second.CurrentStats.ATK * 2f) + (self.CurrentStats.ATK * 2f) - target.CurrentStats.DEF);
			},
			hidden: true
		);
		Skills["PassToHero1"] = new Skill(
			name: "Pass To Hero 1",
			description: "Kel Followup",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				PartyMember third = BattleManager.Instance.GetPartyMemberAtPosition(3);
				BattleLogManager.Instance.QueueMessage(self, third, "[actor] passes to [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(69, true);
				BattleLogManager.Instance.QueueMessage(self, third, "[actor] dunks on the foes!");
				foreach (Actor enemy in targets)
				{
					// VANILLA BUG: uses Aubrey's attack instead of Hero's
					BattleManager.Instance.Damage(self, enemy, () => second.CurrentStats.ATK + self.CurrentStats.ATK - enemy.CurrentStats.DEF, false);
				}
			},
			hidden: true
		);
		Skills["PassToHero2"] = new Skill(
			name: "Pass To Hero 2",
			description: "Kel Followup",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				PartyMember third = BattleManager.Instance.GetPartyMemberAtPosition(3);
				BattleLogManager.Instance.QueueMessage(self, third, "[actor] passes to [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(70, true);
				BattleLogManager.Instance.QueueMessage(self, third, "[actor] dunks on the foes!");
				foreach (Actor enemy in targets)
				{
					// VANILLA BUG: uses Aubrey's attack instead of Hero's
					BattleManager.Instance.Damage(self, enemy, () => second.CurrentStats.ATK + (self.CurrentStats.ATK * 1.5f) - enemy.CurrentStats.DEF, false);
				}
			},
			hidden: true
		);
		Skills["PassToHero3"] = new Skill(
		   name: "Pass To Hero 3",
		   description: "Kel Followup",
		   target: SkillTarget.AllEnemies,
		   cost: 0,
		   effect: async (self, targets) =>
		   {
			   PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
			   PartyMember third = BattleManager.Instance.GetPartyMemberAtPosition(3);
			   BattleLogManager.Instance.QueueMessage(self, third, "[actor] passes to [target].");
			   await Task.Delay(1000);
			   await AnimationManager.Instance.WaitForScreenAnimation(71, true);
			   BattleLogManager.Instance.QueueMessage(self, third, "[actor] dunks on the foes with style!");
			   foreach (Actor enemy in targets)
			   {
				   // VANILLA BUG: uses Aubrey's attack instead of Hero's
				   BattleManager.Instance.Damage(self, enemy, () => (second.CurrentStats.ATK * 1.5f) + (self.CurrentStats.ATK * 1.5f) - enemy.CurrentStats.DEF, false);
				   AnimationManager.Instance.PlayAnimation(219, enemy);
				   enemy.AddStatModifier("AttackDown");
			   }
		   },
		   hidden: true
	   );

		// HERO //
		Skills["HAttack"] = new Skill(
			name: "HAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForAnimation(83, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true,
			showFollowups: true
		);
		Skills["Massage"] = new Skill(
			name: "MASSAGE",
			description: "Removes a friend or foe's EMOTION.\nCost: 5",
			target: SkillTarget.AllyOrEnemy,
			cost: 5,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] massages [target]!");
				await AnimationManager.Instance.WaitForScreenAnimation(86, target is Enemy);
				target.SetState("neutral", true);
				if (target.CurrentState == "neutral")
					BattleLogManager.Instance.QueueMessage(target.Name.ToUpper() + " calms down...");
			}
		);
		Skills["Charm"] = new Skill(
			name: "CHARM",
			description: "Acts first, a foe targets [actor] for 1 turn.\nCost: 10",
			target: SkillTarget.Enemy,
			cost: 10,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] draws [target]'s\nattention.");
				await AnimationManager.Instance.WaitForScreenAnimation(90, false);
				target.AddStatModifier("Charm");
				await Task.Delay(2000);
			},
			priority: SkillPriority.First
		);
		Skills["Enchant"] = new Skill(
			name: "ENCHANT",
			description: "Acts first. A foe targets [actor] for 1 turn\nand becomes HAPPY. Cost: 15",
			target: SkillTarget.Enemy,
			cost: 15,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] draws the foe's attention\nwith a smile.");
				await AnimationManager.Instance.WaitForScreenAnimation(90, false);
				target.AddStatModifier("Charm");
				MakeHappy(target);
				await Task.Delay(2000);
			},
			priority: SkillPriority.First
		);
		Skills["Captivate"] = new Skill(
			name: "CAPTIVATE",
			description: "Acts first. All foes target [actor] for 1 turn.\nCost: 20",
			target: SkillTarget.Self,
			cost: 20,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] draws the foe's attention.");
				await AnimationManager.Instance.WaitForScreenAnimation(91, false);
				self.AddStatModifier("Taunt");
				await Task.Delay(1000);
			},
			priority: SkillPriority.First
		);
		Skills["Mesmerize"] = new Skill(
			name: "MESMERIZE",
			description: "Acts first. All foes target [actor] for 1 turn.\n[actor] takes less damage. Cost: 30",
			target: SkillTarget.Self,
			cost: 30,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] draws the foe's attention.");
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] prepares to block enemy attacks.");
				await AnimationManager.Instance.WaitForScreenAnimation(92, false);
				self.AddStatModifier("Taunt");
				self.AddStatModifier("Guard");
				await Task.Delay(1000);
			},
			priority: SkillPriority.First
		);
		Skills["SpicyFood"] = new Skill(
			name: "SPICY FOOD",
			description: "Damages a foe and makes them ANGRY.\nCost: 15",
			target: SkillTarget.Enemy,
			cost: 15,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(98, target);
				MakeAngry(target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] cooks some spicy food!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);
		Skills["Tenderize"] = new Skill(
			name: "TENDERIZE",
			description: "Deals big damage to a foe and reduces\ntheir DEFENSE. Cost: 30",
			target: SkillTarget.Enemy,
			cost: 30,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(86, true);
				await Task.Delay(332);
				AnimationManager.Instance.PlayAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] intensely massages\n[target]!");
				target.AddStatModifier("DefenseDown");
				AnimationManager.Instance.PlayAnimation(219, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 4f - target.CurrentStats.DEF, false);
			}
		);
		Skills["Smile"] = new Skill(
		   name: "SMILE",
		   description: "Acts first, reducing a foe's ATTACK.\nCost: 25",
		   target: SkillTarget.Enemy,
		   cost: 25,
		   priority: SkillPriority.First,
		   effect: async (self, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] smiles.");
			   await AnimationManager.Instance.WaitForScreenAnimation(87, false);
			   await Task.Delay(332);
			   target.AddStatModifier("AttackDown");
			   await AnimationManager.Instance.WaitForAnimation(219, target);
		   }
		);
		Skills["Dazzle"] = new Skill(
		   name: "DAZZLE",
		   description: "Acts first. Reduces all foes' ATTACK and\nmakes them HAPPY. Cost: 35",
		   target: SkillTarget.AllEnemies,
		   cost: 35,
		   priority: SkillPriority.First,
		   effect: async (self, targets) =>
		   {
			   AnimationManager.Instance.PlayScreenAnimation(90, targets[0] is Enemy);
			   await Task.Delay(500);
			   foreach (Actor enemy in targets)
			   {
				   BattleLogManager.Instance.QueueMessage(self, enemy, "[actor] smiles at [target]!");
				   AnimationManager.Instance.PlayAnimation(276, enemy);
				   enemy.AddStatModifier("AttackDown");
				   MakeHappy(enemy);
				   AnimationManager.Instance.PlayAnimation(219, enemy);
			   }
		   }
		);
		Skills["FastFood"] = new Skill(
		   name: "FAST FOOD",
		   description: "Acts first, healing a friend for 40% of\ntheir HEART. Cost: 15",
		   target: SkillTarget.Ally,
		   cost: 15,
		   priority: SkillPriority.First,
		   effect: async (self, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] prepares a quick meal for [target].");
			   await AnimationManager.Instance.WaitForAnimation(85, target);
			   int rounded = (int)Math.Round(target.CurrentStats.MaxHP * .4f, MidpointRounding.AwayFromZero);
			   target.Heal(rounded);
			   BattleManager.Instance.SpawnDamageNumber(rounded, target.CenterPoint, DamageType.Heal);
			   BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {rounded} HEART!");
			   AnimationManager.Instance.PlayAnimation(212, target);
			   await Task.Delay(1000);
		   }
		);
		Skills["ShareFood"] = new Skill(
		   name: "SHARE FOOD",
		   description: "[actor] and a friend recover some HEART.\nCost: 15",
		   target: SkillTarget.Ally,
		   cost: 15,
		   effect: async (self, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] shares food with [target]!");
			   AnimationManager.Instance.PlayAnimation(85, target);
			   AnimationManager.Instance.PlayAnimation(85, self);

			   int rounded = (int)Math.Round(target.CurrentStats.MaxHP * .5f, MidpointRounding.AwayFromZero);
			   target.Heal(rounded);
			   BattleManager.Instance.SpawnDamageNumber(rounded, target.CenterPoint, DamageType.Heal);
			   AnimationManager.Instance.PlayAnimation(212, target);

			   rounded = (int)Math.Round(self.CurrentStats.MaxHP * .5f, MidpointRounding.AwayFromZero);
			   self.Heal(rounded);
			   BattleManager.Instance.SpawnDamageNumber(rounded, self.CenterPoint, DamageType.Heal);
			   AnimationManager.Instance.PlayAnimation(212, self);
			   await Task.Delay(1000);
		   }
		);
		Skills["SnackTime"] = new Skill(
		   name: "SNACK TIME",
		   description: "Heals all friends for 40% of their HEART.\nCost: 25",
		   target: SkillTarget.AllAllies,
		   cost: 25,
		   effect: async (self, targets) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self,"[actor] made snacks for everyone!");
			   AnimationManager.Instance.PlayScreenAnimation(88, false);
			   await Task.Delay(1666);
			   foreach (Actor member in targets)
			   {
				   BattleManager.Instance.Heal(self, member, () => member.CurrentStats.MaxHP * 0.4f, 0f);
				   AnimationManager.Instance.PlayAnimation(212, member);
			   }
		   }
		);
		Skills["GatorAid"] = new Skill(
		   name: "GATOR AID",
		   description: "Boosts all friends' DEFENSE.\nCost: 15",
		   target: SkillTarget.AllAllies,
		   cost: 15,
		   effect: async (self, targets) =>
		   {
			   await AnimationManager.Instance.WaitForScreenAnimation(100, false);
			   BattleLogManager.Instance.QueueMessage(self, "[actor] gets a little help from a friend.");
			   BattleLogManager.Instance.QueueMessage("Everyone's DEFENSE rose!");
			   foreach (Actor member in targets)
			   {
					member.AddStatModifier("DefenseUp", silent: true);
					AnimationManager.Instance.PlayAnimation(214, member);
			   }
		   }
		);
		Skills["TeaTime"] = new Skill(
			name: "TEA TIME",
			description: "Heals some of a friend's HEART and JUICE.\nCost: 25",
			target: SkillTarget.AllyNotSelf,
			cost: 10,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(89, target);
				await Task.Delay(2000);
				BattleLogManager.Instance.QueueMessage(self, "[actor] brings out some tea for a break.");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] feels refreshed!");
				int heartHeal = (int)Math.Round(target.CurrentStats.MaxHP * 0.3f, MidpointRounding.AwayFromZero);
				target.Heal(heartHeal);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovers {heartHeal} HEART!");
				BattleManager.Instance.SpawnDamageNumber(heartHeal, target.CenterPoint, DamageType.Heal);
				int juiceHeal = (int)Math.Round(target.CurrentStats.MaxJuice * 0.2f, MidpointRounding.AwayFromZero);
				target.HealJuice(juiceHeal);
				BattleManager.Instance.SpawnDamageNumber(juiceHeal, target.CenterPoint + new Vector2(0, 50), DamageType.JuiceGain);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovers {juiceHeal} JUICE!");
			}
		);
		Skills["Cook"] = new Skill(
			name: "COOK",
			description: "Heals a friend for 75% of their HEART.\nCost: 10",
			target: SkillTarget.Ally,
			cost: 10,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] makes a cookie just for [target]!");
				await AnimationManager.Instance.WaitForAnimation(85, target);
				BattleManager.Instance.Heal(self, target, () => target.CurrentStats.MaxHP * 0.75f);
				AnimationManager.Instance.PlayAnimation(212, target);
				await Task.Delay(1000);
			}
		);
		Skills["Refresh"] = new Skill(
			name: "REFRESH",
			description: "Heals 50% of a friend's JUICE.\nCost: 40",
			target: SkillTarget.Ally,
			cost: 40,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] makes a refreshment for [target].");
				AnimationManager.Instance.PlayAnimation(213, target);
				BattleManager.Instance.HealJuice(self, target, () => target.CurrentStats.MaxJuice * 0.5f);
				await Task.Delay(1000);
			}
		);
		Skills["HomemadeJam"] = new Skill(
			name: "HOMEMADE JAM",
			description: "Brings back a friend that is TOAST.\nCost: 40",
			target: SkillTarget.DeadAlly,
			cost: 40,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] makes HOMEMADE JAM!");
				if (target.CurrentState != "toast")
				{
					target = BattleManager.Instance.GetRandomDeadPartyMember();
					if (target == null)
					{
						BattleLogManager.Instance.QueueMessage("It had no effect.");
						return;
					}
				}
				await AnimationManager.Instance.WaitForAnimation(269, target);
				target.SetState("neutral", true);
				int heal = (int)Math.Round(target.CurrentStats.MaxHP * 0.7f, MidpointRounding.AwayFromZero);
				target.Heal(heal);
				BattleManager.Instance.SpawnDamageNumber(heal, target.CenterPoint, DamageType.Heal);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {heal} HEART!");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] rose again!");
				await Task.Delay(1000);
			}
		);

		Skills["CallOmori1"] = new Skill(
			name: "Call Omori 1",
			description: "Hero Followup",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (Actor self, Actor target) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] calls out to [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(93, false);
				await AnimationManager.Instance.WaitForAnimation(212, first);
				int heal = (int)Math.Round(first.CurrentStats.MaxHP * 0.15f, MidpointRounding.AwayFromZero);
				first.Heal(heal);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] signals to [target]!");
				BattleLogManager.Instance.QueueMessage(self, first, $"[target] recovers {heal} HEART!");
				BattleManager.Instance.ForceCommand(first, BattleManager.Instance.GetRandomAliveEnemy(), Skills["OAttack"]);
			},
			hidden: true
		);
		Skills["CallOmori2"] = new Skill(
			name: "Call Omori 2",
			description: "Hero Followup",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (Actor self, Actor target) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] calls out to [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(93, false);
				await AnimationManager.Instance.WaitForAnimation(212, first);
				int heal = (int)Math.Round(first.CurrentStats.MaxHP * 0.25f, MidpointRounding.AwayFromZero);
				int juice = (int)Math.Round(first.CurrentStats.MaxJuice * 0.1f, MidpointRounding.AwayFromZero);
				first.Heal(heal);
				first.HealJuice(juice);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] signals to [target]!");
				BattleLogManager.Instance.QueueMessage(self, first, $"[target] recovers {heal} HEART!");
				BattleLogManager.Instance.QueueMessage(self, first, $"[target] recovers {juice} JUICE!");
				BattleManager.Instance.ForceCommand(first, BattleManager.Instance.GetRandomAliveEnemy(), Skills["OAttack"]);
			},
			hidden: true
		);
		Skills["CallOmori3"] = new Skill(
			name: "Call Omori 3",
			description: "Hero Followup",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (Actor self, Actor target) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] calls out to [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(93, false);
				await AnimationManager.Instance.WaitForAnimation(212, first);
				int heal = (int)Math.Round(first.CurrentStats.MaxHP * 0.4f, MidpointRounding.AwayFromZero);
				int juice = (int)Math.Round(first.CurrentStats.MaxJuice * 0.2f, MidpointRounding.AwayFromZero);
				first.Heal(heal);
				first.HealJuice(juice);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] signals to [target]!");
				BattleLogManager.Instance.QueueMessage(self, first, $"[target] recovers {heal} HEART!");
				BattleLogManager.Instance.QueueMessage(self, first, $"[target] recovers {juice} JUICE!");
				BattleManager.Instance.ForceCommand(first, BattleManager.Instance.GetRandomAliveEnemy(), Skills["OAttack"]);
			},
			hidden: true
		);

		Skills["CallAubrey1"] = new Skill(
			name: "Call Aubrey 1",
			description: "Hero Followup",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (Actor self, Actor target) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] calls out to [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(94, false);
				await AnimationManager.Instance.WaitForAnimation(212, second);
				int heal = (int)Math.Round(second.CurrentStats.MaxHP * 0.15f, MidpointRounding.AwayFromZero);
				second.Heal(heal);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] encourages [target]!");
				BattleLogManager.Instance.QueueMessage(self, second, $"[target] recovers {heal} HEART!");
				BattleManager.Instance.ForceCommand(second, BattleManager.Instance.GetRandomAliveEnemy(), Skills["AAttack"]);
			},
			hidden: true
		);

		Skills["CallAubrey2"] = new Skill(
			name: "Call Aubrey 2",
			description: "Hero Followup",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (Actor self, Actor target) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] calls out to [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(94, false);
				await AnimationManager.Instance.WaitForAnimation(212, second);
				int heal = (int)Math.Round(second.CurrentStats.MaxHP * 0.25f, MidpointRounding.AwayFromZero);
				int juice = (int)Math.Round(second.CurrentStats.MaxJuice * 0.1f, MidpointRounding.AwayFromZero);
				second.Heal(heal);
				second.HealJuice(juice);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] encourages [target]!");
				BattleLogManager.Instance.QueueMessage(self, second, $"[target] recovers {heal} HEART!");
				BattleLogManager.Instance.QueueMessage(self, second, $"[target] recovers {juice} JUICE!");
				BattleManager.Instance.ForceCommand(second, BattleManager.Instance.GetRandomAliveEnemy(), Skills["AAttack"]);
			},
			hidden: true
		);

		Skills["CallAubrey3"] = new Skill(
			name: "Call Aubrey 3",
			description: "Hero Followup",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (Actor self, Actor target) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] calls out to [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(94, false);
				await AnimationManager.Instance.WaitForAnimation(212, second);
				int heal = (int)Math.Round(second.CurrentStats.MaxHP * 0.40f, MidpointRounding.AwayFromZero);
				int juice = (int)Math.Round(second.CurrentStats.MaxJuice * 0.2f, MidpointRounding.AwayFromZero);
				second.Heal(heal);
				second.HealJuice(juice);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] encourages [target]!");
				BattleLogManager.Instance.QueueMessage(self, second, $"[target] recovers {heal} HEART!");
				BattleLogManager.Instance.QueueMessage(self, second, $"[target] recovers {juice} JUICE!");
				BattleManager.Instance.ForceCommand(second, BattleManager.Instance.GetRandomAliveEnemy(), Skills["AAttack"]);
			},
			hidden: true
		);

		Skills["CallKel1"] = new Skill(
			name: "Call Kel 1",
			description: "Hero Followup",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (Actor self, Actor target) =>
			{
				PartyMember fourth = BattleManager.Instance.GetPartyMemberAtPosition(2);
				BattleLogManager.Instance.QueueMessage(self, fourth, "[actor] calls out to [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(95, false);
				await AnimationManager.Instance.WaitForAnimation(212, fourth);
				int heal = (int)Math.Round(fourth.CurrentStats.MaxHP * 0.15f, MidpointRounding.AwayFromZero);
				fourth.Heal(heal);
				BattleLogManager.Instance.QueueMessage(self, fourth, "[actor] psyches up [target]!");
				BattleLogManager.Instance.QueueMessage(self, fourth, $"[target] recovers {heal} HEART!");
				BattleManager.Instance.ForceCommand(fourth, BattleManager.Instance.GetRandomAliveEnemy(), Skills["KAttack"]);
			},
			hidden: true
		);

		Skills["CallKel2"] = new Skill(
			name: "Call Kel 2",
			description: "Hero Followup",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (Actor self, Actor target) =>
			{
				PartyMember fourth = BattleManager.Instance.GetPartyMemberAtPosition(2);
				BattleLogManager.Instance.QueueMessage(self, fourth, "[actor] calls out to [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(95, false);
				await AnimationManager.Instance.WaitForAnimation(212, fourth);
				int heal = (int)Math.Round(fourth.CurrentStats.MaxHP * 0.25f, MidpointRounding.AwayFromZero);
				int juice = (int)Math.Round(fourth.CurrentStats.MaxJuice * 0.1f, MidpointRounding.AwayFromZero);
				fourth.Heal(heal);
				fourth.HealJuice(juice);
				BattleLogManager.Instance.QueueMessage(self, fourth, "[actor] psyches up [target]!");
				BattleLogManager.Instance.QueueMessage(self, fourth, $"[target] recovers {heal} HEART!");
				BattleLogManager.Instance.QueueMessage(self, fourth, $"[target] recovers {juice} JUICE!");
				BattleManager.Instance.ForceCommand(fourth, BattleManager.Instance.GetRandomAliveEnemy(), Skills["KAttack"]);
			},
			hidden: true
		);

		Skills["CallKel3"] = new Skill(
			name: "Call Kel 3",
			description: "Hero Followup",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (Actor self, Actor target) =>
			{
				PartyMember fourth = BattleManager.Instance.GetPartyMemberAtPosition(2);
				BattleLogManager.Instance.QueueMessage(self, fourth, "[actor] calls out to [target].");
				await Task.Delay(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(95, false);
				await AnimationManager.Instance.WaitForAnimation(212, fourth);
				int heal = (int)Math.Round(fourth.CurrentStats.MaxHP * 0.4f, MidpointRounding.AwayFromZero);
				int juice = (int)Math.Round(fourth.CurrentStats.MaxJuice * 0.2f, MidpointRounding.AwayFromZero);
				fourth.Heal(heal);
				fourth.HealJuice(juice);
				BattleLogManager.Instance.QueueMessage(self, fourth, "[actor] psyches up [target]!");
				BattleLogManager.Instance.QueueMessage(self, fourth, $"[target] recovers {heal} HEART!");
				BattleLogManager.Instance.QueueMessage(self, fourth, $"[target] recovers {juice} JUICE!");
				BattleManager.Instance.ForceCommand(fourth, BattleManager.Instance.GetRandomAliveEnemy(), Skills["KAttack"]);
			},
			hidden: true
		);

		Skills["HRWAttack"] = new Skill(
			name: "Attack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(99, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		);

		Skills["FirstAid"] = new Skill(
			name: "FIRST AID",
			description: "Heals a friend for 25% of their HEART.\nCost: 10",
			target: SkillTarget.Ally,
			cost: 10,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] provides first aid!");
				await AnimationManager.Instance.WaitForAnimation(114, target);
				float heal = target.CurrentStats.MaxHP * 0.25f;
				float variance = GameManager.Instance.Random.RandfRange(0.8f, 1.2f);
				int finalHeal = (int)Math.Round(heal * variance, MidpointRounding.AwayFromZero);
				target.Heal(finalHeal);
				BattleManager.Instance.SpawnDamageNumber(finalHeal, target.CenterPoint, DamageType.Heal);
				AnimationManager.Instance.PlayAnimation(212, target);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {finalHeal} HEART!");
				await Task.Delay(1000);
			}
		);

		// LOST SPROUT MOLE //
		Skills["LSMAttack"] = new Skill(
			name: "Attack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] bumps into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["LSMDoNothing"] = new Skill(
			name: "Do Nothing",
			description: "Does nothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_dance");
				BattleLogManager.Instance.QueueMessage(target, "[actor] is rolling around.");
				await Task.CompletedTask;
			},
			hidden: true
		);

		Skills["LSMRunAround"] = new Skill(
			name: "Run Around",
			description: "Run Around",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(200, targets[0] is Enemy);
				BattleLogManager.Instance.QueueMessage(self, targets[0], "[actor] runs around!");
				await Task.Delay(100);
				BattleManager.Instance.Damage(self, targets[0], () => self.CurrentStats.ATK * 1.5f - targets[0].CurrentStats.DEF, false);
				await Task.Delay(917);
				BattleManager.Instance.Damage(self, targets[0], () => self.CurrentStats.ATK * 1.5f - targets[0].CurrentStats.DEF, false);
			},
			hidden: true
		);

		// FOREST BUNNY? //
		Skills["FBQAttack"] = new Skill(
			name: "Attack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] nibbles at [target]?");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["FBQDoNothing"] = new Skill(
			name: "Do Nothing",
			description: "Does nothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_falls_over");
				BattleLogManager.Instance.QueueMessage(target, "[actor] is hopping around?");
				await Task.CompletedTask;
			},
			hidden: true
		);

		Skills["FBQBeCute"] = new Skill(
			name: "Be Cute",
			description: "Be Cute",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] winks at [target]?");
				await AnimationManager.Instance.WaitForAnimation(148, self);
				await AnimationManager.Instance.WaitForAnimation(215, target);
				target.AddStatModifier("AttackDown");
			},
			hidden: true
		);

		Skills["FBQSadEyes"] = new Skill(
			name: "Sad Eyes",
			description: "Sad Eyes",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks sadly at [target]?");
				await AnimationManager.Instance.WaitForAnimation(149, self);
				MakeSad(target);
			},
			hidden: true
		);

		// SWEETHEART //
		Skills["SHAttack"] = new Skill(
			name: "Attack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(132, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slaps [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["SharpInsult"] = new Skill(
			name: "Sharp Insult",
			description: "Sharp Insult",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] insults everyone!");
				await AnimationManager.Instance.WaitForScreenAnimation(183, false);
				foreach (Actor member in targets) {
					BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK, false, 0.1f, neverCrit: true);
					MakeAngry(member);
				}
			},
			hidden: true
		);

		Skills["SwingMace"] = new Skill(
			name: "Swing Mace",
			description: "Swing Mace",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] swings her mace!");
				await AnimationManager.Instance.WaitForScreenAnimation(206, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2.5f - member.CurrentStats.DEF, false);
				}
			},
			hidden: true
		);

		Skills["Brag"] = new Skill(
			name: "Brag",
			description: "Brag",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] boasts about one of her\nmany, many talents!");
				await AnimationManager.Instance.WaitForScreenAnimation(162, false);
				MakeHappy(target);
			},
			hidden: true
		);

		// SLIME GIRLS //
		Skills["ComboAttack"] = new Skill(
			name: "ComboAttack",
			description: "ComboAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "The [actor] attack all at once!");
				AnimationManager.Instance.PlayAnimation(133, target);
				await Task.Delay(580);
				AnimationManager.Instance.PlayAnimation(134, target);
				await Task.Delay(580);
				AnimationManager.Instance.PlayAnimation(135, target);
				await Task.Delay(580);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true
		);

		Skills["StrangeGas"] = new Skill(
			name: "StrangeGas",
			description: "StrangeGas",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage("MEDUSA threw a bottle...");
				AnimationManager.Instance.PlayScreenAnimation(194, false);
				await Task.Delay(1500);
				AnimationManager.Instance.PlayScreenAnimation(181, false);
				BattleLogManager.Instance.QueueMessage("A strange gas fills the room.");
				await Task.Delay(2000);

				foreach (Actor member in targets)
				{
					BattleManager.Instance.RandomEmotion(member);
				}
			},
			hidden: true
		);

		Skills["Dynamite"] = new Skill(
			name: "Dynamite",
			description: "Dynamite",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage("MEDUSA threw a bottle...");
				AnimationManager.Instance.PlayScreenAnimation(194, false);
				await Task.Delay(1500);
				AnimationManager.Instance.PlayScreenAnimation(172, false);
				BattleLogManager.Instance.QueueMessage("And it explodes!");
				await Task.Delay(2000);

				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 75, false, 0f, false, true);
				}
			},
			hidden: true
		);

		Skills["StingRay"] = new Skill(
			name: "StingRay",
			description: "StingRay",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "MOLLY fires her stingers!\n[target] gets struck!");
				await AnimationManager.Instance.WaitForAnimation(193, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2, false, neverCrit: true);
				AnimationManager.Instance.PlayAnimation(215, target);
				target.AddTierStatModifier("SpeedDown", 3);
			},
			hidden: true
		);

		Skills["Chainsaw"] = new Skill(
			name: "Chainsaw",
			description: "Chainsaw",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] pulls out a chainsaw!");
				await AnimationManager.Instance.WaitForAnimation(208, target);
				for (int i = 0; i < 3; i++)
				{
					BattleManager.Instance.Damage(self, target, () => 40, false, 0.75f, false, true);
					await Task.Delay(500);
				}
			},
			hidden: true
		);
		
		Skills["ChainsawAlt"] = new Skill(
			name: "ChainsawAlt",
			description: "ChainsawAlt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] pulls out a chainsaw!");
				await AnimationManager.Instance.WaitForAnimation(208, target);
				for (int i = 0; i < 3; i++)
				{
					BattleManager.Instance.Damage(self, target, () => 100, false, 0.75f, false, true);
					await Task.Delay(500);
				}
			},
			hidden: true
		);

		Skills["Swap"] = new Skill(
			name: "Swap",
			description: "Swap",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] did their thing!\nHEART and JUICE were swapped!");
				await AnimationManager.Instance.WaitForScreenAnimation(191, false);
				foreach (Actor member in targets)
				{
					int hp = member.CurrentHP;
					int juice = member.CurrentJuice;
					member.CurrentHP = Math.Min(member.CurrentStats.MaxHP, juice + 1);
					member.CurrentJuice = Math.Min(member.CurrentStats.MaxJuice, hp);
				}
			},
			hidden: true
		);

		Skills["SlimeUltimateAttack"] = new Skill(
			name: "SlimeUltimateAttack",
			description: "SlimeUltimateAttack",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] throw everything they have!");
				AnimationManager.Instance.PlayScreenAnimation(293, false);
				await Task.Delay(1162);
				AnimationManager.Instance.TintScreen(new Color(0, 0, 1f, 0.61f), 1f);
				AnimationManager.Instance.PlayScreenAnimation(181, false);
				await Task.Delay(332);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.SpawnDamageNumber(member.CurrentJuice, member.CenterPoint, DamageType.JuiceLoss);
					member.CurrentJuice = 0;
				}
				await Task.Delay(1660);
				AnimationManager.Instance.TintScreen(new Color(0.63f, 0.63f, 0f, 0.61f), 1f);
				await Task.Delay(332);
				foreach (Actor member in targets)
					AnimationManager.Instance.PlayAnimation(193, member);
				await Task.Delay(664);
				foreach (Actor member in targets)
				{
					member.AddTierStatModifier("AttackDown", 3, silent: true);
					member.AddTierStatModifier("DefenseDown", 3, silent: true);
					member.AddTierStatModifier("SpeedDown", 3, silent: true);
					AnimationManager.Instance.PlayAnimation(215, member);
				}
				BattleLogManager.Instance.QueueMessage("Everyone's ATTACK fell.");
				await Task.Delay(166);
				BattleLogManager.Instance.QueueMessage("Everyone's DEFENSE fell.");
				await Task.Delay(166);
				BattleLogManager.Instance.QueueMessage("Everyone's SPEED fell.");
				await Task.Delay(1330);
				AnimationManager.Instance.TintScreen(new Color(1f, 0f, 0f, 0.61f), 1f);
				await Task.Delay(332);
				AnimationManager.Instance.PlayScreenAnimation(172, false);
				await Task.Delay(332);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => member.CurrentStats.MaxHP * 0.4f, false, 0f, neverCrit: true);
					BattleManager.Instance.RandomEmotion(member);
				}
				AnimationManager.Instance.TintScreen(Colors.Transparent);
				await Task.Delay(664);
			},
			hidden: true
		);

		// BIG STRONG TREE //
		Skills["BSTDoNothing"] = new Skill(
			name: "BSTDoNothing",
			description: "BSTDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				int roll = GameManager.Instance.Random.RandiRange(0, 1);
				if (roll == 0)
					BattleLogManager.Instance.QueueMessage("A gentle breeze blows across the leaves.");
				else
					BattleLogManager.Instance.QueueMessage(target, "[actor] stands firm\nbecause it is a tree.");
				await Task.CompletedTask;
			},
			hidden: true
		);

		// DOWNLOAD WINDOW //
		Skills["DWDoNothing1"] = new Skill(
			name: "DWDoNothing1",
			description: "DWDoNothing1",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] is at 99%.");
				await Task.CompletedTask;
			},
			hidden: true
		);
		Skills["DWDoNothing2"] = new Skill(
			name: "DWDoNothing2",
			description: "DWDoNothing2",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] is still at 99%.");
				await Task.CompletedTask;
			},
			hidden: true
		);
		Skills["Crash"] = new Skill(
			name: "Crash",
			description: "Crash",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] crashes and burns!");
				AnimationManager.Instance.PlayScreenAnimation(165, targets[0] is Enemy);
				await Task.Delay(3652);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => member.CurrentStats.MaxHP * 0.8f, true, 0f, false, true);
				}
			},
			hidden: true
		);

		// SPACE EX BOYFRIEND //
		Skills["SEBAttack"] = new Skill(
			name: "SEBAttack",
			description: "SEBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] kicks [target]!");
				BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK * 2) + 5 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["SEBDoNothing"] = new Skill(
			name: "SEBDoNothing",
			description: "SEBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_,target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] looks wistfully\ninto the distance.");
				await Task.CompletedTask;
			},
			hidden: true
		);

		Skills["AngstySong"] = new Skill(
			name: "AngstySong",
			description: "AngstySong",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] sings sadly...");
				await AnimationManager.Instance.WaitForScreenAnimation(154, target is Enemy);
				MakeSad(target);
			},
			hidden: true
		);

		Skills["AngrySong"] = new Skill(
			name: "AngrySong",
			description: "AngrySong",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] wails wildly!");
				await AnimationManager.Instance.WaitForScreenAnimation(153, targets[0] is Enemy);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
				}
			},
			hidden: true
		);

		Skills["SpaceLaser"] = new Skill(
			name: "SpaceLaser",
			description: "SpaceLaser",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(160, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] fires his laser!");
				BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK * 2.5f) - target.CurrentStats.DEF, false);
			},
			hidden: true
		);

		Skills["BulletHell"] = new Skill(
			name: "BulletHell",
			description: "BulletHell",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] fires wildly!");
				await AnimationManager.Instance.WaitForScreenAnimation(168, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 20, false, neverCrit: true);
				}
			},
			hidden: true
		);
		
		Skills["BRBulletHell"] = new Skill(
			name: "BRBulletHell",
			description: "BRBulletHell",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] fires wildly!");
				await AnimationManager.Instance.WaitForScreenAnimation(168, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 100, false, neverCrit: true);
				}
			},
			hidden: true
		);

		// AUBREY (Enemy) //

		Skills["AEAttack"] = new Skill(
			name: "AEAttack",
			description: "AEAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(28, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true
		);

		Skills["AEDoNothing"] = new Skill(
			name: "AEDoNothing",
			description: "AEDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] spits on your shoe.");
				await Task.CompletedTask;
			},
			hidden: true
		);

		Skills["AEHeadbutt"] = new Skill(
			name: "AEHeadbutt",
			description: "AEHeadbutt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] headbutts [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true
		);

		// Gator Guy //

		Skills["GGAttack"] = new Skill(
			name: "GGAttack",
			description: "GGAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] karate chops [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true
		);

		Skills["GGDoNothing"] = new Skill(
			name: "GGDoNothing",
			description: "GGDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] cracks his knuckles.");
				await Task.CompletedTask;
			},
			hidden: true
		);

		Skills["GGRoughUp"] = new Skill(
			name: "GGRoughUp",
			description: "GGRoughUp",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] gets rough.");
				await Task.Delay(100);
				AnimationManager.Instance.PlayAnimation(123, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF, false, neverCrit: true);
				await Task.Delay(917);
				AnimationManager.Instance.PlayAnimation(123, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true
		);

		// Mr. Jawsum //
		Skills["MJAttackOrder"] = new Skill(
			name: "MJAttackOrder",
			description: "MJAttackOrder",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] gives orders to attack!");
				AudioManager.Instance.PlaySFX("SE_dinosaur", 0.8f, 1f);
				await Task.Delay(250);
				foreach (Actor enemy in targets)
				{
					MakeAngry(enemy);
				}
			},
			hidden: true
		);

		Skills["MJSummonGator"] = new Skill(
			name: "MJSummonGator",
			description: "MJSummonGator",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForScreenAnimation(146, true);
				BattleLogManager.Instance.QueueMessage(target, "[actor] picks up the phone and\ncalls a GATOR GUY!");
				if (target is MrJawsum jawsum)
					jawsum.SpawnGatorGuy();
				if (target is MrJawsumAlt jawsumAlt)
					jawsumAlt.SpawnGatorGuy();
			},
			hidden: true
		);

		// Fear of Spiders //
		Skills["FOSAttack"] = new Skill(
		   name: "FOSAttack",
		   description: "FOSAttack",
		   target: SkillTarget.Enemy,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   await AnimationManager.Instance.WaitForAnimation(287, target);
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] wraps up and eats [target].");
			   BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
		   },
		   hidden: true
		);

		Skills["FOSDoNothing"] = new Skill(
		   name: "FOSDoNothing",
		   description: "FOSDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(target, "[actor] is trying to talk to you...");
			   await Task.CompletedTask;
		   },
		   hidden: true
		);

		Skills["FOSSpinWeb"] = new Skill(
		   name: "FOSSpinWeb",
		   description: "FOSSpinWeb",
		   target: SkillTarget.Enemy,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   AnimationManager.Instance.PlayScreenAnimation(176, false);
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] entangles [target]\nin sticky webs.");
			   target.AddStatModifier("SpeedDown");
			   await Task.CompletedTask;
		   },
		   hidden: true
		);

		Skills["FOSAttackAll"] = new Skill(
		   name: "FOSAttackAll",
		   description: "FOSAttackAll",
		   target: SkillTarget.AllEnemies,
		   cost: 0,
		   effect: async (self, targets) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, "[actor] catches everyone!");
			   AnimationManager.Instance.PlayScreenAnimation(176, false);
			   await Task.Delay(1000);
			   foreach (Actor member in targets)
			   {
				   BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2f - member.CurrentStats.DEF, false);
				   AnimationManager.Instance.PlayAnimation(287, member);
			   }
		   },
		   hidden: true
		);

		// Unbread Twins //
		Skills["UBTAttack"] = new Skill(
		   name: "UBTAttack",
		   description: "UBTAttack",
		   target: SkillTarget.Enemy,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] attack together!");
			   await AnimationManager.Instance.WaitForAnimation(124, target);
			   BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			   await Task.Delay(500);
			   BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
		   },
		   hidden: true
		);

		Skills["UBTDoNothing"] = new Skill(
		   name: "UBTDoNothing",
		   description: "UBTDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] forget something\nin the oven!");
			   await Task.CompletedTask;
		   },
		   hidden: true
		);

		Skills["UBTCheerUp"] = new Skill(
		   name: "UBTCheerUp",
		   description: "UBTCheerUp",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   await AnimationManager.Instance.WaitForAnimation(180, target);
			   BattleLogManager.Instance.QueueMessage(target, "[actor] do their best to not\nbe SAD.");
			   target.SetState("neutral", true);
		   },
		   hidden: true
		);

		Skills["UBTCook"] = new Skill(
		   name: "UBTCook",
		   description: "UBTCook",
		   target: SkillTarget.Ally,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   await AnimationManager.Instance.WaitForAnimation(85, target);
			   BattleLogManager.Instance.QueueMessage(self, "[actor] makes a cookie!");
			   BattleManager.Instance.Heal(self, target, () =>
			   {
				   if (self.CurrentJuice <= 0)
					   return 1;
				   return self.CurrentJuice * 0.4f;
			   }, 0f);
			   await AnimationManager.Instance.WaitForAnimation(216, target);
		   },
		   hidden: true
		);

		Skills["UBTBakeBread"] = new Skill(
		   name: "UBTBakeBread",
		   description: "UBTBakeBread",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   await AnimationManager.Instance.WaitForAnimation(145, target);
			   BattleLogManager.Instance.QueueMessage(target,"[actor] pull out some\nBREAD from the oven!");
			   if (target is UnbreadTwins twins)
				   twins.SpawnBread();
			   if (target is UnbreadTwinsAlt twinsAlt)
				   twinsAlt.SpawnBread();
		   },
		   hidden: true
		);

		// Bun Bunny //
		Skills["BBAttack"] = new Skill(
		  name: "BBAttack",
		  description: "BBAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(122, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] bumps buns with [target]!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
		  },
		  hidden: true
		);

		Skills["BBDoNothing"] = new Skill(
		   name: "BBDoNothing",
		   description: "BBDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(target, "[actor] is loafing around.");
			   AudioManager.Instance.PlaySFX("BA_Drink", volume: 0.9f);
			   await Task.CompletedTask;
		   },
		   hidden: true
		);

		Skills["BBHide"] = new Skill(
		   name: "BBHide",
		   description: "BBHide",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   await AnimationManager.Instance.WaitForAnimation(178, target);
			   BattleLogManager.Instance.QueueMessage(target, "[actor] hides in its bun.");
			   target.AddStatModifier("Guard");
		   },
		   hidden: true,
		   priority: SkillPriority.First
		);

		// Creepypasta //
		Skills["CPAttack"] = new Skill(
		  name: "CPAttack",
		  description: "CPAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] makes [target] feel uncomfortable.");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
		  },
		  hidden: true
		);

		Skills["CPDoNothing"] = new Skill(
		   name: "CPDoNothing",
		   description: "CPDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(target, "[actor] does nothing...menacingly!");
			   AudioManager.Instance.PlaySFX("SE_evil5", volume: 0.9f);
			   await Task.CompletedTask;
		   },
		   hidden: true
		);

		Skills["CPScare"] = new Skill(
		   name: "CPScare",
		   description: "CPScare",
		   target: SkillTarget.AllEnemies,
		   cost: 0,
		   effect: async (self, targets) =>
		   {
			   AnimationManager.Instance.PlayAnimation(195, self);
			   BattleLogManager.Instance.QueueMessage(self, "[actor] shows everyone their worst nightmare!");
			   await Task.Delay(1500);
			   foreach (Actor member in targets)
			   {
				   member.SetState("afraid");
			   }
		   },
		   hidden: true
		);

		// Slice //
		Skills["SLAttack"] = new Skill(
		  name: "SLAttack",
		  description: "SLAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] charges into [target].");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
		  },
		  hidden: true
		);

		Skills["SLDoNothing"] = new Skill(
		   name: "SLDoNothing",
		   description: "SLDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(target, "[actor] picks its nose.");
			   AudioManager.Instance.PlaySFX("BA_Drink", volume: 0.9f);
			   await Task.CompletedTask;
		   },
		   hidden: true
		);

		Skills["SLRile"] = new Skill(
		   name: "SLRile",
		   description: "SLRile",
		   target: SkillTarget.AllAllies,
		   cost: 0,
		   effect: async (self, targets) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, "[actor] gives a controversial speech!");
			   foreach (Actor enemy in targets)
			   {
				   MakeAngry(enemy);
				   enemy.AddTierStatModifier("AttackUp", silent: true);
			   }
			   await Task.CompletedTask;
		   },
		   hidden: true
		);

		// Sourdough //
		Skills["SDAttack"] = new Skill(
		  name: "SDAttack",
		  description: "SDAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] steps on [target].");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
		  },
		  hidden: true
		);

		Skills["SDDoNothing"] = new Skill(
		   name: "SLDoNothing",
		   description: "SLDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(target, "[actor] kicks some dirt.");
			   AudioManager.Instance.PlaySFX("BA_INK", volume: 0.9f);
			   await Task.CompletedTask;
		   },
		   hidden: true
		);

		Skills["SDBadWord"] = new Skill(
		   name: "SDBadWord",
		   description: "SDBadWord",
		   target: SkillTarget.Enemy,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   AnimationManager.Instance.PlayAnimation(188, self);
			   BattleLogManager.Instance.QueueMessage(self, target, "Oh no! [actor] says a bad word!");
			   await Task.Delay(1500);
			   BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK, false, neverCrit: true);
		   },
		   hidden: true
		);

		// Sesame //
		Skills["SESAttack"] = new Skill(
		  name: "SESAttack",
		  description: "SESAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws seeds at [target].");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
		  },
		  hidden: true
		);

		Skills["SESDoNothing"] = new Skill(
		   name: "SESDoNothing",
		   description: "SESDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(target, "[actor] scratches their head.");
			   AudioManager.Instance.PlaySFX("BA_do_nothing_dance", volume: 0.9f);
			   await Task.CompletedTask;
		   },
		   hidden: true
		);

		Skills["SESBreadRoll"] = new Skill(
		   name: "SESBreadRoll",
		   description: "SESBreadRoll",
		   target: SkillTarget.AllEnemies,
		   cost: 0,
		   effect: async (self, targets) =>
		   {
			   await AnimationManager.Instance.WaitForScreenAnimation(207, false);
			   BattleLogManager.Instance.QueueMessage(self, "[actor] rolls over everyone!");
			   foreach (Actor member in targets)
				   BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false, neverCrit: true);
		   },
		   hidden: true
		);

		// Living Bread //
		Skills["LBAttack"] = new Skill(
		  name: "LBAttack",
		  description: "LBAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] bites at [target].");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
		  },
		  hidden: true
		);

		Skills["LBDoNothing"] = new Skill(
		   name: "LBDoNothing",
		   description: "LBDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] slowly inches towards [target]!");
			   AudioManager.Instance.PlaySFX("BA_do_nothing_space_out", volume: 0.9f);
			   await Task.CompletedTask;
		   },
		   hidden: true
		);

		Skills["LBBite"] = new Skill(
		   name: "LBBite",
		   description: "LBBite",
		   target: SkillTarget.Enemy,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   await AnimationManager.Instance.WaitForAnimation(157, target);
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] bites [target]!");
			   BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false, neverCrit: true);
		   },
		   hidden: true
		);

		// Boss //
		Skills["BSSAttack"] = new Skill(
		  name: "BSSAttack",
		  description: "BSSAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(139, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] punches [target]!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
		  },
		  hidden: true
		);

		Skills["BSSAttackTwice"] = new Skill(
		  name: "BSSAttackTwice",
		  description: "BSSAttackTwice",
		  target: SkillTarget.XRandomEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(139, targets[0]);
			  BattleLogManager.Instance.QueueMessage(self, targets[0], "[actor] punches [target]!");
			  BattleManager.Instance.Damage(self, targets[0], () => self.CurrentStats.ATK * 2 - targets[0].CurrentStats.DEF, false);
			  await Task.Delay(1000);
			  await AnimationManager.Instance.WaitForAnimation(139, targets[1]);
			  BattleLogManager.Instance.QueueMessage(self, targets[1], "[actor] punches [target]!");
			  BattleManager.Instance.Damage(self, targets[1], () => self.CurrentStats.ATK * 2 - targets[1].CurrentStats.DEF, false);
		  },
		  hidden: true
		);

		Skills["BSSDoNothing"] = new Skill(
		  name: "BSSDoNothing",
		  description: "BSSDoNothing",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (_, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(target, "[actor] cracks his knuckles.");
			  await Task.CompletedTask;
		  },
		  hidden: true
		);

		Skills["BSSAttackAll"] = new Skill(
		  name: "BSSAttackAll",
		  description: "BSSAttackAll",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  foreach (Actor member in targets)
			  {
				  await Task.Delay(1000);
				  BattleManager.Instance.Damage(self, member, () => 100, true, 0f, neverCrit: true);
			  }
		  },
		  hidden: true
		);

		// Ye Old Sprout //
		Skills["YOSRollOver"] = new Skill(
		  name: "YOSRollOver",
		  description: "YOSRollOver",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, "[actor] rolls over!");
			  foreach (Actor member in targets)
			  {
				  AnimationManager.Instance.PlayAnimation(124, member);
				  BattleManager.Instance.Damage(self, member, () => 4, false, 0.5f, neverCrit: true);
			  }
			  await Task.CompletedTask;
		  },
		  hidden: true
		);
		
		// Ye Old Sprout (Boss Rush) //
		Skills["YOSBRRollOver"] = new Skill(
			name: "YOSBRRollOver",
			description: "YOSBRRollOver",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] rolls over!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(124, member);
					BattleManager.Instance.Damage(self, member, () => 2 * self.CurrentStats.ATK - member.CurrentStats.DEF, false, neverCrit: true);
				}
				await Task.CompletedTask;
			},
			hidden: true
		);

		// Mutantheart //
		Skills["MHWink"] = new Skill(
		  name: "MHWink",
		  description: "MHWink",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(298, self);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] winks at [target]!\nIt was kind of cute...");
			  MakeHappy(target);
		  },
		  hidden: true
		);

		Skills["MHCry"] = new Skill(
		  name: "MHCry",
		  description: "MHCry",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(297, self);
			  BattleLogManager.Instance.QueueMessage(self, target, "Tears well up in [actor]'s eyes.");
			  MakeSad(target);
		  },
		  hidden: true
		);

		Skills["MHInsult"] = new Skill(
		  name: "MHInsult",
		  description: "MHInsult",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  AudioManager.Instance.PlaySFX("BA_INK", volume: 0.9f);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] accidentally says\nsomething mean.");
			  MakeAngry(target);
			  await Task.CompletedTask;
		  },
		  hidden: true
		);

		Skills["MHInstakill"] = new Skill(
		  name: "MHInstakill",
		  description: "MHInstakill",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(122, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] slaps [target]!");
			  BattleManager.Instance.Damage(target, target, () => 999, true, 0f, neverCrit: true);
		  },
		  hidden: true
		);

		// Nefarious Chip //
		Skills["NCAttack"] = new Skill(
		  name: "NCAttack",
		  description: "NCAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] charges into [target]!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
		  },
		  hidden: true
		);

		Skills["NCDoNothing"] = new Skill(
		  name: "NCDoNothing",
		  description: "NCDoNothing",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (_, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(target, "[actor] strokes his evil\nmoustache!");
			  await Task.CompletedTask;
		  },
		  hidden: true
		);

		Skills["NCLaugh"] = new Skill(
		  name: "NCLaugh",
		  description: "NCLaugh",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (_, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(162, target);
			  BattleLogManager.Instance.QueueMessage(target, "[actor] laughs like the evil villain he is!");
			  MakeHappy(target);
		  },
		  hidden: true
		);

		Skills["NCCookies"] = new Skill(
		  name: "NCCookies",
		  description: "NCCookies",
		  target: SkillTarget.XRandomEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, "[actor] throws OATMEAL\nCOOKIES!");
			  foreach (Actor member in targets)
			  {
				  BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
			  }
			  await AnimationManager.Instance.WaitForScreenAnimation(196, targets[0] is Enemy);
		  },
		  hidden: true
		);

		Skills["NCCookiesHappy"] = new Skill(
		  name: "NCCookiesHappy",
		  description: "NCCookiesHappy",
		  target: SkillTarget.XRandomEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  Actor target;
			  BattleLogManager.Instance.QueueMessage(self,"[actor] launches OATMEAL\nCOOKIES!");
			  foreach (Actor member in targets)
			  {
				  BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
			  }
			  await AnimationManager.Instance.WaitForScreenAnimation(196, targets[0] is Enemy);
		  },
		  hidden: true
		);

		// Earth //
		Skills["TEAttack"] = new Skill(
		  name: "TEAttack",
		  description: "TEAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(124, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
		  },
		  hidden: true
		);

		Skills["TEDoNothing"] = new Skill(
		  name: "TEDoNothing",
		  description: "TEDoNothing",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (_, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(target, "[actor] is rotating slowly.");
			  await Task.CompletedTask;
		  },
		  hidden: true
		);

		Skills["TECruel"] = new Skill(
		  name: "TECruel",
		  description: "TECruel",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  AnimationManager.Instance.PlayScreenAnimation(169, target is Enemy);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] is cruel to [target]!");
			  MakeSad(target);
			  await Task.CompletedTask;
		  },
		  hidden: true
		);

		Skills["TEProtect"] = new Skill(
		  name: "TEProtect",
		  description: "TEProtect",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, "[actor] uses her ultimate attack!");
			  AnimationManager.Instance.PlayScreenAnimation(170, targets[0] is Enemy);
			  await Task.Delay(1000);
			  foreach (Actor member in targets)
				  BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF);
		  },
		  hidden: true
		);
		
		// Earth Alt //
		Skills["TEACruel"] = new Skill(
			name: "TEACruel",
			description: "TEACruel",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(169, targets[0] is Enemy);
				await Task.Delay(2000);
				BattleLogManager.Instance.QueueMessage(self, targets[0], "[actor] is cruel to everyone!");
				foreach (Actor target in targets)
					MakeSad(target);
			},
			hidden: true
		);
		
		Skills["TEAProtect"] = new Skill(
			name: "TEAProtect",
			description: "TEAProtect",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses her ultimate attack!");
				await AnimationManager.Instance.WaitForScreenAnimation(170, targets[0] is Enemy);
				foreach (Actor member in targets)
					BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
			},
			hidden: true
		);

		// Perfectheart //
		Skills["PHStealHeart"] = new Skill(
		 name: "PHStealHeart",
		 description: "PHStealHeart",
		 target: SkillTarget.Enemy,
		 cost: 0,
		 effect: async (self, target) =>
		 {
			 await AnimationManager.Instance.WaitForAnimation(122, target);
			 BattleLogManager.Instance.QueueMessage(self, target, "[actor] steals [target]'s HEART.");
			 int damage = BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			 if (damage > 0)
			 {
				 self.Heal(damage);
				 BattleManager.Instance.SpawnDamageNumber(damage, self.CenterPoint, DamageType.Heal);
			 }
		 },
		 hidden: true
		);

		Skills["PHStealBreath"] = new Skill(
		 name: "PHStealBreath",
		 description: "PHStealBreath",
		 target: SkillTarget.Enemy,
		 cost: 0,
		 effect: async (self, target) =>
		 {
			 await AnimationManager.Instance.WaitForAnimation(122, target);
			 BattleLogManager.Instance.QueueMessage(self, target, "[actor] steals [target]'s\nbreath away.");
			 target.CurrentJuice = 0;
			 BattleManager.Instance.SpawnDamageNumber(target.CurrentStats.MaxJuice, target.CenterPoint, DamageType.JuiceLoss);
			 self.HealJuice(target.CurrentStats.MaxJuice);
			 BattleManager.Instance.SpawnDamageNumber(target.CurrentStats.MaxJuice, self.CenterPoint, DamageType.JuiceGain);
		 },
		 hidden: true
		);

		Skills["PHWrath"] = new Skill(
			 name: "PHWrath",
			 description: "PHWrath",
			 target: SkillTarget.AllEnemies,
			 cost: 0,
			 effect: async (self, targets) =>
			 {
				 BattleLogManager.Instance.QueueMessage(self, "[actor] unleashes her wrath.");
				 foreach (Actor member in targets)
					 AnimationManager.Instance.PlayAnimation(210, member);
				 await Task.Delay(1500);
				 foreach (Actor member in targets)
				 {
					 BattleManager.Instance.RandomEmotion(member);
					 BattleManager.Instance.Damage(self, member, () => member.CurrentStats.MaxHP * 0.75f, false, 0.15f, neverCrit: true);
				 }
			 },
			 hidden: true
		 );

		Skills["PHExploitEmotion"] = new Skill(
			 name: "PHExploitEmotion",
			 description: "PHExploitEmotion",
			 target: SkillTarget.Enemy,
			 cost: 0,
			 effect: async (self, target) =>
			 {
				 await AnimationManager.Instance.WaitForAnimation(124, target);
				 BattleLogManager.Instance.QueueMessage(self, target, "[actor] exploits [target]'s\nEMOTION!");
				 string old = self.CurrentState;
				 self.ForceState("EmotionExploit", old);
				 BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0f, neverCrit: true);
				 self.ForceState(old);
			 },
			 hidden: true
		);

		Skills["PHSpare"] = new Skill(
			 name: "PHSpare",
			 description: "PHSpare",
			 target: SkillTarget.Enemy,
			 cost: 0,
			 effect: async (self, target) =>
			 {
				 await AnimationManager.Instance.WaitForAnimation(122, target);
				 BattleLogManager.Instance.QueueMessage(self, target, "[actor] decides to let [target] live.");
				 int damage = 1;
				 if (target.CurrentHP > 1)
					 damage = target.CurrentHP - 1;
				 target.Damage(damage);
				 BattleManager.Instance.AddEnergy(1);
				 AudioManager.Instance.PlaySFX("SE_dig", 0.7f, 0.9f);
				 BattleManager.Instance.SpawnDamageNumber(damage, target.CenterPoint);
			 },
			 hidden: true
		);

		Skills["PHAngelicVoice"] = new Skill(
			 name: "PHAngelicVoice",
			 description: "PHAngelicVoice",
			 target: SkillTarget.AllEnemies,
			 cost: 0,
			 effect: async (self, targets) =>
			 {
				 AnimationManager.Instance.PlayAnimation(154, self);
				 await Task.Delay(166);
				 AnimationManager.Instance.PlayAnimation(155, self);
				 MakeSad(self);
				 foreach (Actor member in targets)
				 {
					 BattleManager.Instance.Damage(self, member, () => 175, false, 0f, neverCrit: true);
					 MakeHappy(member);
				 }
			 },
			 hidden: true
		);

		// Roboheart //
		Skills["RHAttack"] = new Skill(
		  name: "RHAttack",
		  description: "RHAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(125, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] fires rocket hands!");
			  BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
		  },
		  hidden: true
		);

		Skills["RHDoNothing"] = new Skill(
		  name: "RHDoNothing",
		  description: "RHDoNothing",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (_, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(target, "[actor] is buffering...");
			  await Task.CompletedTask;
		  },
		  hidden: true
		);

		Skills["RHLaser"] = new Skill(
		  name: "RHLaser",
		  description: "RHLaser",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(160, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] opens her mouth and\nfires a laser!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
		  },
		  hidden: true
		);

		Skills["RHSnack"] = new Skill(
		  name: "RHSnack",
		  description: "RHSnack",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (_, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(target, "[actor] opens her mouth!\nA nutritious SNACK appears!");
			  await AnimationManager.Instance.WaitForAnimation(216, target);
			  target.Heal(200);
			  BattleManager.Instance.SpawnDamageNumber(200, target.CenterPoint, DamageType.Heal);
		  },
		  hidden: true
		);

		Skills["RHExplode"] = new Skill(
		  name: "RHExplode",
		  description: "RHExplode",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, "[actor] sheds a single tear...\nand bids everyone farewell!");
			  await AnimationManager.Instance.WaitForScreenAnimation(216, false);
			  foreach (Actor member in targets)
			  {
				  BattleManager.Instance.Damage(self, member, () => member.CurrentStats.MaxHP * 0.1f, false, 0f, neverCrit: true);
			  }
			  self.Damage(self.CurrentStats.MaxHP);
		  },
		  hidden: true
		);

		// Fear of Heights //
		Skills["FOHAttack"] = new Skill(
		  name: "FOHAttack",
		  description: "FOHAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(140, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] strikes [target].");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
		  },
		  hidden: true
		);

		Skills["FOHDoNothing"] = new Skill(
		  name: "FOHDoNothing",
		  description: "FOHDoNothing",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] taunts [target] as they fall.");
			  await Task.CompletedTask;
		  },
		  hidden: true
		);

		Skills["FOHGrab"] = new Skill(
		  name: "FOHGrab",
		  description: "FOHGrab",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  BattleLogManager.Instance.QueueMessage("Hands appear and grab everyone!");
			  foreach (Actor member in targets)
				  AnimationManager.Instance.PlayAnimation(164, member);
			  await Task.Delay(2000);
			  BattleLogManager.Instance.QueueMessage("Everyone's ATTACK fell!");
			  foreach (Actor member in targets)
			  {
				  AnimationManager.Instance.PlayAnimation(215, member);
				  member.AddTierStatModifier("AttackDown", silent: true);
			  }
			  await Task.Delay(1000);
		  },
		  hidden: true
		);

		Skills["FOHHands"] = new Skill(
		  name: "FOHHands",
		  description: "FOHHands",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, target, "More hands appear and\nsurround [actor].");
			  AnimationManager.Instance.PlayAnimation(11, self);
			  await Task.Delay(2000);
			  AnimationManager.Instance.PlayAnimation(218, self);
			  self.AddStatModifier("DefenseUp");
			  await Task.Delay(1000);
		  },
		  hidden: true
		);

		Skills["FOHShove"] = new Skill(
		  name: "FOHShove",
		  description: "FOHShove",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(209, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] shoves [target].");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK, neverCrit: true);
			  target.SetState("afraid");
		  },
		  hidden: true
		);

		// Space Ex-Husband //
		Skills["SEHAttack"] = new Skill(
		  name: "SEHAttack",
		  description: "SEHAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(124, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] kicks [target]!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
		  },
		  hidden: true
		);

		Skills["SEHLaser"] = new Skill(
		  name: "SEHLaser",
		  description: "SEHLaser",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(160, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] fires his laser!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
		  },
		  hidden: true
		);

		Skills["SEHAngrySong"] = new Skill(
		  name: "SEHAngrySong",
		  description: "SEHAngrySong",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  AnimationManager.Instance.PlayScreenAnimation(153, false);
			  BattleLogManager.Instance.QueueMessage(self, "[actor] wails with all his might!");
			  foreach (Actor member in targets)
			  {
				  BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
				  MakeAngry(member);
			  }
			  await Task.CompletedTask;
		  },
		  hidden: true
		);

		Skills["SEHAngstySong"] = new Skill(
		  name: "SEHAngstySong",
		  description: "SEHAngstySong",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  AnimationManager.Instance.PlayScreenAnimation(154, false);
			  BattleLogManager.Instance.QueueMessage(self, "[actor] sings with all the\ndarkness in his soul!");
			  foreach (Actor member in targets)
			  {
				  BattleManager.Instance.DamageJuice(self, member, () => member.CurrentStats.MaxJuice * 0.25f, false);
				  MakeSad(member);
			  }
			  await Task.CompletedTask;
		  },
		  hidden: true
		);

		Skills["SEHJoyfulSong"] = new Skill(
		  name: "SEHJoyfulSong",
		  description: "SEHJoyfulSong",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  AnimationManager.Instance.PlayScreenAnimation(155, false);
			  BattleLogManager.Instance.QueueMessage(self, "[actor] sings with all the\njoy in his heart!");
			  foreach (Actor member in targets)
			  {
				  BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 + self.CurrentStats.LCK - member.CurrentStats.DEF, false);
				  MakeHappy(member);
			  }
			  await Task.CompletedTask;
		  },
		  hidden: true
		);

		Skills["SEHSpinningKick"] = new Skill(
		  name: "SEHSpinningKick",
		  description: "SEHSpinningKick",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] does a spinning kick!");
			  for (int i = 0; i < 3; i++)
			  {
				  await AnimationManager.Instance.WaitForAnimation(124, target);
				  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
				  await Task.Delay(500);
			  }
		  },
		  hidden: true
		);

		Skills["SEHBulletHell"] = new Skill(
		  name: "SEHBulletHell",
		  description: "SEHBulletHell",
		  target: SkillTarget.XRandomEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, "[actor] fires wildly!");
			  AnimationManager.Instance.PlayScreenAnimation(168, false);
			  foreach (Actor member in targets)
			  {
				  BattleManager.Instance.Damage(self, member, () => 50, false);
				  await Task.Delay(250);
			  }
		  },
		  hidden: true
		);
		
		// Sir Maximus I //
		
		Skills["SMIAttack"] = new Skill(
			name: "SMIAttack",
			description: "SMIAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(3, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] swings his sword!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["SMIDoNothing"] = new Skill(
			name: "SMIDoNothing",
			description: "SMIDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] pulled his back...");
				MakeSad(target);
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["SMIStrikeTwice"] = new Skill(
			name: "SMIStrikeTwice",
			description: "SMIStrikeTwice",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,  "[actor] strikes twice!");
				await AnimationManager.Instance.WaitForAnimation(3, targets[0]);
				BattleManager.Instance.Damage(self, targets[0], () => self.CurrentStats.ATK * 2 - targets[0].CurrentStats.DEF, false);
				await Task.Delay(500);
				await AnimationManager.Instance.WaitForAnimation(3, targets[1]);
				BattleManager.Instance.Damage(self, targets[1],
					() => self.CurrentStats.ATK * 2 - targets[1].CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["SMIUltimateAttack"] = new Skill(
			name: "SMIUltimateAttack",
			description: "SMIUltimateAttack",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his\nultimate attack!");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(186, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 50, true, 0.25f, neverCrit: true);
				}
			},
			hidden: true
		);
		
		Skills["SMIUltimateAttackx1"] = new Skill(
			name: "SMIUltimateAttackx1",
			description: "SMIUltimateAttackx1",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his\nultimate attack!");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(186, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 50, false, 0f, neverCrit: true);
				}
			},
			hidden: true
		);
		
		Skills["SMIUltimateAttackx2"] = new Skill(
			name: "SMIUltimateAttackx2",
			description: "SMIUltimateAttackx2",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his\nultimate attack!");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(186, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 75, false, 0f, neverCrit: true);
				}
			},
			hidden: true
		);
		
		Skills["SMIUltimateAttackx3"] = new Skill(
			name: "SMIUltimateAttackx3",
			description: "SMIUltimateAttackx3",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his\nultimate attack!");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(186, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 100, false, 0f, neverCrit: true);
				}
			},
			hidden: true
		);
		
		// Sir Maximus II //
		
		Skills["SMIIDoNothing"] = new Skill(
			name: "SMIIDoNothing",
			description: "SMIIDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] remembers his father's dying words.");
				MakeSad(target);
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["SMIISpin"] = new Skill(
			name: "SMIISpin",
			description: "SMIISpin",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] spins quickly!");
				await Task.Delay(500);
				float damage = targets.Count switch
				{
					1 => self.CurrentStats.ATK * 4,
					2 => self.CurrentStats.ATK * 3,
					_ => self.CurrentStats.ATK * 2
				};
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(123, member);
					BattleManager.Instance.Damage(self, member, () => damage, false);
				}
			},
			hidden: true
		);
		
		Skills["SMIIUltimateAttack"] = new Skill(
			name: "SMIIUltimateAttack",
			description: "SMIIUltimateAttack",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his father's\nultimate attack!");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(186, targets[0] is Enemy);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 50, true, 0.25f, neverCrit: true);
				}
			},
			hidden: true
		);
		
		// Sir Maximus III //
		Skills["SMIIIDoNothing"] = new Skill(
			name: "SMIIIDoNothing",
			description: "SMIIIDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_,target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] remembers his grandfather's dying words.");
				MakeSad(target);
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["SMIIIFlex"] = new Skill(
			name: "SMIIIFlex",
			description: "SMIIIFlex",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] flexes and feels\nhis best!");
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s HIT RATE rose!");
				await AnimationManager.Instance.WaitForAnimation(218, target);
				target.AddStatModifier("Flex");
				MakeHappy(target);
			},
			hidden: true
		);
		
		Skills["SMIIIUltimateAttack"] = new Skill(
			name: "SMIIIUltimateAttack",
			description: "SMIIIUltimateAttack",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his grandfather's\nultimate attack!");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(186, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 50, true, 0.25f, neverCrit: true);
				}
			},
			hidden: true
		);
		
		// Fear of Drowning //
		
		Skills["FODAttack"] = new Skill(
			name: "FODAttack",
			description: "FODAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(140, target);
				BattleLogManager.Instance.QueueMessage(self, target, "Water pulls [target] in different directions.");
				BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.15f, false, 0f, neverCrit: true);
			},
			hidden: true
		);
		
		Skills["FODDoNothing"] = new Skill(
			name: "FODDoNothing",
			description: "FODDoNothing",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] listens to [target] struggle.");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["FODDragDown"] = new Skill(
			name: "FODDragDown",
			description: "FODDragDown",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(197, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] grabs [target]'s leg and drags them down!");
				BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.5f, false, 0f, neverCrit: true);
			},
			hidden: true
		);
		
		Skills["FODWhirlpool"] = new Skill(
			name: "FODWhirlpool",
			description: "FODWhirlpool",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] creates a whirlpool.");
				BattleLogManager.Instance.QueueMessage("Everyone's SPEED fell...");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(140, member);
					member.AddTierStatModifier("SpeedDown", silent: true);
				}
				await Task.Delay(1000);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(215, member);
					BattleManager.Instance.Damage(self, member, () => member.CurrentStats.MaxHP * 0.1f, false,  neverCrit: true);
				}
			},
			hidden: true
		);
		
		Skills["FODDrowning1"] = new Skill(
			name: "FODDrowning1",
			description: "FODDrowning1",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				DialogueManager.Instance.QueueMessage("You feel like you can't breathe.");
				await DialogueManager.Instance.WaitForDialogue();
				await Task.Delay(500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(140, member);
					member.Damage(50);
					BattleManager.Instance.SpawnDamageNumber(50, member.CenterPoint);
					BattleLogManager.Instance.QueueMessage(self, member, "[target] takes 50 damage!");
				}
			},
			hidden: true
		);
		
		Skills["FODDrowning2"] = new Skill(
			name: "FODDrowning2",
			description: "FODDrowning2",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				DialogueManager.Instance.QueueMessage("You feel like you can't breathe.");
				await DialogueManager.Instance.WaitForDialogue();
				await Task.Delay(500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(140, member);
					member.Damage(100);
					BattleManager.Instance.SpawnDamageNumber(100, member.CenterPoint);
					BattleLogManager.Instance.QueueMessage(self, member, "[target] takes 100 damage!");
				}
			},
			hidden: true
		);
		
		Skills["FODDrowning3"] = new Skill(
			name: "FODDrowning3",
			description: "FODDrowning3",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				DialogueManager.Instance.QueueMessage("You feel like you can't breathe.");
				await DialogueManager.Instance.WaitForDialogue();
				await Task.Delay(500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(140, member);
					member.Damage(150);
					BattleManager.Instance.SpawnDamageNumber(150, member.CenterPoint);
					BattleLogManager.Instance.QueueMessage(self, member, "[target] takes 150 damage!");
				}
			},
			hidden: true
		);
		
		// Pluto (Expanded) //
		Skills["PEAttack"] = new Skill(
			name: "PEAttack",
			description: "PEAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(131, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws the Moon at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["PESubmissionHold"] = new Skill(
			name: "PESubmissionHold",
			description: "PESubmissionHold",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{ 
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] puts [target] into a submission hold!");
				target.AddTierStatModifier("SpeedDown");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF, false, 0.3f);
				await AnimationManager.Instance.WaitForAnimation(164, target);
				AnimationManager.Instance.PlayAnimation(215, target);
			},
			hidden: true
		);
		
		Skills["PEHeadbutt"] = new Skill(
			name: "PEHeadbutt",
			description: "PEHeadbutt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams his head into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
				self.CurrentHP = Math.Max(1, self.CurrentHP - (int)Math.Round(self.CurrentStats.MaxHP * 0.01f));
			},
			hidden: true
		);
		
		Skills["PEDoNothing"] = new Skill(
			name: "PEDoNothing",
			description: "PEDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s muscles intimidated you.");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["PEExpandFurther"] = new Skill(
			name: "PEExpandFurther",
			description: "PEExpandFurther",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] expands even further!");
				await Task.Delay(500);
				await AnimationManager.Instance.WaitForAnimation(218, target);
				target.AddTierStatModifier("AttackUp");
				target.AddTierStatModifier("DefenseUp");
				if (target.GetStatModifierTier("SpeedDown") < 2)
					target.AddTierStatModifier("SpeedDown");
			},
			hidden: true
		);
		
		Skills["PEEarthsFinale"] = new Skill(
			name: "PEEarthsFinale",
			description: "PEEarthsFinale",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] picks up THE EARTH\nand slams it into everyone!");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayScreenAnimation(198, targets[0] is Enemy);
				await Task.Delay(4000);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member,
						() => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, true);
				}
				if (self is PlutoExpandedEarth pluto)
					pluto.KillEarth();
			},
			hidden: true
		);
		
		Skills["PEMeteor"] = new Skill(
			name: "PEMeteor",
			description: "PEMeteor",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] summons a\nmeteor shower!");
				int skipped = -1;
				if (targets.Count > 3)
				{
					skipped = GameManager.Instance.Random.RandiRange(0, targets.Count - 1);
				}
				AnimationManager.Instance.PlayScreenAnimation(288, targets[0] is Enemy);
				await Task.Delay(2000);
				for (int i = 0; i < targets.Count; i++)
				{
					// meteor always skips one target
					if (i == skipped)
						continue;
					BattleManager.Instance.Damage(self, targets[i], () => 100, false, 0.1f, neverCrit: true);
					BattleManager.Instance.Damage(self, targets[i], () => 100, false, 0.1f, neverCrit: true);
				}
			},
			hidden: true
		);
		
		
		// King Crawler //
		Skills["KCAttack"] = new Skill(
			name: "KCAttack",
			description: "KCAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["KCDoNothing"] = new Skill(
			name: "KCDoNothing",
			description: "KCDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_roar", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] lets out an ear-piercing screech!");
				MakeAngry(target);
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["KCCrunch"] = new Skill(
			name: "KCCrunch",
			description: "KCCrunch",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(157, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] chomps [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["KCRam"] = new Skill(
			name: "KCRam",
			description: "KCRam",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] charges forward!");
				await AnimationManager.Instance.WaitForScreenAnimation(179, targets[0] is Enemy);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member,
						() => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
				}
			},
			hidden: true
		);
		
		Skills["KCEat"] = new Skill(
			name: "KCEat",
			description: "KCEat",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(157, target);
				BattleManager.Instance.SpawnDamageNumber(target.CurrentHP, target.CenterPoint);
				target.Damage(target.CurrentHP);
			},
			hidden: true
		);
		
		Skills["KCRecover"] = new Skill(
			name: "KCRecover",
			description: "KCRecover",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(216, target);
				int heal = Math.Min(170, target.CurrentStats.MaxHP - target.CurrentHP);
				target.Heal(heal);
				BattleManager.Instance.SpawnDamageNumber(heal, target.CenterPoint, DamageType.Heal);
				MakeHappy(target);
			},
			hidden: true
		);

		// Kite Kid //
		Skills["KKAttack"] = new Skill(
			name: "KKAttack",
			description: "KKAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws JACKS at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["KKBrag"] = new Skill(
			name: "KKBrag",
			description: "KKBrag",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(162, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] brags about KID'S KITE!");
				MakeHappy(target);
			},
			hidden: true
		);
		
		// Kid's Kite //
		Skills["KSKAttack"] = new Skill(
			name: "KSKAttack",
			description: "KSKAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] dives at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["KSKDoNothing"] = new Skill(
			name: "KSKDoNothing",
			description: "KSKDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] puffs its chest proudly!");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["KSKFly"] = new Skill(
			name: "KSKFly",
			description: "KSKFly",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] swoops down!");
				await Task.Delay(1000);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(123, member);
					BattleManager.Instance.Damage(self, member,
						() => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
				}
			},
			hidden: true
		);

		// Pluto //
		Skills["PLDoNothing"] = new Skill(
			name: "PLDoNothing",
			description: "PLDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] strikes a pose!");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["PLHeadbutt"] = new Skill(
			name: "PLHeadbutt",
			description: "PLHeadbutt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] bolts forward and slams [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["PLBrag"] = new Skill(
			name: "PLBrag",
			description: "PLBrag",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(162, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] brags about his muscles!");
				MakeHappy(target);
			},
			hidden: true
		);
		
		Skills["PLExpand"] = new Skill(
			name: "PLExpand",
			description: "PLExpand",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] expands.");
				await AnimationManager.Instance.WaitForAnimation(292, target);
				target.AddTierStatModifier("AttackUp", 2);
				target.AddTierStatModifier("DefenseUp", 2);
				target.AddTierStatModifier("SpeedDown", 2);
				AnimationManager.Instance.PlayAnimation(218, target);
				await Task.Delay(1000);
			},
			hidden: true
		);
		
		// Right Arm //
		Skills["RAAttack"] = new Skill(
			name: "RAAttack",
			description: "RAAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] chops [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		
		Skills["RAFlex"] = new Skill(
			name: "RAFlex",
			description: "RAFlex",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] flexes and feels his best!");
				BattleLogManager.Instance.QueueMessage(self, target, "[actor]'s HIT RATE rose!");
				await AnimationManager.Instance.WaitForAnimation(218, self);
				self.AddStatModifier("Flex");
			},
			hidden: true
		);
		
		Skills["RAGrab"] = new Skill(
			name: "RAGrab",
			description: "RAGrab",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] grabs [target]!");
				await AnimationManager.Instance.WaitForAnimation(164, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0.4f, neverCrit: true);
				target.AddTierStatModifier("SpeedDown");
			},
			hidden: true
		);
		
		// Left Arm //
		Skills["LAAttack"] = new Skill(
			name: "LAAttack",
			description: "LAAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] punches [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["LAPoke"] = new Skill(
			name: "LAPoke",
			description: "LAPoke",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] pokes [target]!");
				await AnimationManager.Instance.WaitForAnimation(163, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0.4f, neverCrit: true);
				MakeAngry(target);
			},
			hidden: true
		);
		
		// Abbi //
		Skills["AbbiAttack"] = new Skill(
			name: "AbbiAttack",
			description: "AbbiAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(144, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["AbbiAttackOrder"] = new Skill(
			name: "AbbiAttackOrder",
			description: "AbbiAttackOrder",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				AudioManager.Instance.PlaySFX("SE_bs_scare6");
				BattleLogManager.Instance.QueueMessage(self, "[actor] stretches her tentacles.");
				foreach (Actor enemy in targets)
				{
					MakeAngry(enemy);
					enemy.AddTierStatModifier("AttackUp", silent: true);
				}
				BattleLogManager.Instance.QueueMessage("Everyone's ATTACK rose!");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["AbbiSummon"] = new Skill(
			name: "AbbiSummon",
			description: "AbbiSummon",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				// this skill just does the effects, the summoning logic is in Abbi's AI
				BattleLogManager.Instance.QueueMessage(target, "[actor] focuses her HEART.");
				AudioManager.Instance.PlaySFX("sys_blackletter1", 1.5f, 0.9f);
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		// Tentacle //
		Skills["TENAttack"] = new Skill(
			name: "TENAttack",
			description: "TENAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["TENWeaken"] = new Skill(
			name: "TENWeaken",
			description: "TENWeaken",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(129, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] weakens [target]!");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] let their guard down!");
				target.AddStatModifier("Tickle");
			},
			hidden: true
		);
		
		Skills["TENGrab"] = new Skill(
			name: "TENGrab",
			description: "TENGrab",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(197, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] wraps around [target]!");
				BattleManager.Instance.Damage(self, target, () => 100, false, 0.1f, neverCrit: true);
				target.SetState("afraid");
			},
			hidden: true
		);
		
		
		Skills["TENGoop"] = new Skill(
			name: "TENGoop",
			description: "TENGoop",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(291, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[target] is drenched in dark liquid!");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] feels weaker...");
				AnimationManager.Instance.PlayAnimation(215, target);
				target.AddTierStatModifier("AttackDown");
				target.AddTierStatModifier("DefenseDown");
				target.AddTierStatModifier("SpeedDown");
			},
			hidden: true
		);
		
		// Recycultist //
		Skills["RCultFlingTrash"] = new Skill(
			name: "RCultFlingTrash",
			description: "RCultFlingTrash",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(201, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws trash!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0f);
			},
			hidden: true
		);
		
		Skills["RCultGatherTrash"] = new Skill(
			name: "RCultGatherTrash",
			description: "RCultGatherTrash",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("SE_shuffle", 1f, 0.8f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] gathers trash!");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		// Recyclepath //
		Skills["RPathAttack"] = new Skill(
			name: "RPathAttack",
			description: "RPathAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(130, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] hits [target] with a bag!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true
		);
		
		Skills["RPathGatherTrash"] = new Skill(
			name: "RPathGatherTrash",
			description: "RPathGatherTrash",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] gathers TRASH!");
				target.AddTierStatModifier("Stockpile");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["RPathFlingTrash"] = new Skill(
			name: "RPathFlingTrash",
			description: "RPathFlingTrash",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(201, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws TRASH!");
				if (!self.StatModifiers.TryGetValue("Stockpile", out StatModifier stockpile))
				{
					GD.PrintErr("Tried to use RPathFlingTrash with no stockpile stacks!");
					return;
				}
				BattleManager.Instance.Damage(self, target, () =>
				{
					return ((TierStatModifier)stockpile).CurrentTier switch
					{
						1 => 3 * self.CurrentStats.ATK - target.CurrentStats.DEF,
						2 => 4 * self.CurrentStats.ATK - target.CurrentStats.DEF,
						_ => 5 * self.CurrentStats.ATK - target.CurrentStats.DEF,
					};
				}, false, 0f);
				self.RemoveStatModifier("Stockpile");
			},
			hidden: true
		);
		
		Skills["RPathSummon"] = new Skill(
			name: "RPathSummon",
			description: "RPathSummon",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(145, self);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] calls a follower!");
				BattleLogManager.Instance.QueueMessage("A RECYCULTIST appeared!");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		// Aubrey Boss //
		Skills["ABossLookAtKel"] = new Skill(
			name: "ABossLookAtKel",
			description: "ABossLookAtKel",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayAnimation(218, self);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks at [target].");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] eggs [actor] on!");
				MakeAngry(self);
				self.AddTierStatModifier("AttackUp");
			},
			hidden: true
		);
		
		Skills["ABossLookAtHero"] = new Skill(
			name: "ABossLookAtHero",
			description: "ABossLookAtHero",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Task.Delay(1000);
				AnimationManager.Instance.PlayAnimation(212, self);
				await Task.Delay(1000);
				AnimationManager.Instance.PlayAnimation(218, self);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks at [target].");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] tells [actor] to focus!");
				self.Heal(500);
				MakeHappy(self);
				self.AddTierStatModifier("DefenseUp");
			},
			hidden: true
		);
		
		Skills["ABossBeatdown"] = new Skill(
			name: "ABossBeatdown",
			description: "ABossBeatdown",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] furiously attacks!");
				await AnimationManager.Instance.WaitForAnimation(17, target);
				for (int i = 0; i < 2; i++)
				{
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 1.35f - target.CurrentStats.DEF, false);
					await Task.Delay(1000);
				}
			},
			hidden: true
		);
		
		Skills["ABossTwirl"] = new Skill(
			name: "ABossTwirl",
			description: "ABossTwirl",
			target: SkillTarget.Enemy,
			cost: 10,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(338, target);
				await Task.Delay(500);
				AnimationManager.Instance.PlayAnimation(28, target);
				await Task.Delay(500);
				int damage = BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK * 2f + self.CurrentStats.LCK) - target.CurrentStats.DEF, false);
				if (damage > -1)
				{
					MakeHappy(self);
				}

			}
		);
		
		// Kel Boss //
		Skills["KBossPassToAubrey"] = new Skill(
			name: "KBossPassToAubrey",
			description: "KBossPassToAubrey",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Task.Delay(1000);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] passes to [target].");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] knocks the ball out of the park!");
				PartyMember member = BattleManager.Instance.GetRandomAlivePartyMember();
				await AnimationManager.Instance.WaitForAnimation(67, member);
				BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF);
			},
			hidden: true
		);
		
		Skills["KBossPassToHero"] = new Skill(
			name: "KBossPassToHero",
			description: "KBossPassToHero",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Task.Delay(1000);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] passes to [target].");
				BattleLogManager.Instance.QueueMessage(self, "[actor] dunks on the foes!");
				PartyMember member = BattleManager.Instance.GetRandomAlivePartyMember();
				await AnimationManager.Instance.WaitForAnimation(339, member);
				BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF);
			},
			hidden: true
		);
		
		Skills["KBossFlex"] = new Skill(
			name: "KBossFlex",
			description: "KBossFlex",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(218, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] flexes and feels his best!");
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s HIT RATE rose!");
				target.AddStatModifier("Flex");
			},
			hidden: true
		);
		
		Skills["KBossRainCloud"] = new Skill(
			name: "KBossRainCloud",
			description: "KBossRainCloud",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses RAIN CLOUD!");
				foreach (Actor enemy in targets)
					AnimationManager.Instance.PlayAnimation(278, enemy);
				await Task.Delay(1000);		
				foreach (Actor enemy in targets)
					MakeSad(enemy);
			},
			hidden: true
		);
		
		// Hero Boss //
		Skills["HBossCallAubrey"] = new Skill(
			name: "HBossCallAubrey",
			description: "HBossCallAubrey",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Task.Delay(1000);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] calls [target].");
				await AnimationManager.Instance.WaitForAnimation(212, target);
				target.Heal(500);
				PartyMember member = BattleManager.Instance.GetRandomAlivePartyMember();
				BattleManager.Instance.ForceCommand(target, member, Skills["AAttack"]);
			},
			hidden: true
		);
		
		Skills["HBossCallKel"] = new Skill(
			name: "HBossCallKel",
			description: "HBossCallKel",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Task.Delay(1000);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] calls [target].");
				await AnimationManager.Instance.WaitForAnimation(212, target);
				target.Heal(500);
				PartyMember member = BattleManager.Instance.GetRandomAlivePartyMember();
				BattleManager.Instance.ForceCommand(target, member, Skills["KAttack"]);
			},
			hidden: true
		);
		
		Skills["HBossSmile"] = new Skill(
			name: "HBossSmile",
			description: "HBossSmile",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] smiles at [target]!");
				await AnimationManager.Instance.WaitForAnimation(334, self);
				await Task.Delay(333);
				await AnimationManager.Instance.WaitForAnimation(219, target);
				target.AddTierStatModifier("AttackDown");
			},
			hidden: true,
			priority: SkillPriority.First
		);
		
		Skills["HBossDazzle"] = new Skill(
			name: "HBossDazzle",
			description: "HBossDazzle",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				await AnimationManager.Instance.WaitForAnimation(335, self);
				await Task.Delay(500);
				foreach (Actor member in targets)
					AnimationManager.Instance.PlayAnimation(276, member);
				await Task.Delay(500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(219, member);
					BattleLogManager.Instance.QueueMessage(self, member, "[actor] smiles at [target]!");
					member.AddTierStatModifier("AttackDown");
					MakeHappy(member);
				}
			},
			hidden: true,
			priority: SkillPriority.First
		);
		
		Skills["HBossCook"] = new Skill(
			name: "HBossCook",
			description: "HBossCook",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(85, target);
				AnimationManager.Instance.PlayAnimation(212, target);
				await Task.Delay(1000);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] makes a cookie just for [target]!");
				BattleManager.Instance.Heal(self, target, () => 2000, 0f);
			},
			hidden: true
		);

		Skills["HBossCoffee"] = new Skill(
			name: "HBossCoffee",
			description: "HBossCoffee",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses COFFEE!");
				target.AddTierStatModifier("SpeedUp", 3);
				BattleManager.Instance.HealJuice(self, target, () => target.CurrentStats.MaxJuice * 0.1f);
				await AnimationManager.Instance.WaitForAnimation(218, target);
			},
			hidden: true
		);
		
		// Bossman Hero //
		Skills["BMHAttack"] = new Skill(
			name: "BMHAttack",
			description: "BMHAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(83, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["BMHThrowMoney"] = new Skill(
			name: "BMHThrowMoney",
			description: "BMHThrowMoney",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(343, target);
				BattleLogManager.Instance.QueueMessage(self,"[actor] throws a bag of CLAMS.");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["BMHFlingMoney"] = new Skill(
			name: "BMHFlingMoney",
			description: "BMHFlingMoney",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] throws CLAMS at everyone!");
				await Task.Delay(250);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(343, member);
					BattleManager.Instance.Damage(self, member,
						() => member.CurrentStats.MaxHP * 0.2f, false);
				}
			},
			hidden: true
		);
		
		Skills["BMHHealFriends"] = new Skill(
			name: "BMHHealFriends",
			description: "BMHHealFriends",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] heals you.");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(212, member);
					member.Heal(member.CurrentStats.MaxHP);
				}
			},
			hidden: true
		);
		
		Skills["BMHHealFoes"] = new Skill(
			name: "BMHHealFoes",
			description: "BMHHealFoes",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] heals the foes.");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(216, member);
					member.Heal(member.CurrentStats.MaxHP);
					member.HealJuice(member.CurrentStats.MaxJuice);
				}
			},
			hidden: true
		);
		
		Skills["BMHBuffFriends"] = new Skill(
			name: "BMHBuffFriends",
			description: "BMHBuffFriends",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes you stronger!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(214, member);
					member.AddTierStatModifier("AttackUp", silent: true);
					member.AddTierStatModifier("DefenseUp", silent: true);
					member.AddTierStatModifier("SpeedUp", silent: true);
				}
			},
			hidden: true
		);
		
		Skills["BMHBuffFoes"] = new Skill(
			name: "BMHBuffFoes",
			description: "BMHBuffFoes",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes the foes stronger!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(218, member);
					member.RemoveStatModifier("AttackDown");
					member.RemoveStatModifier("DefenseDown");
					member.RemoveStatModifier("SpeedDown");
					member.AddTierStatModifier("AttackUp",3, silent: true);
					member.AddTierStatModifier("DefenseUp", 3, silent: true);
					member.AddTierStatModifier("SpeedUp", 3, silent: true);
				}
			},
			hidden: true
		);
		
		Skills["BMHDebuffFriends"] = new Skill(
			name: "BMHDebuffFriends",
			description: "BMHDebuffFriends",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes you weaker!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(331, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(215, member);
					member.AddTierStatModifier("AttackDown",3, silent: true);
					member.AddTierStatModifier("DefenseDown", 3, silent: true);
					member.AddTierStatModifier("SpeedDown", 3, silent: true);
				}
			},
			hidden: true
		);
		
		Skills["BMHDebuffFoes"] = new Skill(
			name: "BMHDebuffFoes",
			description: "BMHDebuffFoes",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes the foes weaker!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(331, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(219, member);
					member.AddTierStatModifier("AttackDown",3, silent: true);
					member.AddTierStatModifier("DefenseDown", 3, silent: true);
					member.AddTierStatModifier("SpeedDown", 3, silent: true);
				}
			},
			hidden: true
		);
		
		Skills["BMHHappyFriends"] = new Skill(
			name: "BMHHappyFriends",
			description: "BMHHappyFriends",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes you HAPPY!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					MakeHappy(member);
				}
			},
			hidden: true
		);
		
		Skills["BMHHappyFoes"] = new Skill(
			name: "BMHHappyFoes",
			description: "BMHHappyFoes",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes the foes HAPPY!");
				foreach (Actor member in targets)
				{
					if (member == self) continue;
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					if (member == self) continue;
					MakeHappy(member);
				}
			},
			hidden: true
		);
		
		Skills["BMHSadFriends"] = new Skill(
			name: "BMHSadFriends",
			description: "BMHSadFriends",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes you SAD!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(331, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					MakeSad(member);
				}
			},
			hidden: true
		);
		
		Skills["BMHSadFoes"] = new Skill(
			name: "BMHSadFoes",
			description: "BMHSadFoes",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes the foes SAD!");
				foreach (Actor member in targets)
				{
					if (member == self) continue;
					AnimationManager.Instance.PlayAnimation(331, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					if (member == self) continue;
					MakeSad(member);
				}
			},
			hidden: true
		);
		
		Skills["BMHAngryFriends"] = new Skill(
			name: "BMHAngryFriends",
			description: "BMHAngryFriends",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes you ANGRY!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					MakeAngry(member);
				}
			},
			hidden: true
		);
		
		Skills["BMHAngryFoes"] = new Skill(
			name: "BMHAngryFoes",
			description: "BMHAngryFoes",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes the foes ANGRY!");
				foreach (Actor member in targets)
				{
					if (member == self) continue;
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					if (member == self) continue;
					MakeAngry(member);
				}
			},
			hidden: true
		);
		
		Skills["BMHCritFriends"] = new Skill(
			name: "BMHCritFriends",
			description: "BMHCritFriends",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] helps you focus!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(214, member);
				}

				foreach (Enemy enemy in BattleManager.Instance.GetAllEnemies())
				{
					enemy.AddStatModifier("Tickle", 2);
				}
			},
			hidden: true
		);
		
		Skills["BMHCritFoes"] = new Skill(
			name: "BMHCritFoes",
			description: "BMHCritFoes",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes the foes focus!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(218, member);
				}

				foreach (PartyMemberComponent member in BattleManager.Instance.GetAllPartyMembers())
				{
					member.Actor.AddStatModifier("Tickle", 2);
				}
			},
			hidden: true
		);
		
		Skills["BMHDamageFriends"] = new Skill(
			name: "BMHDamageFriends",
			description: "BMHDamageFriends",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] damages you!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(331, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => member.CurrentStats.MaxHP * 0.5f,  neverCrit: true);
					member.DamageJuice((int)Math.Round(member.CurrentStats.MaxJuice * 0.5f));
				}
			},
			hidden: true
		);
		
		Skills["BMHDamageFoes"] = new Skill(
			name: "BMHDamageFoes",
			description: "BMHDamageFoes",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] damages the foes!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(331, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 1000, variance: 0, neverCrit: true);
					member.DamageJuice((int)Math.Round(member.CurrentStats.MaxJuice * 0.1f));
				}
			},
			hidden: true
		);
		
		Skills["BMHGivePizzaFriends"] = new Skill(
			name: "BMHGivePizzaFriends",
			description: "BMHGivePizzaFriends",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (_, targets) =>
			{
				DialogueManager.Instance.QueueMessage($"{targets[0].Name.ToUpper()} got 10 WHOLE PIZZAS.");
				await DialogueManager.Instance.WaitForDialogue();
				BattleManager.Instance.AddItem("Whole Pizza", 10);
			},
			hidden: true
		);
		
		Skills["GGPizzaParty"] = new Skill(
			name: "GGPizzaParty",
			description: "GGPizzaParty",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] throws a PIZZA PARTY!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Task.Delay(1500);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Heal(self, member, () => 2500, variance: 0);
					AnimationManager.Instance.PlayAnimation(216, member);
				}
			},
			hidden: true
		);
		
		// Snaley //
		Skills["SNAttack"] = new Skill(
			name: "SNAttack",
			description: "SNAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0.1f);
			},
			hidden: true
		);

		Skills["RabbitAttack"] = new Skill(
			name: "RabbitAttack",
			description: "RabbitAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] nibbles at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0.2f);
			},
			hidden: true
		);
		
		Skills["SNAttackFollowup"] = new Skill(
			name: "SNAttackFollowup",
			description: "SNAttackFollowup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0.1f);
				await Task.Delay(1000);
			},
			hidden: true
		);
		
		Skills["SNFollowup"] = new Skill(
			name: "SNFollowup",
			description: "SNFollowup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Task.Delay(500);
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] attacks again!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF, false, 0.1f);
			},
			hidden: true
		);
		
		Skills["SNDoNothing"] = new Skill(
			name: "SNDoNothing",
			description: "SNDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] falls over.");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["SNBeatdown"] = new Skill(
			name: "SNBeatdown",
			description: "SNBeatdown",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] furiously attacks!");
				await AnimationManager.Instance.WaitForAnimation(326, target);
				for (int i = 0; i < 3; i++)
				{
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
					await Task.Delay(500);
				}
			},
			hidden: true
		);
		
		Skills["SNMegaphone"] = new Skill(
			name: "SNMegaphone",
			description: "SNMegaphone",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses an AIRHORN!");
				AudioManager.Instance.PlaySFX("SE_airhorn");
				await Task.Delay(500);
				foreach (Actor target in targets)
				{
					MakeAngry(target);
				}
			},
			hidden: true
		);
		
		Skills["SNReleaseEnergy"] = new Skill(
			name: "SNReleaseEnergy",
			description: "SNReleaseEnergy",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] releases energy!");
				AudioManager.Instance.PlaySFX("Skill2");
				if (self is Enemy enemy)
					enemy.SetOpacity(0f, 0.5f);
				await Task.Delay(500);
				await AnimationManager.Instance.WaitForSnaley();
				await Task.Delay(1000);
				foreach (Actor target in targets)
				{
					AnimationManager.Instance.PlayAnimation(327, target);
					BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.5f, false,
						neverCrit: true, ignoreEmotion: true);
				}
				if (self is Enemy stillAnEnemy)
					stillAnEnemy.SetOpacity(1f, 0.5f);
				await Task.Delay(500);
			},
			hidden: true
		);
		
		// Shady Mole //
		Skills["SMAttack"] = new Skill(
			name: "SMAttack",
			description: "SMAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(3, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] cuts [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["SMB.E.D."] = new Skill(
			name: "SMB.E.D.",
			description: "SMB.E.D.",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(182, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] pulls out the B.E.D.!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["SMDynamite"] = new Skill(
			name: "SMDynamite",
			description: "SMDynamite",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] lobs DYNAMITE!");
				await AnimationManager.Instance.WaitForScreenAnimation(270, targets[0] is Enemy);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => 25, false, 0.25f, neverCrit: true);
			},
			hidden: true
		);
		
		// Humphrey Swarm //
		Skills["HUSAttack"] = new Skill(
			name: "HUSAttack",
			description: "HUSAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.4f, false, neverCrit: true);
				await AnimationManager.Instance.WaitForAnimation(295, target);
			},
			hidden: true
		);
		
		Skills["HUSAttack2"] = new Skill(
			name: "HUSAttack2",
			description: "HUSAttack2",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] appears and attacks!");
				foreach (Actor target in targets)
				{
					BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.35f, false,
						neverCrit: true);
					await AnimationManager.Instance.WaitForAnimation(295, target);
				}
			},
			hidden: true
		);
		
		Skills["HUSAttack3"] = new Skill(
			name: "HUSAttack3",
			description: "HUSAttack3",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] appears everywhere!");
				BattleLogManager.Instance.QueueMessage(self, "[actor] appears and attacks!");
				foreach (Actor target in targets)
				{
					BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.3f, false,
						neverCrit: true);
					await AnimationManager.Instance.WaitForAnimation(295, target);
				}
			},
			hidden: true
		);
		
		// Humphrey Grande //
		Skills["HUGAttack"] = new Skill(
			name: "HUGAttack",
			description: "HUGAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams into [target]!");
				await AnimationManager.Instance.WaitForAnimation(295, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		// Humphrey Face //
		Skills["HUFChomp"] = new Skill(
			name: "HUFChomp",
			description: "HUFChomp",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] sinks his teeth into [target]!");
				await AnimationManager.Instance.WaitForAnimation(157, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true
		);
			
		Skills["HUFDoNothing"] = new Skill(
			name: "HUFDoNothing",
			description: "HUFDoNothing",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] stares at [target]!");
				BattleLogManager.Instance.QueueMessage(self, "[actor]'s mouth waters incessantly.");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["HUFSwallow"] = new Skill(
			name: "HUFSwallow",
			description: "HUFSwallow",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] swallows everyone!");
				await Task.Delay(1000);
				AnimationManager.Instance.TintScreen(Colors.Black, 1f);
				if (self is Enemy enemy)
					enemy.SetOpacity(0f);
				await AnimationManager.Instance.WaitForHumphreyFaceSwallow();
				await Task.Delay(500);
				int totalDamage = targets.Sum(target => BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.25f, true, 0.5f, neverCrit: true));
				self.Heal((int)Math.Floor(totalDamage * 0.25d));
				await BattleLogManager.Instance.WaitForBattleLog();
				await Task.Delay(750);
				if (self is Enemy stillAnEnemy)
					stillAnEnemy.SetOpacity(1f);
				await AnimationManager.Instance.WaitForTintScreen(ColorsExtension.TransparentBlack, 1f);
			},
			hidden: true
		);
		
		// Angel //
		Skills["ANAttack"] = new Skill(
			name: "ANAttack",
			description: "ANAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] swiftly strikes [target]!");
				await AnimationManager.Instance.WaitForAnimation(136, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true
		);
		
		Skills["ANDoNothing"] = new Skill(
			name: "ANDoNothing",
			description: "ANDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] does a flip and strikes a pose!");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["ANQuickAttack"] = new Skill(
			name: "ANQuickAttack",
			description: "ANQuickAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] teleports behind [target]!");
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true,
			priority: SkillPriority.First
		);
		
		Skills["ANTease"] = new Skill(
			name: "ANTease",
			description: "ANTease",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] says mean things about [target]!");
				target.SetState("sad");
				await Task.CompletedTask;
			},
			hidden: true
		);
		// Charlene //
		Skills["CHAttack"] = new Skill(
			name: "CHAttack",
			description: "CHAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "CHARLIE punches [target]!");
				await AnimationManager.Instance.WaitForAnimation(136, target);
				BattleManager.Instance.Damage(self, target, () => 1, false, 0f, neverCrit: true);
			},
			hidden: true
		);
		
		Skills["CHDoNothing"] = new Skill(
			name: "CHDoNothing",
			description: "CHDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "CHARLIE is standing there.");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		// The Maverick //
		Skills["TMAttack"] = new Skill(
			name: "TMAttack",
			description: "TMAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] hits [target]!");
				await AnimationManager.Instance.WaitForAnimation(137, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true
		);
		
		Skills["TMDoNothing"] = new Skill(
			name: "TMDoNothing",
			description: "TMDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] starts bragging to his adoring fans!");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["TMSmile"] = new Skill(
			name: "TMSmile",
			description: "TMSmile",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] smiles seductively!");
				await AnimationManager.Instance.WaitForAnimation(148, self);
				await AnimationManager.Instance.WaitForAnimation(215, target);
				target.AddStatModifier("AttackDown");
			},
			hidden: true
		);
		
		Skills["TMTaunt"] = new Skill(
			name: "TMTaunt",
			description: "TMTaunt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] starts making fun of [target]!");
				target.SetState("angry");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		// Kim //
		Skills["KMAttack"] = new Skill(
			name: "KMAttack",
			description: "KMAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] punches [target]!");
				await AnimationManager.Instance.WaitForAnimation(138, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true
		);
		
		Skills["KMDoNothing"] = new Skill(
			name: "KMDoNothing",
			description: "KMDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s phone rang... it was a wrong number.");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["KMSmash"] = new Skill(
			name: "KMSmash",
			description: "KMSmash",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] grabs [target]'s shirt and punches them in the face!");
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f, false, neverCrit: true);
			},
			hidden: true
		);
		
		Skills["KMTaunt"] = new Skill(
			name: "KMTaunt",
			description: "KMTaunt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] starts making fun of [target]!");
				target.SetState("sad");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		// Vance //
		Skills["VAAttack"] = new Skill(
			name: "VAAttack",
			description: "VAAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] punches [target]!");
				await AnimationManager.Instance.WaitForAnimation(139, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true
		);
		
		Skills["VADoNothing"] = new Skill(
			name: "VADoNothing",
			description: "VADoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] scratches his belly.");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["VACandy"] = new Skill(
			name: "VACandy",
			description: "VACandy",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] throws old candy!");
				foreach (Actor target in targets)
				{
					AnimationManager.Instance.PlayAnimation(123, target);
					BattleManager.Instance.Damage(self, target, () => 7, false, neverCrit: true);	
				}

				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["VATease"] = new Skill(
			name: "VATease",
			description: "VATease",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] starts making fun of [target]!");
				target.SetState("sad");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		// The Hooligans //
		Skills["HOAngelAttack"] = new Skill(
			name: "HOAngelAttack",
			description: "HOAngelAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "ANGEL swiftly strikes [target]!");
				await AnimationManager.Instance.WaitForAnimation(136, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true
		);
		
		Skills["HOMaverickCharm"] = new Skill(
			name: "HOMaverickCharm",
			description: "HOMaverickCharm",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "THE MAVERICK winks at [target]!");
				await AnimationManager.Instance.WaitForAnimation(148, self);
				await AnimationManager.Instance.WaitForAnimation(215, target);
				target.AddStatModifier("AttackDown");
			},
			hidden: true
		);
		
		Skills["HOKimHeadbutt"] = new Skill(
			name: "HOKimHeadbutt",
			description: "HOKimHeadbutt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "KIM slams her head into [target]!");
				await AnimationManager.Instance.WaitForAnimation(138, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, false, neverCrit: true);
			},
			hidden: true
		);
		
		Skills["HOVanceCandy"] = new Skill(
			name: "HOVanceCandy",
			description: "HOVanceCandy",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage("THE HOOLIGANS threw old candy!");
				foreach (Actor target in targets)
				{
					AnimationManager.Instance.PlayAnimation(123, target);
					BattleManager.Instance.Damage(self, target, () => 20, false, neverCrit: true);	
				}

				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["HOGroupAttack"] = new Skill(
			name: "HOGroupAttack",
			description: "HOGroupAttack",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage("THE HOOLIGANS attack together!");
				await AnimationManager.Instance.WaitForScreenAnimation(202, targets[0] is Enemy);
				foreach (Actor target in targets)
				{
					BattleManager.Instance.Damage(self, target, () => 30, false, neverCrit: true);
					await Task.Delay(500);
				}
			},
			hidden: true
		);
		
		// Jackson //
		Skills["JKWalkSlowly"] = new Skill(
			name: "JKWalkSlowly",
			description: "JKWalkSlowly",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] walks toward you slowly.");
				await Task.CompletedTask;
			},
			hidden: true
		);
		
		Skills["JKAutoKill"] = new Skill(
			name: "JKAutoKill",
			description: "JKAutoKill",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] catches you!");
				AudioManager.Instance.PlaySFX("SE_bs_ghost_moving", volume: 0.9f);
				AnimationManager.Instance.InitShake(new Shake(255, 100, 5));
				await Task.Delay(100);
				AnimationManager.Instance.InitShake(new Shake(255, 100, 5));
				await Task.Delay(400);
				AnimationManager.Instance.InitShake(new Shake(255, 100, 5));
				await Task.Delay(500);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => 999, neverMiss: false, 0f, neverCrit: true);
			},
			hidden: true
		);
		#endregion

		#region MODIFIERS
		Modifiers.Add("Neutral", () => new StatModifier());
		Modifiers.Add("Happy", () => new StatModifier(new StatBonus(StatType.LCK, 2f), new StatBonus(StatType.SPD, 1.25f), new StatBonus(StatType.HIT, -10)));
		Modifiers.Add("Ecstatic", () => new StatModifier(new StatBonus(StatType.LCK, 3f), new StatBonus(StatType.SPD, 1.5f), new StatBonus(StatType.HIT, -20)));
		Modifiers.Add("Manic", () => new StatModifier(new StatBonus(StatType.LCK, 4f), new StatBonus(StatType.SPD, 2f), new StatBonus(StatType.HIT, -30)));
		Modifiers.Add("Angry", () => new StatModifier(new StatBonus(StatType.ATK, 1.3f), new StatBonus(StatType.DEF, 0.5f)));
		Modifiers.Add("Enraged", () => new StatModifier(new StatBonus(StatType.ATK, 1.5f), new StatBonus(StatType.DEF, 0.3f)));
		Modifiers.Add("Furious", () => new StatModifier(new StatBonus(StatType.ATK, 2f), new StatBonus(StatType.DEF, 0.15f)));
		Modifiers.Add("Sad", () => new StatModifier(new StatBonus(StatType.DEF, 1.25f), new StatBonus(StatType.SPD, 0.8f)));
		Modifiers.Add("Depressed", () => new StatModifier(new StatBonus(StatType.DEF, 1.35f), new StatBonus(StatType.SPD, 0.65f)));
		Modifiers.Add("Miserable", () => new StatModifier(new StatBonus(StatType.DEF, 1.5f), new StatBonus(StatType.SPD, 0.5f)));
		Modifiers.Add("Stressed", () => new StatModifier(new StatBonus(StatType.ATK, 1.2f), new StatBonus(StatType.DEF, 0.9f)));
		Modifiers.Add("AttackUp", () => CreateBuffDebuff(new StatBonus(StatType.ATK, 1.1f), new StatBonus(StatType.ATK, 1.25f), new StatBonus(StatType.ATK, 1.5f))
			.WithMessages("ATTACK rose!", "ATTACK cannot go\nany higher!")
			.WithStateIcons(new StateIcon("bnw_+1att", "Attack Up 1: x1.1 ATK"), new StateIcon("bnw_+2att", "Attack Up 2: x1.25 ATK"), new StateIcon("bnw_+3att", "Attack Up 3: x1.5 ATK")));
		Modifiers.Add("AttackDown", () => CreateBuffDebuff(new StatBonus(StatType.ATK, 0.9f), new StatBonus(StatType.ATK, 0.8f), new StatBonus(StatType.ATK, 0.7f))
			.WithMessages("ATTACK fell.", "ATTACK cannot go\nany lower!")
			.WithStateIcons(new StateIcon("bnw_-1att", "Attack Down 1: x0.9 ATK"), new StateIcon("bnw_-2att", "Attack Down 2: x0.8 ATK"), new StateIcon("bnw_-3att", "Attack Down 3: x0.7 ATK")));
		Modifiers.Add("DefenseUp", () =>
		{
			float[] values = SettingsMenuManager.Instance.UseConsoleDefense ? [1.1f, 1.2f, 1.3f] : [1.15f, 1.3f, 1.5f];
			return CreateBuffDebuff(new StatBonus(StatType.DEF, values[0]), new StatBonus(StatType.DEF, values[1]),
					new StatBonus(StatType.DEF, values[2]))
				.WithMessages("DEFENSE rose!", "DEFENSE cannot go\nany higher!")
				.WithStateIcons(new StateIcon("bnw_+1def", $"Defense Up 1: x{values[0]} DEF"), new StateIcon("bnw_+2def", $"Defense Up 2: x{values[1]} DEF"), new StateIcon("bnw_+3def", $"Defense Up 3: x{values[2]} DEF"));

		});
		Modifiers.Add("DefenseDown", () => CreateBuffDebuff(new StatBonus(StatType.DEF, 0.75f), new StatBonus(StatType.DEF, 0.5f), new StatBonus(StatType.DEF, 0.25f))
			.WithMessages("DEFENSE fell.", "DEFENSE cannot go\nany lower!")
			.WithStateIcons(new StateIcon("bnw_-1def", "Defense Down 1: x0.75 DEF"), new StateIcon("bnw_-2def", "Defense Down 2: x0.5 DEF"), new StateIcon("bnw_-3def", "Defense Down 3: x0.25 DEF")));
		Modifiers.Add("SpeedUp", () =>
		{
			float speedUp3 = SettingsMenuManager.Instance.UseConsoleSpeed ? 3f : 5f;
			return CreateBuffDebuff(new StatBonus(StatType.SPD, 1.5f), new StatBonus(StatType.SPD, 2f),
					new StatBonus(StatType.SPD, speedUp3))
				.WithMessages("SPEED rose!", "SPEED cannot go\nany higher!")
				.WithStateIcons(new StateIcon("bnw_+1spd", "Speed Up 1: x1.5 SPD"), new StateIcon("bnw_+2spd", "Speed Up 2: x2 SPD"), new StateIcon("bnw_+3spd", $"Speed Up 3: x{speedUp3} SPD"));

		});
		Modifiers.Add("SpeedDown", () => CreateBuffDebuff(new StatBonus(StatType.SPD, 0.8f), new StatBonus(StatType.SPD, 0.5f), new StatBonus(StatType.SPD, 0.25f))
			.WithMessages("SPEED fell.", "SPEED cannot go\nany lower!")
			.WithStateIcons(new StateIcon("bnw_-1spd", "Speed Down 1: x0.8 SPD"), new StateIcon("bnw_-2spd", "Speed Down 2: x0.5 SPD"), new StateIcon("bnw_-3spd", "Speed Down 3: x0.25 SPD")));

		Modifiers.Add("ReleaseEnergy", () => new StatModifier(new StatBonus(StatType.SPD, 1.25f), new StatBonus(StatType.ATK, 1.25f), new StatBonus(StatType.DEF, 1.25f), new StatBonus(StatType.LCK, 1.25f)));
		Modifiers.Add("ReleaseEnergyBasil", () => new ReleaseEnergyBasilStatModifier(new StatBonus(StatType.SPD, 1.25f), new StatBonus(StatType.ATK, 1.25f), new StatBonus(StatType.DEF, 1.25f), new StatBonus(StatType.LCK, 1.25f))
			.WithStateIcons(new StateIcon("bnw_regen", "HP Regen: 10% HEART/turn"), new StateIcon("bnw_regenmana", "Mana Regen: 5% JUICE/turn")));
		Modifiers.Add("ReleaseEnergyBasilBonus",
			() => new StatModifier(4, new StatBonus(StatType.SPD, 1.2f), new StatBonus(StatType.ATK, 1.2f),
				new StatBonus(StatType.DEF, 1.2f), new StatBonus(StatType.LCK, 1.2f)));
		Modifiers.Add("SnoCone", () => new StatModifier(new StatBonus(StatType.SPD, 1.2f), new StatBonus(StatType.ATK, 1.2f), new StatBonus(StatType.DEF, 1.2f), new StatBonus(StatType.LCK, 1.2f))
			.WithStateIcons(new StateIcon("bnw_snocone", "Sno-Cone: ATK/DEF/SPD/LCK x1.2")));
		Modifiers.Add("Flex", () => new FlexStatModifier(new StatBonus(StatType.HIT, 1000))
			.WithStateIcons(new StateIcon("bnw_flex", "Flex: Next Physical hit x2.5 damage, HIT x1000")));
		// see if these even need to be their own classes
		Modifiers.Add("Guard", () => new GuardStatModifier(1)
			.WithStateIcons(new StateIcon("bnw_guard", "Guard: x0.5 incoming damage")));
		Modifiers.Add("SecondChance", () => new SecondChanceStatModifier(1));
		Modifiers.Add("PlotArmor", () => new PlotArmorStatModifier());
		Modifiers.Add("Immortal", () => new ImmortalStatModifier());
		Modifiers.Add("Tickle", () => new StatModifier(1));
		Modifiers.Add("SweetheartHappy", () => new EmotionLockStatModifier("happy", new StatBonus(StatType.LCK, 2f), new StatBonus(StatType.SPD, 1.25f), new StatBonus(StatType.HIT, -10)));
		Modifiers.Add("SweetheartEcstatic", () => new EmotionLockStatModifier("happy", new StatBonus(StatType.LCK, 3f), new StatBonus(StatType.SPD, 1.5f), new StatBonus(StatType.HIT, -20)));
		Modifiers.Add("SweetheartManic", () => new EmotionLockStatModifier("happy", new StatBonus(StatType.LCK, 4f), new StatBonus(StatType.SPD, 2f), new StatBonus(StatType.HIT, -30)));
		Modifiers.Add("SpaceExAngry", () => new EmotionLockStatModifier("angry", new StatBonus(StatType.ATK, 1.25f), new StatBonus(StatType.DEF, 0.9f)));
		Modifiers.Add("SpaceExEnraged", () => new EmotionLockStatModifier("angry", new StatBonus(StatType.ATK, 1.5f), new StatBonus(StatType.DEF, 0.5f)));
		Modifiers.Add("SpaceExFurious", () => new EmotionLockStatModifier("angry", new StatBonus(StatType.ATK, 2f), new StatBonus(StatType.DEF, 0.3f)));
		Modifiers.Add("UnbreadTwinsSad", () => new EmotionLockStatModifier("sad", new StatBonus(StatType.DEF, 1.25f), new StatBonus(StatType.SPD, 0.8f)));
		Modifiers.Add("UnbreadTwinsDepressed", () => new EmotionLockStatModifier("sad", new StatBonus(StatType.DEF, 1.35f), new StatBonus(StatType.SPD, 0.65f)));
		Modifiers.Add("UnbreadTwinsMiserable", () => new EmotionLockStatModifier("sad", new StatBonus(StatType.DEF, 1.5f), new StatBonus(StatType.SPD, 0.5f)));
		Modifiers.Add("MrJawsumBarrier", () => new MrJawsumStatModifier());
		Modifiers.Add("Taunt", () => new StatModifier(1));
		Modifiers.Add("AubreyCounter", () => new AubreyCounterModifier(1));
		Modifiers.Add("HitRateDown", () => new StatModifier(2, new StatBonus(StatType.HIT, -55)));
		Modifiers.Add("PhotographHitRateDown", () => new StatModifier(1, new StatBonus(StatType.HIT, -25)));
		Modifiers.Add("Charm", () => new CharmStatModifier(1));
		Modifiers.Add("EmotionExploit", () => new EmotionLockStatModifier("emotion"));
		Modifiers.Add("SpaceExHusbandBlock", () => new SpaceExHusbandStatModifier());
		Modifiers.Add("Stockpile", () => new TierStatModifier().WithMaxTier(10));
		Modifiers.Add("PlutoCharging", () => new StatModifier(3, new StatBonus(StatType.DEF, 3f)));
		Modifiers.Add("PlutoBuff", () => new StatModifier(new StatBonus(StatType.ATK, 1.5f), new StatBonus(StatType.DEF, 1.5f), new StatBonus(StatType.LCK, 1.5f), new StatBonus(StatType.SPD, 10f)));
		Modifiers.Add("Encore", () => new EncoreStatModifier(3));
		Modifiers.Add("SalesTag", () => new SalesTagStatModifier());
		Modifiers.Add("Immune", () => new ImmuneStatModifier());
		#endregion

		#region SNACKS

		AddSnack("Tofu", "Soft cardboard, basically.\nHeals 5 HEART.", 5, 0);
		AddSnack("Candy", "A child's favorite food. Sweet!\nHeals 30 HEART.", 30, 17);
		AddSnack("Smores", "S'more smores, please!\nHeals 50 HEART.", 50, 34);
		AddSnack("Granola Bar", "A healthy stick of grain.\nHeals 60 HEART.", 60, 51);
		AddSnack("Bread", "A slice of life.\nHeals 60 HEART.", 60, 68);
		AddSnack("Nachos", "Suggested serving size: 6-8 nachos.\nHeals 75 HEART.", 75, 14);
		AddSnack("Chicken Wing", "Wing of chicken.\nHeals 80 HEART.", 80, 31);
		AddSnack("Hot Dog", "Better than a cold dog.\nHeals 100 HEART.", 100, 63);
		AddSnack("Waffle", "Designed to hold syrup!\nHeals 150 HEART.", 150, 71);
		AddSnack("Pancake", "Not designed to hold syrup...\nHeals 150 HEART.", 150, 8);
		AddSnack("Pizza Slice", "1/8th of a Whole pizza.\nHeals 175 HEART.", 175, 16);
		AddSnack("Fish Taco", "Aquatic taco.\nHeals 200 HEART.", 200, 24);
		AddSnack("Cheeseburger", "Contains all food groups, so it's healthy!\nHeals 250 HEART.", 250, 32);

		AddSnack("Chocolate", "Chocolate!? Oh, it's baking chocolate...\nHeals 40% of HEART.", 0.4f, 40);
		AddSnack("Donut", "Circular bread with a hole in it.\nHeals 60% of HEART.", 0.6f, 48);
		AddSnack("Ramen", "Now that is a lot of sodium!\nHeals 80% of HEART.", 0.8f, 56);
		AddSnack("Spaghetti", "Wet noodles slathered with chunky sauce.\nFully heals a friend's HEART.", 1.0f, 64);
		AddSnack("Dino Pasta", "Pasta shaped line dinosaurs.\nFully restores a friend's HEART.", 1.0f, 10);

		AddGroupSnack("Popcorn", "9/10 dentists hate it.\nHeals 35 HEART to all friends.", 35, 1);
		AddGroupSnack("Fries", "From France, wherever that is...\nHeals 60 HEART to all friends.", 60, 9);
		AddGroupSnack("Cheese Wheel", "Delicious, yet functional.\nHeals 100 HEART to all friends.", 100, 25);
		AddGroupSnack("Whole Chicken", "An entire chicken, wings and all.\nHeals 175 HEART to all friends.", 175, 33);
		AddGroupSnack("Whole Pizza", "8/8ths of a whole pizza.\nHeals 250 HEART to all friends.", 250, 41);
		AddGroupSnack("Dino Clumps", "Chicken nuggets shaped like dinosaurs.\nHeals 250 HEART to all friends.", 250, 2);

		AddJuiceSnack("Plum Juice", "For seniors. Wait, that's prune juice.\nHeals 15 JUICE.", 15, 26);
		AddJuiceSnack("Apple Juice", "Apparently better than orange juice.\nHeals 25 JUICE.", 25, 42);
		AddJuiceSnack("Breadfruit Juice", "Does not taste like bread.\nHeals 50 JUICE.", 50, 66);
		AddJuiceSnack("Lemonade", "When life gives you lemons, make this!\nHeals 75 JUICE.", 75, 11);
		AddJuiceSnack("Orange Juice", "Apparently better than apple juice.\nHeals 100 JUICE.", 100, 35);
		AddJuiceSnack("Pineapple Juice", "Painful... Why do you drink it?\nHeals 150 JUICE.", 150, 43);
		AddJuiceSnack("Bottled Water", "Water in a bottle.\nHeals 100 JUICE.", 100, 44);
		AddJuiceSnack("Fruit Juice?", "You're not sure what fruit it is.\nHeals 75 JUICE.", 75, 29);


		AddJuiceSnack("Cherry Soda", "Carbonated hell sludge.\nHeals 25% of JUICE.", 0.25f, 50);
		AddJuiceSnack("Star Fruit Soda", "To be shared with a friend.\nHeals 35% of JUICE.", 0.35f, 58);
		AddJuiceSnack("Tasty Soda", "Tasty soda for thirsty people.\nHeals 50% of JUICE.", 0.5f, 3);
		AddJuiceSnack("Peach Soda", "A regular peach soda.\nHeals 60% of JUICE.", 0.6f, 19);
		AddJuiceSnack("Butt Peach Soda", "An irregular peach soda.\nHeals 61% of JUICE.", 0.61f, 27);
		AddJuiceSnack("Watermelon Juice", "Heavenly nectar.\nFully heals a friend's JUICE.", 1.0f, 36);
		AddJuiceSnack("Dino Melon Soda", "Melon soda in a dino-shaped bottle.\nFully heals a friend's JUICE.", 1.0f, 5);

		AddGroupJuiceSnack("Banana Smoothie", "A little bland, but it does the job.\nHeals 20 JUICE to all friends.", 20, 67);
		AddGroupJuiceSnack("Mango Smoothie", "Makes you tango!\nHeals 40 JUICE to all friends.", 40, 52);
		AddGroupJuiceSnack("Berry Smoothie", "A healthy smoothie that tastes like dirt.\nHeals 60 JUICE to all friends.", 60, 12);
		AddGroupJuiceSnack("Melon Smoothie", "Chunky green melon goodness.\nHeals 80 JUICE to all friends.", 80, 20);
		AddGroupJuiceSnack("S.berry Smoothie", "The default smoothie.\nHeals 100 JUICE to all friends.", 100, 28);
		AddGroupJuiceSnack("Dino Smoothie", "Berry smoothie in a dino-shaped cup.\nHeals 150 JUICE to all friends.", 150, 13);

		AddComboSnack("Tomato", "You say tomato, I say tomato.\nHeals 100 HEART and 50 JUICE.", 100, 50, 57);
		AddComboSnack("Combo Meal", "What more could you ask for?\nHeals 250 HEART and 100 JUICE.", 250, 100, 65);

		Items["Grape Soda"] = new Item(
			name: "GRAPE SODA",
			description: "Objectively the best soda.\nHeals 80% of JUICE.",
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses GRAPE SODA!");
				AnimationManager.Instance.PlayAnimation(212, target);
				// grape soda uses emotion due to an oversight
				BattleManager.Instance.HealJuice(self, target, () => target.CurrentStats.MaxJuice * 0.8f);
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 59
		);

		Items["Coffee"] = new Item(
			name: "COFFEE",
			description: "Bitter bean juice.\nIncreases a friend's SPEED.",
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses COFFEE!");
				AnimationManager.Instance.PlayAnimation(214, target);
				// coffee heals, uses emotion, and has a variance due to an oversight
				BattleManager.Instance.Heal(self, target, () => target.CurrentStats.MaxJuice * 0.1f);
				target.AddTierStatModifier("SpeedUp", 3);
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 39
		);

		Items["☐☐☐"] = new Item(
		   name: "☐☐☐",
		   description: "☐☐☐☐☐☐☐☐☐ ☐☐☐ ☐☐☐",
		   target: SkillTarget.Ally,
		   effect: async (self, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses ☐☐☐!");
			   AnimationManager.Instance.PlayAnimation(215, target);
			   // ☐☐☐ uses emotion due to an oversight
			   BattleManager.Instance.Heal(self, target, () => 50, 0f);
			   await Task.CompletedTask;
		   },
		   spritesheetPath: "res://assets/system/itemConsumables.png",
		   spriteIndex: 0
	   );

		Items["Prune Juice"] = new Item(
			name: "PRUNE JUICE",
			description: "This tastes horrible. Don't drink it.\nHeals 30 JUICE...probably.",
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses PRUNE JUICE!");
				AnimationManager.Instance.PlayAnimation(213, target);
				int total = 30;
				if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Blender" || x.Actor.Weapon.Name == "Ol' Reliable"))
					total = 45;
				target.HealJuice(total);
				BattleManager.Instance.SpawnDamageNumber(total, target.CenterPoint, DamageType.JuiceGain);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
				int hpLoss = (int)Math.Round(target.CurrentHP * 0.3f, MidpointRounding.AwayFromZero);
				target.Damage(hpLoss);
				// damaging items don't kill
				if (target.CurrentHP == 0)
					target.CurrentHP = 1;
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 26
		);

		Items["Rotten Milk"] = new Item(
			name: "ROTTEN MILK",
			description: "This is bad. Don't drink it.\nHeals 10 JUICE + ???",
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses ROTTEN MILK!");
				AnimationManager.Instance.PlayAnimation(213, target);
				int total = 10;
				if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Blender" || x.Actor.Weapon.Name == "Ol' Reliable"))
					total = 15;
				target.HealJuice(total);
				BattleManager.Instance.SpawnDamageNumber(total, target.CenterPoint, DamageType.JuiceGain);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
				int hpLoss = (int)Math.Round(target.CurrentHP * 0.5f, MidpointRounding.AwayFromZero);
				target.Damage(hpLoss);
				// damaging items don't kill
				if (target.CurrentHP == 0)
					target.CurrentHP = 1;
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 79
		);

		Items["Milk"] = new Item(
			name: "MILK",
			description: "Good for your bones. Heals 10 JUICE\nand increases DEFENSE for the battle.",
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses MILK!");
				AnimationManager.Instance.PlayAnimation(213, target);
				await Task.Delay(2000);
				AnimationManager.Instance.PlayAnimation(214, target);
				int total = 10;
				if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Blender" || x.Actor.Weapon.Name == "Ol' Reliable"))
					total = 15;
				target.HealJuice(total);
				BattleManager.Instance.SpawnDamageNumber(total, target.CenterPoint, DamageType.JuiceGain);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
				target.AddStatModifier("DefenseUp");
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 60
		);

		Items["Sno-Cone"] = new Item(
			name: "SNO-CONE",
			description: "Heals a friend's HEART and JUICE, and\nraises ALL STATS for the battle.",
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses SNO-CONE!");
				await AnimationManager.Instance.WaitForAnimation(212, target);
				target.Heal(target.CurrentStats.MaxHP);
				BattleManager.Instance.SpawnDamageNumber(target.CurrentStats.MaxHP, target.CenterPoint, DamageType.Heal);
				target.HealJuice(target.CurrentStats.MaxJuice);
				BattleManager.Instance.SpawnDamageNumber(target.CurrentStats.MaxJuice, target.CenterPoint, DamageType.JuiceGain);
				target.AddStatModifier("SnoCone");
				AnimationManager.Instance.PlayAnimation(214, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s ATTACK rose!");
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s DEFENSE rose!");
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s SPEED rose!");
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s LUCK rose!");
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 49
		);

		Items["Life Jam"] = new Item(
			name: "LIFE JAM",
			description: "Infused with the spirit of life.\nRevives a friend that is TOAST.",
			target: SkillTarget.DeadAlly,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses LIFE JAM!");
				if (target.CurrentState != "toast")
				{
					target = BattleManager.Instance.GetRandomDeadPartyMember();
					if (target == null)
					{
						BattleLogManager.Instance.QueueMessage("It had no effect.");
						return;
					}
				}
				await AnimationManager.Instance.WaitForAnimation(269, target);
				if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Charm?.Name == "Breadphones"))
					target.CurrentHP = target.CurrentStats.MaxHP;
				else
					target.CurrentHP = target.CurrentStats.MaxHP / 2;
				target.SetState("neutral", true);
				BattleLogManager.Instance.QueueMessage(self, target, "[target] rose again!");
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 15
		);

		Items["Dino Jam"] = new Item(
		   name: "DINO JAM",
		   description: "Infused with the spirit of dino life.\nFully revives a friend that is TOAST.",
		   target: SkillTarget.DeadAlly,
		   effect: async (self, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses DINO JAM!");
			   if (target.CurrentState != "toast")
			   {
				   target = BattleManager.Instance.GetRandomDeadPartyMember();
				   if (target == null)
				   {
					   BattleLogManager.Instance.QueueMessage("It had no effect.");
					   return;
				   }
			   }
			   await AnimationManager.Instance.WaitForAnimation(269, target);
			   target.CurrentHP = target.CurrentStats.MaxHP;
			   target.SetState("neutral", true);
			   BattleLogManager.Instance.QueueMessage(self, target, "[target] rose again!");
		   },
		   spritesheetPath: "res://assets/system/itemConsumables.png",
		   spriteIndex: 47
		);

		Items["Jam Packets"] = new Item(
		   name: "JAM PACKETS",
		   description: "Infused with the spirit of life.\nRevives all friends that are TOAST.",
		   target: SkillTarget.AllDeadAllies,
		   effect: async (self, targets) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, "[actor] uses JAM PACKETS!");
			   if (!targets.Any(x => x.CurrentState == "toast"))
			   {
				   BattleLogManager.Instance.QueueMessage("It had no effect.");
				   return;
			   }
			   foreach (Actor member in targets)
			   {
				   AnimationManager.Instance.PlayAnimation(269, member);
				   member.CurrentHP = member.CurrentStats.MaxHP / 4;
				   member.SetState("neutral", true);
				   BattleLogManager.Instance.QueueMessage(self, member, "[target] rose again!");
			   }
			   await Task.CompletedTask;
		   },
		   spritesheetPath: "res://assets/system/itemConsumables.png",
		   spriteIndex: 82
		);

		// TODO: faraway town snacks

		#endregion

		#region TOYS
		Items["Rubber Band"] = new Item(
			name: "RUBBER BAND",
			description: "Deals damage to a foe and reduces\ntheir DEFENSE.",
			target: SkillTarget.Enemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses RUBBER BAND!");
				BattleManager.Instance.Damage(self, target, () => 50, true, 0, neverCrit: true, ignoreEmotion: !SettingsMenuManager.Instance.ToysUseEmotionDamage);
				await AnimationManager.Instance.WaitForAnimation(219, target);
				target.AddStatModifier("DefenseDown");
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 69
		);

		Items["Big Rubber Band"] = new Item(
			name: "BIG RUBBER BAND",
			description: "Deals big damage to a foe and reduces\ntheir DEFENSE.",
			target: SkillTarget.Enemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses BIG RUBBER BAND!");
				BattleManager.Instance.Damage(self, target, () => 150, true, 0, neverCrit: true, ignoreEmotion: !SettingsMenuManager.Instance.ToysUseEmotionDamage);
				await AnimationManager.Instance.WaitForAnimation(219, target);
				target.AddStatModifier("DefenseDown");
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 69
		);

		Items["Jacks"] = new Item(
			name: "JACKS",
			description: "Deals small damage to all foes\nand reduces their SPEED.",
			target: SkillTarget.AllEnemies,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses JACKS!");
				foreach (Actor enemy in targets)
				{
					AnimationManager.Instance.PlayAnimation(122, enemy);
				}
				await Task.Delay(1000);
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy, () => 25, true, 0, neverCrit: true, ignoreEmotion: !SettingsMenuManager.Instance.ToysUseEmotionDamage);
					AnimationManager.Instance.PlayAnimation(219, enemy);
					enemy.AddStatModifier("SpeedDown", silent: true);
				}
				BattleLogManager.Instance.QueueMessage("All foes' SPEED fell.");
				await Task.Delay(500);
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 61
		);

		Items["Dynamite"] = new Item(
			name: "DYNAMITE",
			description: "Actually dangerous...\nDeals heavy damage to all foes.",
			target: SkillTarget.AllEnemies,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses DYNAMITE!");
				await AnimationManager.Instance.WaitForScreenAnimation(172, true);
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy, () => 150, false, 0, neverCrit: true, ignoreEmotion: !SettingsMenuManager.Instance.ToysUseEmotionDamage);
				}
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 6
		);

		Items["Air Horn"] = new Item(
			name: "AIR HORN",
			description: "Who would invent this!?\nInflicts ANGER on all friends.",
			target: SkillTarget.AllAllies,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses AIR HORN!");
				AudioManager.Instance.PlaySFX("SE_airhorn", 1, 0.9f);
				foreach (Actor member in targets)
				{
					MakeAngry(member);
				}
				await Task.CompletedTask;
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 62
		);

		Items["Rain Cloud"] = new Item(
			name: "RAIN CLOUD",
			description: "Angsty water droplets.\nInflicts SAD on all friends.",
			target: SkillTarget.AllAllies,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses RAIN CLOUD!");
				AudioManager.Instance.PlaySFX("BA_sad_level_2", 1, 0.9f);
				foreach (Actor member in targets)
				{
					MakeSad(member);
				}
				await Task.CompletedTask;
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 46
		);

		Items["Confetti"] = new Item(
			name: "CONFETTI",
			description: "Small squares of colorful paper.\nInflicts HAPPY on all friends.",
			target: SkillTarget.AllAllies,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses CONFETTI!");
				AudioManager.Instance.PlaySFX("GEN_ta_da", 1, 0.9f);
				foreach (Actor member in targets)
				{
					MakeHappy(member);
				}
				await Task.CompletedTask;
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 30
		);

		Items["Sparkler"] = new Item(
			name: "SPARKLER",
			description: "Little fires.\nInflicts HAPPY on a friend or foe.",
			target: SkillTarget.AllyOrEnemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses SPARKLER!");
				AudioManager.Instance.PlaySFX("GEN_pahpuh", 1, 0.9f);
				MakeHappy(target);
				await Task.CompletedTask;
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 22
		);

		Items["Poetry Book"] = new Item(
			name: "POETRY BOOK",
			description: "Sad words string together.\nInflicts SAD on a friend or foe.",
			target: SkillTarget.AllyOrEnemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses POETRY BOOK!");
				AudioManager.Instance.PlaySFX("BA_angsty_song", volume: 0.9f);
				await Task.Delay(360);
				AudioManager.Instance.PlaySFX("BA_sad_poem", volume: 0.9f);
				await Task.Delay(1000);
				MakeSad(target);
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 38
		);

		Items["Present"] = new Item(
			name: "PRESENT",
			description: "It's not what you wanted...\nInflicts ANGER on a friend or foe.",
			target: SkillTarget.AllyOrEnemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses PRESENT!");
				AudioManager.Instance.PlaySFX("SE_shuffle", 1, 0.9f);
				MakeAngry(target);
				await Task.CompletedTask;
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 54
		);

		Items["Dandelion"] = new Item(
			name: "DANDELION",
			description: "Has a calming effect.\nRemoves emotion from a friend or foe.",
			target: SkillTarget.AllyOrEnemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses DANDELION!");
				AudioManager.Instance.PlaySFX("BA_calm_down", 1, 0.9f);
				// this should be changed once boss specific/special states are improved
				if (target.CurrentState == "neutral" || target.HasLockedEmotion())
				{
					BattleLogManager.Instance.QueueMessage("It had no effect.");
				}
				else
				{
					BattleLogManager.Instance.QueueMessage(self, target, "[target] feels NEUTRAL.");
					target.SetState("neutral", true);
				}
				await Task.CompletedTask;
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 70
		);
		Items["Pepper Spray"] = new Item(
			name: "PEPPER SPRAY",
			description: "For self-defense only.",
			target: SkillTarget.Enemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses PEPPER SPRAY!");
				await AnimationManager.Instance.WaitForScreenAnimation(271, target is Enemy);
				BattleManager.Instance.Damage(self, target, () => 500, false, 0f, neverCrit: true);
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 83
		);
		#endregion

		#region WEAPONS
		Weapons["Shiny Knife"] = new Weapon("Shiny Knife", new StatBonus(StatType.ATK, 5), new StatBonus(StatType.HIT, 100));
		Weapons["Knife"] = new Weapon("Shiny Knife", new StatBonus(StatType.ATK, 7), new StatBonus(StatType.SPD, 2), new StatBonus(StatType.HIT, 100));
		Weapons["Dull Knife"] = new Weapon("Dull Knife", new StatBonus(StatType.ATK, 9), new StatBonus(StatType.SPD, 4), new StatBonus(StatType.LCK, 2), new StatBonus(StatType.HIT, 100));
		Weapons["Rusty Knife"] = new Weapon("Rusty Knife", new StatBonus(StatType.ATK, 11), new StatBonus(StatType.DEF, 2), new StatBonus(StatType.SPD, 6), new StatBonus(StatType.LCK, 4), new StatBonus(StatType.HIT, 100));
		Weapons["Red Knife"] = new Weapon("Red Knife", new StatBonus(StatType.ATK, 13), new StatBonus(StatType.DEF, 6), new StatBonus(StatType.SPD, 6), new StatBonus(StatType.LCK, 6), new StatBonus(StatType.HIT, 100));

		Weapons["Fly Swatter"] = new Weapon("Fly Swatter", new StatBonus(StatType.ATK, 1), new StatBonus(StatType.HIT, 1000));
		Weapons["Steak Knife"] = new Weapon("Steak Knife", new StatBonus(StatType.ATK, 30), new StatBonus(StatType.HIT, 25));
		Weapons["Hands"] = new Weapon("Hands", new StatBonus(StatType.ATK, 2), new StatBonus(StatType.HIT, 95));
		// potential todo: other violin variants?
		Weapons["Violin"] = new Weapon("Violin", new StatBonus(StatType.ATK, 14), new StatBonus(StatType.HIT, 1000));

		Weapons["Stuffed Toy"] = new Weapon("Stuffed Toy", new StatBonus(StatType.ATK, 4), new StatBonus(StatType.HIT, 100));
		Weapons["Comet Hammer"] = new Weapon("Comet Hammer", new StatBonus(StatType.ATK, 6), new StatBonus(StatType.LCK, 2), new StatBonus(StatType.HIT, 100));
		Weapons["Body Pillow"] = new Weapon("Body Pillow", new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.ATK, 8), new StatBonus(StatType.HIT, 100));
		Weapons["Pool Noodle"] = new Weapon("Pool Noodle", new StatBonus(StatType.ATK, -5), new StatBonus(StatType.DEF, -5), new StatBonus(StatType.SPD, -5), new StatBonus(StatType.LCK, -5), new StatBonus(StatType.HIT, 100));
		Weapons["Cool Noodle"] = new Weapon("Cool Noodle", new StatBonus(StatType.ATK, 15), new StatBonus(StatType.HIT, 100));
		Weapons["Hero's Trophy"] = new Weapon("Hero's Trophy", new StatBonus(StatType.ATK, 10), new StatBonus(StatType.DEF, 5), new StatBonus(StatType.HIT, 100));
		Weapons["Mailbox"] = new Weapon("Mailbox", new StatBonus(StatType.ATK, 12), new StatBonus(StatType.HIT, 100));
		Weapons["Baguette"] = new Weapon("Baguette", new StatBonus(StatType.ATK, 10), new StatBonus(StatType.DEF, 10), new StatBonus(StatType.HIT, 100));
		Weapons["Sweetheart Bust"] = new Weapon("Sweetheart Bust", new StatBonus(StatType.ATK, 20), new StatBonus(StatType.SPD, -30), new StatBonus(StatType.HIT, 75));
		Weapons["Baseball Bat"] = new Weapon("Baseball Bat", new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.ATK, 20), new StatBonus(StatType.SPD, 10), new StatBonus(StatType.LCK, 10), new StatBonus(StatType.HIT, 100));

		Weapons["Nail Bat"] = new Weapon("Nail Bat", new StatBonus(StatType.ATK, 3), new StatBonus(StatType.HIT, 95));

		Weapons["Rubber Ball"] = new Weapon("Rubber Ball", new StatBonus(StatType.ATK, 3), new StatBonus(StatType.HIT, 100));
		Weapons["Meteor Ball"] = new Weapon("Meteor Ball", new StatBonus(StatType.ATK, 4), new StatBonus(StatType.LCK, 2), new StatBonus(StatType.HIT, 100));
		Weapons["Blood Orange"] = new Weapon("Blood Orange", new StatBonus(StatType.MaxJuice, 30), new StatBonus(StatType.ATK, 6), new StatBonus(StatType.HIT, 100));
		Weapons["Jack"] = new Weapon("Jack", new StatBonus(StatType.ATK, 12), new StatBonus(StatType.DEF, -6), new StatBonus(StatType.LCK, -6), new StatBonus(StatType.HIT, 100));
		Weapons["Beach Ball"] = new Weapon("Beach Ball", new StatBonus(StatType.ATK, 10), new StatBonus(StatType.SPD, 25), new StatBonus(StatType.HIT, 100));
		Weapons["Coconut"] = new Weapon("Coconut", new StatBonus(StatType.MaxJuice, 50), new StatBonus(StatType.ATK, 8), new StatBonus(StatType.HIT, 100));
		Weapons["Globe"] = new Weapon("Globe", new StatBonus(StatType.ATK, 10), new StatBonus(StatType.HIT, 1000));
		Weapons["Chicken Ball"] = new Weapon("Chicken Ball", new StatBonus(StatType.SPD, 200), new StatBonus(StatType.HIT, 100));
		Weapons["Snowball"] = new Weapon("Snowball", new StatBonus(StatType.ATK, 13), new StatBonus(StatType.HIT, 100));
		Weapons["Basketball"] = new Weapon("Basketball", new StatBonus(StatType.MaxJuice, 50), new StatBonus(StatType.ATK, 15), new StatBonus(StatType.SPD, 100), new StatBonus(StatType.LCK, 15), new StatBonus(StatType.HIT, 100));

		Weapons["Basketball (Real World)"] = new Weapon("Basketball", new StatBonus(StatType.ATK, 2), new StatBonus(StatType.HIT, 95));

		Weapons["Spatula"] = new Weapon("Spatula", new StatBonus(StatType.ATK, 4), new StatBonus(StatType.HIT, 100));
		Weapons["Rolling Pin"] = new Weapon("Rolling Pin", new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.ATK, 12), new StatBonus(StatType.DEF, 12), new StatBonus(StatType.HIT, 100));
		Weapons["Teapot"] = new Weapon("Teapot", new StatBonus(StatType.MaxJuice, 30), new StatBonus(StatType.ATK, 6), new StatBonus(StatType.HIT, 100));
		Weapons["Frying Pan"] = new Weapon("Frying Pan", new StatBonus(StatType.MaxHP, 30), new StatBonus(StatType.ATK, 7), new StatBonus(StatType.HIT, 100));
		Weapons["Blender"] = new Weapon("Blender", new StatBonus(StatType.MaxJuice, 30), new StatBonus(StatType.ATK, 7), new StatBonus(StatType.HIT, 100));
		Weapons["Baking Pan"] = new Weapon("Baking Pan", new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.ATK, 6), new StatBonus(StatType.HIT, 100));
		Weapons["Tenderizer"] = new Weapon("Tenderizer", new StatBonus(StatType.ATK, 30), new StatBonus(StatType.HIT, 100));
		Weapons["LOL Sword"] = new Weapon("LOL Sword", new StatBonus(StatType.MaxJuice, 10), new StatBonus(StatType.ATK, 14), new StatBonus(StatType.HIT, 100));
		Weapons["Ol' Reliable"] = new Weapon("Ol' Reliable", new StatBonus(StatType.MaxHP, 20), new StatBonus(StatType.MaxJuice, 20), new StatBonus(StatType.ATK, 20), new StatBonus(StatType.HIT, 100));
		Weapons["Shucker"] = new Weapon("Shucker", new StatBonus(StatType.ATK, 10), new StatBonus(StatType.HIT, 100));

		Weapons["Fist"] = new Weapon("Fist", new StatBonus(StatType.ATK, 1), new StatBonus(StatType.HIT, 95));

		Weapons["Garden Shears"] = new Weapon("Garden Shears", new StatBonus(StatType.ATK, 13), new StatBonus(StatType.DEF, 6), new StatBonus(StatType.SPD, 6), new StatBonus(StatType.LCK, 6), new StatBonus(StatType.HIT, 100));
		#endregion

		#region CHARMS
		// TODO: missing charms (special behavior/unused): unused charms
		Charms["3-leaf Clover"] = new Charm("3-leaf Clover", new StatBonus(StatType.LCK, 3));
		Charms["4-leaf Clover"] = new Charm("4-leaf Clover", [new StatBonus(StatType.MaxHP, 4), new StatBonus(StatType.LCK, 4)]);
		Charms["5-leaf Clover"] = new Charm("5-leaf Clover").WithApplyEffect(() =>
		{
			return [new StatBonus(StatType.LCK, 2 + BattleManager.Instance.Energy)];
		});
		Charms["Backpack"] = new Charm("Backpack", new StatBonus(StatType.DEF, 2));
		Charms["Baseball Cap"] = new Charm("Baseball Cap", [new StatBonus(StatType.DEF, 10), new StatBonus(StatType.DEF, 15)]);
		Charms["Binoculars"] = new Charm("Binoculars", [new StatBonus(StatType.DEF, 2), new StatBonus(StatType.HIT, 200)]);
		Charms["Blanket"] = new Charm("Blanket", [new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.DEF, 1)]);
		Charms["Bow Tie"] = new Charm("Bow Tie", new StatBonus(StatType.DEF, 4));
		Charms["Bracelet"] = new Charm("Bracelet", new StatBonus(StatType.DEF, 1));
		Charms["Breadphones"] = new Charm("Breadphones", [new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.DEF, 5)]);
		Charms["Bubble Wrap"] = new Charm("Bubble Wrap", new StatBonus(StatType.DEF, 3));
		Charms["Bunny Ears"] = new Charm("Bunny Ears", [new StatBonus(StatType.DEF, 3), new StatBonus(StatType.SPD, 12)]);
		Charms["Cat Ears"] = new Charm("Cat Ears", [new StatBonus(StatType.DEF, 1), new StatBonus(StatType.SPD, 10)]);
		Charms["Cellphone"] = new Charm("Cellphone", new StatBonus(StatType.DEF, 10));
		Charms["Cool Glasses"] = new Charm("Cool Glasses", [new StatBonus(StatType.ATK, 5), new StatBonus(StatType.DEF, 5)]);
		Charms["Cough Mask"] = new Charm("Cough Mask", [new StatBonus(StatType.MaxHP, 25), new StatBonus(StatType.MaxJuice, 25), 
			new StatBonus(StatType.ATK, 10), new StatBonus(StatType.DEF, 10), 
			new StatBonus(StatType.SPD, 10), new StatBonus(StatType.LCK, 10)]);
		Charms["Daisy"] = new Charm("Daisy", [new StatBonus(StatType.MaxHP, 10)]).WithStartOfBattleEffect((actor) =>
		{
			actor.SetState("happy", true);
			return Task.CompletedTask;
		});
		Charms["Eye Patch"] = new Charm("Eye Patch", [new StatBonus(StatType.ATK, 7), new StatBonus(StatType.HIT, -25)]);
		Charms["Faux Tail"] = new Charm("Faux Tail", new StatBonus(StatType.SPD, 15));
		Charms["Fedora"] = new Charm("Fedora", [new StatBonus(StatType.DEF, 5), new StatBonus(StatType.DEF, 5)]);
		Charms["Finger"] = new Charm("Finger", [new StatBonus(StatType.ATK, 10), new StatBonus(StatType.DEF, -5)]).WithStartOfBattleEffect((actor) =>
		{
			actor.SetState("angry", true);
			return Task.CompletedTask;
		});
		Charms["Fox Tail"] = new Charm("Fox Tail").WithApplyEffect(() =>
		{
			return [new StatBonus(StatType.SPD, 5 + (3 * BattleManager.Instance.Energy))];
		});
		Charms["Friendship Bracelet"] = new Charm("Friendship Bracelet",[new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.MaxJuice, 10)]);
		Charms["Nerdy Glasses"] = new Charm("Nerdy Glasses", [new StatBonus(StatType.DEF, 5), new StatBonus(StatType.HIT, 200)]);
		Charms["Gold Watch"] = new Charm("Gold Watch", new StatBonus(StatType.SPD, -10));
		Charms["Hard Hat"] = new Charm("Hard Hat", new StatBonus(StatType.DEF, 6));
		Charms["Headband"] = new Charm("Headband", [new StatBonus(StatType.MaxJuice, 20), new StatBonus(StatType.ATK, 10), 
			new StatBonus(StatType.DEF, 3), new StatBonus(StatType.SPD, 15)]);
		Charms["Heart String"] = new Charm("Heart String", [new StatBonus(StatType.MaxHP, 30)]).WithStartOfBattleEffect((actor) =>
		{
			actor.SetState("happy", true);
			return Task.CompletedTask;
		});
		Charms["High Heels"] = new Charm("High Heels", [new StatBonus(StatType.ATK, 10), new StatBonus(StatType.SPD, 10)]);
		Charms["Homework"] = new Charm("Homework").WithStartOfBattleEffect((actor) =>
		{
			actor.SetState("sad", true);
			return Task.CompletedTask;
		});
		Charms["Inner Tube"] = new Charm("Inner Tube").WithApplyEffect(() =>
		{
			return [new StatBonus(StatType.DEF, 2 + BattleManager.Instance.Energy)];
		});
		Charms["Magical Bean"] = new Charm("Magical Bean").WithStartOfBattleEffect((actor) =>
		{
			BattleManager.Instance.RandomEmotion(actor);
			return Task.CompletedTask;
		});
		Charms["Onion Ring"] = new Charm("Onion Ring", [new StatBonus(StatType.MaxHP, 20), new StatBonus(StatType.MaxJuice, 20)]);
		Charms["Paper Bag"] = new Charm("Paper Bag", [new StatBonus(StatType.MaxHP, 40), new StatBonus(StatType.DEF, 13)]);
		Charms["Hector"] = new Charm("Hector", []);
		Charms["Pretty Bow"] = new Charm("Pretty Bow", [new StatBonus(StatType.MaxHP, 50), new StatBonus(StatType.ATK, 10), new StatBonus(StatType.DEF, 3)]);
		Charms["Punching Bag"] = new Charm("Punching Bag").WithStartOfBattleEffect((actor) =>
		{
			actor.SetState("angry", true);
			return Task.CompletedTask;
		});
		Charms["Rabbit Foot"] = new Charm("Rabbit Foot", [new StatBonus(StatType.SPD, 15), new StatBonus(StatType.LCK, 10)] );
		Charms["Red Ribbon"] = new Charm("Red Ribbon").WithApplyEffect(() =>
		{
			return [new StatBonus(StatType.ATK, 1 + (2 * BattleManager.Instance.Energy)), new StatBonus(StatType.DEF, 5)];
		});
		Charms["Deep Poetry Book"] = new Charm("Deep Poetry Book").WithStartOfBattleEffect((actor) =>
		{
			actor.SetState("sad", true);
			return Task.CompletedTask;
		});
		Charms["Rubber Duck"] = new Charm("Rubber Duck", new StatBonus(StatType.DEF, 7));
		Charms["Seer Goggles"] = new Charm("Seer Goggles", [new StatBonus(StatType.DEF, 1), new StatBonus(StatType.LCK, 3), new StatBonus(StatType.HIT, 200)]);
		Charms["Top Hat"] = new Charm("Top Hat", [new StatBonus(StatType.MaxHP, 13), new StatBonus(StatType.DEF, 13), new StatBonus(StatType.LCK, 13)]);
		Charms["Hector Jr."] = new Charm("Hector Jr.").WithApplyEffect(() =>
		{
			int energy = BattleManager.Instance.Energy;
			return
			[
				new StatBonus(StatType.ATK, 1 + energy), new StatBonus(StatType.DEF, 1 + energy),
				new StatBonus(StatType.SPD, 1 + energy), new StatBonus(StatType.LCK, energy)
			];
		});
		Charms["Wedding Ring"] = new Charm("Wedding Ring", [new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.MaxJuice, 10), 
			new StatBonus(StatType.ATK, 3), new StatBonus(StatType.DEF, 3), new StatBonus(StatType.SPD, 3), new StatBonus(StatType.LCK, 3)])
			.WithStartOfBattleEffect((actor) =>
		{
			actor.SetState("happy", true);
			return Task.CompletedTask;
		});
		Charms["Wishbone"] = new Charm("Wishbone", new StatBonus(StatType.LCK, 7));
		Charms["Veggie Kid"] = new Charm("Veggie Kid", [new StatBonus(StatType.MaxHP, 15), new StatBonus(StatType.MaxJuice, 15)]);
		Charms["Watering Pail"] = new Charm("Watering Pail", new StatBonus(StatType.MaxJuice, 10));
		Charms["Sunscreen"] = new Charm("Sunscreen", new StatBonus(StatType.MaxHP, 15));
		Charms["Rake"] = new Charm("Rake", new StatBonus(StatType.ATK, 3));
		Charms["Scarf"] = new Charm("Scarf", new StatBonus(StatType.DEF, 3));
		Charms["Cotton Ball"] = new Charm("Cotton Ball", [new StatBonus(StatType.DEF, 1), new StatBonus(StatType.SPD, 3)]);
		Charms["Flashlight"] = new Charm("Flashlight", new StatBonus(StatType.DEF, 4));
		Charms["Universal Remote"] = new Charm("Universal Remote", [new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.MaxJuice, 10), 
			new StatBonus(StatType.ATK, 5), new StatBonus(StatType.DEF, 5), new StatBonus(StatType.SPD, 5), new StatBonus(StatType.LCK, 5)]);
		Charms["TV Remote"] = new Charm("TV Remote", [new StatBonus(StatType.MaxHP, 5), new StatBonus(StatType.DEF, 2)]);
		Charms["Flower Crown"] = new Charm("Flower Crown", [new StatBonus(StatType.MaxHP, 100), new StatBonus(StatType.MaxJuice, 25)]);
		Charms["Tulip Hairstick"] = new Charm("Tulip Hairstick", new StatBonus(StatType.MaxHP, 50));
		Charms["Gladiolus Hairband"] = new Charm("Gladiolus Hairband", [new StatBonus(StatType.ATK, 10), new StatBonus(StatType.LCK, 10), new StatBonus(StatType.HIT, 100)]);
		Charms["Cactus Hairclip"] = new Charm("Cactus Hairclip", [new StatBonus(StatType.DEF, 15), new StatBonus(StatType.MaxHP, 15)]);
		Charms["Rose Hairclip"] = new Charm("Rose Hairclip", [new StatBonus(StatType.MaxHP, 15), new StatBonus(StatType.MaxJuice, 15), 
			new StatBonus(StatType.ATK, 5), new StatBonus(StatType.DEF, 5), new StatBonus(StatType.SPD, 5), new StatBonus(StatType.LCK, 5), new StatBonus(StatType.HIT, 100)]);
		Charms["Seashell Necklace"] = new Charm("Seashell Necklace", [new StatBonus(StatType.MaxHP, 25), new StatBonus(StatType.MaxJuice, 25), new StatBonus(StatType.DEF, 5)]);
		Charms["Contract"] = new Charm("Contract", [
			new StatBonus(StatType.MaxHP, 0.2f), new StatBonus(StatType.MaxJuice, 0.2f),
			new StatBonus(StatType.ATK, 20), new StatBonus(StatType.DEF, 20), new StatBonus(StatType.SPD, 20),
			new StatBonus(StatType.LCK, 20)
		]);
		Charms["Chef's Hat"] = new Charm("Chef's Hat", new StatBonus(StatType.DEF, 15)).WithStartOfTurnEffect((actor) =>
		{
			int juice = (int)Math.Round(actor.CurrentStats.MaxJuice * 0.05f, MidpointRounding.AwayFromZero);
			actor.HealJuice(juice);
			BattleManager.Instance.SpawnDamageNumber(juice, actor.CenterPoint, DamageType.JuiceGain);
			return Task.CompletedTask;
		});
		Charms["Sales Tag"] = new Charm("Sales Tag").WithStartOfBattleEffect(actor =>
		{
			actor.AddStatModifier("SalesTag");
			return Task.CompletedTask;
		});
		Charms["Abbi's Eye"] = new Charm("Abbi's Eye", [new StatBonus(StatType.MaxHP, 0.1f),
			new StatBonus(StatType.MaxJuice, 0.01f),
			new StatBonus(StatType.ATK, 40), new StatBonus(StatType.SPD, 40), new StatBonus(StatType.LCK, 40),
			new StatBonus(StatType.HIT, 100)]).WithStartOfBattleEffect(async actor =>
		{
			Enemy target = BattleManager.Instance.GetRandomAliveEnemy();
			await Task.Delay(1000);
			BattleLogManager.Instance.QueueMessage(actor, target,"[actor] focuses their vision and observes\n[target]!");
			AnimationManager.Instance.PlayAnimation(4, target);
			await Task.Delay(1000);
			List<PartyMemberComponent> members = BattleManager.Instance.GetAlivePartyMembers();
			PartyMemberComponent taunting = members.FirstOrDefault(x => x.Actor.HasStatModifier("Taunt"));
			if (taunting != null)
			{
				await AnimationManager.Instance.WaitForAnimation(4, taunting.Actor);
				target.ObserveTarget = taunting.Actor;
				BattleLogManager.Instance.QueueMessage(target, taunting.Actor,"[actor] has their eyes on\n[target]!");
				return;
			}

			bool multi = GameManager.Instance.Random.RandiRange(1, 2) == 1;
			if (multi)
			{
				// vanilla omori technically stops after the 4th attempt
				// maybe add a toggle for this?
				Enemy enemy = BattleManager.Instance.GetAllEnemies().FirstOrDefault(x => x.HasMultiTargetSkill);
				if (enemy != null)
				{
					enemy.ObserveMultiTarget = true;
					BattleLogManager.Instance.QueueMessage(enemy, "[actor] has their eyes on\neveryone!");
					foreach (PartyMemberComponent m in members)
						AnimationManager.Instance.PlayAnimation(4, m.Actor);
					await Task.Delay(1000);
					return;
				}
			}
				
			PartyMember member = members[GameManager.Instance.Random.RandiRange(0, members.Count - 1)].Actor;
			BattleLogManager.Instance.QueueMessage(target, member,"[actor] has their eyes on\n[target]!");
			await AnimationManager.Instance.WaitForAnimation(4, member);
			target.ObserveTarget = member;
		});

		#endregion
	}

	// Helper method for creating buffs and debuffs affected by the InfiniteBuffsDebuffs setting
	private static TierStatModifier CreateBuffDebuff(params StatBonus[] bonuses)
	{
		return SettingsMenuManager.Instance.InfiniteBuffsDebuffs
			? new TierStatModifier(bonuses)
			: new TierStatModifier(6, bonuses);
	}

	/// <summary>
	/// Adds a snack that provides flat healing
	/// </summary>
	private static void AddSnack(string name, string description, int healing, int iconIndex)
	{
		Items[name] = new Item(
			name: name.ToUpper(),
			description: description,
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
				AnimationManager.Instance.PlayAnimation(212, target);
				int heal = healing;
				if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Frying Pan" || x.Actor.Weapon.Name == "Ol' Reliable"))
					heal = (int)Math.Round(heal * 1.5f, MidpointRounding.AwayFromZero);
				target.Heal(heal);
				BattleManager.Instance.SpawnDamageNumber(heal, target.CenterPoint, DamageType.Heal);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {heal} HEART!");
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: iconIndex
		);
	}

	/// <summary>
	/// Adds a snack that provides flat juice healing
	/// </summary>
	private static void AddJuiceSnack(string name, string description, int juice, int iconIndex)
	{
		Items[name] = new Item(
			name: name.ToUpper(),
			description: description,
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
				AnimationManager.Instance.PlayAnimation(213, target);
				int total = juice;
				if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Blender" || x.Actor.Weapon.Name == "Ol' Reliable"))
					total = (int)Math.Round(total * 1.5f, MidpointRounding.AwayFromZero);
				target.HealJuice(total);
				BattleManager.Instance.SpawnDamageNumber(total, target.CenterPoint, DamageType.JuiceGain);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: iconIndex
		);
	}

	/// <summary>
	/// Adds a snack that provides percentage-based juice healing
	/// </summary>
	private static void AddJuiceSnack(string name, string description, float percentage, int iconIndex)
	{
		Items[name] = new Item(
			name: name.ToUpper(),
			description: description,
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
				AnimationManager.Instance.PlayAnimation(213, target);
				float juice = target.CurrentStats.MaxJuice * percentage;
				if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Blender" || x.Actor.Weapon.Name == "Ol' Reliable"))
					juice *= 1.5f;
				int finalJuice = (int)Math.Round(juice, MidpointRounding.AwayFromZero);
				target.HealJuice(finalJuice);
				BattleManager.Instance.SpawnDamageNumber(finalJuice, target.CenterPoint, DamageType.JuiceGain);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {finalJuice} JUICE!");
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: iconIndex
		);
	}

	/// <summary>
	/// Adds a snack that provides percentage-based healing
	/// </summary>
	private static void AddSnack(string name, string description, float percentage, int iconIndex)
	{
		Items[name] = new Item(
			name: name.ToUpper(),
			description: description,
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
				AnimationManager.Instance.PlayAnimation(212, target);
				float heal = target.CurrentStats.MaxHP * percentage;
				if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Frying Pan" || x.Actor.Weapon.Name == "Ol' Reliable"))
					heal *= 1.5f;
				int finalHeal = (int)Math.Round(heal, MidpointRounding.AwayFromZero);
				target.Heal(finalHeal);
				BattleManager.Instance.SpawnDamageNumber(finalHeal, target.CenterPoint, DamageType.Heal);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {finalHeal} HEART!");
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: iconIndex
		);
	}

	/// <summary>
	/// Adds a snack that provides flat healing to all allies
	/// </summary>
	private static void AddGroupSnack(string name, string description, int healing, int iconIndex)
	{
		Items[name] = new Item(
		   name: name.ToUpper(),
		   description: description,
		   target: SkillTarget.AllAllies,
		   effect: async (self, targets) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, $"[actor] uses {name.ToUpper()}!");
			   int heal = healing;
			   if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name is "Frying Pan" or "Ol' Reliable"))
				   heal = (int)Math.Round(heal * 1.5f, MidpointRounding.AwayFromZero);
			   foreach (Actor member in targets)
			   {
				   AnimationManager.Instance.PlayAnimation(212, member);
				   member.Heal(heal);
				   BattleManager.Instance.SpawnDamageNumber(heal, member.CenterPoint, DamageType.Heal);
				   BattleLogManager.Instance.QueueMessage(self, member, $"[target] recovered {heal} HEART!");
			   }
			   await Task.CompletedTask;
		   },
		   spritesheetPath: "res://assets/system/itemConsumables.png",
		   spriteIndex: iconIndex
	   );
	}

	/// <summary>
	/// Adds a snack that provides flat juice healing to all allies
	/// </summary>
	private static void AddGroupJuiceSnack(string name, string description, int juice, int iconIndex)
	{
		Items[name] = new Item(
		   name: name.ToUpper(),
		   description: description,
		   target: SkillTarget.AllAllies,
		   effect: async (self, targets) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, $"[actor] uses {name.ToUpper()}!");
			   int total = juice;
			   if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name is "Blender" or "Ol' Reliable"))
				   total = (int)Math.Round(total * 1.5f, MidpointRounding.AwayFromZero);
			   foreach (Actor member in targets)
			   {
				   AnimationManager.Instance.PlayAnimation(213, member);
				   member.HealJuice(total);
				   BattleManager.Instance.SpawnDamageNumber(total, member.CenterPoint, DamageType.JuiceGain);
				   BattleLogManager.Instance.QueueMessage(self, member, $"[target] recovered {total} JUICE!");
			   }
			   await Task.CompletedTask;
		   },
		   spritesheetPath: "res://assets/system/itemConsumables.png",
		   spriteIndex: iconIndex
	   );
	}

	/// <summary>
	/// A snack that provides flat healing and juice
	/// </summary>
	private static void AddComboSnack(string name, string description, int healing, int juice, int iconIndex)
	{
		Items[name] = new Item(
			name: name.ToUpper(),
			description: description,
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
				AnimationManager.Instance.PlayAnimation(212, target);
				int heal = healing;
				int total = juice;
				if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Frying Pan" || x.Actor.Weapon.Name == "Ol' Reliable"))
					heal = (int)Math.Round(heal * 1.5f, MidpointRounding.AwayFromZero);
				// donald compiler please come save us donald compiler please save us
				if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Blender" || x.Actor.Weapon.Name == "Ol' Reliable"))
					total = (int)Math.Round(total * 1.5f, MidpointRounding.AwayFromZero);
				target.Heal(heal);
				target.HealJuice(total);
				BattleManager.Instance.SpawnDamageNumber(heal, target.CenterPoint, DamageType.Heal);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {heal} HEART!");
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: iconIndex
		);
	}

	/// <summary>
	/// Makes the given <see cref="Actor"/> sad, if possible. Increases the tier if the actor is already sad.
	/// </summary>
	/// <param name="who">The <see cref="Actor"/> to make sad.</param>
	public static void MakeSad(Actor who)
	{
		string state = "sad";
		string current = who.CurrentState;
		switch (current)
		{
			case "miserable":
				BattleLogManager.Instance.QueueMessage(null, who, "[target] can't get SADDER!");
				return;
			case "depressed":
				state = "miserable";
				break;
			case "sad":
				state = "depressed";
				break;
		}
		if (who.IsStateValid(state))
			who.SetState(state);
		else
			BattleLogManager.Instance.QueueMessage(null, who, "[target] can't get SADDER!");
	}

	/// <summary>
	/// Makes the given <see cref="Actor"/> happy, if possible. Increases the tier if the actor is already happy.
	/// </summary>
	/// <param name="who">The <see cref="Actor"/> to make happy.</param>
	public static void MakeHappy(Actor who)
	{
		string state = "happy";
		string current = who.CurrentState;
		switch (current)
		{
			case "manic":
				BattleLogManager.Instance.QueueMessage(null, who, "[target] can't get HAPPIER!");
				return;
			case "ecstatic":
				state = "manic";
				break;
			case "happy":
				state = "ecstatic";
				break;
		}
		if (who.IsStateValid(state))
			who.SetState(state);
		else
			BattleLogManager.Instance.QueueMessage(null, who, "[target] can't get HAPPIER!");
	}

	/// <summary>
	/// Makes the given <see cref="Actor"/> angry, if possible. Increases the tier if the actor is already angry.
	/// </summary>
	/// <param name="who">The <see cref="Actor"/> to make angry.</param>
	public static void MakeAngry(Actor who)
	{
		string state = "angry";
		string current = who.CurrentState;
		switch (current)
		{
			case "furious":
				BattleLogManager.Instance.QueueMessage(null, who, "[target] can't get ANGRIER!");
				return;
			case "enraged":
				state = "furious";
				break;
			case "angry":
				state = "enraged";
				break;
		}
		if (who.IsStateValid(state))
			who.SetState(state);
		else
			BattleLogManager.Instance.QueueMessage(null, who, "[target] can't get ANGRIER!");
	}
}
