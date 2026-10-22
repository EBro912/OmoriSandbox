using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class SnowglobeMountainRegistry
{
	internal static void Register()
	{
		Enemies.Add("SpaceExHusband", () => new SpaceExHusband());

		// Space Ex-Husband //
		Skills["SEHAttack"] = new Skill(
		  name: "SEHAttack",
		  description: "SEHAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] swiftly kicks [target]!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
		  }
		);

		Skills["SEHLaser"] = new Skill(
		  name: "SEHLaser",
		  description: "SEHLaser",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(160, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] fires his laser!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
		  }
		);

		Skills["SEHAngrySong"] = new Skill(
		  name: "SEHAngrySong",
		  description: "SEHAngrySong",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  AnimationManager.Instance.PlayScreenAnimation(153, false);
			  BattleLogManager.Instance.QueueMessage(self, "[actor] wails with all his rage!\nEveryone feels ANGRY!");
			  foreach (Actor member in targets)
			  {
				  BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
				  BattleManager.Instance.MakeAngry(member, silent: true);
			  }
			  await Task.CompletedTask;
		  }
		);

		Skills["SEHAngstySong"] = new Skill(
		  name: "SEHAngstySong",
		  description: "SEHAngstySong",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  AnimationManager.Instance.PlayScreenAnimation(154, false);
			  BattleLogManager.Instance.QueueMessage(self, "[actor] sings with all the darkness\nin his soul!\nEveryone feels SAD.");
			  foreach (Actor member in targets)
			  {
				  BattleManager.Instance.DamageJuice(self, member, () => member.CurrentStats.MaxJuice * 0.25f, false);
				  BattleManager.Instance.MakeSad(member, silent: true);
			  }
			  await Task.CompletedTask;
		  }
		);

		Skills["SEHJoyfulSong"] = new Skill(
		  name: "SEHJoyfulSong",
		  description: "SEHJoyfulSong",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  AnimationManager.Instance.PlayScreenAnimation(155, false);
			  BattleLogManager.Instance.QueueMessage(self, "[actor] sings with all the joy\nin his heart!\nEveryone feels HAPPY!");
			  foreach (Actor member in targets)
			  {
				  BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 + self.CurrentStats.LCK - member.CurrentStats.DEF, false);
				  BattleManager.Instance.MakeHappy(member, silent: true);
			  }
			  await Task.CompletedTask;
		  }
		);

		Skills["SEHSpinningKick"] = new Skill(
		  name: "SEHSpinningKick",
		  description: "SEHSpinningKick",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] does a spinning kick!");
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  for (int i = 0; i < 3; i++)
			  {
				  if (i > 0)
					  await Wait.Frames(8);
				  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			  }
		  }
		);

		Skills["SEHBulletHell"] = new Skill(
		  name: "SEHBulletHell",
		  description: "SEHBulletHell",
		  target: SkillTarget.XRandomEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, "[actor] fires wildly!");
			  AnimationManager.Instance.PlayScreenAnimation(168, false);
			  foreach (Actor member in targets)
			  {
				  BattleManager.Instance.Damage(self, member, () => 50, false);
				  await Wait.Milliseconds(250);
			  }
		  },
		  hidden: true
		);
	}
}
