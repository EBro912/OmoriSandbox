using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class VastForestRegistry
{
	internal static void Register()
	{
		Enemies.Add("ForestBunny?", () => new ForestBunnyQuestion());
		Enemies.Add("BigStrongTree", () => new BigStrongTree());
		Enemies.Add("Boss", () => new Boss());
		Enemies.Add("YeOldSprout", () => new YeOldSprout());
		Enemies.Add("KiteKid", () => new KiteKid());
		Enemies.Add("KiteKid (Epilogue)", () => new KiteKidAlt());
		Enemies.Add("KidsKite", () => new KidsKite());
		Enemies.Add("KidsKite (Epilogue)", () => new KidsKiteAlt());
		Enemies.Add("ForestBunny", () => new ForestBunny());

		// FOREST BUNNY? //
		Skills["FBQAttack"] = new Skill(
			name: "Attack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] nibbles at [target]?");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false);
			}
		);

		Skills["FBQDoNothing"] = new Skill(
			name: "Do Nothing",
			description: "Does nothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_falls_over");
				BattleLogManager.Instance.QueueMessage(target, "[actor] is hopping around?");
				await Task.CompletedTask;
			}
		);

		Skills["FBQBeCute"] = new Skill(
			name: "Be Cute",
			description: "Be Cute",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] winks at [target]?");
				await AnimationManager.Instance.WaitForAnimation(148, self);
				await AnimationManager.Instance.WaitForAnimation(215, target);
				target.AddStatModifier("AttackDown", silent: true);
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s ATTACK fell?");
			}
		);

		Skills["FBQSadEyes"] = new Skill(
			name: "Sad Eyes",
			description: "Sad Eyes",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks sadly at [target]...");
				await AnimationManager.Instance.WaitForAnimation(149, self);
				Emotion before = target.CurrentEmotion;
				BattleManager.Instance.MakeSad(target, silent: true);
				BattleLogManager.Instance.QueueMessage(target, before == target.CurrentEmotion ? "[actor] can't get SADDER!" : "[actor] feels SAD?");
			}
		);

		// BIG STRONG TREE //
		Skills["BSTDoNothing"] = new Skill(
			name: "BSTDoNothing",
			description: "BSTDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor _, Actor _) =>
			{
				BattleLogManager.Instance.QueueMessage("A gentle breeze blows across the leaves.");
				await Task.CompletedTask;
			}
		);

		// Boss //
		Skills["BSSAttack"] = new Skill(
		  name: "BSSAttack",
		  description: "BSSAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(139, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] punches [target]!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
		  }
		);

		Skills["BSSAttackTwice"] = new Skill(
		  name: "BSSAttackTwice",
		  description: "BSSAttackTwice",
		  target: SkillTarget.XRandomEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(139, targets[0]);
			  BattleLogManager.Instance.QueueMessage(self, targets[0], "[actor] punches [target]!");
			  BattleManager.Instance.Damage(self, targets[0], () => self.CurrentStats.ATK * 2 - targets[0].CurrentStats.DEF, false);
			  await Wait.Milliseconds(1000);
			  await AnimationManager.Instance.WaitForAnimation(139, targets[1]);
			  BattleLogManager.Instance.QueueMessage(self, targets[1], "[actor] punches [target]!");
			  BattleManager.Instance.Damage(self, targets[1], () => self.CurrentStats.ATK * 2 - targets[1].CurrentStats.DEF, false);
		  },
		  hidden: true
		);

		Skills["BSSDoNothing"] = new Skill(
		  name: "BSSDoNothing",
		  description: "BSSDoNothing",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (_, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(target, "[actor] cracks his knuckles.");
			  await Task.CompletedTask;
		  }
		);

		Skills["BSSAttackAll"] = new Skill(
		  name: "BSSAttackAll",
		  description: "BSSAttackAll",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  foreach (Actor member in targets)
			  {
				  await Wait.Milliseconds(1000);
				  BattleManager.Instance.Damage(self, member, () => 100, true, 0f, neverCrit: true);
			  }
		  }
		);

		// Ye Old Sprout //
		Skills["YOSRollOver"] = new Skill(
		  name: "YOSRollOver",
		  description: "YOSRollOver",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, "[actor] rolls over!");
			  foreach (Actor member in targets)
			  {
				  AnimationManager.Instance.PlayAnimation(124, member);
				  BattleManager.Instance.Damage(self, member, () => 4, false, 0.5f, neverCrit: true);
			  }
			  await Task.CompletedTask;
		  }
		);

		// Kite Kid //
		Skills["KKAttack"] = new Skill(
			name: "KKAttack",
			description: "KKAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws JACKS at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["KKBrag"] = new Skill(
			name: "KKBrag",
			description: "KKBrag",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(162, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] brags about KID's KITE!");
				BattleManager.Instance.MakeHappy(target);
			}
		);

		// Kid's Kite //
		Skills["KSKAttack"] = new Skill(
			name: "KSKAttack",
			description: "KSKAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] dives at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["KSKDoNothing"] = new Skill(
			name: "KSKDoNothing",
			description: "KSKDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] puffs its chest proudly!");
				await Task.CompletedTask;
			}
		);
		
		Skills["KSKFly"] = new Skill(
			name: "KSKFly",
			description: "KSKFly",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] swoops down!");
				await Wait.Milliseconds(1000);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(123, member);
					BattleManager.Instance.Damage(self, member,
						() => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
				}
			}
		);

		// FOREST BUNNY //
		Skills["FBAttack"] = new Skill(
			name: "FBAttack",
			description: "FBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] nibbles at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["FBDoNothing"] = new Skill(
			name: "FBDoNothing",
			description: "FBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_falls_over", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] is hopping around!");
				await Task.CompletedTask;
			}
		);

		Skills["FBBeCute"] = new Skill(
			name: "FBBeCute",
			description: "FBBeCute",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				target.AddStatModifier("AttackDown", silent: true);
				await AnimationManager.Instance.WaitForAnimation(148, self);
				await AnimationManager.Instance.WaitForAnimation(215, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] winks at [target]!");
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s ATTACK fell...");
			}
		);

		Skills["FBSadEyes"] = new Skill(
			name: "FBSadEyes",
			description: "FBSadEyes",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks sadly at [target].");
				bool canBeSad = target.IsEmotionValid("sad");
				BattleManager.Instance.MakeSad(target, silent: true);
				// DIALOGUE QUIRK: always prints the tier-1 SAD line regardless of the tier the target reaches
				if (canBeSad)
					BattleLogManager.Instance.QueueMessage(target, "[actor] feels SAD.");
				else
					BattleManager.Instance.ShowEmotionNoEffect(target, "sad");
				await Task.CompletedTask;
			},
			setup: async (self, _) =>
			{
				await AnimationManager.Instance.WaitForAnimation(149, self);
			}
		);
	}
}
