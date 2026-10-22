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
	}
}
