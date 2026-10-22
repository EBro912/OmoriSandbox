using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class PyreflyForestRegistry
{
	internal static void Register()
	{
		Enemies.Add("LostSproutMole", () => new LostSproutMole());
		Enemies.Add("KingCrawler", () => new KingCrawler());
		Enemies.Add("KingCarnivore", () => new KingCarnivore());
		Enemies.Add("Root", () => new Root());

		// LOST SPROUT MOLE //
		Skills["LSMAttack"] = new Skill(
			name: "Attack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] bumps into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					new DamageOptions { NeverMiss = false, CritMultiplier = 0.6 });
			}
		);

		Skills["LSMDoNothing"] = new Skill(
			name: "Do Nothing",
			description: "Does nothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_dance");
				BattleLogManager.Instance.QueueMessage(target, "[actor] is rolling around.");
				await Task.CompletedTask;
			}
		);

		Skills["LSMRunAround"] = new Skill(
			name: "Run Around",
			description: "Run Around",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(200, targets[0] is Enemy);
				BattleLogManager.Instance.QueueMessage(self, targets[0], "[actor] runs around!");
				await Wait.Milliseconds(100);
				BattleManager.Instance.Damage(self, targets[0],
					() => self.CurrentStats.ATK * 1.5f - targets[0].CurrentStats.DEF, false);
				await Wait.Milliseconds(917);
				BattleManager.Instance.Damage(self, targets[0],
					() => self.CurrentStats.ATK * 1.5f - targets[0].CurrentStats.DEF, false);
			},
			hidden: true
		);

		// King Crawler //
		Skills["KCAttack"] = new Skill(
			name: "KCAttack",
			description: "KCAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["KCDoNothing"] = new Skill(
			name: "KCDoNothing",
			description: "KCDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_roar", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] lets out an ear-piercing\nscreech!");
				BattleManager.Instance.MakeAngry(target);
				await Task.CompletedTask;
			}
		);
		
		Skills["KCCrunch"] = new Skill(
			name: "KCCrunch",
			description: "KCCrunch",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(157, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] chomps [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["KCRam"] = new Skill(
			name: "KCRam",
			description: "KCRam",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] charges forward!");
				await AnimationManager.Instance.WaitForScreenAnimation(179, targets[0] is Enemy);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member,
						() => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
				}
			}
		);
		
		Skills["KCEat"] = new Skill(
			name: "KCEat",
			description: "KCEat",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(157, target);
				BattleManager.Instance.SpawnDamageNumber(target.CurrentStats.MaxHP + 100, target);
				target.Damage(target.CurrentHP);
			},
			hidden: true
		);
		
		Skills["KCRecover"] = new Skill(
			name: "KCRecover",
			description: "KCRecover",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(216, target);
				int healed = BattleManager.Instance.Heal(self, target, () => 170, silent: true);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovers {healed} HEART!");
				BattleManager.Instance.MakeHappy(target);
			}
		);

		// King Carnivore
		Skills["UPCAttack"] = new Skill(
			name: "UPCAttack",
			description: "UPCAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(126, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] wraps\n[target] with vines!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["UPCDoNothing"] = new Skill(
			name: "UPCDoNothing",
			description: "UPCDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] roars!");
				await Task.CompletedTask;
			}
		);
		
		Skills["UPCSweetGas"] = new Skill(
			name: "UPCSweetGas",
			description: "UPCSweetGas",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] releases gas!\nIt smells sweet!\nEveryone feels HAPPY!");
				await AnimationManager.Instance.WaitForScreenAnimation(181, targets[0] is Enemy);
				foreach (Actor target in targets)
					BattleManager.Instance.MakeHappy(target, silent: true);
			}
		);

		// Root
		Skills["ROAttack"] = new Skill(
			name: "ROAttack",
			description: "ROAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(126, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["RODoNothing"] = new Skill(
			name: "RODoNothing",
			description: "RODoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] wiggles around.");
				await Task.CompletedTask;
			}
		);
		
		Skills["ROHealPlant"] = new Skill(
			name: "ROHealPlant",
			description: "ROHealPlant",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] absorbs nutrients.");
				await Wait.Milliseconds(500);
				foreach (Actor target in targets)
				{
					AnimationManager.Instance.PlayAnimation(216, target);
					BattleManager.Instance.Heal(self, target, () => target.CurrentStats.MaxHP * 0.05f);
				}
			},
			hidden: true
		);
	}
}
