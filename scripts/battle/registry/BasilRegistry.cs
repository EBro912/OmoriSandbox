using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class BasilRegistry
{
	internal static void Register()
	{
		PartyMembers.Add("Basil", () => new Basil());

		// BASIL //
		Skills["BAttack"] = new Skill(
			name: "BAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForAnimation(142, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement(_ => true);

		Skills["BodySlam"] = new Skill(
			name: "BODY SLAM",
			description: "Deals damage that increases with more ENERGY.\nCost: 40",
			target: SkillTarget.Enemy,
			cost: 40,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] body slams [target]!");
				BattleManager.Instance.Damage(self, target,
					() => self.CurrentStats.ATK * 2 + (BattleManager.Instance.Energy * self.Level) -
						  target.CurrentStats.DEF, false);
			}
		);

		Skills["Cheer"] = new Skill(
			name: "CHEER",
			description:
			"Heals all friends JUICE by 20%. Greatly increases\na STAT if [actor] is feeling an EMOTION. Cost: 80",
			target: SkillTarget.AllAllies,
			cost: 80,
			effect: async (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(340, false);
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.QueueMessage(self, "[actor] cheers!");
				foreach (Actor member in targets)
				{
					BattleManager.Instance.HealJuice(self, member, () => member.CurrentStats.MaxJuice * 0.2);
					string modifier = member.CurrentEmotion.Group?.Id switch
					{
						"happy" => "SpeedUp",
						"sad" => "DefenseUp",
						"angry" => "AttackUp",
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
			description:
			"Acts first, reducing HIT RATE for all foes for 1\nturn. All foes target [actor] for 1 turn. Cost: 50",
			target: SkillTarget.AllEnemies,
			cost: 50,
			effect: async (self, targets) =>
			{
				AudioManager.Instance.PlaySFX("SYS_tag1", volume: 0.9f);
				AnimationManager.Instance.PlayPhotograph();
				await Wait.Milliseconds(500);
				self.AddStatModifier("Taunt");
				foreach (Actor enemy in targets)
				{
					AnimationManager.Instance.PlayAnimation(219, enemy);
					enemy.AddStatModifier("PhotographHitRateDown");
				}

				BattleLogManager.Instance.QueueMessage(self, "[actor] takes a picture.");
				BattleLogManager.Instance.QueueMessage("The foe's HIT RATE fell!");
			},
			priority: 2000
		).WithLastTarget(LastTargetRecord.User);

		Skills["HerbalRemedy"] = new Skill(
			name: "HERBAL REMEDY",
			description: "Heals a friend for 75% of their HEART. Also\nincreases ENERGY by 1. Cost: 35",
			target: SkillTarget.Ally,
			cost: 35,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(341, false);
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayAnimation(342, target);
				await Wait.Milliseconds(1000);
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
				await AnimationManager.Instance.WaitForBasilSpecialAnimation("res://assets/pictures/border_tulip.png",
					326);
				PartyMember first = BattleManager.Instance.GetPartyMember(0);
				BattleLogManager.Instance.QueueMessage(self, "[actor] plants a TULIP.");
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy,
						() => (first.CurrentStats.ATK + first.CurrentStats.DEF + first.CurrentStats.SPD +
							   (first.CurrentStats.LCK * 5)) - enemy.CurrentStats.DEF, false);
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
				await AnimationManager.Instance.WaitForBasilSpecialAnimation(
					"res://assets/pictures/border_gladiolus.png", 290);
				BattleLogManager.Instance.QueueMessage(self, "[actor] plants a GLADIOLUS.");
				BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 4; }, false, 0.1f,
					true);
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
				await AnimationManager.Instance.WaitForBasilSpecialAnimation("res://assets/pictures/border_cactus.png",
					124);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] plants a CACTUS.");
				BattleManager.Instance.Damage(self, target,
					() => (self.CurrentStats.DEF * 2) + self.CurrentHP - target.CurrentStats.DEF, false, 0.1f);
			}
		);

		Skills["Rose"] = new Skill(
			name: "ROSE",
			description: "Acts first, reducing all foes' ATTACK. Heals\nall friends for 40% of their HEART. Cost: 50",
			target: SkillTarget.AllEnemies,
			cost: 50,
			priority: 2000,
			effect: async (self, targets) =>
			{
				AudioManager.Instance.PlaySFX("GEN_shine", 0.5f, 0.9f);
				await AnimationManager.Instance.WaitForBasilSpecialAnimation("res://assets/pictures/border_rose.png",
					335);
				BattleLogManager.Instance.QueueMessage(self, "[actor] plants a ROSE.");
				foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
				{
					AnimationManager.Instance.PlayAnimation(212, member);
					// rose only heals omori (slot 0) and basil
					if (member.Component.Position != 0 && member != self)
						continue;
					int heal = BattleManager.PercentOf(member.CurrentStats.MaxHP, 40);
					heal = member.Heal(heal);
					BattleManager.Instance.SpawnDamageNumber(heal, member, DamageType.Heal);
				}

				await Wait.Milliseconds(500);
				foreach (Actor enemy in targets)
				{
					AnimationManager.Instance.PlayAnimation(219, enemy);
					enemy.AddTierStatModifier("AttackDown", silent: true);
				}
			}
		).WithTiersFromFirstTarget();

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
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF);
				}
			}
		);

		Skills["Vent"] = new Skill(
			name: "Vent",
			description: "Basil Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(142, target);
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForAnimation(3, target);
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] and [target] vent their ANGER!");
				BattleManager.Instance.MakeAngry(first);
				BattleManager.Instance.MakeAngry(self);
			},
			effect: async (self, target) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleManager.Instance.Damage(self, target,
					() => (first.CurrentStats.ATK * 1.5f) + (self.CurrentStats.ATK * 1.5f) - target.CurrentStats.DEF,
					true, 0.1f);
				await Task.CompletedTask;
			},
			hidden: true
		).WithCustomRequirement(_ => true);

		Skills["Mull"] = new Skill(
			name: "Mull",
			description: "Basil Followup",
			target: SkillTarget.AllAllies,
			cost: 0,
			setup: async (_, targets) =>
			{
				List<Task> animations = targets.Select(member => AnimationManager.Instance.WaitForAnimation(213, member)).ToList();
				foreach (Actor member in targets)
					BattleManager.Instance.MakeSad(member, silent: true);
				await Task.WhenAll(animations);
			},
			effect: async (self, targets) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] and [target] mull over SAD thoughts!");
				foreach (Actor member in targets)
				{
					// DIALOGUE QUIRK: prints the held tier line even when the add changed nothing
					if (member.CurrentEmotion.Group?.Id == "sad")
						BattleLogManager.Instance.QueueMessage(member.CurrentEmotion.FeelsMessage(member.Name));
					BattleManager.Instance.HealJuice(self, member, () => member.CurrentStats.MaxJuice * 0.25f, 0.2f, silent: true);
				}
				await Task.CompletedTask;
			},
			hidden: true
		).WithTiersFromFirstTarget().WithCustomRequirement(_ => true);

		Skills["Comfort"] = new Skill(
			name: "Comfort",
			description: "Basil Followup",
			target: SkillTarget.AllAllies,
			cost: 0,
			setup: async (_, targets) =>
			{
				List<Task> animations = targets.Select(member => AnimationManager.Instance.WaitForAnimation(212, member)).ToList();
				foreach (Actor member in targets)
					BattleManager.Instance.MakeHappy(member, silent: true);
				await Task.WhenAll(animations);
			},
			effect: async (self, targets) =>
			{
				PartyMember first = BattleManager.Instance.GetPartyMemberAtPosition(0);
				BattleLogManager.Instance.QueueMessage(self, first, "[actor] and [target] comfort each other!");
				foreach (Actor member in targets)
				{
					// DIALOGUE QUIRK: prints the held tier line even when the add changed nothing
					if (member.CurrentEmotion.Group?.Id == "happy")
						BattleLogManager.Instance.QueueMessage(member.CurrentEmotion.FeelsMessage(member.Name));
					BattleManager.Instance.Heal(self, member, () => member.CurrentStats.MaxHP * 0.25f, silent: true);
				}
				await Task.CompletedTask;
			},
			hidden: true
		).WithTiersFromFirstTarget().WithCustomRequirement(_ => true);

		Skills["ReleaseEnergyBasil"] = new Skill(
			name: "Release Energy Basil",
			description: "Omori Followup",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,
					"[actor] and friends come together and\nuse their ultimate attack!");
				foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
				{
					AnimationManager.Instance.PlayAnimation(243, member);
				}

				await AnimationManager.Instance.WaitForReleaseEnergyBasil();
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForRedHands();
				await AnimationManager.Instance.WaitForFlowerCrown();
				await AnimationManager.Instance.WaitForScreenAnimation(344, true);
				AnimationManager.Instance.PlayPhotograph();
				foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
				{
					AnimationManager.Instance.PlayAnimation(212, member);
					member.Heal(member.CurrentStats.MaxHP);
					member.HealJuice(member.CurrentStats.MaxJuice);
				}

				await Wait.Milliseconds(1000);
				foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
				{
					AnimationManager.Instance.PlayAnimation(214, member);
					if (member.HasStatModifier("ReleaseEnergyBasil"))
						member.AddStatModifier("ReleaseEnergyBasilBonus", silent: true);
					else
						member.AddStatModifier("ReleaseEnergyBasil", silent: true);
				}

				await Wait.Frames(20);
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy, () => 1000, true, 0f, false, true);
				}
			},
			hidden: true
		).WithCustomRequirement(_ => true);
	}
}
