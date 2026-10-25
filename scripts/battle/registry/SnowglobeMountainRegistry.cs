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
		Enemies.Add("SnowAngel", () => new SnowAngel());
		Enemies.Add("SnowBunny", () => new SnowBunny());
		Enemies.Add("SnowBunny (Easy)", () => new SnowBunnyEasy());
		Enemies.Add("SnowPile", () => new SnowPile());

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

		// SNOW ANGEL //
		Skills["SNAAttack"] = new Skill(
			name: "SNAAttack",
			description: "SNAAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] touches [target]");
				BattleLogManager.Instance.QueueMessage("with its cold hands.");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["SNAUpliftingHymn"] = new Skill(
			name: "SNAUpliftingHymn",
			description: "SNAUpliftingHymn",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(155, targets[0] is Enemy);
				BattleLogManager.Instance.QueueMessage(self, "[actor] sings a beautiful song...");
				// DIALOGUE QUIRK: prints "Everyone feels HAPPY!" even when a target's emotion can't move
				BattleLogManager.Instance.QueueMessage("Everyone feels HAPPY!");
				foreach (Actor member in targets)
					BattleManager.Instance.MakeHappy(member, silent: true);
				await Wait.Frames(60);
			}
		);

		Skills["SNAPierceHeart"] = new Skill(
			name: "SNAPierceHeart",
			description: "SNAPierceHeart",
			target: SkillTarget.Enemy,
			cost: 0,
			element: "emotion",
			effect: async (self, target) =>
			{
				Task anim = AnimationManager.Instance.WaitForAnimation(204, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] pierces [target]'s HEART.");
				bool hit = BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					new DamageOptions { NeverMiss = false, Crit = CritMode.Never }) >= 0;
				await anim;
				if (hit && target.CurrentHP > 0 && (target.CurrentEmotion.Group?.Id is "happy" or "sad" or "angry" || target.CurrentEmotion.Id == "afraid"))
				{
					target.SetEmotion("neutral", true);
					BattleLogManager.Instance.QueueMessage(target, "[actor] feels NEUTRAL.");
				}
				await Wait.Frames(20);
			}
		);

		// SNOW BUNNY //
		Skills["SNBAttack"] = new Skill(
			name: "SNBAttack",
			description: "SNBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] kicks snow at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["SNBDoNothing"] = new Skill(
			name: "SNBDoNothing",
			description: "SNBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_space_out", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] is chilling out.");
				await Task.CompletedTask;
			}
		);

		Skills["SNBSmallSnowStorm"] = new Skill(
			name: "SNBSmallSnowStorm",
			description: "SNBSmallSnowStorm",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] creates the world's tiniest");
				BattleLogManager.Instance.QueueMessage("snowstorm!");
				await AnimationManager.Instance.WaitForScreenAnimation(173, targets[0] is Enemy);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		// SNOW PILE //
		Skills["SNPAttack"] = new Skill(
			name: "SNPAttack",
			description: "SNPAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws snow at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["SNPDoNothing"] = new Skill(
			name: "SNPDoNothing",
			description: "SNPDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_space_out", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] is feeling frosty.");
				await Task.CompletedTask;
			}
		);

		Skills["SNPEngulf"] = new Skill(
			name: "SNPEngulf",
			description: "SNPEngulf",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: (_, target) =>
			{
				// VANILLA BUG: ENGULF's missing "end" skips DEFENSE DOWN while the target holds SPEED DOWN 1 or 2
				bool skipsDefense = target.GetStatModifierTier("SpeedDown") is 1 or 2;
				target.AddTierStatModifier("SpeedDown", silent: true);
				if (!skipsDefense)
					target.AddTierStatModifier("DefenseDown", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(205, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] engulfs [target] in snow!");
				// DIALOGUE QUIRK: names the user instead of the target in the SPEED and DEFENSE lines
				// DIALOGUE QUIRK: prints the SPEED and DEFENSE lines whether or not either stat changed
				BattleLogManager.Instance.QueueMessage(self, "[actor]'s SPEED fell.");
				BattleLogManager.Instance.QueueMessage(self, "[actor]'s DEFENSE fell.");
				// DIALOGUE QUIRK: logs no damage line
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					new DamageOptions { NeverMiss = false, Crit = CritMode.Never, Silent = true });
			}
		);

		Task moreSnowAnimation = Task.CompletedTask;
		Skills["SNPMoreSnow"] = new Skill(
			name: "SNPMoreSnow",
			description: "SNPMoreSnow",
			target: SkillTarget.Self,
			cost: 0,
			setup: async (_, target) =>
			{
				AnimationManager.Instance.PlayAnimation(216, target);
				await Wait.Frames(30);
				moreSnowAnimation = AnimationManager.Instance.WaitForAnimation(218, target);
				await Wait.Frames(30);
				target.AddTierStatModifier("AttackUp", silent: true);
				target.AddTierStatModifier("DefenseUp", silent: true);
			},
			effect: async (self, target) =>
			{
				await Wait.Frames(10);
				await moreSnowAnimation;
				BattleLogManager.Instance.QueueMessage(self, "[actor] piles snow on itself!");
				// DIALOGUE QUIRK: prints the ATTACK and DEFENSE lines even when the stats are already at their limit
				BattleLogManager.Instance.QueueMessage(self, "[actor]'s ATTACK rose!");
				BattleLogManager.Instance.QueueMessage(self, "[actor]'s DEFENSE rose!");
				BattleManager.Instance.Heal(self, target, () => 100, silent: true);
			}
		);
	}
}
