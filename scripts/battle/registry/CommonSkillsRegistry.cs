using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class CommonSkillsRegistry
{
	internal static void Register()
	{
		Skills["Guard"] = new Skill(
			name: "GUARD",
			description: "Acts first, reducing damage taken for 1 turn.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] guards.");
				await AnimationManager.Instance.WaitForAnimation(115, self);
				self.AddStatModifier("Guard");
			},
			priority: 2000
			// guard can always be used regardless of emotion
		).WithCustomRequirement(_ => true);

		Skills["Idle"] = new Skill(
			name: "Idle",
			description: "Idle",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor _, Actor _) => { await Task.CompletedTask; },
			hidden: true
		);
	}
}
