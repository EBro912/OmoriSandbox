using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Editor;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class OmoriRegistry
{
	internal static void Register()
	{
		PartyMembers.Add("Omori", () => new Omori());

		// OMORI //
		Skills["OAttack"] = new Skill(
			name: "OAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForAnimation(3, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement((_) => true);

		Skills["SadPoem"] = new Skill(
			name: "SAD POEM",
			description: "Inflicts SAD on a friend or foe.\nCost: 5",
			target: SkillTarget.AllyOrEnemy,
			cost: 5,
			setup: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] reads a sad poem.");
				await AnimationManager.Instance.WaitForAnimation(5, self);
				BattleManager.Instance.MakeSad(target);
			},
			effect: (_, _) => Task.CompletedTask
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
				if (self.CurrentEmotion.Group?.Id == "happy")
					BattleManager.Instance.Damage(self, target,
						() => (self.CurrentStats.ATK + self.CurrentStats.LCK) * 2f - target.CurrentStats.DEF, false);
				else
					BattleManager.Instance.Damage(self, target,
						() => (self.CurrentStats.ATK + self.CurrentStats.LCK) * 1.5f - target.CurrentStats.DEF, false);
			},
			priority: 2000
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
				if (self.CurrentEmotion.Group?.Id == "sad")
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f, false,
						guaranteeCrit: true);
				else
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF, false, guaranteeCrit: true);
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
				if (target.CurrentEmotion.Group?.Id == "happy")
				{
					if (AddTierThreeStatModifier(target, "SpeedDown"))
						AnimationManager.Instance.PlayAnimation(219, target);
				}

				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF);
				await Wait.Milliseconds(334);
			}
		);

		Skills["Observe"] = new Skill(
			name: "OBSERVE",
			description: "Predicts who a foe will target next turn.\nCost: 0",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target,
					"[actor] focuses their vision and observes\n[target]!");
				AnimationManager.Instance.PlayAnimation(4, target);
				await Wait.Milliseconds(1000);
				BattleManager battle = BattleManager.Instance;
				foreach (int slot in new[] { 1, 3 })
				{
					PartyMember charmer = battle.GetPartyMemberAtPosition(slot);
					if (charmer != null && charmer.HasStatModifier("CharmCheck"))
					{
						await Lock(charmer);
						return;
					}
				}

				bool multi = GameManager.Instance.Random.RandiRange(1, 2) == 1;
				List<PartyMember> party = battle.GetAllPartyMembers();
				PartyMember pick = party[GameManager.Instance.Random.RandiRange(0, party.Count - 1)];
				List<PartyMember> members = battle.GetAlivePartyMembers();
				if (multi)
				{
					// if the party is multi observed, any enemy that checks for state 185 in their AI is affected
					// (vanilla technically only checks the first four troop slots)
					for (int i = 0; i < 4; i++)
					{
						Enemy speaker = battle.GetEnemy(i);
						if (speaker == null || !speaker.ObserveHasMulti)
							continue;
						// VANILLA BUG: the multi branch ends with no observe state when its first holder can't act
						if (!battle.GetAllAliveEnemies().Contains(speaker))
							return;
						BattleLogManager.Instance.QueueMessage(speaker, "[actor] has their eyes on everyone!");
						foreach (PartyMember m in members)
							m.AddStatModifier("ObserveMulti", silent: true);
						foreach (PartyMember m in members)
							AnimationManager.Instance.PlayAnimation(4, m);
						await Wait.Milliseconds(1000);
						return;
					}
				}

				PartyMember member = null;
				for (int slot = pick.Component.Position; slot > 0 && member == null; slot--)
				{
					PartyMember atSlot = battle.GetPartyMemberAtPosition(slot);
					if (atSlot != null && atSlot.CurrentHP > 0)
						member = atSlot;
				}
				member ??= battle.GetPartyMemberAtPosition(0);
				if (member == null || member.CurrentHP <= 0)
					return;
				foreach (PartyMember m in members)
					m.AddStatModifier("ObserveSingle", silent: true);
				await Lock(member);
				member.ClearResult();
			},
			priority: -1000
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

				BattleManager.Instance.RecordLastTarget(self, randomTargets[^1]);

				foreach (Actor target in randomTargets)
				{
					BattleManager.Instance.Damage(self, target, () =>
					{
						if (self.CurrentEmotion.Group?.Id == "angry")
						{
							return self.CurrentStats.ATK * 2.25f - target.CurrentStats.DEF;
						}

						return self.CurrentStats.ATK * 2f - target.CurrentStats.DEF;
					}, false);
				}
			}
		);

		bool truthTargetSad = false;
		bool truthSelfSad = false;
		Skills["PainfulTruth"] = new Skill(
			name: "PAINFUL TRUTH",
			description: "Deals damage to a foe. [actor] and the foe\nbecome SAD. Cost: 10",
			target: SkillTarget.Enemy,
			cost: 10,
			setup: (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(5, self);
				AnimationManager.Instance.PlayAnimation(19, target);

				truthTargetSad = BattleManager.Instance.MakeSad(target, silent: true);
				truthSelfSad = BattleManager.Instance.MakeSad(self, silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				await Wait.Milliseconds(1000);

				BattleLogManager.Instance.QueueMessage(self, target, "[actor] whispers something to\n[target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false);
				// DIALOGUE QUIRK: always prints the tier-1 SAD line for the target regardless of the tier it reaches
				QueueTierOneEmotionLine(target, "sad", truthTargetSad);
				QueueEmotionLine(self, "sad", truthSelfSad);
			}
		);

		Skills["Mock"] = new Skill(
			name: "MOCK",
			description: "Deals damage. If the foe is ANGRY, greatly\nreduce its ATTACK. Cost: 20",
			target: SkillTarget.Enemy,
			cost: 20,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(12, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] mocks [target].");
				int damage = BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF,
					false);
				if (damage > -1 && target.CurrentEmotion.Group?.Id == "angry")
				{
					if (AddTierThreeStatModifier(target, "AttackDown", true))
						AnimationManager.Instance.PlayAnimation(219, target);
					else
						QueueNoStateChange(target.Name.ToUpper(), "ATTACK", "lower!");
				}
				await Wait.Milliseconds(334);
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
				if (target.CurrentEmotion.Group?.Id == "sad")
				{
					if (AddTierThreeStatModifier(target, "DefenseDown"))
						AnimationManager.Instance.PlayAnimation(219, target);
				}

				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF,
					false);
				await Wait.Milliseconds(334);
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
				await Wait.Milliseconds(1660);
				AnimationManager.Instance.PlayAnimation(219, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] stares at [target].");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] feels uncomfortable.");
				target.AddStatModifier("AttackDown");
				target.AddStatModifier("DefenseDown");
				target.AddStatModifier("SpeedDown");
				await Wait.Milliseconds(334);
			}
		);

		Skills["Exploit"] = new Skill(
			name: "EXPLOIT",
			description: "Deals extra damage to a HAPPY, SAD, or\nANGRY foe. Cost: 30",
			target: SkillTarget.Enemy,
			cost: 30,
			effect: async (self, target) =>
			{
				switch (target.CurrentEmotion.Group?.Id)
				{
					case "happy":
						await AnimationManager.Instance.WaitForAnimation(10, target);
						break;
					case "sad":
						await AnimationManager.Instance.WaitForAnimation(11, target);
						break;
					case "angry":
						await AnimationManager.Instance.WaitForAnimation(12, target);
						break;
					default:
						await AnimationManager.Instance.WaitForAnimation(123, target);
						break;
				}

				// vanilla only breaks the line (and adds the "!") once the text reaches 34 characters
				string exploitText = $"{self.Name.ToUpper()} exploits {target.Name.ToUpper()}'s EMOTIONS";
				BattleLogManager.Instance.QueueMessage(self, target, exploitText.Length >= 34
					? "[actor] exploits [target]'s\nEMOTIONS!"
					: "[actor] exploits [target]'s EMOTIONS");
				if (target.CurrentEmotion.Id != "neutral")
				{
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 3.5f - target.CurrentStats.DEF, false);
				}
				else
				{
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF, false);
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
				BattleLogManager.Instance.QueueMessage(self, "[actor] unleashes his ultimate attack!");
				await AnimationManager.Instance.WaitForScreenAnimation(13, targets[0] is Enemy);
				float multiplier = self.CurrentEmotion.Group == null ? 3f : self.CurrentEmotion.Tier switch
				{
					>= 3 => 6f,
					2 => 5f,
					_ => 4f
				};
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy,
						() => self.CurrentStats.ATK * multiplier - enemy.CurrentStats.DEF, false);
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
					BattleManager.Instance.Damage(self, target,
						() => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, false);
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
				BattleLogManager.Instance.QueueMessage(self, "[actor] throws the foes off balance!");
				BattleLogManager.Instance.QueueMessage("All foes' ATTACK fell!");
				foreach (Actor enemy in targets)
				{
					enemy.AddTierStatModifier("AttackDown", 3, silent: true);
					AnimationManager.Instance.PlayAnimation(219, enemy);
					if (SettingsMenuManager.Instance.VertigoUsesAtk)
						BattleManager.Instance.Damage(self, enemy,
							() => self.CurrentStats.ATK * 3f - enemy.CurrentStats.DEF, false);
					else
						BattleManager.Instance.Damage(self, enemy,
							() => self.CurrentStats.SPD * 3f - enemy.CurrentStats.DEF, false);
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
					BattleManager.Instance.Damage(self, enemy,
						() => self.CurrentStats.ATK * 3.5f - enemy.CurrentStats.DEF, false);
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
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForAnimation(3, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks again!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false);
			},
			hidden: true
		).WithCustomRequirement((_) => true);

		Skills["AttackAgain2"] = new Skill(
			name: "Attack Again 2",
			description: "Omori Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] readies his blade.");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForAnimation(3, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks again!");
				BattleManager.Instance.Damage(self, target,
					() => (self.CurrentStats.ATK * 2) + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
			},
			hidden: true
		).WithCustomRequirement((_) => true);

		Skills["AttackAgain3"] = new Skill(
			name: "Attack Again 3",
			description: "Omori Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] readies his blade.");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForAnimation(290, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks again!");
				BattleManager.Instance.Damage(self, target,
					() => (self.CurrentStats.ATK * 2) + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
				await Wait.Milliseconds(500);
				BattleManager.Instance.Damage(self, target,
					() => (self.CurrentStats.ATK * 2) + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
			},
			hidden: true
		).WithCustomRequirement((_) => true);

		Skills["Trip1"] = new Skill(
			name: "Trip 1",
			description: "Omori Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] walks forward.");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForAnimation(14, target);
				AnimationManager.Instance.PlayAnimation(219, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] trips [target]!");
				target.AddStatModifier("SpeedDown");
				BattleManager.Instance.Damage(self, target,
					() => self.CurrentStats.ATK + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
			},
			hidden: true
		).WithCustomRequirement((_) => true);

		Skills["Trip2"] = new Skill(
			name: "Trip 2",
			description: "Omori Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] walks forward.");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForAnimation(14, target);
				AnimationManager.Instance.PlayAnimation(219, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] trips [target]!");
				target.AddTierStatModifier("SpeedDown", 2);
				// DIALOGUE QUIRK: always prints the tier-1 SAD line regardless of the tier the target reaches
				QueueTierOneEmotionLine(target, "sad", BattleManager.Instance.MakeSad(target, silent: true));
			},
			effect: async (self, target) =>
			{
				BattleManager.Instance.Damage(self, target,
					() => self.CurrentStats.ATK + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
				await Task.CompletedTask;
			},
			hidden: true
		).WithCustomRequirement((_) => true);

		Skills["Trip3"] = new Skill(
			name: "Trip 3",
			description: "Omori Followup",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] walks forward.");
				await Wait.Milliseconds(1000);
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForAnimation(289, target);
				AnimationManager.Instance.PlayAnimation(219, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] trips [target]!");
				AddTierThreeStatModifier(target, "SpeedDown");
				// DIALOGUE QUIRK: always prints the tier-1 SAD line regardlesso of the tier the target reaches
				QueueTierOneEmotionLine(target, "sad", BattleManager.Instance.MakeSad(target, silent: true));
			},
			effect: async (self, target) =>
			{
				BattleManager.Instance.Damage(self, target,
					() => self.CurrentStats.ATK + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
				await Task.CompletedTask;
			},
			hidden: true
		).WithCustomRequirement((_) => true);

		Skills["ReleaseEnergy1"] = new Skill(
			name: "Release Energy 1",
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

				await AnimationManager.Instance.WaitForReleaseEnergy();
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(15, true);
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy, () => 300, true, 0f, false, true);
				}

				foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
				{
					member.AddStatModifier("ReleaseEnergy");
				}
			},
			hidden: true
		).WithCustomRequirement((_) => true);

		Skills["ReleaseEnergy2"] = new Skill(
			name: "Release Energy 2",
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

				await AnimationManager.Instance.WaitForReleaseEnergy();
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(15, true);
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy, () => 600, true, 0f, false, true);
				}

				foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
				{
					member.AddStatModifier("ReleaseEnergy");
				}
			},
			hidden: true
		).WithCustomRequirement((_) => true);

		Skills["ReleaseEnergy3"] = new Skill(
			name: "Release Energy 3",
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

				await AnimationManager.Instance.WaitForReleaseEnergy();
				BattleLogManager.Instance.ClearBattleLog();
				await AnimationManager.Instance.WaitForScreenAnimation(15, true);
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy, () => 1000, true, 0f, false, true);
				}

				foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
				{
					member.AddStatModifier("ReleaseEnergy");
				}
			},
			hidden: true
		).WithCustomRequirement((_) => true);
	}

	private static async Task Lock(PartyMember member)
	{
		BattleManager battle = BattleManager.Instance;
		Enemy enemy = battle.SmoothEnemyTarget(member.LastTargetIndex);
		if (enemy == null)
			return;
		enemy.AddStatModifier("ObserveIgnoreTaunt", silent: true);
		if (enemy.StatModifiers.TryGetValue("ObserveIgnoreTaunt", out StatModifier held) && held is ObserveStatModifier observe)
			observe.ObservedBy = member;
		battle.RecordLastTarget(member, enemy);
		BattleLogManager.Instance.QueueMessage(enemy, member, "[actor] has their eyes on\n[target]!");
		await AnimationManager.Instance.WaitForAnimation(4, member);
	}

	private static void QueueNoStateChange(string name, string stat, string direction)
	{
		string first = name + "'s " + stat + " can't go";
		string second = "any " + direction;
		string complete = first + " " + second;
		BattleLogManager.Instance.QueueMessage(complete.Length < 40 ? complete : first + "\n" + second);
	}
}
