using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class UnderwaterHighwayRegistry
{
	internal static void Register()
	{
		Enemies.Add("Snaley 1", () => new SnaleyOne());
		Enemies.Add("Snaley 2", () => new SnaleyTwo());
		Enemies.Add("Snaley 3", () => new SnaleyThree());
		Enemies.Add("ShadyMole", () => new ShadyMole());
		Enemies.Add("Mussel", () => new Mussel());
		Enemies.Add("ReverseMermaid", () => new ReverseMermaid());
		Enemies.Add("SharkFin", () => new SharkFin());
		Enemies.Add("Squizzard", () => new Squizzard());
		Enemies.Add("WormBot", () => new WormBot());
		Enemies.Add("FishBunny", () => new FishBunny());

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
			}
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
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
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
				BattleManager.Instance.QueueForceCommand(self, BattleManager.Instance.GetPartyMemberTargetOrFirst(self.LastTargetIndex), Skills["SNFollowup"]);
				await Wait.Milliseconds(1000);
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
				await Wait.Milliseconds(500);
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
			}
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
					await Wait.Milliseconds(500);
				}
			}
		);
		
		Skills["SNMegaphone"] = new Skill(
			name: "SNMegaphone",
			description: "SNMegaphone",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			priority: -500,
			setup: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses an AIRHORN!");
				AudioManager.Instance.PlaySFX("SE_airhorn");
				await Wait.Milliseconds(500);
				foreach (Actor target in targets)
				{
					BattleManager.Instance.MakeAngry(target);
				}
			}
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
				await Wait.Milliseconds(500);
				await AnimationManager.Instance.WaitForSnaley();
				await Wait.Milliseconds(1000);
				foreach (Actor target in targets)
				{
					AnimationManager.Instance.PlayAnimation(327, target);
					BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.5f, false,
						neverCrit: true, ignoreEmotion: true);
				}
				if (self is Enemy stillAnEnemy)
					stillAnEnemy.SetOpacity(1f, 0.5f);
				await Wait.Milliseconds(500);
			}
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
			}
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
			}
		);
		
		Skills["SMDynamite"] = new Skill(
			name: "SMDynamite",
			description: "SMDynamite",
			target: SkillTarget.Everybody,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] lobs DYNAMITE!");
				await AnimationManager.Instance.WaitForScreenAnimation(270, self is Enemy);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => 25, false, 0.25f, neverCrit: true);
			}
		).WithCountsAsSingleTarget();

		// MUSSEL //
		Skills["MUSAttack"] = new Skill(
			name: "MUSAttack",
			description: "MUSAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] punches [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["MUSFlex"] = new Skill(
			name: "MUSFlex",
			description: "MUSFlex",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(218, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] flexes and feels its best!");
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s HIT RATE rose!");
				target.AddStatModifier("Flex");
			}
		);

		Skills["MUSHide"] = new Skill(
			name: "MUSHide",
			description: "MUSHide",
			target: SkillTarget.Self,
			cost: 0,
			priority: 1000,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(178, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] hides in its shell.");
				target.AddStatModifier("Guard");
			}
		);

		// REVERSE MERMAID //
		Skills["RVMAttack"] = new Skill(
			name: "RVMAttack",
			description: "RVMAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[target] bumps into [actor]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["RVMDoNothing"] = new Skill(
			name: "RVMDoNothing",
			description: "RVMDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_weird", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] does a backflip!");
				BattleLogManager.Instance.QueueMessage("WOW!");
				await Task.CompletedTask;
			}
		);

		Skills["RVMRunAround"] = new Skill(
			name: "RVMRunAround",
			description: "RVMRunAround",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] runs amok!");
				foreach (Actor target in targets)
				{
					await AnimationManager.Instance.WaitForAnimation(123, target);
					BattleManager.Instance.Damage(self, target, () => self.CurrentEmotion.Id switch
					{
						"happy" => self.CurrentStats.ATK * 2 + 5 - target.CurrentStats.DEF,
						"ecstatic" => self.CurrentStats.ATK * 2 + 7 - target.CurrentStats.DEF,
						"manic" => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF,
						_ => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF
					}, false);
				}
			}
		).WithRandomTargets(2);

		// SHARK FIN //
		Skills["SHFAttack"] = new Skill(
			name: "SHFAttack",
			description: "SHFAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] charges at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["SHFDoNothing"] = new Skill(
			name: "SHFDoNothing",
			description: "SHFDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_dance", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] swims in a circle.");
				await Task.CompletedTask;
			}
		);

		Skills["SHFBite"] = new Skill(
			name: "SHFBite",
			description: "SHFBite",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(157, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] chomps [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentEmotion.Id switch
				{
					"angry" => self.CurrentStats.ATK * 3,
					"enraged" => self.CurrentStats.ATK * 3 + 5,
					"furious" => self.CurrentStats.ATK * 4,
					_ => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF
				}, false);
			}
		);

		Skills["SHFWorkUp"] = new Skill(
			name: "SHFWorkUp",
			description: "SHFWorkUp",
			target: SkillTarget.Self,
			cost: 0,
			setup: async (self, target) =>
			{
				target.AddTierStatModifier("SpeedUp", silent: true);
				bool applied = BattleManager.Instance.MakeAngry(target, silent: true);
				await AnimationManager.Instance.WaitForAnimation(218, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] works itself up!");
				// DIALOGUE QUIRK: prints the SPEED line even when the stat is already at its limit
				BattleLogManager.Instance.QueueMessage(self, "[actor]'s SPEED increased!");
				// DIALOGUE QUIRK: always prints the tier-1 ANGRY line regardless of the tier the target reaches
				QueueTierOneEmotionLine(target, "angry", applied);
			},
			effect: (_, _) => Task.CompletedTask
		);

		// SQUIZZARD //
		Skills["SQZAttack"] = new Skill(
			name: "SQZAttack",
			description: "SQZAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses magic on [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["SQZDoNothing"] = new Skill(
			name: "SQZDoNothing",
			description: "SQZDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_weird", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] mutters nonsense.");
				await Task.CompletedTask;
			}
		);

		Skills["SQZSquidWard"] = new Skill(
			name: "SQZSquidWard",
			description: "SQZSquidWard",
			target: SkillTarget.Ally,
			cost: 0,
			priority: 1000,
			setup: async (_, target) =>
			{
				Task playing = AnimationManager.Instance.WaitForAnimation(178, target);
				target.AddTierStatModifier("DefenseUp", silent: true);
				await playing;
			},
			effect: (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] creates a squid ward.");
				// DIALOGUE QUIRK: prints the DEFENSE line even when the stat is already at its limit
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s DEFENSE increased.");
				return Task.CompletedTask;
			}
		);

		Skills["SQZSquidMagic"] = new Skill(
			name: "SQZSquidMagic",
			description: "SQZSquidMagic",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses squid magic!");
				BattleLogManager.Instance.QueueMessage("Everyone feels weird...");
				await Task.WhenAll(targets.Select(member => AnimationManager.Instance.WaitForAnimation(161, member)));
				foreach (PartyMember member in targets.OfType<PartyMember>().Where(x => x.CurrentHP > 0))
					member.AddStatModifier("RandomEmotion", silent: true);
				RandomPlayerEmotion();
			}
		);

		// WORM-BOT //
		Skills["WBTAttack"] = new Skill(
			name: "WBTAttack",
			description: "WBTAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(125, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["WBTDoNothing"] = new Skill(
			name: "WBTDoNothing",
			description: "WBTDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_roar", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] crunches loudly!");
				await Task.CompletedTask;
			}
		);

		Skills["WBTLaser"] = new Skill(
			name: "WBTLaser",
			description: "WBTLaser",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(158, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] fires a laser at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
			}
		);

		// FISH BUNNY //
		Skills["FSBAttack"] = new Skill(
			name: "FSBAttack",
			description: "FSBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] swims into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["FSBDoNothing"] = new Skill(
			name: "FSBDoNothing",
			description: "FSBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_dance", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] swims in a circle. ");
				await Task.CompletedTask;
			}
		);

		Skills["FSBSchooling"] = new Skill(
			name: "FSBSchooling",
			description: "FSBSchooling",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(145, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] calls for friends!");
				if (self is Enemy enemy)
					ReinforcementSlots.Summon(enemy, ReinforcementSlots.FishBunny);
			}
		);
	}

	internal static void RandomPlayerEmotion()
	{
		for (int slot = 0; slot < 4; slot++)
		{
			int roll = GameManager.Instance.Random.RandiRange(0, 2);
			PartyMember member = BattleManager.Instance.GetAlivePartyMembers()
				.FirstOrDefault(x => x.Component.Position == slot && x.HasStatModifier("RandomEmotion"));
			if (member != null)
				SquidMagicEmotion(member, slot, roll);
		}
	}

	private static readonly string[][] SquidMagicGroups = [["sad", "angry", "happy"], ["happy", "angry", "sad"], ["angry", "happy", "sad"], ["sad", "happy", "angry"]];

	private static void SquidMagicEmotion(PartyMember member, int slot, int roll)
	{
		string group = SquidMagicGroups[slot][roll];
		string[] tiers = group switch
		{
			"sad" => ["sad", "depressed", "miserable"],
			"angry" => ["angry", "enraged", "furious"],
			_ => ["happy", "ecstatic", "manic"]
		};
		string[] others = group switch
		{
			"sad" => ["happy", "ecstatic", "manic", "angry", "enraged", "furious", "afraid"],
			"angry" => ["happy", "ecstatic", "manic", "sad", "depressed", "miserable", "afraid"],
			_ => ["angry", "enraged", "furious", "sad", "depressed", "miserable", "afraid"]
		};
		RemoveEmotions(member, others);
		string held = member.CurrentEmotion.Id;
		if (held == tiers[2])
			return;
		if (held == tiers[1])
		{
			RemoveEmotions(member, tiers[1], tiers[0]);
			// VANILLA BUG: SQUID MAGIC's HAPPY branch stops at ECSTATIC
			AddEmotion(member, group == "happy" ? tiers[1] : tiers[2]);
		}
		else if (held == tiers[0])
		{
			RemoveEmotions(member, tiers[0]);
			AddEmotion(member, tiers[1]);
		}
		else
			AddEmotion(member, tiers[0]);
	}

	private static void RemoveEmotions(PartyMember member, params string[] emotions)
	{
		foreach (string emotion in emotions)
		{
			if (member.CurrentEmotion.Id == emotion)
				member.SetEmotion("neutral", silent: true);
			member.ClearResult();
		}
	}

	private static void AddEmotion(PartyMember member, string emotion)
	{
		member.SetEmotion(emotion, silent: true);
		member.ClearResult();
	}
}
