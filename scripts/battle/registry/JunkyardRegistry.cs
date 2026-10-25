using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class JunkyardRegistry
{
	internal static void Register()
	{
		Enemies.Add("DownloadWindow", () => new DownloadWindow());
		Enemies.Add("DialUp", () => new DialUp());
		Enemies.Add("Doombox", () => new Doombox());
		Enemies.Add("Mixtape", () => new Mixtape());
		Enemies.Add("SharkPlane", () => new SharkPlane());

		// DOWNLOAD WINDOW //
		Skills["DWDoNothing1"] = new Skill(
			name: "DWDoNothing1",
			description: "DWDoNothing1",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] is at 99%.");
				await Task.CompletedTask;
			}
		);
		Skills["DWDoNothing2"] = new Skill(
			name: "DWDoNothing2",
			description: "DWDoNothing2",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] is still at 99%...");
				await Task.CompletedTask;
			}
		);
		Skills["Crash"] = new Skill(
			name: "Crash",
			description: "Crash",
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
				BattleLogManager.Instance.QueueMessage(self, "[actor] crashes and burns!");
				AnimationManager.Instance.PlayScreenAnimation(165, targets[0] is Enemy);
				await Wait.Milliseconds(3652);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => member.CurrentStats.MaxHP * 0.8f, false, 0f, false, true);
				}
			}
		);

		// DIAL-UP //
		Skills["DUAttack"] = new Skill(
			name: "DUAttack",
			description: "DUAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(125, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] is slow.");
				BattleLogManager.Instance.QueueMessage(self, target, "[target] hurts themself in frustration!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
				await Wait.Frames(30);
			}
		);

		Skills["DUDoNothing"] = new Skill(
			name: "DUDoNothing",
			description: "DUDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_dialtone", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] is buffering...");
				await Task.CompletedTask;
			}
		);

		Skills["DUSlowDownAll"] = new Skill(
			name: "DUSlowDownAll",
			description: "DUSlowDownAll",
			target: SkillTarget.AllEnemies,
			cost: 0,
			setup: (Actor self, IReadOnlyList<Actor> targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] slows down...");
				// DIALOGUE QUIRK: prints the "SPEED fell." line even when SPEED is already at its limit
				BattleLogManager.Instance.QueueMessage("Everyone's SPEED fell.");
				List<Actor> everybody = [.. BattleManager.Instance.GetAllAliveEnemies(), .. targets];
				BattleManager.Instance.BeginTiersFromFirstTarget(Skills["DUSlowDownAll"], everybody);
				foreach (Actor member in everybody)
					member.AddTierStatModifier("SpeedDown", silent: true);
				return Task.CompletedTask;
			},
			effect: async (Actor _, IReadOnlyList<Actor> targets) =>
			{
				foreach (Actor member in BattleManager.Instance.GetAllAliveEnemies().Cast<Actor>().Concat(targets))
					await AnimationManager.Instance.WaitForAnimation(220, member);
			}
		).WithTiersFromFirstTarget();

		// DOOMBOX //
		Skills["DMBSelfAngry"] = new Skill(
			name: "DMBSelfAngry",
			description: "DMBSelfAngry",
			target: SkillTarget.Self,
			cost: 0,
			setup: (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] becomes ANGRIER!");
				BattleManager.Instance.MakeAngry(target, silent: true);
				return Task.CompletedTask;
			},
			effect: (_, _) => Task.CompletedTask
		);

		Skills["DMBAttack"] = new Skill(
			name: "DMBAttack",
			description: "DMBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(125, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["DMBDoNothing"] = new Skill(
			name: "DMBDoNothing",
			description: "DMBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_INK", 0.7f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] is adjusting the radio.");
				await Task.CompletedTask;
			}
		);

		Skills["DMBBlastMusic"] = new Skill(
			name: "DMBBlastMusic",
			description: "DMBBlastMusic",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] drops some sick beats!");
				await AnimationManager.Instance.WaitForScreenAnimation(152, targets[0] is Enemy);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		// MIXTAPE //
		Skills["MXTAttack"] = new Skill(
			name: "MXTAttack",
			description: "MXTAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(125, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slaps [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["MXTDoNothing"] = new Skill(
			name: "MXTDoNothing",
			description: "MXTDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_INK", 0.7f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] is untangling itself.");
				await Task.CompletedTask;
			}
		);

		Skills["MXTTangle"] = new Skill(
			name: "MXTTangle",
			description: "MXTTangle",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: async (_, target) =>
			{
				AnimationManager.Instance.PlayAnimation(156, target);
				await Wait.Frames(90);
				AnimationManager.Instance.PlayAnimation(215, target);
				await Wait.Frames(30);
				target.AddTierStatModifier("SpeedDown", silent: true);
				await Wait.Frames(30);
			},
			effect: (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[target] becomes caught in [actor]!");
				// DIALOGUE QUIRK: prints the "SPEED fell..." line even when SPEED is already at its limit
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s SPEED fell...");
				return Task.CompletedTask;
			}
		);

		// SHARK PLANE //
		Skills["SHPAttack"] = new Skill(
			name: "SHPAttack",
			description: "SHPAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] rams into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["SHPDoNothing"] = new Skill(
			name: "SHPDoNothing",
			description: "SHPDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_space_out", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] is picking its teeth.");
				await Task.CompletedTask;
			}
		);

		Skills["SHPOverclockEngines"] = new Skill(
			name: "SHPOverclockEngines",
			description: "SHPOverclockEngines",
			target: SkillTarget.Self,
			cost: 0,
			setup: (_, target) =>
			{
				// VANILLA BUG: OVERCLOCK ENGINES' second whole-action block replaces the first, so it never raises SPEED
				BattleManager.Instance.MakeAngry(target, silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(218, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] revs its engine!");
				BattleLogManager.Instance.QueueMessage(self, target, "[actor]'s SPEED rose!");
			}
		);

		Skills["SHPChomp"] = new Skill(
			name: "SHPChomp",
			description: "SHPChomp",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(157, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] chomps [target]!");
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
