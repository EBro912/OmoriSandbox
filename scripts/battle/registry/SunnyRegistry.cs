using OmoriSandbox.Animation;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class SunnyRegistry
{
	internal static void Register()
	{
		PartyMembers.Add("Sunny", () => new Sunny());
		PartyMembers.Add("Sunny (Alt)", () => new SunnyAlt());

		// SUNNY

		Skills["SRWAttack"] = new Skill(
			name: "SRWAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForAnimation(108, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement((_) => true);

		Skills["SRWAltAttack"] = new Skill(
			name: "SRWAltAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForAnimation(21, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement((_) => true);

		Skills["CalmDown"] = new Skill(
			name: "CALM DOWN",
			description: "Removes EMOTIONS and heals some HEART.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleManager.Instance.Energy = 0;
				AnimationManager.Instance.PlayScreenAnimation(104, false);
				int heal = target.Heal(BattleManager.RoundHealing(target.CurrentStats.MaxHP * 0.5));
				target.SetEmotion("neutral", true);
				BattleManager.Instance.SpawnDamageNumber(heal, target, DamageType.Heal);
				BattleLogManager.Instance.QueueMessage(target, "[actor] calms down.");
				if (heal > 0)
					BattleLogManager.Instance.QueueMessage(target, $"[actor] recovers {heal} HEART!");
				await Wait.Frames(250);
			},
			priority: 2000
			// calm down can be used while afraid
		).WithCustomRequirement((actor) => actor.CurrentEmotion.Id is not "stressed");

		Skills["BrokenCalmDown"] = new Skill(
			name: "CALM DOWN",
			description: "Removes EMOTIONS and heals some HEART.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor _, Actor _) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(110, false);
				await Wait.Frames(250);
				BattleManager.Instance.Energy = 0;
			},
			priority: 1999
		).WithCustomRequirement((actor) => actor.CurrentEmotion.Id is not "stressed");

		Skills["Focus"] = new Skill(
			name: "FOCUS",
			description: "[actor]'s next attack deals more damage.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] focuses.");
				AnimationManager.Instance.PlayScreenAnimation(105, false);
				await Wait.Milliseconds(2500);
				target.AddStatModifier("Flex");
			},
			priority: 2000
		);

		Skills["Persist"] = new Skill(
			name: "PERSIST",
			description: "HEART cannot reach 0 for 1 turn.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] persists.");
				AnimationManager.Instance.PlayScreenAnimation(106, false);
				await Wait.Milliseconds(2500);
				target.Heal(20);
				target.AddStatModifier("SecondChance");
			},
			priority: 2000
		);


		Skills["Allegro"] = new Skill(
			name: "ALLEGRO",
			description: "Attacks 3 times. \nCost: 19",
			target: SkillTarget.Enemy,
			cost: 19,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(103, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] strikes three times.");
				for (int i = 0; i < 3; i++)
				{
					if (i > 0)
						await Wait.Frames(8);
					BattleManager.Instance.Damage(self, target,
						() => target.CurrentStats.MaxHP * 0.15f + self.CurrentStats.ATK - target.CurrentStats.DEF,
						false);
				}
			}
		);

		Skills["Encore"] = new Skill(
			name: "ENCORE",
			description: "Your JUICE will not fall for 3 turns.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			priority: 1000,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] gathered themself...");
				target.AddStatModifier("Encore");
				await AnimationManager.Instance.WaitForEncore();
			}
		).WithCustomRequirement(actor =>
			!actor.HasStatModifier("Encore") && !actor.CurrentEmotion.BlocksActions);

		Skills["Cherish"] = new Skill(
			name: "CHERISH",
			description: "Heal your wounds and come back stronger.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			priority: 1000,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] steadies their breathing.");
				target.AddTierStatModifier("AttackUp", silent: true);
				target.AddTierStatModifier("DefenseUp", silent: true);
				target.AddTierStatModifier("SpeedUp", silent: true);
				int index = target.GetStatModifierTier("CherishDialogue");
				if (index < 4)
					target.AddTierStatModifier("CherishDialogue");
				await AnimationManager.Instance.PlayCherish(index);
				if (index == 4)
					target.Skills.Remove("Cherish");
				if (target.HasStatModifier("Encore"))
				{
					// VANILLA BUG: CHERISH removes MARI POISON instead of ENCORE, so ENCORE stays
					foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
						member.Heal(300);
					foreach (PartyMember member in BattleManager.Instance.GetAllPartyMembers())
						member.HealJuice(30);
				}
				else
				{
					target.RecoverAll();
				}

				target.AddTierStatModifier("AttackUp", 3, silent: true);
				target.AddTierStatModifier("DefenseUp", 1, silent: true);
				target.AddTierStatModifier("SpeedUp", 2, silent: true);
				if (target.GetStatModifierTier("CherishDialogue") == 4)
					target.AddStatModifier("Cherished");
				if (OmoriEnemy.SealsCherish())
				{
					foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
						member.AddStatModifier("CherishSeal", silent: true);
				}
				await AnimationManager.Instance.HideCherish();
				foreach (Enemy enemy in BattleManager.Instance.GetAllAliveEnemies())
					enemy.AddStatModifier("CallForFriendDelay", silent: true);
			}
		).WithCustomRequirement(actor => !actor.CurrentEmotion.BlocksActions && !actor.HasStatModifier("CherishSeal"));
	}
}
