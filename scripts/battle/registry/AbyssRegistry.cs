using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class AbyssRegistry
{
	internal static void Register()
	{
		Enemies.Add("Abbi", () => new Abbi());
		Enemies.Add("Tentacle", () => new Tentacle());

		// Abbi //
		Skills["AbbiAttack"] = new Skill(
			name: "AbbiAttack",
			description: "AbbiAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(144, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["AbbiAttackOrder"] = new Skill(
			name: "AbbiAttackOrder",
			description: "AbbiAttackOrder",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, targets) =>
			{
				AudioManager.Instance.PlaySFX("SE_bs_scare6");
				BattleLogManager.Instance.QueueMessage(self, "[actor] stretches her tentacles.");
				BattleLogManager.Instance.QueueMessage("Everyone's ATTACK rose!!");
				BattleLogManager.Instance.QueueMessage("Everyone feels ANGRY!");
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.MakeEmotionTier(enemy, "angry", 1, true);
					enemy.AddTierStatModifier("AttackUp", silent: true);
				}
				await Task.CompletedTask;
			},
			hidden: true
		).WithTiersFromFirstTarget();
		
		Skills["AbbiSummon"] = new Skill(
			name: "AbbiSummon",
			description: "AbbiSummon",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] focuses her HEART.");
				AudioManager.Instance.PlaySFX("sys_blackletter1", 1.5f, 0.9f);
				if (target is Abbi abbi)
					abbi.ReviveTentacle();
				await Task.CompletedTask;
			},
			hidden: true
		);

		// Tentacle //
		Skills["TENAttack"] = new Skill(
			name: "TENAttack",
			description: "TENAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["TENWeaken"] = new Skill(
			name: "TENWeaken",
			description: "TENWeaken",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(129, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] weakens [target]!");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] let their guard down!");
				target.AddStatModifier("Tickle");
			}
		);
		
		Skills["TENGrab"] = new Skill(
			name: "TENGrab",
			description: "TENGrab",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(197, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] wraps around [target]!");
				BattleManager.Instance.Damage(self, target, () => 100, new DamageOptions
				{
					NeverMiss = false,
					Variance = 0.1f,
					Crit = CritMode.Never,
					OnHit = () =>
					{
						if (target.SetEmotion("afraid", true))
							BattleLogManager.Instance.QueueMessage(target, "[actor] feels AFRAID.");
						else
							BattleManager.Instance.ShowEmotionNoEffect(target, "afraid");
					}
				});
			}
		);
		
		
		Skills["TENGoop"] = new Skill(
			name: "TENGoop",
			description: "TENGoop",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(291, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[target] is drenched in dark liquid!");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] feels weaker...");
				AnimationManager.Instance.PlayAnimation(215, target);
				// DIALOGUE QUIRK: prints the "fell." lines even when a stat is already at its limit
				target.AddTierStatModifier("AttackDown", silent: true);
				target.AddTierStatModifier("DefenseDown", silent: true);
				target.AddTierStatModifier("SpeedDown", silent: true);
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s ATTACK fell.\n[target]'s DEFENSE fell.\n[target]'s SPEED fell.");
			}
		);
	}
}
