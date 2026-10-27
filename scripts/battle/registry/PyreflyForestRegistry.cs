using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class PyreflyForestRegistry
{
	internal static void Register()
	{
		Enemies.Add("LostSproutMole", () => new LostSproutMole());
		Enemies.Add("KingCrawler", () => new KingCrawler());
		Enemies.Add("KingCarnivore", () => new KingCarnivore());
		Enemies.Add("Root", () => new Root());
		Enemies.Add("BugBunny", () => new BugBunny());
		Enemies.Add("RareBear", () => new RareBear());
		Enemies.Add("SproutMole?", () => new SproutMoleQuestion());
		Enemies.Add("GhostBunny", () => new GhostBunny());
		Enemies.Add("PottedPlant", () => new PottedPlant());

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
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					new DamageOptions { NeverMiss = false, CritMultiplier = 0.6 });
			}
		);

		Skills["LSMDoNothing"] = new Skill(
			name: "Do Nothing",
			description: "Does nothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_dance", volume: 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] is rolling around.");
				await Task.CompletedTask;
			}
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
				await Wait.Frames(10);
				BattleManager.Instance.Damage(self, targets[0],
					() => self.CurrentStats.ATK * 1.5f - targets[0].CurrentStats.DEF, false);
				await Wait.Frames(55);
				BattleManager.Instance.Damage(self, targets[0],
					() => self.CurrentStats.ATK * 1.5f - targets[0].CurrentStats.DEF, false);
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
			}
		);
		
		Skills["KCDoNothing"] = new Skill(
			name: "KCDoNothing",
			description: "KCDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_roar", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] lets out an ear-piercing\nscreech!");
				BattleManager.Instance.MakeAngry(target);
				await Task.CompletedTask;
			}
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
			}
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
			}
		);
		
		Skills["KCEat"] = new Skill(
			name: "KCEat",
			description: "KCEat",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(157, target);
				BattleManager.Instance.SpawnDamageNumber(target.CurrentStats.MaxHP + 100, target);
				target.Damage(target.CurrentHP);
			},
			hidden: true
		);
		
		Skills["KCRecover"] = new Skill(
			name: "KCRecover",
			description: "KCRecover",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(216, target);
				int healed = BattleManager.Instance.Heal(self, target, () => 170, silent: true);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovers {healed} HEART!");
				BattleManager.Instance.MakeHappy(target);
			}
		);

		// King Carnivore
		Skills["UPCAttack"] = new Skill(
			name: "UPCAttack",
			description: "UPCAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(126, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] wraps\n[target] with vines!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["UPCDoNothing"] = new Skill(
			name: "UPCDoNothing",
			description: "UPCDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] roars!");
				await Task.CompletedTask;
			}
		);
		
		Skills["UPCSweetGas"] = new Skill(
			name: "UPCSweetGas",
			description: "UPCSweetGas",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] releases gas!\nIt smells sweet!\nEveryone feels HAPPY!");
				await AnimationManager.Instance.WaitForScreenAnimation(181, targets[0] is Enemy);
				foreach (Actor target in targets)
					BattleManager.Instance.MakeHappy(target, silent: true);
			}
		);

		// Root
		Skills["ROAttack"] = new Skill(
			name: "ROAttack",
			description: "ROAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(126, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["RODoNothing"] = new Skill(
			name: "RODoNothing",
			description: "RODoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] wiggles around.");
				await Task.CompletedTask;
			}
		);
		
		Skills["ROHealPlant"] = new Skill(
			name: "ROHealPlant",
			description: "ROHealPlant",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] absorbs nutrients.");
				await Wait.Milliseconds(500);
				foreach (Actor target in targets)
				{
					AnimationManager.Instance.PlayAnimation(216, target);
					BattleManager.Instance.Heal(self, target, () => target.CurrentStats.MaxHP * 0.05f);
				}
			},
			hidden: true
		);

		// BUG BUNNY //
		Skills["BGBAttack"] = new Skill(
			name: "BGBAttack",
			description: "BGBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(286, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] bites [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["BGBDoNothing"] = new Skill(
			name: "BGBDoNothing",
			description: "BGBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_dance", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] tries to balance on its head. ");
				await Task.CompletedTask;
			}
		);

		Skills["BGBSuddenJump"] = new Skill(
			name: "BGBSuddenJump",
			description: "BGBSuddenJump",
			target: SkillTarget.Enemy,
			cost: 0,
			priority: 999,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(8, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] suddenly lunges at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentEmotion.Id switch
				{
					"happy" => self.CurrentStats.ATK * 2 + 2 - target.CurrentStats.DEF,
					"ecstatic" => self.CurrentStats.ATK * 2 + 5,
					"manic" => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF,
					_ => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF
				}, false);
			}
		);

		Skills["BGBScuttle"] = new Skill(
			name: "BGBScuttle",
			description: "BGBScuttle",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, target) =>
			{
				bool applied = BattleManager.Instance.MakeHappy(target, silent: true);
				await AnimationManager.Instance.WaitForAnimation(218, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] happily scuttles around.");
				BattleLogManager.Instance.QueueMessage("It was really cute!");
				// DIALOGUE QUIRK: always prints the tier-1 HAPPY line regardless of the tier the target reaches
				if (applied)
					BattleLogManager.Instance.QueueMessage(target, "[actor] feels HAPPY!");
				else
					BattleManager.Instance.ShowEmotionNoEffect(target, "happy");
			}
		);

		// RARE BEAR //
		Skills["RBAttack"] = new Skill(
			name: "RBAttack",
			description: "RBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(127, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slashes at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["RBRoar"] = new Skill(
			name: "RBRoar",
			description: "RBRoar",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForScreenAnimation(175, target is Enemy);
				BattleLogManager.Instance.QueueMessage(self, "[actor] lets out a huge roar!");
				// DIALOGUE QUIRK: always prints the tier-1 ANGRY line regardless of the tier the target reaches
				BattleLogManager.Instance.QueueMessage(target, "[actor] feels ANGRY!");
				BattleManager.Instance.MakeAngry(target, silent: true);
			}
		);

		// SPROUT MOLE? //
		Skills["SMQAttack"] = new Skill(
			name: "SMQAttack",
			description: "SMQAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(126, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slaps [target]?");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["SMQDoNothing"] = new Skill(
			name: "SMQDoNothing",
			description: "SMQDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_dance", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] is rolling around?");
				await Task.CompletedTask;
			}
		);

		Skills["SMQRunAround"] = new Skill(
			name: "SMQRunAround",
			description: "SMQRunAround",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] runs around!");
				await AnimationManager.Instance.WaitForScreenAnimation(200, targets[0] is Enemy);
				foreach (Actor target in targets)
				{
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

		// GHOST BUNNY //
		Skills["GHBAttack"] = new Skill(
			name: "GHBAttack",
			description: "GHBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] phases through [target]!");
				BattleLogManager.Instance.QueueMessage(target, "[actor] feels tired.");
				BattleManager.Instance.DamageJuice(self, target, () => target.CurrentJuice * 0.20f, new DamageOptions { NeverMiss = false, Crit = CritMode.Never });
			}
		);

		Skills["GHBDoNothing"] = new Skill(
			name: "GHBDoNothing",
			description: "GHBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] floats in place.");
				await Task.CompletedTask;
			},
			setup: (_, _) =>
			{
				AudioManager.Instance.PlaySFX("SE_creepy_tune");
				return Task.CompletedTask;
			}
		);

		// POTTED PLANT //
		Skills["PPAttack"] = new Skill(
			name: "PPAttack",
			description: "PPAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(126, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["PPDoNothing"] = new Skill(
			name: "PPDoNothing",
			description: "PPDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_Drink", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] is resting in its pot. ");
				await Task.CompletedTask;
			}
		);

		Skills["PPTrip"] = new Skill(
			name: "PPTrip",
			description: "PPTrip",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: (_, target) =>
			{
				target.AddTierStatModifier("SpeedDown", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(171, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[target] trips on [actor]'s roots.");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
				// DIALOGUE QUIRK: prints the SPEED line even when SPEED is already at its limit
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s SPEED fell.");
				AnimationManager.Instance.PlayAnimation(215, target);
				await Wait.Frames(60);
			}
		);

		Skills["PPExplode"] = new Skill(
			name: "PPExplode",
			description: "PPExplode",
			target: SkillTarget.AllEnemies,
			cost: 0,
			setup: (self, _) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] explodes!");
				return Task.CompletedTask;
			},
			effect: (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(172, self is Enemy);
				foreach (Enemy ally in BattleManager.Instance.GetAllAliveEnemies().ToList())
					Explode(self, ally, 40f);
				foreach (Actor target in targets)
					Explode(self, target, 40f);
				return Task.CompletedTask;
			}
		);
	}

	private static void Explode(Actor self, Actor target, float damage)
	{
		BattleManager.Instance.Damage(self, target, () => damage,
			new DamageOptions
			{
				NeverMiss = false,
				Variance = 0f,
				Crit = CritMode.Never,
				OnHit = () =>
				{
					self.CurrentHP = 0;
					self.ClearResult();
				}
			});
	}
}
