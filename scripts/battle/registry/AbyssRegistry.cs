using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class AbyssRegistry
{
	internal static void Register()
	{
		Enemies.Add("Abbi", () => new Abbi());
		Enemies.Add("Tentacle", () => new Tentacle());
		Enemies.Add("Merci (Abyss)", () => new MerciAbyss());
		Enemies.Add("Angi", () => new Angi());

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
						// DIALOGUE QUIRK: a failed luck roll still prints "feels AFRAID." for anyone who can be AFRAID
						if (RollLuckStateChance(self, target, 1) ? target.SetEmotion("afraid", true) : target.IsEmotionValid("afraid"))
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

		// MERCI //
		Skills["MERAttack"] = new Skill(
			name: "MERAttack",
			description: "MERAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(144, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] touches [target]'s chest.");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] feels their organs being torn!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["MERMelody"] = new Skill(
			name: "MERMelody",
			description: "MERMelody",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: (_, target) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(299, target is Enemy);
				BattleManager.Instance.MakeHappy(target, silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] sings a song.");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] hears a familiar melody.");
				// DIALOGUE QUIRK: MERCI has these hardcoded regardless of whether the raise was successful or not
				switch (target.CurrentEmotion.Id)
				{
					case "happy":
						BattleLogManager.Instance.QueueMessage(target, "[actor] feels HAPPY!");
						break;
					case "ecstatic":
						BattleLogManager.Instance.QueueMessage(target, "[actor] feels ECSTATIC!!");
						break;
					case "manic":
						BattleLogManager.Instance.QueueMessage(target, "[actor] feels MANIC!!!");
						break;
				}
				await Task.CompletedTask;
			}
		);

		Skills["MERScream"] = new Skill(
			name: "MERScream",
			description: "MERScream",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: (_, target) =>
			{
				AudioManager.Instance.PlaySFX("SE_bs_scare5");
				// VANILLA BUG: SCREAM clears HAPPY, SAD and ANGRY before its hit roll, so a miss still leaves the target NEUTRAL
				if (target.IsEmotionValid("afraid") && target.CurrentEmotion.Group?.Id is "happy" or "sad" or "angry")
					target.SetEmotion("neutral", true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] makes a horrifying shriek!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2, new DamageOptions
				{
					NeverMiss = false,
					OnHit = () =>
					{
						if (RollLuckStateChance(self, target, 1))
							target.SetEmotion("afraid", true);
					}
				});
				await Task.CompletedTask;
			}
		);

		Skills["MERLiar"] = new Skill(
			name: "MERLiar",
			description: "MERLiar",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "Liar.");
				await Task.CompletedTask;
			}
		);

		// ANGI //
		Skills["ANGAttack"] = new Skill(
			name: "ANGAttack",
			description: "ANGAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(129, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] bites [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF);
			}
		);

		Skills["ANGDoNothing"] = new Skill(
			name: "ANGDoNothing",
			description: "ANGDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("SE_evil5", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s stomach growls.");
				await Task.CompletedTask;
			}
		);

		Skills["ANGCrunch"] = new Skill(
			name: "ANGCrunch",
			description: "ANGCrunch",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(157, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] impales [target] with its teeth!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentEmotion.Id switch
				{
					"angry" => self.CurrentStats.ATK * 3,
					"enraged" => self.CurrentStats.ATK * 3 + 5,
					"furious" => self.CurrentStats.ATK * 4,
					_ => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF
				}, false, neverCrit: true);
			}
		);
	}
}
