using Godot;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Actors;
using OmoriSandbox.Editor;
using OmoriSandbox.Modding;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class BlackspaceRegistry
{
	internal static void Register()
	{
		Enemies.Add("OmoriEnemy", () => new OmoriEnemy());

		// FINAL OMORI //
		
		Skills["OMEAttack"] = new Skill(
			name: "OMEAttack",
			description: "OMEAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(3, target);
				await Wait.Frames(60);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF + 4,
					new DamageOptions { NeverMiss = false, Crit = CritMode.Never });
			}
		);
		
		Skills["OMEHackAway"] = new Skill(
			name: "OMEHackAway",
			description: "OMEHackAway",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] slashes wildly.");
				await Wait.Frames(10);
				await Task.WhenAll(targets.Select(member => AnimationManager.Instance.WaitForAnimation(294, member)));
				for (int i = 0; i < 3; i++)
				{
					if (i > 0)
						await Wait.Frames(8);
					foreach (Actor target in targets)
						BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF + 4,
							new DamageOptions { NeverMiss = false, Crit = CritMode.Never });
				}
			}
		);
		
		Skills["OMEStab"] = new Skill(
			name: "OMEStab",
			description: "OMEStab",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(20, target);
				await Wait.Frames(60);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] stabs [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					new DamageOptions { NeverMiss = false, Crit = CritMode.Always });
			}
		);
		
		Skills["OMERedHands"] = new Skill(
			name: "OMERedHands",
			description: "OMERedHands",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] erases the enemy.");
				foreach (Actor target in targets)
					AnimationManager.Instance.PlayAnimation(307, target);
				await Wait.Frames(20);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
						DamageOptions.DefaultCanMiss);
				await Wait.Frames(20);
			}
		);
		
		Skills["OMERedHandAttack"] = new Skill(
			name: "OMERedHandAttack",
			description: "OMERedHandAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] erases the enemy.");
				Task animation = AnimationManager.Instance.WaitForAnimation(23, target);
				await Wait.Frames(20);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 4 - target.CurrentStats.DEF,
					new DamageOptions { Crit = CritMode.Never });
				await animation;
				Task statDown = AnimationManager.Instance.WaitForAnimation(215, target);
				// VANILLA BUG: prints the "fell." line even when DEFENSE is already at its limit
				target.AddTierStatModifier("DefenseDown", silent: true);
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s DEFENSE fell.");
				await statDown;
			}
		);
		
		Skills["OMEHairAttack"] = new Skill(
			name: "OMEHairAttack",
			description: "OMEHairAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] erases the enemy.");
				Task animation = AnimationManager.Instance.WaitForAnimation(24, target);
				await Wait.Frames(20);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 4 - target.CurrentStats.DEF,
					new DamageOptions { NeverMiss = false, Crit = CritMode.Never });
				await animation;
				Task statDown = AnimationManager.Instance.WaitForAnimation(215, target);
				// VANILLA BUG: prints the "STATS fell." line even when the stats are already at their limit
				target.AddTierStatModifier("SpeedDown", silent: true);
				target.AddTierStatModifier("AttackDown", silent: true);
				target.AddTierStatModifier("DefenseDown", silent: true);
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s STATS fell.");
				await statDown;
			}
		);
		
		Skills["OMEMariAttack"] = new Skill(
			name: "OMEMariAttack",
			description: "OMEMariAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				target.SetEmotion("afraid", true);
				await Wait.Frames(10);
				await AnimationManager.Instance.WaitForAnimation(25, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] erases the enemy.");
				// VANILLA BUG: prints the "feels AFRAID." line even when the target doesn't become AFRAID
				BattleLogManager.Instance.QueueMessage(self, target, "[target] feels AFRAID.");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 4 - target.CurrentStats.DEF,
					new DamageOptions { NeverMiss = false, Crit = CritMode.Never });
			}
		);
		
		Skills["OMEFinalAttack"] = new Skill(
			name: "OMEFinalAttack",
			description: "OMEFinalAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] erases the enemy.");
				await Wait.Frames(4);
				await AnimationManager.Instance.WaitForAnimation(26, target);
				await Wait.Frames(160);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 5 - target.CurrentStats.DEF,
					new DamageOptions { Crit = CritMode.Never });
				await Wait.Frames(20);
				// VANILLA BUG: prints the "feels AFRAID." and "fell." lines even when the target doesn't become AFRAID or DEFENSE is already at its limit
				target.SetEmotion("afraid", true);
				BattleLogManager.Instance.QueueMessage(self, target, "[target] feels AFRAID.");
				await Wait.Frames(4);
				Task statDown = AnimationManager.Instance.WaitForAnimation(215, target);
				target.AddTierStatModifier("DefenseDown", silent: true);
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s DEFENSE fell.");
				await statDown;
			}
		);
		
		Skills["Play"] = new Skill(
			name: "PLAY",
			description: "Skill for final battle\n",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (Actor _, Actor _) =>
			{
				OmoriEnemy owner = BattleManager.Instance.GetAllEnemies().OfType<OmoriEnemy>()
					.FirstOrDefault(x => x is { IsToast: false, Phase: >= 5, CanChangeBGM: true });
				await Wait.Frames(60);
				owner?.FadeBGM(0.001f, 6f);
				await AnimationManager.Instance.WaitForTintOverlay(Colors.White, 90 / 60f, layer: 22);
				await Wait.Frames(360);
				if (SettingsMenuManager.Instance.PlayFinalDuet)
					await AnimationManager.Instance.WaitForVideoInternal("res://assets/movies/final_duet.ogv", layer: 100);
				BattleManager.Instance.PlayMode = false;
				foreach (Enemy enemy in BattleManager.Instance.GetAllAliveEnemies())
				{
					if (enemy is OmoriEnemy { Phase: >= 5 } omori)
						await omori.AbortBattle();
				}
				bool enemiesRemain = BattleManager.Instance.GetAllAliveEnemies().Any(x => x.CurrentHP > 0 || x.IsImmortal);
				if (owner != null && enemiesRemain && !BattleManager.Instance.AbortRequested)
				{
					BattleManager.Instance.PlayPresetBGM(0.05f);
					owner.FadeBGM(1f, 180 / 60f);
				}
				await AnimationManager.Instance.WaitForTintOverlay(new Color(1, 1, 1, 0), 180 / 60f, layer: 22);
			},
			hidden: true,
			priority: 2000
		).WithCustomRequirement((_) => true);
	}
}
