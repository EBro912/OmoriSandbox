using System;
using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class AubreyRegistry
{
	internal static void Register()
	{
		PartyMembers.Add("Aubrey", () => new Aubrey());
		PartyMembers.Add("AubreyRW", () => new AubreyRW());

		// AUBREY //
		Skills["AAttack"] = new Skill(
			name: "AAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForAnimation(28, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement(_ => true);
		Skills["PepTalk"] = new Skill(
			name: "PEP TALK",
			description: "Makes a friend or foe HAPPY.\nCost: 5",
			target: SkillTarget.AllyOrEnemy,
			cost: 5,
			setup: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] cheers on [target]!");
				await AnimationManager.Instance.WaitForScreenAnimation(29, target is Enemy);
				QueueEmotionLine(target, "happy", BattleManager.Instance.MakeHappy(target, silent: true));
			},
			effect: (_, _) => Task.CompletedTask
		);
		Skills["Headbutt"] = new Skill(
			name: "HEADBUTT",
			description: "Deals big damage, but [actor] also takes damage.\nStronger when [actor] is ANGRY. Cost: 5",
			target: SkillTarget.Enemy,
			cost: 5,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(30, target is Enemy);
				await Wait.Milliseconds(1500);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] headbutts [target]!");
				// vanilla intended behavior: FURIOUS is excluded from the bonus, so only the first two tiers count
				int damage;
				if (self.GetEmotionTier("angry") is 1 or 2)
					damage = BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, false);
				else
					damage = BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF, false);
				if (damage > -1)
					self.CurrentHP = (int)Math.Max(1f, self.CurrentHP - Math.Floor(self.CurrentStats.MaxHP * 0.2));
			}
		).WithCustomRequirement(actor =>
		{
			if (actor.CurrentEmotion.BlocksActions)
				return false;
			double neededHp = Math.Floor(actor.CurrentStats.MaxHP * 0.2d);
			return actor.CurrentHP >= neededHp;
		}).WithRequirementFailureMessage("[actor] does not have enough HP!");

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
				target.AddStatModifier("CharmCheck", silent: true);
				await Task.CompletedTask;
			},
			priority: 2000
		);

		Skills["CounterAttack"] = new Skill(
			name: "CounterAttack",
			description: "Counter Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] swings back!");
				await AnimationManager.Instance.WaitForAnimation(28, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false);
			},
			hidden: true
		// COUNTER itself is gated on afraid/stressed, the forced counterattack must still fire
		).WithCustomRequirement(_ => true);

		Skills["PowerHit"] = new Skill(
			name: "POWER HIT",
			description: "An attack that ignore's a foe's DEFENSE,\nthen reduces the foe's DEFENSE. Cost: 20",
			target: SkillTarget.Enemy,
			cost: 20,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(31, target);
				await Wait.Milliseconds(1000);
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
				await Wait.Milliseconds(500);
				AnimationManager.Instance.PlayAnimation(28, target);
				await Wait.Milliseconds(500);
				int damage = BattleManager.Instance.Damage(self, target,
					() => (self.CurrentStats.ATK * 2f + self.CurrentStats.LCK) - target.CurrentStats.DEF, false);
				if (damage > -1)
				{
					BattleManager.Instance.MakeHappy(self);
				}

			}
		);

		Skills["MoodWrecker"] = new Skill(
			name: "MOOD WRECKER",
			description: "A swing that doesn't miss. Deals extra damage to\nHAPPY foes. Cost: 10",
			target: SkillTarget.Enemy,
			cost: 10,
			// vanilla adds ACCURACY UP in the setup action, before the hit applies, so a use right after it expired can be refused
			setup: (self, _) =>
			{
				self.AddStatModifier("AccuracyUp", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(46, target);
				await Wait.Milliseconds(500);
				if (target.IsFeeling("happy"))
				{
					if (target.GetEmotionTier("happy") >= 2)
						await AnimationManager.Instance.WaitForAnimation(279, target);
					else
						await AnimationManager.Instance.WaitForAnimation(278, target);
					BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, neverMiss: false);
				}
				else
				{
					BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 2.25f - target.CurrentStats.DEF, neverMiss: false);
				}
			}
		);

		Skills["TeamSpirit"] = new Skill(
			name: "TEAM SPIRIT",
			description: "Makes [actor] and a friend HAPPY.\nCost: 10",
			target: SkillTarget.AllyNotSelf,
			cost: 10,
			setup: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] cheers on [target]!");
				AnimationManager.Instance.PlayAnimation(49, self);
				await Wait.Milliseconds(500);
				AnimationManager.Instance.PlayScreenAnimation(29, target is Enemy);
				bool targetApplied = BattleManager.Instance.MakeHappy(target, silent: true);
				bool selfApplied = BattleManager.Instance.MakeHappy(self, silent: true);
				if (target == self)
					targetApplied = selfApplied = targetApplied && selfApplied;
				QueueEmotionLine(target, "happy", targetApplied);
				QueueEmotionLine(self, "happy", selfApplied);
			},
			effect: (_, _) => Task.CompletedTask
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
							BattleManager.Instance.Damage(self, enemy,
								() => self.CurrentStats.ATK * 3f - enemy.CurrentStats.DEF, false);
							break;
						case 2:
							BattleManager.Instance.Damage(self, enemy,
								() => self.CurrentStats.ATK * 2.5f - enemy.CurrentStats.DEF, false);
							break;
						default:
							BattleManager.Instance.Damage(self, enemy,
								() => self.CurrentStats.ATK * 2f - enemy.CurrentStats.DEF, false);
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
				await Wait.Milliseconds(500);
				AnimationManager.Instance.PlayAnimation(213, target);
				await Wait.Milliseconds(500);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target,
					() => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF, false);
				if (target.CurrentHP == 0)
				{
					AnimationManager.Instance.PlayAnimation(213, self);
					self.HealJuice(self.CurrentStats.MaxJuice);
					BattleManager.Instance.SpawnDamageNumber(self.CurrentStats.MaxJuice, self,
						DamageType.JuiceGain);
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
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false);
					await Wait.Milliseconds(1000);
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
				BattleLogManager.Instance.QueueMessage(self, target,
					"[actor] strikes [target]\nwith all her strength!");
				int damage = BattleManager.Instance.Damage(self, target, () => self.CurrentHP * 4f, false);
				if (damage > -1)
				{
					BattleManager.Instance.SpawnDamageNumber(self.CurrentHP, self);
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
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(35, false);
				await AnimationManager.Instance.WaitForAnimation(28, target);
				BattleLogManager.Instance.QueueMessage(self, other,
					"[target] didn't notice [actor], so\n[actor] attacks again!");
				BattleManager.Instance.Damage(self, target,
					() => (self.CurrentStats.ATK * 2 + self.CurrentStats.LCK) - target.CurrentStats.DEF, false);
			},
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["LookAtOmori2"] = new Skill(
			name: "Look At Omori 2",
			description: "Aubrey Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(36, false);
				await AnimationManager.Instance.WaitForAnimation(28, target);
				BattleLogManager.Instance.QueueMessage(self, other,
					"[target] still didn't notice [actor], so\n[actor] attacks harder!");
				BattleManager.Instance.Damage(self, target,
					() => (self.CurrentStats.ATK * 3 + self.CurrentStats.LCK) - target.CurrentStats.DEF, false);
			},
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["LookAtOmori3"] = new Skill(
			name: "Look At Omori 3",
			description: "Aubrey Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(37, false);
				await AnimationManager.Instance.WaitForAnimation(44, target);
				BattleLogManager.Instance.QueueMessage(self, other, "[target] finally notices [actor]!");
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] swings her bat in happiness!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 + self.CurrentStats.LCK,
					false);
			},
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["LookAtKel1"] = new Skill(
			name: "Look At Kel 1",
			description: "Aubrey Followup",
			target: SkillTarget.Self,
			cost: 0,
			setup: async (self, _) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(2);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				AnimationManager.Instance.PlayScreenAnimation(38, false);
				await Wait.Milliseconds(2000);
				BattleLogManager.Instance.QueueMessage(self, other, "[target] eggs [actor] on!");
				// DIALOGUE QUIRK: always prints the tier-1 ANGRY line, whatever tier the user reaches, even when the add changed nothing
				BattleManager.Instance.MakeAngry(self, silent: true);
				BattleLogManager.Instance.QueueMessage(self, "[actor] feels ANGRY!");
			},
			effect: (Actor _, Actor _) => Task.CompletedTask,
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["LookAtKel2"] = new Skill(
			name: "Look At Kel 2",
			description: "Aubrey Followup",
			target: SkillTarget.Self,
			cost: 0,
			setup: async (self, _) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(2);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				AnimationManager.Instance.PlayScreenAnimation(39, false);
				await Wait.Milliseconds(2000);
				BattleLogManager.Instance.QueueMessage(self, other, "[target] eggs [actor] on!");
				self.AddStatModifier("AttackUp", silent: true);
				// VANILLA BUG: says both ATTACKs rose, but only the user's ATTACK goes up
				BattleLogManager.Instance.QueueMessage(self, other, "[target]'s and [actor]'s ATTACK rose!");
				AnimationManager.Instance.PlayAnimation(214, self);
				AnimationManager.Instance.PlayAnimation(214, other);
				BattleManager.Instance.MakeAngry(self, silent: true);
				BattleManager.Instance.MakeAngry(other, silent: true);
				QueueLookAtKelLines(self, other);
			},
			effect: (Actor _, Actor _) => Task.CompletedTask,
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["LookAtKel3"] = new Skill(
			name: "Look At Kel 3",
			description: "Aubrey Followup",
			target: SkillTarget.Self,
			cost: 0,
			setup: async (self, _) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(2);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				AnimationManager.Instance.PlayScreenAnimation(40, false);
				await Wait.Milliseconds(2000);
				BattleLogManager.Instance.QueueMessage(self, other, "[target] eggs [actor] on!");
				self.AddTierStatModifier("AttackUp", 3, silent: true);
				// VANILLA BUG: says both ATTACKs rose, but only the user's ATTACK goes up
				BattleLogManager.Instance.QueueMessage(self, other, "[target]'s and [actor]'s ATTACK rose!");
				AnimationManager.Instance.PlayAnimation(214, self);
				AnimationManager.Instance.PlayAnimation(214, other);
				BattleManager.Instance.MakeEmotionTier(self, "angry", 2, true);
				BattleManager.Instance.MakeEmotionTier(other, "angry", 2, true);
				QueueLookAtKelLines(self, other);
			},
			effect: (Actor _, Actor _) => Task.CompletedTask,
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["LookAtHero1"] = new Skill(
			name: "Look At Hero 1",
			description: "Aubrey Followup",
			target: SkillTarget.Self,
			cost: 0,
			setup: async (self, _) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(3);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				AnimationManager.Instance.PlayScreenAnimation(41, false);
				await Wait.Milliseconds(2000);
				AnimationManager.Instance.PlayAnimation(214, self);
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.QueueMessage(self, other, "[target] tells [actor] to focus!");
				self.AddStatModifier("DefenseUp", silent: true);
				BattleManager.Instance.MakeHappy(self);
				// DIALOGUE QUIRK: prints the DEFENSE line even when the stat is already at its limit
				BattleLogManager.Instance.QueueMessage(self, "[actor]'s DEFENSE rose!!");
			},
			effect: (Actor _, Actor _) => Task.CompletedTask,
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["LookAtHero2"] = new Skill(
			name: "Look At Hero 2",
			description: "Aubrey Followup",
			target: SkillTarget.Self,
			cost: 0,
			setup: async (self, _) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(3);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				AnimationManager.Instance.PlayScreenAnimation(42, false);
				await Wait.Milliseconds(2000);
				AnimationManager.Instance.PlayAnimation(214, self);
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForAnimation(212, self);
				int hpBefore = self.CurrentHP;
				BattleLogManager.Instance.QueueMessage(self, other, "[target] cheers [actor]!");
				self.Heal(BattleManager.PercentOf(self.CurrentStats.MaxHP, 25));
				self.AddTierStatModifier("DefenseUp", 2);
				BattleManager.Instance.MakeHappy(self);
				int heal = self.CurrentHP - hpBefore;
				if (heal > 0)
					BattleLogManager.Instance.QueueMessage(self, $"[actor] recovers {heal} HEART!");
			},
			effect: (Actor _, Actor _) => Task.CompletedTask,
			hidden: true
		).WithCustomRequirement(_ => true);

		bool lookHero3Ecstatic = false;
		Skills["LookAtHero3"] = new Skill(
			name: "Look At Hero 3",
			description: "Aubrey Followup",
			target: SkillTarget.Self,
			cost: 0,
			setup: async (self, _) =>
			{
				PartyMember other = BattleManager.Instance.GetPartyMemberAtPosition(3);
				BattleLogManager.Instance.QueueMessage(self, other, "[actor] looks at [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				AnimationManager.Instance.PlayScreenAnimation(43, false);
				await Wait.Milliseconds(2000);
				AnimationManager.Instance.PlayAnimation(214, self);
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForAnimation(212, self);
				BattleLogManager.Instance.QueueMessage(self, other, "[target] cheers [actor]!");
				lookHero3Ecstatic = BattleManager.Instance.MakeEmotionTier(self, "happy", 2, true);
			},
			effect: (Actor self, Actor _) =>
			{
				int hpBefore = self.CurrentHP;
				int juiceBefore = self.CurrentJuice;
				self.Heal(BattleManager.PercentOf(self.CurrentStats.MaxHP, 75));
				self.HealJuice(BattleManager.PercentOf(self.CurrentStats.MaxJuice, 50));
				// DIALOGUE QUIRK: prints the success line even when the stat is already at its limit
				self.AddTierStatModifier("DefenseUp", 3);
				QueueEmotionLine(self, "happy", lookHero3Ecstatic);
				int heal = self.CurrentHP - hpBefore;
				if (heal > 0)
					BattleLogManager.Instance.QueueMessage(self, $"[actor] recovers {heal} HEART!");
				int juice = self.CurrentJuice - juiceBefore;
				// DIALOGUE QUIRK: the JUICE line is skipped when the target had no JUICE before the heal
				if (juiceBefore > 0 && juice > 0)
					BattleLogManager.Instance.QueueMessage(self, $"[actor] recovers {juice} JUICE...");
				return Task.CompletedTask;
			},
			hidden: true
		).WithCustomRequirement(_ => true);


		Skills["ARWAttack"] = new Skill(
			name: "ARWAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(48, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement(_ => true);

		Skills["Homerun"] = new Skill(
			name: "HOMERUN",
			description: "Has a chance to instantly defeat a\nfoe. [actor] also takes damage. Cost: 25",
			target: SkillTarget.Enemy,
			cost: 25,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(32, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] hits a home run!");
				int damage = BattleManager.Instance.Damage(self, target,
					() => self.CurrentStats.ATK * 4f - target.CurrentStats.DEF,
					false, neverCrit: true);
				// the instant-defeat effect only applies if the attack lands
				if (damage > -1)
				{
					if (RollLuckStateChance(self, target, 0.1))
					{
						target.Damage(target.CurrentHP);
					}
				}

				// vanilla pays the 20% HP cost up front, so the recoil applies even on a miss
				self.CurrentHP = Math.Max(0,
					(int)Math.Round(self.CurrentHP - self.CurrentStats.MaxHP * 0.2f, MidpointRounding.AwayFromZero));
			}
		).WithCustomRequirement(actor =>
		{
			if (actor.CurrentEmotion.BlocksActions)
				return false;
			// vanilla YEP requires the 20% HP cost to be strictly payable
			return actor.CurrentHP > (int)Math.Round(actor.CurrentStats.MaxHP * 0.2f, MidpointRounding.AwayFromZero);
		}).WithRequirementFailureMessage("[actor] does not have enough HP!");
	}

	private static void QueueLookAtKelLines(Actor self, Actor other)
	{
		int selfTier = self.CurrentEmotion.Group?.Id == "angry" ? self.CurrentEmotion.Tier : 0;
		int otherTier = other.CurrentEmotion.Group?.Id == "angry" ? other.CurrentEmotion.Tier : 0;
		if (selfTier is 1 or 2 && otherTier is 1 or 2 && selfTier != otherTier)
		{
			BattleLogManager.Instance.QueueMessage(other.CurrentEmotion.FeelsMessage(other.Name));
			BattleLogManager.Instance.QueueMessage(self.CurrentEmotion.FeelsMessage(self.Name));
			return;
		}
		// VANILLA BUG: any other pair gets the ANGRY line, even when one of them isn't ANGRY at all
		BattleLogManager.Instance.QueueMessage(self, other, selfTier == 2 && otherTier == 2
			? "[target] and [actor] feel ENRAGED!!"
			: "[target] and [actor] feel ANGRY!");
	}
}
