using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class SweetheartsCastleRegistry
{
	internal static void Register()
	{
		Enemies.Add("Sweetheart", () => new Sweetheart());
		Enemies.Add("SirMaximusI", () => new SirMaximusI());
		Enemies.Add("SirMaximusII", () => new SirMaximusII());
		Enemies.Add("SirMaximusIII", () => new SirMaximusIII());
		Enemies.Add("Harold", () => new Harold());
		Enemies.Add("HorseButt", () => new HorseButt());
		Enemies.Add("HorseHead", () => new HorseHead());
		Enemies.Add("Lucius", () => new Lucius());
		Enemies.Add("Marsha", () => new Marsha());
		Enemies.Add("Therese", () => new Therese());

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
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false);
			}
		);

		Skills["SharpInsult"] = new Skill(
			name: "Sharp Insult",
			description: "Sharp Insult",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] insults everyone!\nEveryone feels ANGRY!");
				await AnimationManager.Instance.WaitForScreenAnimation(183, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK, false, 0.1f,
						neverCrit: true);
					BattleManager.Instance.MakeAngry(member, silent: true);
				}
			}
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
					BattleManager.Instance.Damage(self, member,
						() => self.CurrentStats.ATK * 2.5f - member.CurrentStats.DEF, false);
				}
			}
		);

		Skills["Brag"] = new Skill(
			name: "Brag",
			description: "Brag",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(162, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] boasts about\none of her many, many talents!");
				BattleManager.Instance.MakeHappy(target);
			}
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
			}
		);
		
		Skills["SMIDoNothing"] = new Skill(
			name: "SMIDoNothing",
			description: "SMIDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] pulled his back...");
				BattleManager.Instance.MakeSad(target);
				await Task.CompletedTask;
			}
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
				await Wait.Milliseconds(500);
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
			priority: 100,
			// vanilla adds ACCURACY UP in the setup action and lets it expire at the end of the turn
			setup: (self, _) =>
			{
				self.AddStatModifier("AccuracyUp", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his\nultimate attack!");
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayScreenAnimation(186, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 50, false, 0.25f, neverCrit: true);
				}
			}
		);
		
		Skills["SMUltimateAttack"] = new Skill(
			name: "SMUltimateAttack",
			description: "SMUltimateAttack",
			target: SkillTarget.AllEnemies,
			cost: 0,
			priority: 100,
			// vanilla adds ACCURACY UP in the setup action and lets it expire at the end of the turn
			setup: (self, _) =>
			{
				self.AddStatModifier("AccuracyUp", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his\nultimate attack!");
				await Wait.Milliseconds(1000);
				Task animation = AnimationManager.Instance.WaitForScreenAnimation(186, false);
				int damage = BattleManager.Instance.GetAliveEnemies().Count switch
				{
					2 => 75,
					1 => 50,
					_ => 100
				};
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => damage, false, 0f, neverCrit: true);
				}

				await animation;
			}
		);

		// Sir Maximus II //
		
		Skills["SMIIDoNothing"] = new Skill(
			name: "SMIIDoNothing",
			description: "SMIIDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] remembers his\nfather's dying words.");
				BattleManager.Instance.MakeSad(target);
				await Task.CompletedTask;
			}
		);
		
		Skills["SMIISpin"] = new Skill(
			name: "SMIISpin",
			description: "SMIISpin",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] spins quickly!");
				await Wait.Milliseconds(500);
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
			}
		);
		
		Skills["SMIIUltimateAttack"] = new Skill(
			name: "SMIIUltimateAttack",
			description: "SMIIUltimateAttack",
			target: SkillTarget.AllEnemies,
			cost: 0,
			priority: 100,
			// vanilla adds ACCURACY UP in the setup action and lets it expire at the end of the turn
			setup: (self, _) =>
			{
				self.AddStatModifier("AccuracyUp", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his father's\nultimate attack!");
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayScreenAnimation(186, targets[0] is Enemy);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 50, false, 0.25f, neverCrit: true);
				}
			}
		);

		// Sir Maximus III //
		Skills["SMIIIDoNothing"] = new Skill(
			name: "SMIIIDoNothing",
			description: "SMIIIDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (_,target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] remembers his\ngrandfather's dying words.\n[actor] feels SAD.");
				BattleManager.Instance.MakeSad(target, silent: true);
				await Task.CompletedTask;
			}
		);
		
		Skills["SMIIIFlex"] = new Skill(
			name: "SMIIIFlex",
			description: "SMIIIFlex",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] flexes and feels\nhis best!");
				await AnimationManager.Instance.WaitForAnimation(218, target);
				target.AddStatModifier("Flex");
				if (target.CurrentEmotion.Id != "happy")
					BattleManager.Instance.MakeHappy(target);
			}
		);
		
		Skills["SMIIIUltimateAttack"] = new Skill(
			name: "SMIIIUltimateAttack",
			description: "SMIIIUltimateAttack",
			target: SkillTarget.AllEnemies,
			cost: 0,
			priority: 100,
			// vanilla adds ACCURACY UP in the setup action and lets it expire at the end of the turn
			setup: (self, _) =>
			{
				self.AddStatModifier("AccuracyUp", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his grandfather's\nultimate attack!");
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayScreenAnimation(186, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 50, false, 0.25f, neverCrit: true);
				}
			}
		);

		// HAROLD //
		Skills["HARAttack"] = new Skill(
			name: "HARAttack",
			description: "HARAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(3, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] swings his sword at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["HARDoNothing"] = new Skill(
			name: "HARDoNothing",
			description: "HARDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_falls_over", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] adjusts his helmet.");
				await Task.CompletedTask;
			}
		);

		Skills["HARProtect"] = new Skill(
			name: "HARProtect",
			description: "HARProtect",
			target: SkillTarget.Self,
			cost: 0,
			priority: 1000,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(178, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] protects himself.");
				target.AddStatModifier("Guard");
			}
		);

		Skills["HARWink"] = new Skill(
			name: "HARWink",
			description: "HARWink",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: (_, target) =>
			{
				BattleManager.Instance.MakeHappy(target, silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(148, self);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] winks at [target].");
				// DIALOGUE QUIRK: always prints the tier-1 HAPPY line regardless of the tier the target reaches
				QueueTierOneEmotionLine(target, "happy", target.IsEmotionValid("happy"));
			}
		);

		// HORSE BUTT //
		Skills["HBTAttack"] = new Skill(
			name: "HBTAttack",
			description: "HBTAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] stomps on [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["HBTDoNothing"] = new Skill(
			name: "HBTDoNothing",
			description: "HBTDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("SE_fart", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] farts.");
				await Task.CompletedTask;
			}
		);

		Skills["HBTKick"] = new Skill(
			name: "HBTKick",
			description: "HBTKick",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] kicks with gusto!");
				foreach (Actor target in targets)
				{
					await AnimationManager.Instance.WaitForAnimation(123, target);
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
					await Wait.Frames(8);
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
				}
			}
		);

		Skills["HBTPrance"] = new Skill(
			name: "HBTPrance",
			description: "HBTPrance",
			target: SkillTarget.Self,
			cost: 0,
			setup: (_, target) =>
			{
				BattleManager.Instance.MakeHappy(target, silent: true);
				return Task.CompletedTask;
			},
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_Horse", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] prances around.");
				await Task.CompletedTask;
			}
		);

		// HORSE HEAD //
		Skills["HHDAttack"] = new Skill(
			name: "HHDAttack",
			description: "HHDAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] bites [target]'s arm.");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["HHDDoNothing"] = new Skill(
			name: "HHDDoNothing",
			description: "HHDDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("GEN_bump", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] burps.");
				await Task.CompletedTask;
			}
		);

		Skills["HHDLick"] = new Skill(
			name: "HHDLick",
			description: "HHDLick",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: (_, target) =>
			{
				BattleManager.Instance.MakeAngry(target, silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] licks [target]'s hair");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
				// DIALOGUE QUIRK: always prints the tier-1 ANGRY line regardless of the tier the target reaches
				QueueTierOneEmotionLine(target, "angry", target.IsEmotionValid("angry"));
			}
		);

		Skills["HHDWhinny"] = new Skill(
			name: "HHDWhinny",
			description: "HHDWhinny",
			target: SkillTarget.Self,
			cost: 0,
			setup: (_, target) =>
			{
				BattleManager.Instance.MakeHappy(target, silent: true);
				return Task.CompletedTask;
			},
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_Horse", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] whinnies happily!");
				await Task.CompletedTask;
			}
		);

		// LUCIUS //
		Skills["LUCAttack"] = new Skill(
			name: "LUCAttack",
			description: "LUCAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] tries casting a spell...");
				BattleLogManager.Instance.QueueMessage(self, "[actor] did something magical!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["LUCDoNothing"] = new Skill(
			name: "LUCDoNothing",
			description: "LUCDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_Drink", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] tries casting a spell...");
				BattleLogManager.Instance.QueueMessage("But nothing happened...");
				await Task.CompletedTask;
			}
		);

		Skills["LUCFireMagic"] = new Skill(
			name: "LUCFireMagic",
			description: "LUCFireMagic",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] tries casting a spell...");
				BattleLogManager.Instance.QueueMessage("LUCIUS sets the party on fire!");
				await AnimationManager.Instance.WaitForScreenAnimation(184, targets[0] is Enemy);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.25f, false);
			}
		);

		Skills["LUCMisfireMagic"] = new Skill(
			name: "LUCMisfireMagic",
			description: "LUCMisfireMagic",
			target: SkillTarget.Everybody,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] tries casting a spell...");
				BattleLogManager.Instance.QueueMessage("LUCIUS sets everyone on fire!");
				await AnimationManager.Instance.WaitForScreenAnimation(185, self is Enemy);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => target.CurrentHP * 0.25f, false);
			}
		);

		// MARSHA //
		Skills["MARAttack"] = new Skill(
			name: "MARAttack",
			description: "MARAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] swings her axe at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["MARDoNothing"] = new Skill(
			name: "MARDoNothing",
			description: "MARDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_Drink", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] falls over. ");
				await Task.CompletedTask;
			}
		);

		Skills["MARSpin"] = new Skill(
			name: "MARSpin",
			description: "MARSpin",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] spins quickly!");
				await Task.WhenAll(targets.Select(member => AnimationManager.Instance.WaitForAnimation(123, member)));
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["MARChop"] = new Skill(
			name: "MARChop",
			description: "MARChop",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams her axe into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
			}
		);

		// THERESE //
		Skills["THRAttack"] = new Skill(
			name: "THRAttack",
			description: "THRAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] shoots an arrow at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["THRDoNothing"] = new Skill(
			name: "THRDoNothing",
			description: "THRDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_falls_over", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] drops an arrow.");
				await Task.CompletedTask;
			}
		);

		Skills["THRSnipe"] = new Skill(
			name: "THRSnipe",
			description: "THRSnipe",
			target: SkillTarget.Enemy,
			cost: 0,
			element: "emotion",
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] snipes [target]'s weak point!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2, false);
			}
		);

		Skills["THRInsult"] = new Skill(
			name: "THRInsult",
			description: "THRInsult",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: (_, target) =>
			{
				BattleManager.Instance.MakeAngry(target, silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] calls [target] a poopy head!");
				// DIALOGUE QUIRK: always prints the tier-1 ANGRY line regardless of the tier the target reaches
				QueueTierOneEmotionLine(target, "angry", target.IsEmotionValid("angry"));
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK, false);
				AnimationManager.Instance.PlayAnimation(122, target);
				await Wait.Frames(90);
			}
		);

		Skills["THRDoubleShot"] = new Skill(
			name: "THRDoubleShot",
			description: "THRDoubleShot",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] fires two arrows!");
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
				await Wait.Frames(8);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
	}
}
