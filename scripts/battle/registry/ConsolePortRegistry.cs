using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class ConsolePortRegistry
{
	internal static void Register()
	{
		Enemies.Add("AubreyBoss", () => new AubreyBoss());
		Enemies.Add("KelBoss", () => new KelBoss());
		Enemies.Add("HeroBoss", () => new HeroBoss());
		Enemies.Add("BossmanHero", () => new BossmanHero());
		Enemies.Add("GatorGuyHero", () => new GatorGuyHero());
		Enemies.Add("Mari", () => new Mari());
		Enemies.Add("HangingBody (Mari)", () => new HangingBodyMari());

		// Aubrey Boss //
		Skills["ABossLookAtKel"] = new Skill(
			name: "ABossLookAtKel",
			description: "ABossLookAtKel",
			target: SkillTarget.Ally,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayAnimation(218, self);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks at [target].");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] eggs [actor] on!");
				// DIALOGUE QUIRK: always prints the ANGRY and ATTACK lines, even when AUBREY is already ANGRY or her ATTACK can't go higher
				BattleManager.Instance.MakeAngry(self, silent: true);
				self.AddTierStatModifier("AttackUp", silent: true);
				BattleLogManager.Instance.QueueMessage(self, "[actor] feels ANGRY!\n[actor]'s ATTACK rose!!");
			},
			hidden: true
		);
		
		Skills["ABossLookAtHero"] = new Skill(
			name: "ABossLookAtHero",
			description: "ABossLookAtHero",
			target: SkillTarget.Ally,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayAnimation(212, self);
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayAnimation(218, self);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks at [target].");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] tells [actor] to focus!");
				self.Heal(500);
				// DIALOGUE QUIRK: always prints the HAPPY and DEFENSE lines, even when AUBREY is already HAPPY or her DEFENSE can't go higher
				BattleManager.Instance.MakeHappy(self, silent: true);
				self.AddTierStatModifier("DefenseUp", silent: true);
				BattleLogManager.Instance.QueueMessage(self, "[actor] feels HAPPY!\n[actor]'s DEFENSE rose!!");
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
					await Wait.Milliseconds(1000);
				}
			}
		);
		
		Skills["ABossTwirl"] = new Skill(
			name: "ABossTwirl",
			description: "ABossTwirl",
			target: SkillTarget.Enemy,
			cost: 10,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				AnimationManager.Instance.PlayAnimation(45, target);
				await Wait.Milliseconds(500);
				AnimationManager.Instance.PlayAnimation(28, target);
				await Wait.Milliseconds(500);
				int damage = BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK * 2f + self.CurrentStats.LCK) - target.CurrentStats.DEF, false);
				if (damage > -1)
				{
					BattleManager.Instance.MakeHappy(self, silent: true);
					// DIALOGUE QUIRK: prints the HAPPY line even when AUBREY was already HAPPY
					if (self.CurrentEmotion.Id is "happy" or "ecstatic")
						BattleLogManager.Instance.QueueMessage(self.CurrentEmotion.FeelsMessage(self.Name));
				}

			}
		);
		
		Skills["ABossLookAtKelAttack"] = new Skill(
			name: "ABossLookAtKelAttack",
			description: "ABossLookAtKelAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Skills["AAttack"].Effect(self, [target]);
				BattleManager.Instance.QueueForceCommand(self, BattleManager.Instance.GetAllAliveEnemies().FirstOrDefault(x => x is KelBoss) ?? self,
					Skills["ABossLookAtKel"]);
				await Wait.Milliseconds(1000);
			},
			hidden: true
		);

		Skills["ABossLookAtHeroAttack"] = new Skill(
			name: "ABossLookAtHeroAttack",
			description: "ABossLookAtHeroAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Skills["AAttack"].Effect(self, [target]);
				BattleManager.Instance.QueueForceCommand(self, BattleManager.Instance.GetAllAliveEnemies().FirstOrDefault(x => x is HeroBoss) ?? self,
					Skills["ABossLookAtHero"]);
				await Wait.Milliseconds(1000);
			},
			hidden: true
		);

		// Kel Boss //
		Skills["KBossPassToAubreyAttack"] = new Skill(
			name: "KBossPassToAubreyAttack",
			description: "KBossPassToAubreyAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Skills["KAttack"].Effect(self, [target]);
				BattleManager.Instance.QueueForceCommand(self, BattleManager.Instance.SmoothPartyTarget(self.LastTargetIndex),
					Skills["KBossPassToAubrey"]);
				await Wait.Milliseconds(1000);
			},
			hidden: true
		);
		
		Skills["KBossPassToAubrey"] = new Skill(
			name: "KBossPassToAubrey",
			description: "KBossPassToAubrey",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForAnimation(67, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF, false,
					silent: true);
				BattleLogManager.Instance.QueueMessage(self, "[actor] passes to AUBREY.");
				BattleLogManager.Instance.QueueMessage("AUBREY knocks the ball out of the park!");
			},
			hidden: true
		);
		
		Skills["KBossPassToHeroAttack"] = new Skill(
			name: "KBossPassToHeroAttack",
			description: "KBossPassToHeroAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Skills["KAttack"].Effect(self, [target]);
				BattleManager.Instance.QueueForceCommand(self, BattleManager.Instance.GetAlivePartyMembers(),
					Skills["KBossPassToHero"]);
				await Wait.Milliseconds(1000);
			},
			hidden: true
		);
		
		Skills["KBossPassToHero"] = new Skill(
			name: "KBossPassToHero",
			description: "KBossPassToHero",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(339, false);
				foreach (Actor member in targets)
					BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false,
						silent: true);
				BattleLogManager.Instance.QueueMessage(self, "[actor] passes to HERO.");
				BattleLogManager.Instance.QueueMessage(self, "[actor] dunks on the foes!");
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
			}
		);
		
		Skills["KBossRainCloud"] = new Skill(
			name: "KBossRainCloud",
			description: "KBossRainCloud",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses RAINCLOUD!");
				foreach (Actor enemy in targets)
					AnimationManager.Instance.PlayAnimation(278, enemy);
				await Wait.Milliseconds(1000);		
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.MakeSad(enemy, silent: true);
					if (enemy.CurrentEmotion.Id == "sad")
						BattleLogManager.Instance.QueueMessage(enemy, "[actor] feels SAD.");
				}
			},
			hidden: true
		).WithTiersFromFirstTarget();
		
		Skills["KBossCurveball"] = new Skill(
			name: "KBossCurveball",
			description: "KBossCurveball",
			target: SkillTarget.Enemy,
			cost: 20,
			setup: async (_, target) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(73, target is Enemy);
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayAnimation(67, target);
				await Wait.Milliseconds(500);
				// VANILLA BUG: the console's CURVEBALL rolls its emotion without setting the state its log text reads, so no emotion line is shown
				BattleManager.Instance.RandomEmotion(target, silent: true);
			},
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws a curveball...\n[target] is thrown for a loop.");
				if (target.CurrentEmotion.Id != "neutral")
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, false);
				else
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false);
				await Task.CompletedTask;
			},
			hidden: true
		);

		// Hero Boss //
		Skills["HBossCallAubreyAttack"] = new Skill(
			name: "HBossCallAubreyAttack",
			description: "HBossCallAubreyAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Skills["HAttack"].Effect(self, [target]);
				BattleManager.Instance.QueueForceCommand(self, BattleManager.Instance.SmoothPartyTarget(self.LastTargetIndex),
					Skills["HBossCallAubrey"]);
				await Wait.Milliseconds(1000);
			},
			hidden: true
		);
		
		Skills["HBossCallAubrey"] = new Skill(
			name: "HBossCallAubrey",
			description: "HBossCallAubrey",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Wait.Milliseconds(1000);
				List<Enemy> enemies = BattleManager.Instance.GetAllAliveEnemies();
				Enemy aubrey = enemies.FirstOrDefault(x => x is AubreyBoss) ?? enemies.FirstOrDefault(x => x != self);
				if (aubrey != null)
				{
					AnimationManager.Instance.PlayAnimation(212, aubrey);
					aubrey.Heal(500);
				}
				await AnimationManager.Instance.WaitForAnimation(28, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false,
					silent: true);
				string partner = aubrey?.Name.ToUpper() ?? "AUBREY";
				// DIALOGUE QUIRK: prints the HAPPY and DEFENSE lines, but the skill only heals AUBREY
				BattleLogManager.Instance.QueueMessage(self, $"[actor] tells {partner} to focus.");
				BattleLogManager.Instance.QueueMessage($"{partner} feels HAPPY!");
				BattleLogManager.Instance.QueueMessage($"{partner}'s DEFENSE rose!!");
			},
			hidden: true
		);
		
		Skills["HBossCallKelAttack"] = new Skill(
			name: "HBossCallKelAttack",
			description: "HBossCallKelAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Skills["HAttack"].Effect(self, [target]);
				BattleManager.Instance.QueueForceCommand(self, BattleManager.Instance.SmoothPartyTarget(self.LastTargetIndex),
					Skills["HBossCallKel"]);
				await Wait.Milliseconds(1000);
			},
			hidden: true
		);
		
		Skills["HBossCallKel"] = new Skill(
			name: "HBossCallKel",
			description: "HBossCallKel",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Wait.Milliseconds(1000);
				List<Enemy> enemies = BattleManager.Instance.GetAllAliveEnemies();
				Enemy kel = enemies.FirstOrDefault(x => x is KelBoss) ?? enemies.FirstOrDefault(x => x != self);
				if (kel != null)
				{
					kel.Heal(500);
					AnimationManager.Instance.PlayAnimation(212, kel);
				}
				await AnimationManager.Instance.WaitForAnimation(54, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false,
					silent: true);
				BattleLogManager.Instance.QueueMessage(self, $"[actor] calls {kel?.Name.ToUpper() ?? "KEL"}.");
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
				await Wait.Milliseconds(333);
				await AnimationManager.Instance.WaitForAnimation(219, target);
				target.AddTierStatModifier("AttackDown");
			}
		);
		
		Skills["HBossDazzle"] = new Skill(
			name: "HBossDazzle",
			description: "HBossDazzle",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, targets) =>
			{
				await AnimationManager.Instance.WaitForAnimation(335, self);
				await Wait.Milliseconds(500);
				foreach (Actor member in targets)
					AnimationManager.Instance.PlayAnimation(276, member);
				await Wait.Milliseconds(500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(219, member);
					if (member is not PartyMember partyMember || partyMember.Component.Position is not (0 or 2))
						continue;
					BattleLogManager.Instance.QueueMessage(self, member, "[actor] smiles at [target]!");
					member.AddTierStatModifier("AttackDown");
					BattleManager.Instance.MakeHappy(member);
				}
			},
			priority: 1000
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
				await Wait.Milliseconds(1000);
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
				BattleLogManager.Instance.QueueMessage(self, "[actor] used coffee.");
				// DIALOGUE QUIRK: prints the SPEED line even when the stat is already at its limit
				target.AddTierStatModifier("SpeedUp", 3, silent: true);
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s SPEED rose!");
				await AnimationManager.Instance.WaitForAnimation(218, target);
				BattleManager.Instance.HealJuice(self, target, () => target.CurrentStats.MaxJuice * 0.1, 0.2f);
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
			}
		).WithCustomRequirement(_ => true);
		
		Skills["BMHThrowMoney"] = new Skill(
			name: "BMHThrowMoney",
			description: "BMHThrowMoney",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(343, target);
				BattleLogManager.Instance.QueueMessage(self,"[actor] throws a bag of CLAMS.");
				BattleManager.Instance.Damage(self, target, () => 200, false, 0f, neverCrit: true);
				await BattleLogManager.Instance.WaitForBattleLog();
				DialogueManager.Instance.QueueMessage(self.CenterPoint, (BattleManager.Instance.GetPartyMember(0)?.Name.ToUpper() ?? "OMORI") + " picked up 200 CLAMS!");
				await DialogueManager.Instance.WaitForDialogue();
				AudioManager.Instance.PlaySFX("SYS_cha_ching", volume: 0.9f);
			}
		);
		
		Skills["BMHFlingMoney"] = new Skill(
			name: "BMHFlingMoney",
			description: "BMHFlingMoney",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] throws CLAMS at\neveryone.");
				await Wait.Milliseconds(250);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(343, member);
					BattleManager.Instance.Damage(self, member,
						() => member.CurrentStats.MaxHP * 0.2f, false);
				}
				await BattleLogManager.Instance.WaitForBattleLog();
				DialogueManager.Instance.QueueMessage(self.CenterPoint, (BattleManager.Instance.GetPartyMember(0)?.Name.ToUpper() ?? "OMORI") + " picked up 600 CLAMS!");
				await DialogueManager.Instance.WaitForDialogue();
				AudioManager.Instance.PlaySFX("SYS_cha_ching", volume: 0.9f);
			}
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
				await Wait.Milliseconds(1500);
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
				await Wait.Milliseconds(1500);
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
				await Wait.Milliseconds(1500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(214, member);
					member.AddTierStatModifier("AttackUp", silent: true);
					member.AddTierStatModifier("DefenseUp", silent: true);
					member.AddTierStatModifier("SpeedUp", silent: true);
				}
			},
			hidden: true
		).WithTiersFromFirstTarget();
		
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
				await Wait.Milliseconds(1500);
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
				await Wait.Milliseconds(1500);
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
				await Wait.Milliseconds(1500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(219, member);
					member.AddTierStatModifier("AttackDown", silent: true);
					member.AddTierStatModifier("DefenseDown", silent: true);
					member.AddTierStatModifier("SpeedDown", silent: true);
				}
			},
			hidden: true
		).WithTiersFromFirstTarget();
		
		Skills["BMHHappyFriends"] = new Skill(
			name: "BMHHappyFriends",
			description: "BMHHappyFriends",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes you HAPPY!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Wait.Milliseconds(1500);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.MakeHappy(member);
				}
			},
			hidden: true
		).WithTiersFromFirstTarget();
		
		Skills["BMHHappyFoes"] = new Skill(
			name: "BMHHappyFoes",
			description: "BMHHappyFoes",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes the foes HAPPY!");
				foreach (Actor member in targets)
				{
					if (member == self) continue;
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Wait.Milliseconds(1500);
				foreach (Actor member in targets)
				{
					if (member == self) continue;
					BattleManager.Instance.MakeHappy(member);
				}
			},
			hidden: true
		).WithTiersFromFirstTarget();
		
		Skills["BMHSadFriends"] = new Skill(
			name: "BMHSadFriends",
			description: "BMHSadFriends",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes you SAD!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(331, member);
				}
				await Wait.Milliseconds(1500);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.MakeSad(member);
				}
			},
			hidden: true
		).WithTiersFromFirstTarget();
		
		Skills["BMHSadFoes"] = new Skill(
			name: "BMHSadFoes",
			description: "BMHSadFoes",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes the foes SAD!");
				foreach (Actor member in targets)
				{
					if (member == self) continue;
					AnimationManager.Instance.PlayAnimation(331, member);
				}
				await Wait.Milliseconds(1500);
				foreach (Actor member in targets)
				{
					if (member == self) continue;
					BattleManager.Instance.MakeSad(member);
				}
			},
			hidden: true
		).WithTiersFromFirstTarget();
		
		Skills["BMHAngryFriends"] = new Skill(
			name: "BMHAngryFriends",
			description: "BMHAngryFriends",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes you ANGRY!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Wait.Milliseconds(1500);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.MakeAngry(member);
				}
			},
			hidden: true
		).WithTiersFromFirstTarget();
		
		Skills["BMHAngryFoes"] = new Skill(
			name: "BMHAngryFoes",
			description: "BMHAngryFoes",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,"[actor] makes the foes ANGRY!");
				foreach (Actor member in targets)
				{
					if (member == self) continue;
					AnimationManager.Instance.PlayAnimation(330, member);
				}
				await Wait.Milliseconds(1500);
				foreach (Actor member in targets)
				{
					if (member == self) continue;
					BattleManager.Instance.MakeAngry(member);
				}
			},
			hidden: true
		).WithTiersFromFirstTarget();
		
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
				await Wait.Milliseconds(1500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(214, member);
				}

				foreach (Enemy enemy in BattleManager.Instance.GetAllAliveEnemies())
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
				await Wait.Milliseconds(1500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(218, member);
				}

				foreach (PartyMember member in BattleManager.Instance.GetAllPartyMembers())
				{
					member.AddStatModifier("Tickle", 2);
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
				await Wait.Milliseconds(1500);
				foreach (Actor member in targets)
				{
					// DIALOGUE QUIRK: the deal promises 50% of max HEART, but this takes 50% of current HEART
					BattleManager.Instance.Damage(self, member, () => member.CurrentHP * 0.5f, neverCrit: true);
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
				await Wait.Milliseconds(1500);
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
			effect: async (Actor _, IReadOnlyList<Actor> _) =>
			{
				BattleManager.Instance.AddItem("Whole Pizza", 10);
				await Task.CompletedTask;
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
				await Wait.Milliseconds(1500);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Heal(self, member, () => 2500, variance: 0);
					AnimationManager.Instance.PlayAnimation(216, member);
				}
			},
			hidden: true
		);

		// MARI //
		Skills["MARMariNothing"] = new Skill(
			name: "MARMariNothing",
			description: "MARMariNothing",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: (Actor self, Actor _) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] smiles at you.");
				return Task.CompletedTask;
			},
			priority: 2000
		);

		Skills["MARPicnicBasket"] = new Skill(
			name: "MARPicnicBasket",
			description: "MARPicnicBasket",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (Actor self, Actor _) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] pulls out fresh fruit from");
				BattleLogManager.Instance.QueueMessage("her picnic basket.");
				await BattleLogManager.Instance.WaitForBattleLog();
				await MariPicnic();
			},
			priority: 2000
		);
	}

	private static async Task MariPicnic()
	{
		foreach (PartyMember member in BattleManager.Instance.GetAllPartyMembers())
		{
			member.RecoverAll();
			if (member.HasPlotArmor)
				member.HasUsedPlotArmor = true;
		}
		AnimationManager animation = AnimationManager.Instance;
		animation.ShowPicture(100, "white_screen", Vector2.Zero, false, Vector2.One, 0);
		animation.TintScreen(143, 143, 143, 0, 30 / 60f);
		await animation.WaitForMovePicture(100, Vector2.Zero, Vector2.One, 200, 30);
		AudioManager.Instance.PlaySFX("SE_munch", 1f, 1f);
		await Wait.Frames(20);
		AudioManager.Instance.PlaySFX("SE_munch", 1f, 1f);
		await Wait.Frames(20);
		AudioManager.Instance.PlaySFX("SE_munch", 1f, 1f);
		await Wait.Frames(60);
		AudioManager.Instance.PlaySFX("SE_Picnic", 1f, 1f);
		await Wait.Frames(100);
		AudioManager.Instance.PlaySFX("BA_Heart_Heal", 1f, 0.9f);
		await Wait.Frames(20);
		AudioManager.Instance.PlaySFX("BA_Heart_Heal", 1f, 0.9f);
		animation.TintScreen(0, 0, 0, 0, 20 / 60f);
		await animation.WaitForMovePicture(100, Vector2.Zero, Vector2.One, 0, 30);
		animation.ErasePicture(100);
		DialogueManager.Instance.QueueMessage("You and your friends feel like new!");
		await DialogueManager.Instance.WaitForDialogue();
	}
}
