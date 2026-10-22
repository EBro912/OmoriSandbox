using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class OtherworldRegistry
{
	internal static void Register()
	{
		Enemies.Add("SpaceExBoyfriend", () => new SpaceExBoyfriend());
		Enemies.Add("TheEarth", () => new TheEarth());
		Enemies.Add("TheEarth (Pluto)", () => new TheEarthAlt());
		Enemies.Add("PlutoExpandedAndEarth", () => new PlutoExpandedEarth());
		Enemies.Add("Pluto", () => new Pluto());
		Enemies.Add("LeftArm", () => new LeftArm());
		Enemies.Add("RightArm", () => new RightArm());

		// SPACE EX BOYFRIEND //
		Skills["SEBAttack"] = new Skill(
			name: "SEBAttack",
			description: "SEBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] kicks [target]!");
				BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK * 2) + 5 - target.CurrentStats.DEF, false);
			}
		);

		Skills["SEBDoNothing"] = new Skill(
			name: "SEBDoNothing",
			description: "SEBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_,target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] looks wistfully\ninto the distance.");
				await Task.CompletedTask;
			}
		);

		Skills["AngstySong"] = new Skill(
			name: "AngstySong",
			description: "AngstySong",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] sings sadly...");
				await AnimationManager.Instance.WaitForScreenAnimation(154, target is Enemy);
				BattleManager.Instance.MakeSad(target, silent: true);
				if (target.CurrentEmotion.Group?.Id == "sad")
					BattleLogManager.Instance.QueueMessage(target.CurrentEmotion.FeelsMessage(target.Name));
			}
		);

		Skills["AngrySong"] = new Skill(
			name: "AngrySong",
			description: "AngrySong",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] wails loudly!");
				await AnimationManager.Instance.WaitForScreenAnimation(153, targets[0] is Enemy);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
				}
			}
		);

		Skills["SpaceLaser"] = new Skill(
			name: "SpaceLaser",
			description: "SpaceLaser",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(160, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] fires his laser!");
				BattleManager.Instance.Damage(self, target, () => (self.CurrentStats.ATK * 2.5f) - target.CurrentStats.DEF, false);
			}
		);

		Skills["BulletHell"] = new Skill(
			name: "BulletHell",
			description: "BulletHell",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] fires wildly!");
				await AnimationManager.Instance.WaitForScreenAnimation(168, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 20, false, neverCrit: true);
				}
			},
			hidden: true
		);
		
		Skills["BRBulletHell"] = new Skill(
			name: "BRBulletHell",
			description: "BRBulletHell",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] fires wildly!");
				await AnimationManager.Instance.WaitForScreenAnimation(168, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 100, false, neverCrit: true);
				}
			},
			hidden: true
		);

		// Earth //
		Skills["TEAttack"] = new Skill(
		  name: "TEAttack",
		  description: "TEAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(124, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
		  }
		);

		Skills["TEDoNothing"] = new Skill(
		  name: "TEDoNothing",
		  description: "TEDoNothing",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (_, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(target, "[actor] is rotating slowly.");
			  await Task.CompletedTask;
		  }
		);

		Skills["TECruel"] = new Skill(
		  name: "TECruel",
		  description: "TECruel",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: (_, _) => Task.CompletedTask,
		  setup: async (self, target) =>
		  {
			  AnimationManager.Instance.PlayScreenAnimation(169, target is Enemy);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] is cruel to [target]!");
			  BattleManager.Instance.MakeSad(target, silent: true);
			  if (target.CurrentEmotion.Group?.Id == "sad")
				  BattleLogManager.Instance.QueueMessage(target.CurrentEmotion.FeelsMessage(target.Name));
			  await Task.CompletedTask;
		  }
		);

		Skills["TEProtect"] = new Skill(
		  name: "TEProtect",
		  description: "TEProtect",
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
			  BattleLogManager.Instance.QueueMessage(self, "[actor] uses her ultimate attack!");
			  AnimationManager.Instance.PlayScreenAnimation(170, targets[0] is Enemy);
			  await Wait.Milliseconds(1000);
			  foreach (Actor member in targets)
				  BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
		  }
		);

		// Earth Alt //
		Skills["TEACruel"] = new Skill(
			name: "TEACruel",
			description: "TEACruel",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(169, targets[0] is Enemy);
				await Wait.Milliseconds(2000);
				BattleLogManager.Instance.QueueMessage(self, targets[0], "[actor] is cruel to everyone...");
				foreach (Actor target in targets)
					BattleManager.Instance.MakeSad(target, silent: true);
				BattleLogManager.Instance.QueueMessage("Everyone feels SAD.");
			}
		).WithTiersFromFirstTarget();
		
		Skills["TEAProtect"] = new Skill(
			name: "TEAProtect",
			description: "TEAProtect",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] unleashes her energy!");
				await AnimationManager.Instance.WaitForScreenAnimation(170, targets[0] is Enemy);
				foreach (Actor member in targets)
					BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
			}
		);

		// Pluto //
		Skills["PLDoNothing"] = new Skill(
			name: "PLDoNothing",
			description: "PLDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] strikes a pose!");
				await Task.CompletedTask;
			}
		);
		
		Skills["PLHeadbutt"] = new Skill(
			name: "PLHeadbutt",
			description: "PLHeadbutt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] bolts forward and slams [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["PLBrag"] = new Skill(
			name: "PLBrag",
			description: "PLBrag",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(162, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] brags about his muscles!");
				BattleManager.Instance.MakeHappy(target);
			}
		);
		
		Skills["PLExpand"] = new Skill(
			name: "PLExpand",
			description: "PLExpand",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] expands.");
				await AnimationManager.Instance.WaitForAnimation(292, target);
				target.AddTierStatModifier("AttackUp", 2);
				target.AddTierStatModifier("DefenseUp", 2);
				target.AddTierStatModifier("SpeedDown", 2);
				AnimationManager.Instance.PlayAnimation(218, target);
				await Wait.Milliseconds(1000);
			}
		);

		// Right Arm //
		Skills["RAAttack"] = new Skill(
			name: "RAAttack",
			description: "RAAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] chops [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		
		Skills["RAFlex"] = new Skill(
			name: "RAFlex",
			description: "RAFlex",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] flexes and feels his best!");
				BattleLogManager.Instance.QueueMessage(self, target, "[actor]'s HIT RATE rose!");
				await AnimationManager.Instance.WaitForAnimation(218, self);
				self.AddStatModifier("Flex");
			}
		);
		
		Skills["RAGrab"] = new Skill(
			name: "RAGrab",
			description: "RAGrab",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] grabs [target]!");
				await AnimationManager.Instance.WaitForAnimation(164, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, true, 0.4f, neverCrit: true);
				target.AddTierStatModifier("SpeedDown");
			}
		);

		// Left Arm //
		Skills["LAAttack"] = new Skill(
			name: "LAAttack",
			description: "LAAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] punches [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["LAPoke"] = new Skill(
			name: "LAPoke",
			description: "LAPoke",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] pokes [target]!");
				await AnimationManager.Instance.WaitForAnimation(163, target);
				BattleManager.Instance.MakeAngry(target);
			},
			effect: async (self, target) =>
			{
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
				await Task.CompletedTask;
			}
		);
	}
}
