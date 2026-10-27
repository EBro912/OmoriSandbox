using System;
using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class HeroRegistry
{
	internal static void Register()
	{
		PartyMembers.Add("Hero", () => new Hero());
		PartyMembers.Add("HeroRW", () => new HeroRW());

		// HERO //
		Skills["HAttack"] = new Skill(
			name: "HAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForAnimation(83, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement(_ => true);
		Skills["Massage"] = new Skill(
			name: "MASSAGE",
			description: "Removes a friend or foe's EMOTION.\nCost: 5",
			target: SkillTarget.AllyOrEnemy,
			cost: 5,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] massages [target]!");
				await AnimationManager.Instance.WaitForScreenAnimation(86, target is Enemy);
				target.SetEmotion("neutral", true);
				if (target.CurrentEmotion.Id == "neutral")
					BattleLogManager.Instance.QueueMessage(target.Name.ToUpper() + " calms down...");
			}
		).WithLastTarget(LastTargetRecord.None);
		Skills["Charm"] = new Skill(
			name: "CHARM",
			description: "Acts first, a foe targets [actor] for 1 turn.\nCost: 10",
			target: SkillTarget.Enemy,
			cost: 10,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] draws [target]'s\nattention.");
				RemoveObserveIgnoreTaunt();
				self.AddStatModifier("CharmCheck", silent: true);
				await AnimationManager.Instance.WaitForScreenAnimation(90, false);
				target.AddStatModifier("Charm");
				if (target.StatModifiers.TryGetValue("Charm", out StatModifier charmMod) &&
					charmMod is CharmStatModifier charmState)
					charmState.CharmedBy = self as PartyMember;
				await Wait.Milliseconds(2000);
			},
			priority: 1000
		);
		Skills["Enchant"] = new Skill(
			name: "ENCHANT",
			description: "Acts first. A foe targets [actor] for 1 turn\nand becomes HAPPY. Cost: 15",
			target: SkillTarget.Enemy,
			cost: 15,
			setup: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target,
					"[actor] draws the foe's attention\nwith a smile.");
				RemoveObserveIgnoreTaunt();
				await AnimationManager.Instance.WaitForScreenAnimation(90, false);
				target.AddStatModifier("Charm");
				if (target.StatModifiers.TryGetValue("Charm", out StatModifier enchantMod) &&
					enchantMod is CharmStatModifier enchantState)
					enchantState.CharmedBy = self as PartyMember;
				// DIALOGUE QUIRK: always prints the tier-1 HAPPY line regardless of the tier the target reaches
				QueueTierOneEmotionLine(target, "happy", BattleManager.Instance.MakeHappy(target, silent: true));
			},
			effect: async (_, _) =>
			{
				await Wait.Milliseconds(2000);
			},
			priority: 1000
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
				await Wait.Milliseconds(1000);
			},
			priority: 1000
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
				RemoveObserveIgnoreTaunt();
				await Wait.Milliseconds(1000);
			},
			priority: 1000
		);
		Skills["SpicyFood"] = new Skill(
			name: "SPICY FOOD",
			description: "Damages a foe and makes them ANGRY.\nCost: 15",
			target: SkillTarget.Enemy,
			cost: 15,
			setup: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(98, target);
				BattleManager.Instance.MakeAngry(target, silent: true);
			},
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] cooks some spicy food!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF,
					false, neverCrit: true);
				await Task.CompletedTask;
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
				await Wait.Milliseconds(332);
				AnimationManager.Instance.PlayAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] intensely massages\n[target]!");
				target.AddStatModifier("DefenseDown");
				AnimationManager.Instance.PlayAnimation(219, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 4f - target.CurrentStats.DEF,
					false);
			}
		);
		Skills["Smile"] = new Skill(
			name: "SMILE",
			description: "Acts first, reducing a foe's ATTACK.\nCost: 25",
			target: SkillTarget.Enemy,
			cost: 25,
			priority: 1000,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] smiles.");
				await AnimationManager.Instance.WaitForScreenAnimation(87, false);
				await Wait.Milliseconds(332);
				target.AddStatModifier("AttackDown");
				await AnimationManager.Instance.WaitForAnimation(219, target);
			}
		);
		Skills["Dazzle"] = new Skill(
			name: "DAZZLE",
			description: "Acts first. Reduces all foes' ATTACK and\nmakes them HAPPY. Cost: 35",
			target: SkillTarget.AllEnemies,
			cost: 35,
			priority: 1000,
			setup: async (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(90, targets[0] is Enemy);
				await Wait.Milliseconds(500);
				foreach (Actor enemy in targets)
				{
					BattleLogManager.Instance.QueueMessage(self, enemy, "[actor] smiles at [target]!");
					AnimationManager.Instance.PlayAnimation(276, enemy);
					enemy.AddTierStatModifier("AttackDown");
					// DIALOGUE QUIRK: always prints the tier-1 HAPPY line regardless of the tier the target reaches
					QueueTierOneEmotionLine(enemy, "happy", BattleManager.Instance.MakeHappy(enemy, silent: true));
					AnimationManager.Instance.PlayAnimation(219, enemy);
				}
			},
			effect: (_, _) => Task.CompletedTask
		).WithTiersFromFirstTarget();
		Skills["FastFood"] = new Skill(
			name: "FAST FOOD",
			description: "Acts first, healing a friend for 40% of\ntheir HEART. Cost: 15",
			target: SkillTarget.Ally,
			cost: 15,
			priority: 1000,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(85, target);
				int heal = target.Heal(BattleManager.PercentOf(target.CurrentStats.MaxHP, 40));
				BattleManager.Instance.SpawnDamageNumber(heal, target, DamageType.Heal);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] prepares a quick meal for [target].");
				AnimationManager.Instance.PlayAnimation(212, target);
				await Wait.Milliseconds(1000);
			}
		).WithLastTarget(LastTargetRecord.None);
		Skills["ShareFood"] = new Skill(
			name: "SHARE FOOD",
			description: "[actor] and a friend recover some HEART.\nCost: 15",
			target: SkillTarget.Ally,
			cost: 15,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(85, self);
				AnimationManager.Instance.PlayAnimation(85, target);
				int selfHeal = self.Heal(BattleManager.PercentOf(self.CurrentStats.MaxHP, 50));
				BattleManager.Instance.SpawnDamageNumber(selfHeal, self, DamageType.Heal);
				int heal = target.Heal(BattleManager.PercentOf(target.CurrentStats.MaxHP, 50));
				BattleManager.Instance.SpawnDamageNumber(heal, target, DamageType.Heal);
				if (target != self)
					BattleLogManager.Instance.QueueMessage(self, target, "[actor] shares food with [target]!");
				await Wait.Milliseconds(1000);
			}
		).WithLastTarget(LastTargetRecord.None);
		Skills["SnackTime"] = new Skill(
			name: "SNACK TIME",
			description: "Heals all friends for 40% of their HEART.\nCost: 25",
			target: SkillTarget.AllAllies,
			cost: 25,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] made snacks for everyone!");
				AnimationManager.Instance.PlayScreenAnimation(88, false);
				await Wait.Milliseconds(1666);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Heal(self, member, () => member.CurrentStats.MaxHP * 0.4, 0f);
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
					member.AddTierStatModifier("DefenseUp", silent: true);
					AnimationManager.Instance.PlayAnimation(214, member);
				}
			}
		).WithTiersFromFirstTarget();
		Skills["TeaTime"] = new Skill(
			name: "TEA TIME",
			description: "Heals some of a friend's HEART and JUICE.\nCost: 25",
			target: SkillTarget.AllyNotSelf,
			cost: 25,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(89, target);
				await Wait.Milliseconds(2000);
				int heartHeal = target.Heal(BattleManager.PercentOf(target.CurrentStats.MaxHP, 30));
				int juiceHeal = target.HealJuice((int)Math.Ceiling(target.CurrentStats.MaxJuice * 0.2));
				AnimationManager.Instance.PlayAnimation(212, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] brings out some tea for a break.");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] feels refreshed!");
				if (heartHeal > 0)
					BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovers {heartHeal} HEART!");
				if (juiceHeal > 0)
					BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovers {juiceHeal} JUICE...");
				BattleManager.Instance.SpawnDamageNumber(heartHeal, target, DamageType.Heal);
				BattleManager.Instance.SpawnDamageNumber(juiceHeal, target, DamageType.JuiceGain);
			}
		).WithLastTarget(LastTargetRecord.None);
		Skills["Cook"] = new Skill(
			name: "COOK",
			description: "Heals a friend for 75% of their HEART.\nCost: 10",
			target: SkillTarget.Ally,
			cost: 10,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(85, target);
				await AnimationManager.Instance.WaitForAnimation(212, target);
				BattleManager.Instance.Heal(self, target, () => target.CurrentStats.MaxHP * 0.75f, silent: true);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] makes a cookie just for [target]!");
				await Wait.Milliseconds(1000);
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
				BattleManager.Instance.HealJuice(self, target, () => target.CurrentStats.MaxJuice * 0.5f, silent: true);
				await Wait.Milliseconds(1000);
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
				if (!target.IsToast)
				{
					target = BattleManager.Instance.GetRandomDeadPartyMember();
					if (target == null)
					{
						BattleLogManager.Instance.QueueMessage("It had no effect.");
						return;
					}
				}

				await AnimationManager.Instance.WaitForAnimation(269, target);
				target.Revive(target.CurrentHP);
				target.SetEmotion("neutral", true);
				int heal = BattleManager.RoundHealing(target.CurrentStats.MaxHP * 0.7);
				heal = target.Heal(heal);
				BattleManager.Instance.SpawnDamageNumber(heal, target, DamageType.Heal);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {heal} HEART!");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] rose again!");
				await Wait.Milliseconds(1000);
			}
		);

		Skills["CallOmori1"] = new Skill(
			name: "Call Omori 1",
			description: "Hero Followup",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor self, Actor _) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] calls out to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(93, false);
				await AnimationManager.Instance.WaitForAnimation(212, first);
				CallHeal(self, first, "[actor] signals to [target]!", 15, 0);
				BattleManager.Instance.QueueForceCommand(first, BattleManager.Instance.SmoothEnemyTarget(first.LastTargetIndex),
					Skills["OAttack"]);
			},
			hidden: true
		).WithCustomRequirement(_ => true);
		Skills["CallOmori2"] = new Skill(
			name: "Call Omori 2",
			description: "Hero Followup",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor self, Actor _) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] calls out to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(93, false);
				await AnimationManager.Instance.WaitForAnimation(212, first);
				CallHeal(self, first, "[actor] signals to [target]!", 25, 10);
				BattleManager.Instance.QueueForceCommand(first, BattleManager.Instance.SmoothEnemyTarget(first.LastTargetIndex),
					Skills["OAttack"]);
			},
			hidden: true
		).WithCustomRequirement(_ => true);
		Skills["CallOmori3"] = new Skill(
			name: "Call Omori 3",
			description: "Hero Followup",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor self, Actor _) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] calls out to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(93, false);
				await AnimationManager.Instance.WaitForAnimation(212, first);
				CallHeal(self, first, "[actor] signals to [target]!", 40, 20);
				BattleManager.Instance.QueueForceCommand(first, BattleManager.Instance.SmoothEnemyTarget(first.LastTargetIndex),
					Skills["OAttack"]);
			},
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["CallAubrey1"] = new Skill(
			name: "Call Aubrey 1",
			description: "Hero Followup",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor self, Actor _) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] calls out to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(94, false);
				await AnimationManager.Instance.WaitForAnimation(212, second);
				CallHeal(self, second, "[actor] encourages [target]!", 15, 0);
				BattleManager.Instance.QueueForceCommand(second, BattleManager.Instance.SmoothEnemyTarget(second.LastTargetIndex),
					Skills["AAttack"]);
			},
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["CallAubrey2"] = new Skill(
			name: "Call Aubrey 2",
			description: "Hero Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (Actor self, Actor _) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] calls out to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(94, false);
				await AnimationManager.Instance.WaitForAnimation(212, second);
				CallHeal(self, second, "[actor] encourages [target]!", 25, 10);
				BattleManager.Instance.QueueForceCommand(second, BattleManager.Instance.SmoothEnemyTarget(second.LastTargetIndex),
					Skills["AAttack"]);
			},
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["CallAubrey3"] = new Skill(
			name: "Call Aubrey 3",
			description: "Hero Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (Actor self, Actor _) =>
			{
				PartyMember second = BattleManager.Instance.GetPartyMemberAtPosition(1);
				BattleLogManager.Instance.QueueMessage(self, second, "[actor] calls out to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(94, false);
				await AnimationManager.Instance.WaitForAnimation(212, second);
				CallHeal(self, second, "[actor] encourages [target]!", 40, 20);
				BattleManager.Instance.QueueForceCommand(second, BattleManager.Instance.SmoothEnemyTarget(second.LastTargetIndex),
					Skills["AAttack"]);
			},
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["CallKel1"] = new Skill(
			name: "Call Kel 1",
			description: "Hero Followup",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor self, Actor _) =>
			{
				PartyMember fourth = BattleManager.Instance.GetPartyMemberAtPosition(2);
				BattleLogManager.Instance.QueueMessage(self, fourth, "[actor] calls out to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(95, false);
				await AnimationManager.Instance.WaitForAnimation(212, fourth);
				CallHeal(self, fourth, "[actor] psyches up [target]!", 15, 0);
				BattleManager.Instance.QueueForceCommand(fourth, BattleManager.Instance.SmoothEnemyTarget(fourth.LastTargetIndex),
					Skills["KAttack"]);
				if (fourth.CurrentEmotion.Id is "sad" or "depressed")
					fourth.SetEmotion("neutral", true);
			},
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["CallKel2"] = new Skill(
			name: "Call Kel 2",
			description: "Hero Followup",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor self, Actor _) =>
			{
				PartyMember fourth = BattleManager.Instance.GetPartyMemberAtPosition(2);
				BattleLogManager.Instance.QueueMessage(self, fourth, "[actor] calls out to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(95, false);
				await AnimationManager.Instance.WaitForAnimation(212, fourth);
				CallHeal(self, fourth, "[actor] psyches up [target]!", 25, 10);
				BattleManager.Instance.QueueForceCommand(fourth, BattleManager.Instance.SmoothEnemyTarget(fourth.LastTargetIndex),
					Skills["KAttack"]);
				if (fourth.CurrentEmotion.Id is "sad" or "depressed")
					fourth.SetEmotion("neutral", true);
			},
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["CallKel3"] = new Skill(
			name: "Call Kel 3",
			description: "Hero Followup",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor self, Actor _) =>
			{
				PartyMember fourth = BattleManager.Instance.GetPartyMemberAtPosition(2);
				BattleLogManager.Instance.QueueMessage(self, fourth, "[actor] calls out to [target].");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(95, false);
				await AnimationManager.Instance.WaitForAnimation(212, fourth);
				CallHeal(self, fourth, "[actor] psyches up [target]!", 40, 20);
				BattleManager.Instance.QueueForceCommand(fourth, BattleManager.Instance.SmoothEnemyTarget(fourth.LastTargetIndex),
					Skills["KAttack"]);
				if (fourth.CurrentEmotion.Id is "sad" or "depressed")
					fourth.SetEmotion("neutral", true);
			},
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["HRWAttack"] = new Skill(
			name: "Attack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(99, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement(_ => true);

		Skills["FirstAid"] = new Skill(
			name: "FIRST AID",
			description: "Heals a friend for 25% of their HEART.\nCost: 10",
			target: SkillTarget.Ally,
			cost: 10,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(114, target);
				AnimationManager.Instance.PlayAnimation(212, target);
				int heal = target.Heal(BattleManager.PercentOf(target.CurrentStats.MaxHP, 25));
				BattleManager.Instance.SpawnDamageNumber(heal, target, DamageType.Heal);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] tends to [target]!");
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovers {heal} HEART!");
				await Wait.Milliseconds(1000);
			}
		).WithLastTarget(LastTargetRecord.None);
	}

	private static void RemoveObserveIgnoreTaunt()
	{
		foreach (Enemy enemy in BattleManager.Instance.GetAllEnemies())
		{
			enemy.RemoveStatModifier("ObserveIgnoreTaunt");
			enemy.ClearResult();
		}
	}

	private static void CallHeal(Actor self, PartyMember member, string line, int hpPercent, int juicePercent)
	{
		int hpBefore = member.CurrentHP;
		int juiceBefore = member.CurrentJuice;
		member.Heal(BattleManager.PercentOf(member.CurrentStats.MaxHP, hpPercent));
		if (juicePercent > 0)
			member.HealJuice(BattleManager.PercentOf(member.CurrentStats.MaxJuice, juicePercent));
		BattleLogManager.Instance.QueueMessage(self, member, line);
		int heal = member.CurrentHP - hpBefore;
		if (heal > 0)
			BattleLogManager.Instance.QueueMessage(self, member, $"[target] recovers {heal} HEART!");
		int juice = member.CurrentJuice - juiceBefore;
		// VANILLA BUG: the JUICE line is skipped when the target had no JUICE before the heal
		if (juicePercent > 0 && juiceBefore > 0 && juice > 0)
			BattleLogManager.Instance.QueueMessage(self, member, $"[target] recovers {juice} JUICE...");
	}
}
