using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class KelRegistry
{
	internal static void Register()
	{
		PartyMembers.Add("Kel", () => new Kel());
		PartyMembers.Add("KelRW", () => new KelRW());

		// KEL //
		Skills["KAttack"] = new Skill(
			name: "KAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForAnimation(54, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement(_ => true);
		Skills["Annoy"] = new Skill(
			name: "ANNOY",
			description: "Makes a friend or foe ANGRY.\nCost: 5",
			target: SkillTarget.AllyOrEnemy,
			cost: 5,
			setup: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] annoys [target]!");
				await AnimationManager.Instance.WaitForScreenAnimation(55, target is Enemy);
				BattleManager.Instance.MakeAngry(target);
			},
			effect: (_, _) => Task.CompletedTask
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
					BattleManager.Instance.Damage(self, enemy,
						() => self.CurrentStats.ATK * 2.5f - enemy.CurrentStats.DEF, false);
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
				await Wait.Milliseconds(500);
				AnimationManager.Instance.PlayAnimation(54, target);
				await Wait.Milliseconds(500);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target,
					() => self.CurrentStats.SPD * 1.5f - target.CurrentStats.DEF, false);
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
			priority: 1000
		).WithLastTarget(LastTargetRecord.User);

		Skills["Curveball"] = new Skill(
			name: "CURVEBALL",
			description: "Makes a foe feel a random EMOTION. Deals\nextra damage to foes with EMOTION. Cost: 20",
			target: SkillTarget.Enemy,
			cost: 20,
			effect: async (self, target) =>
			{
				EmotionGroup rolled = RollRandomEmotionGroup();
				AnimationManager.Instance.PlayScreenAnimation(73, target is Enemy);
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayAnimation(67, target);
				await Wait.Milliseconds(500);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws a curveball...\n[target] is thrown for a loop.");
				if (rolled != null && TryGetEmotionByGroupTier(rolled.Id, 1, out Emotion added))
				{
					if (target.CurrentEmotion.Group != rolled && !target.IsEmotionLocked && target.IsEmotionValid(added.Id))
						BattleLogManager.Instance.QueueMessage(added.FeelsMessage(target.Name));
					else
						BattleManager.Instance.ShowEmotionNoEffect(target, rolled.Id);
				}
				if (target.CurrentEmotion.Id != "neutral")
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, false);
				else
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false);
				if (rolled != null)
					BattleManager.Instance.MakeEmotionTier(target, rolled.Id, 1, true);
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
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false, 0.3f);
				await Wait.Milliseconds(1000);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false, 0.3f);
				await Wait.Milliseconds(1000);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false, 0.3f);
			}
		);

		Skills["Megaphone"] = new Skill(
			name: "MEGAPHONE",
			description: "Makes all friends ANGRY.\nCost: 45",
			target: SkillTarget.AllAllies,
			cost: 45,
			setup: async (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(74, targets[0] is Enemy);
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForScreenAnimation(55, targets[0] is Enemy);
				BattleLogManager.Instance.QueueMessage(self, "[actor] runs around and annoys everyone!");
				foreach (Actor member in targets)
				{
					BattleManager.Instance.MakeAngry(member, silent: true);
					// DIALOGUE QUIRK: prints the tier line even when the friend was already FURIOUS
					if (member.CurrentEmotion.Group?.Id == "angry")
						BattleLogManager.Instance.QueueMessage(member.CurrentEmotion.FeelsMessage(member.Name));
				}
			},
			effect: (_, _) => Task.CompletedTask
		);

		Skills["Rally"] = new Skill(
			name: "RALLY",
			description: "[actor] becomes HAPPY. [actor]'s friends recover\nsome ENERGY and JUICE. Cost: 50",
			target: SkillTarget.AllAllies,
			cost: 50,
			setup: (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(61, targets[0] is Enemy);
				BattleLogManager.Instance.QueueMessage(self, "[actor] gets everyone pumped up!");
				BattleManager.Instance.MakeHappy(self);
				return Task.CompletedTask;
			},
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage("Everyone gains ENERGY!");
				BattleManager.Instance.AddEnergy(4);
				foreach (Actor member in targets.Where(member => member != self))
					AnimationManager.Instance.PlayAnimation(213, member);
				IEnumerable<Actor> friends = self is PartyMember ? BattleManager.Instance.GetAllPartyMembers() : targets;
				foreach (Actor member in friends.Where(member => member != self))
				{
					int juice = member.HealJuice(BattleManager.PercentOf(member.CurrentStats.MaxJuice, 30));
					if (juice > 0)
						BattleLogManager.Instance.QueueMessage(self, member, $"[target] recovers {juice} JUICE...");
				}

				await Wait.Milliseconds(500);
			}
		).WithLastTarget(LastTargetRecord.User);

		Skills["Comeback"] = new Skill(
			name: "COMEBACK",
			description: "Makes [actor] HAPPY. If SAD was removed,\n[actor] gains FLEX. Cost: 25",
			target: SkillTarget.Self,
			cost: 25,
			setup: async (self, target) =>
			{
				if (target.CurrentEmotion.Group?.Id == "sad")
				{
					AnimationManager.Instance.PlayAnimation(76, target);
					await Wait.Milliseconds(1000);
					target.AddStatModifier("Flex");
					AnimationManager.Instance.PlayAnimation(214, target);
				}
				else
				{
					AnimationManager.Instance.PlayAnimation(75, target);
				}

				// DIALOGUE QUIRK: reuses PEP TALK's battle text, so the user cheers themselves on
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] cheers on [target]!");
				BattleManager.Instance.MakeHappy(target);
			},
			effect: (_, _) => Task.CompletedTask
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
				BattleLogManager.Instance.QueueMessage(self, target,
					"[actor] passes the COCONUT to [target]!");
				AnimationManager.Instance.PlayAnimation(123, target);
				// juice me can miss
				BattleManager.Instance.Damage(self, target, () => target.CurrentHP * .25f, new DamageOptions
				{
					NeverMiss = false,
					Variance = 0f,
					Crit = CritMode.Never,
					ShowJuiceLoss = false,
					OnHit = () =>
					{
						int juice = BattleManager.Instance.DebugScale((int)Math.Floor(target.CurrentStats.MaxJuice * 0.4));
						if (juice > 0)
							BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovers {juice} JUICE...");
						target.HealJuice(juice, applyDebugScale: false);
						if (juice > 0)
							BattleManager.Instance.SpawnDamageNumber(juice, target, DamageType.JuiceGain);
					}
				});

				await Task.CompletedTask;
			}
		);

		Skills["Snowball"] = new Skill(
			name: "SNOWBALL",
			description: "Makes a foe SAD.\nAlso deals big damage to SAD foes. Cost: 20",
			target: SkillTarget.Enemy,
			cost: 20,
			setup: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(60, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws a SNOWBALL at\n[target]!");
				// DIALOGUE QUIRK: always prints the tier-1 SAD line regardless of the tier the target reaches
				QueueTierOneEmotionLine(target, "sad", BattleManager.Instance.MakeSad(target, silent: true));
			},
			effect: async (self, target) =>
			{
				if (target.CurrentEmotion.Group?.Id == "sad")
				{
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, false);
				}
				else
				{
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF, false);
				}
				await Task.CompletedTask;
			}
		);

		Skills["Flex"] = new Skill(
			name: "FLEX",
			description: "[actor] deals more damage next turn and increases HIT RATE for their next attack. Cost: 10",
			target: SkillTarget.Self,
			cost: 10,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(57, true);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] flexes and feels their best!");
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
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement(_ => true);

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
				await Wait.Milliseconds(1000);
				// VANILLA BUG: measures the "\n[8]" code instead of the name, so this line never wraps
				if (!AddTierThreeStatModifier(first, "AttackUp", true))
					BattleLogManager.Instance.QueueMessage(first, "[actor]'s ATTACK can't go any higher!");
			}
		);
		Skills["PassToOmori1"] = new Skill(
			name: "Pass To Omori 1",
			description: "Kel Followup",
			target: SkillTarget.Ally,
			cost: 0,
			setup: async (self, _) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] passes to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				AnimationManager.Instance.PlayScreenAnimation(62, false);
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.QueueMessage(self, first, "[target] wasn't looking and gets bopped!");
				BattleManager.Instance.MakeEmotionTier(first, "sad", 1, true);
				// vanilla implements this bop as a hardcoded notetag hit, so it grants no energy
				int bop = first.Damage(1);
				BattleManager.Instance.SpawnDamageNumber(bop, first);
				BattleLogManager.Instance.QueueMessage(self, first, $"[target] takes {bop} damage!");
			},
			effect: (Actor _, Actor _) => Task.CompletedTask,
			hidden: true
		).WithCustomRequirement(_ => true);
		Skills["PassToOmori2"] = new Skill(
			name: "Pass To Omori 2",
			description: "Kel Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] passes to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(63, false);
				BattleLogManager.Instance.QueueMessage(self, first, "[target] catches [actor]'s ball!");
				BattleLogManager.Instance.QueueMessage(first, target, "[actor] throws the ball at\n[target]!");
				QueueEmotionTierLine(first, "happy", 1);
				BattleManager.Instance.Damage(self, target,
					() => (first.CurrentStats.ATK * 1.5f) + (self.CurrentStats.ATK * 1.5f) - target.CurrentStats.DEF,
					false);
				BattleManager.Instance.MakeEmotionTier(first, "happy", 1, true);
			},
			hidden: true
		).WithCustomRequirement(_ => true);
		Skills["PassToOmori3"] = new Skill(
			name: "Pass To Omori 3",
			description: "Kel Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] passes to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(64, false);
				BattleLogManager.Instance.QueueMessage(self, first, "[target] catches [actor]'s ball!");
				BattleLogManager.Instance.QueueMessage(first, target, "[actor] throws the ball at\n[target]!");
				QueueEmotionTierLine(first, "happy", 2);
				BattleManager.Instance.Damage(self, target,
					() => (first.CurrentStats.ATK * 2f) + (self.CurrentStats.ATK * 2f) - target.CurrentStats.DEF,
					false);
				BattleManager.Instance.MakeEmotionTier(first, "happy", 2, true);
			},
			hidden: true
		).WithCustomRequirement(_ => true);
		Skills["PassToAubrey1"] = new Skill(
			name: "Pass To Aubrey 1",
			description: "Kel Followup",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] passes to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				AnimationManager.Instance.PlayScreenAnimation(65, true);
				await Wait.Milliseconds(2000);
				await AnimationManager.Instance.WaitForAnimation(66, target);
				BattleLogManager.Instance.QueueMessage(self, second, "[target] knocks the ball out of the park!");
				BattleManager.Instance.Damage(self, target,
					() => second.CurrentStats.ATK + self.CurrentStats.ATK - target.CurrentStats.DEF);
			},
			hidden: true
		).WithCustomRequirement(_ => true);
		Skills["PassToAubrey2"] = new Skill(
			name: "Pass To Aubrey 2",
			description: "Kel Followup",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] passes to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				AnimationManager.Instance.PlayScreenAnimation(65, true);
				await Wait.Milliseconds(2000);
				await AnimationManager.Instance.WaitForAnimation(67, target);
				BattleLogManager.Instance.QueueMessage(self, second, "[target] knocks the ball out of the park!");
				BattleManager.Instance.Damage(self, target,
					() => (second.CurrentStats.ATK * 2f) + self.CurrentStats.ATK - target.CurrentStats.DEF);
			},
			hidden: true
		).WithCustomRequirement(_ => true);
		Skills["PassToAubrey3"] = new Skill(
			name: "Pass To Aubrey 3",
			description: "Kel Followup",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] passes to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				AnimationManager.Instance.PlayScreenAnimation(79, true);
				await Wait.Milliseconds(2000);
				await AnimationManager.Instance.WaitForAnimation(68, target);
				BattleLogManager.Instance.QueueMessage(self, second, "[target] knocks the ball out of the park!");
				BattleManager.Instance.Damage(self, target,
					() => (second.CurrentStats.ATK * 2f) + (self.CurrentStats.ATK * 2f) - target.CurrentStats.DEF);
			},
			hidden: true
		).WithCustomRequirement(_ => true);
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
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(69, true);
				BattleLogManager.Instance.QueueMessage(self, third, "[actor] dunks on the foes!");
				foreach (Actor enemy in targets)
				{
					// VANILLA BUG: uses Aubrey's attack instead of Hero's
					BattleManager.Instance.Damage(self, enemy,
						() => second.CurrentStats.ATK + self.CurrentStats.ATK - enemy.CurrentStats.DEF, false);
				}
			},
			hidden: true
		).WithCustomRequirement(_ => true);
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
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(70, true);
				BattleLogManager.Instance.QueueMessage(self, third, "[actor] dunks on the foes!");
				foreach (Actor enemy in targets)
				{
					// VANILLA BUG: uses Aubrey's attack instead of Hero's
					BattleManager.Instance.Damage(self, enemy,
						() => second.CurrentStats.ATK + (self.CurrentStats.ATK * 1.5f) - enemy.CurrentStats.DEF, false);
				}
			},
			hidden: true
		).WithCustomRequirement(_ => true);
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
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(71, true);
				foreach (Actor enemy in targets)
				{
					AnimationManager.Instance.PlayAnimation(219, enemy);
					enemy.AddTierStatModifier("AttackDown", silent: true);
				}
				BattleLogManager.Instance.QueueMessage(self, third, "[actor] dunks on the foes with style!");
				BattleLogManager.Instance.QueueMessage("All foes' ATTACK fell!");
				foreach (Actor enemy in targets)
				{
					// VANILLA BUG: uses Aubrey's attack instead of Hero's
					BattleManager.Instance.Damage(self, enemy,
						() => (second.CurrentStats.ATK * 1.5f) + (self.CurrentStats.ATK * 1.5f) -
							  enemy.CurrentStats.DEF, false);
				}
			},
			hidden: true
		).WithTiersFromFirstTarget().WithCustomRequirement(_ => true);
	}

	private static void QueueEmotionTierLine(Actor who, string groupId, int tier)
	{
		if (!TryGetEmotionByGroupTier(groupId, tier, out Emotion next))
			return;
		int held = who.CurrentEmotion.Group == next.Group ? who.CurrentEmotion.Tier : 0;
		bool applies = held == tier
			? who is not Enemy && !who.IsImmuneToEmotion(next)
			: held < tier && !who.IsEmotionLocked && who.IsEmotionValid(next) && !who.IsImmuneToEmotion(next);
		if (applies)
			BattleLogManager.Instance.QueueMessage(next.FeelsMessage(who.Name));
		else
			BattleManager.Instance.ShowEmotionNoEffect(who, groupId);
	}
}
