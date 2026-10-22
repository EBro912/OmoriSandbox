using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class OrangeOasisRegistry
{
	internal static void Register()
	{
		Enemies.Add("UnbreadTwins", () => new UnbreadTwins());
		Enemies.Add("UnbreadTwins (Epilogue)", () => new UnbreadTwinsAlt());
		Enemies.Add("BunBunny", () => new BunBunny());
		Enemies.Add("Creepypasta", () => new Creepypasta());
		Enemies.Add("Slice", () => new Slice());
		Enemies.Add("Slice (Epilogue)", () => new SliceAlt());
		Enemies.Add("Sourdough", () => new Sourdough());
		Enemies.Add("Sourdough (Epilogue)", () => new SourdoughAlt());
		Enemies.Add("Sesame", () => new Sesame());
		Enemies.Add("Sesame (Epilogue)", () => new SesameAlt());
		Enemies.Add("LivingBread", () => new LivingBread());
		Enemies.Add("NefariousChip", () => new NefariousChip());

		// Unbread Twins //
		Skills["UBTAttack"] = new Skill(
		   name: "UBTAttack",
		   description: "UBTAttack",
		   target: SkillTarget.Enemy,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] attack together!");
			   await AnimationManager.Instance.WaitForAnimation(124, target);
			   BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			   await Wait.Milliseconds(500);
			   BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
		   }
		);

		Skills["UBTDoNothing"] = new Skill(
		   name: "UBTDoNothing",
		   description: "UBTDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] forget something\nin the oven!");
			   await Task.CompletedTask;
		   }
		);

		Skills["UBTCheerUp"] = new Skill(
		   name: "UBTCheerUp",
		   description: "UBTCheerUp",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   await AnimationManager.Instance.WaitForAnimation(180, target);
			   BattleLogManager.Instance.QueueMessage(target, "[actor] do their best to not\nbe SAD.");
			   target.SetEmotion("neutral", true);
		   }
		);

		Skills["UBTCook"] = new Skill(
		   name: "UBTCook",
		   description: "UBTCook",
		   target: SkillTarget.Ally,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   await AnimationManager.Instance.WaitForAnimation(85, target);
			   int healed = BattleManager.Instance.Heal(self, target, () =>
			   {
				   if (self.CurrentJuice <= 0)
					   return 1;
				   return self.CurrentJuice * 0.4f;
			   }, 0f, silent: true);
			   BattleLogManager.Instance.QueueMessage(self, target, $"[actor] makes a cookie!\n[target] recovers {healed}\nHEART!");
			   await AnimationManager.Instance.WaitForAnimation(216, target);
		   },
		   hidden: true
		);

		Skills["UBTBakeBread"] = new Skill(
		   name: "UBTBakeBread",
		   description: "UBTBakeBread",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   await AnimationManager.Instance.WaitForAnimation(145, target);
			   BattleLogManager.Instance.QueueMessage(target,"[actor] pull out some\nBREAD from the oven!");
			   if (target is UnbreadTwins twins)
				   twins.SpawnBread();
			   if (target is UnbreadTwinsAlt twinsAlt)
				   twinsAlt.SpawnBread();
		   },
		   hidden: true
		);

		// Bun Bunny //
		Skills["BBAttack"] = new Skill(
		  name: "BBAttack",
		  description: "BBAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(122, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] bumps buns with [target]!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
		  }
		);

		Skills["BBDoNothing"] = new Skill(
		   name: "BBDoNothing",
		   description: "BBDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(target, "[actor] is loafing around.");
			   AudioManager.Instance.PlaySFX("BA_Drink", volume: 0.9f);
			   await Task.CompletedTask;
		   }
		);

		Skills["BBHide"] = new Skill(
		   name: "BBHide",
		   description: "BBHide",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   await AnimationManager.Instance.WaitForAnimation(178, target);
			   BattleLogManager.Instance.QueueMessage(target, "[actor] hides in its bun.");
			   target.AddStatModifier("Guard");
		   },
		   priority: 2000
		);

		// Creepypasta //
		Skills["CPAttack"] = new Skill(
		  name: "CPAttack",
		  description: "CPAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] makes [target] feel\nuncomfortable.");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
		  }
		);

		Skills["CPDoNothing"] = new Skill(
		   name: "CPDoNothing",
		   description: "CPDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(target, "[actor] does nothing... menacingly!");
			   AudioManager.Instance.PlaySFX("SE_evil5", volume: 0.9f);
			   await Task.CompletedTask;
		   }
		);

		Skills["CPScare"] = new Skill(
		   name: "CPScare",
		   description: "CPScare",
		   target: SkillTarget.AllEnemies,
		   cost: 0,
		   effect: async (self, targets) =>
		   {
			   AnimationManager.Instance.PlayAnimation(195, self);
			   BattleLogManager.Instance.QueueMessage(self, "[actor] shows everyone their\nworst nightmares!");
			   await Wait.Milliseconds(1500);
			   foreach (Actor member in targets)
			   {
				   if (member.SetEmotion("afraid", true))
					   BattleLogManager.Instance.QueueMessage(member, "[actor] feels AFRAID...");
			   }
		   }
		);

		// Slice //
		Skills["SLAttack"] = new Skill(
		  name: "SLAttack",
		  description: "SLAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] charges into [target].");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, neverCrit: true);
		  }
		);

		Skills["SLDoNothing"] = new Skill(
		   name: "SLDoNothing",
		   description: "SLDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(target, "[actor] picks its nose.");
			   AudioManager.Instance.PlaySFX("BA_Drink", volume: 0.9f);
			   await Task.CompletedTask;
		   }
		);

		Skills["SLRile"] = new Skill(
		   name: "SLRile",
		   description: "SLRile",
		   target: SkillTarget.AllAllies,
		   cost: 0,
		   effect: async (self, targets) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, "[actor] gives a controversial speech!\nEveryone feels ANGRY!");
			   foreach (Actor enemy in targets)
			   {
				   BattleManager.Instance.MakeEmotionTier(enemy, "angry", 1, true);
				   enemy.AddTierStatModifier("AttackUp", silent: true);
			   }
			   await Task.CompletedTask;
		   },
		   hidden: true
		).WithTiersFromFirstTarget();

		// Sourdough //
		Skills["SDAttack"] = new Skill(
		  name: "SDAttack",
		  description: "SDAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] steps on [target]'s toe!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
		  }
		);

		Skills["SDDoNothing"] = new Skill(
		   name: "SDDoNothing",
		   description: "SDDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(target, "[actor] kicks some dirt.");
			   AudioManager.Instance.PlaySFX("BA_INK", volume: 0.9f);
			   await Task.CompletedTask;
		   }
		);

		Skills["SDBadWord"] = new Skill(
		   name: "SDBadWord",
		   description: "SDBadWord",
		   target: SkillTarget.Enemy,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   AnimationManager.Instance.PlayAnimation(188, self);
			   BattleLogManager.Instance.QueueMessage(self, target, "Oh no! [actor] says a bad word!");
			   await Wait.Milliseconds(1500);
			   BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK, false, neverCrit: true);
		   }
		);

		// Sesame //
		Skills["SESAttack"] = new Skill(
		  name: "SESAttack",
		  description: "SESAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws seeds at [target].");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
		  }
		);

		Skills["SESDoNothing"] = new Skill(
		   name: "SESDoNothing",
		   description: "SESDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(target, "[actor] scratches their head.");
			   AudioManager.Instance.PlaySFX("BA_do_nothing_dance", volume: 0.9f);
			   await Task.CompletedTask;
		   }
		);

		Skills["SESBreadRoll"] = new Skill(
		   name: "SESBreadRoll",
		   description: "SESBreadRoll",
		   target: SkillTarget.AllEnemies,
		   cost: 0,
		   effect: async (self, targets) =>
		   {
			   await AnimationManager.Instance.WaitForScreenAnimation(207, false);
			   BattleLogManager.Instance.QueueMessage(self, "[actor] rolls over everyone!");
			   foreach (Actor member in targets)
				   BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false, neverCrit: true);
		   }
		);

		// Living Bread //
		Skills["LBAttack"] = new Skill(
		  name: "LBAttack",
		  description: "LBAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] swipes at [target]!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
		  }
		);

		Skills["LBDoNothing"] = new Skill(
		   name: "LBDoNothing",
		   description: "LBDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] slowly inches toward\n[target]!");
			   AudioManager.Instance.PlaySFX("BA_do_nothing_space_out", volume: 0.9f);
			   await Task.CompletedTask;
		   }
		);

		Skills["LBBite"] = new Skill(
		   name: "LBBite",
		   description: "LBBite",
		   target: SkillTarget.Enemy,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   await AnimationManager.Instance.WaitForAnimation(157, target);
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] bites [target]!");
			   BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false, neverCrit: true);
		   }
		);

		// Nefarious Chip //
		Skills["NCAttack"] = new Skill(
		  name: "NCAttack",
		  description: "NCAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(123, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] charges into [target]!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
		  }
		);

		Skills["NCDoNothing"] = new Skill(
		  name: "NCDoNothing",
		  description: "NCDoNothing",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (_, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(target, "[actor] strokes his evil\nmoustache!");
			  await Task.CompletedTask;
		  }
		);

		Skills["NCLaugh"] = new Skill(
		  name: "NCLaugh",
		  description: "NCLaugh",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: (_, _) => Task.CompletedTask,
		  setup: async (_, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(162, target);
			  BattleLogManager.Instance.QueueMessage(target, "[actor] laughs like the evil\nvillain he is!");
			  BattleManager.Instance.MakeHappy(target);
		  }
		);

		Skills["NCCookies"] = new Skill(
		  name: "NCCookies",
		  description: "NCCookies",
		  target: SkillTarget.XRandomEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, "[actor] throws OATMEAL\nCOOKIES!");
			  foreach (Actor member in targets)
			  {
				  BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
			  }
			  await AnimationManager.Instance.WaitForScreenAnimation(196, targets[0] is Enemy);
		  },
		  hidden: true
		);

		Skills["NCCookiesHappy"] = new Skill(
		  name: "NCCookiesHappy",
		  description: "NCCookiesHappy",
		  target: SkillTarget.XRandomEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self,"[actor] launches OATMEAL\nCOOKIES!");
			  foreach (Actor member in targets)
			  {
				  BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
			  }
			  await AnimationManager.Instance.WaitForScreenAnimation(196, targets[0] is Enemy);
		  },
		  hidden: true
		);
	}
}
