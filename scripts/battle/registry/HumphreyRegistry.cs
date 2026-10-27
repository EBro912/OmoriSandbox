using System;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class HumphreyRegistry
{
	internal static void Register()
	{
		Enemies.Add("HumphreySwarm", () => new HumphreySwarm());
		Enemies.Add("HumphreyGrande", () => new HumphreyGrande());
		Enemies.Add("HumphreyFace", () => new HumphreyFace());
		Enemies.Add("HumphreyUvula", () => new HumphreyUvula());

		// Humphrey Uvula //
		Skills["HUUDoNothing"] = new Skill(
			name: "HUUDoNothing",
			description: "HUUDoNothing",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] smirks at [target].");
				await Task.CompletedTask;
			}
		);

		// Humphrey Swarm //
		Skills["HUSAttack"] = new Skill(
			name: "HUSAttack",
			description: "HUSAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.4f, false, neverCrit: true);
				await AnimationManager.Instance.WaitForAnimation(295, target);
			}
		);
		
		Skills["HUSAttack2"] = new Skill(
			name: "HUSAttack2",
			description: "HUSAttack2",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] appears and attacks!");
				foreach (Actor target in targets)
				{
					BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.35f, false,
						neverCrit: true);
					await AnimationManager.Instance.WaitForAnimation(295, target);
				}
			},
			hidden: true
		);
		
		Skills["HUSAttack3"] = new Skill(
			name: "HUSAttack3",
			description: "HUSAttack3",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] appears everywhere!");
				foreach (Actor target in targets)
				{
					BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.3f, false,
						neverCrit: true);
					await AnimationManager.Instance.WaitForAnimation(295, target);
				}
			},
			hidden: true
		);

		// Humphrey Grande //
		Skills["HUGAttack"] = new Skill(
			name: "HUGAttack",
			description: "HUGAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams into [target]!");
				await AnimationManager.Instance.WaitForAnimation(295, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		// Humphrey Face //
		Skills["HUFChomp"] = new Skill(
			name: "HUFChomp",
			description: "HUFChomp",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] sinks his teeth into [target]!");
				await AnimationManager.Instance.WaitForAnimation(157, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);
			
		Skills["HUFDoNothing"] = new Skill(
			name: "HUFDoNothing",
			description: "HUFDoNothing",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] stares at [target]!");
				BattleLogManager.Instance.QueueMessage(self, "[actor]'s mouth waters incessantly.");
				await Task.CompletedTask;
			}
		);
		
		Skills["HUFSwallow"] = new Skill(
			name: "HUFSwallow",
			description: "HUFSwallow",
			target: SkillTarget.AllEnemies,
			cost: 0,
			// vanilla adds ACCURACY UP in the setup action and lets it expire at the end of the turn
			setup: (self, _) =>
			{
				self.AddStatModifier("AccuracyUp", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] swallows everyone!");
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.TintScreen(-255, -255, -255, 0, 1f);
				if (self is Enemy enemy)
					enemy.SetOpacity(0f);
				await AnimationManager.Instance.WaitForHumphreyFaceSwallow();
				await Wait.Milliseconds(500);
				AudioManager.Instance.PlaySFX("GEN_Bite", 0.5f, 0.9f);
				// Damage returns -1 on a miss/evade, which must not count against the steal
				int stolen = targets.Sum(target => (int)Math.Floor(Math.Max(0, BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.25f, false, 0.5f, neverCrit: true)) * 0.25d));
				self.Heal(stolen, applyDebugScale: false);
				await Wait.Frames(8);
				AudioManager.Instance.PlaySFX("BA_Bite", 0.5f, 0.9f);
				await BattleLogManager.Instance.WaitForBattleLog();
				await Wait.Milliseconds(750);
				if (self is Enemy stillAnEnemy)
					stillAnEnemy.SetOpacity(1f);
				await AnimationManager.Instance.WaitForTintScreen(0, 0, 0, 0, 1f);
			}
		);

		Skills["HUFSwallowAlt"] = new Skill(
			name: "HUFSwallowAlt",
			description: "HUFSwallowAlt",
			target: SkillTarget.AllEnemies,
			cost: 0,
			// vanilla adds ACCURACY UP in the setup action and lets it expire at the end of the turn
			setup: (self, _) =>
			{
				self.AddStatModifier("AccuracyUp", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] swallows everyone!");
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.TintScreen(-255, -255, -255, 0, 1f);
				if (self is Enemy enemy)
					enemy.SetOpacity(0f);
				await AnimationManager.Instance.WaitForHumphreyFaceSwallow();
				await Wait.Milliseconds(500);
				AudioManager.Instance.PlaySFX("GEN_Bite", 0.5f, 0.9f);
				// Damage returns -1 on a miss/evade, which must not count against the steal
				int stolen = targets.Sum(target => (int)Math.Floor(Math.Max(0, BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.35f, false, 0.5f, neverCrit: true)) * 0.5d));
				self.Heal(stolen, applyDebugScale: false);
				await Wait.Frames(8);
				AudioManager.Instance.PlaySFX("BA_Bite", 0.5f, 0.9f);
				await BattleLogManager.Instance.WaitForBattleLog();
				await Wait.Milliseconds(750);
				if (self is Enemy stillAnEnemy)
					stillAnEnemy.SetOpacity(1f);
				await AnimationManager.Instance.WaitForTintScreen(0, 0, 0, 0, 1f);
			}
		);
	}
}
