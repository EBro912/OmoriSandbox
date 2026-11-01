using System;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

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
		Enemies.Add("GingerdeadMan", () => new GingerdeadMan());
		Enemies.Add("Hushpuppy", () => new Hushpuppy());
		Enemies.Add("Porcupie", () => new Porcupie());
		Enemies.Add("Rabbit?", () => new RabbitQuestion());
		Enemies.Add("BunBunny (Dino's Dig)", () => new BunBunnyDinosDig());
		Enemies.Add("Celery", () => new Celery());
		Enemies.Add("Cilantro", () => new Cilantro());
		Enemies.Add("Ginger", () => new Ginger());
		Enemies.Add("SproutBunny", () => new SproutBunny());
		Enemies.Add("ToastGhost", () => new ToastGhost());
		Enemies.Add("CupcakeBunny", () => new CupcakeBunny());
		Enemies.Add("MilkshakeBunny", () => new MilkshakeBunny());
		Enemies.Add("PancakeBunny", () => new PancakeBunny());
		Enemies.Add("SSSnake", () => new SSSnake());
		Enemies.Add("Dorothi (Train)", () => new DorothiTrain());
		Enemies.Add("Copypasta", () => new Copypasta());
		Enemies.Add("Chicken?", () => new ChickenQuestion());
		Enemies.Add("Chicken? (Chicken Ball)", () => new ChickenQuestionBall());
		Enemies.Add("Chicken? (Weak)", () => new ChickenQuestionWeak());

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
			   AudioManager.Instance.PlaySFX("BA_Drink", volume: 0.9f);
			   BattleLogManager.Instance.QueueMessage(target, "[actor] is loafing around.");
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

		// GINGERDEAD MAN //
		Skills["GDMAttack"] = new Skill(
			name: "GDMAttack",
			description: "GDMAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["GDMDoNothing"] = new Skill(
			name: "GDMDoNothing",
			description: "GDMDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_dance", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s head falls off...");
				BattleLogManager.Instance.QueueMessage(target, "[actor] puts it back on.");
				await Task.CompletedTask;
			}
		);

		Skills["GDMBiteArm"] = new Skill(
			name: "GDMBiteArm",
			description: "GDMBiteArm",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] bites [target]'s arm.");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		// HUSHPUPPY //
		Skills["HSPAttack"] = new Skill(
			name: "HSPAttack",
			description: "HSPAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			element: "afraid",
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] rams into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["HSPDoNothing"] = new Skill(
			name: "HSPDoNothing",
			description: "HSPDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("SE_bs_popping", 1.2f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] tries to bark...");
				BattleLogManager.Instance.QueueMessage("But nothing happened...");
				await Task.CompletedTask;
			}
		);

		Skills["HSPMuffledScreams"] = new Skill(
			name: "HSPMuffledScreams",
			description: "HSPMuffledScreams",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("SE_bs_popping", 0.6f, 0.9f);
				BattleLogManager.Instance.QueueMessage(self, "[actor] starts screaming!");
				bool canBeAfraid = target.IsEmotionValid("afraid");
				target.SetEmotion("afraid", true);
				if (canBeAfraid)
					BattleLogManager.Instance.QueueMessage(target, "[actor] feels AFRAID.");
				else
					BattleManager.Instance.ShowEmotionNoEffect(target, "afraid");
				await Task.CompletedTask;
			}
		);

		// PORCUPIE //
		Skills["PCPDoNothing"] = new Skill(
			name: "PCPDoNothing",
			description: "PCPDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_falls_over", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] sniffs around.");
				await Task.CompletedTask;
			}
		);

		Skills["PCPPierce"] = new Skill(
			name: "PCPPierce",
			description: "PCPPierce",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] impales [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3, false, neverCrit: true);
			}
		);

		// RABBIT? //
		Skills["RBQAttack"] = new Skill(
			name: "RBQAttack",
			description: "RBQAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] nibbles at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["RBQDoNothing"] = new Skill(
			name: "RBQDoNothing",
			description: "RBQDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] is hopping around!");
				await Task.CompletedTask;
			}
		);

		Skills["RBQBeCute"] = new Skill(
			name: "RBQBeCute",
			description: "RBQBeCute",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: (_, target) =>
			{
				target.AddStatModifier("AttackDown", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(148, self);
				await AnimationManager.Instance.WaitForAnimation(215, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] winks at [target]!");
				// DIALOGUE QUIRK: prints the ATTACK line even when the stat is already at its limit
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s ATTACK fell...");
			}
		);

		Skills["RBQSadEyes"] = new Skill(
			name: "RBQSadEyes",
			description: "RBQSadEyes",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(149, target);
				BattleManager.Instance.MakeSad(target, silent: true);
			},
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks sadly at [target].");
				// DIALOGUE QUIRK: always prints the tier-1 SAD line regardless of the tier the target reaches
				QueueTierOneEmotionLine(target, "sad", target.IsEmotionValid("sad"));
				await Task.CompletedTask;
			}
		);

		// CELERY //
		Skills["CELAttack"] = new Skill(
			name: "CELAttack",
			description: "CELAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] rams into [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["CELDoNothing"] = new Skill(
			name: "CELDoNothing",
			description: "CELDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] falls over.");
				await Task.CompletedTask;
			}
		);

		// CILANTRO //
		Skills["CILAttack"] = new Skill(
			name: "CILAttack",
			description: "CILAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] whacks [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF);
			}
		);

		Skills["CILDoNothing"] = new Skill(
			name: "CILDoNothing",
			description: "CILDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] contemplates life.");
				await Task.CompletedTask;
			}
		);

		// GINGER //
		Skills["GINAttack"] = new Skill(
			name: "GINAttack",
			description: "GINAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] snaps and attacks [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["GINDoNothing"] = new Skill(
			name: "GINDoNothing",
			description: "GINDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] finds inner peace.");
				await Task.CompletedTask;
			}
		);

		Skills["GINSoothe"] = new Skill(
			name: "GINSoothe",
			description: "GINSoothe",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(180, target);
				target.SetEmotion("neutral", true);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] calms down [target].");
			}
		);

		// SPROUT BUNNY //
		Skills["SPRBAttack"] = new Skill(
			name: "SPRBAttack",
			description: "SPRBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(126, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slaps [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["SPRBDoNothing"] = new Skill(
			name: "SPRBDoNothing",
			description: "SPRBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] nibbles on some grass.");
				await Task.CompletedTask;
			}
		);

		Skills["SPRBFeed"] = new Skill(
			name: "SPRBFeed",
			description: "SPRBFeed",
			target: SkillTarget.Ally,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(216, target);
				int healed = 0;
				if (BattleManager.Instance.RollHit(self, target, true))
					healed = BattleManager.Instance.Heal(self, target, () => target.CurrentStats.MaxHP * 0.25f, silent: true);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] feeds [target].");
				// DIALOGUE QUIRK: names the user instead of the target in the recovers line
				BattleLogManager.Instance.QueueMessage(self, $"[actor] recovers {healed} HEART!");
			}
		);

		// TOAST GHOST //
		Skills["TGAttack"] = new Skill(
			name: "TGAttack",
			description: "TGAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			priority: 2000,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] phases through [target]!");
				BattleLogManager.Instance.QueueMessage(target, "[actor] feels tired.");
				int drain = -(int)Math.Floor(target.CurrentStats.MaxJuice * -0.25f);
				if (BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, new DamageOptions { NeverMiss = false, Crit = CritMode.Never, ShowJuiceLoss = drain == 0 }) >= 0 && drain > 0)
				{
					int lost = target.DamageJuice(drain);
					BattleLogManager.Instance.QueueMessage(self, target, "[target] lost " + lost + " JUICE...");
					if (!target.IsToast)
						BattleManager.Instance.SpawnDamageNumber(lost, target, DamageType.JuiceLoss);
				}
			}
		);

		Skills["TGDoNothing"] = new Skill(
			name: "TGDoNothing",
			description: "TGDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] makes a spooky noise.");
				await Task.CompletedTask;
			},
			setup: (_, _) =>
			{
				AudioManager.Instance.PlaySFX("SE_creepy_tune");
				return Task.CompletedTask;
			}
		);

		// CUPCAKE BUNNY //
		Skills["CCBAttack"] = new Skill(
			name: "CCBAttack",
			description: "CCBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] bumps into [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["CCBDoNothing"] = new Skill(
			name: "CCBDoNothing",
			description: "CCBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] hops in place.");
				await Task.CompletedTask;
			}
		);

		bool sprinklesHappy = false;
		Skills["CCBSprinkles"] = new Skill(
			name: "CCBSprinkles",
			description: "CCBSprinkles",
			target: SkillTarget.Ally,
			cost: 0,
			setup: (_, target) =>
			{
				sprinklesHappy = BattleManager.Instance.MakeHappy(target, silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(276, target);
				BattleManager.Instance.Heal(self, target, () => target.CurrentStats.MaxHP * 0.5f, silent: true);
				AddFixedTierStatModifier(target, "AttackUp", 1);
				AddFixedTierStatModifier(target, "DefenseUp", 1);
				AddFixedTierStatModifier(target, "SpeedUp", 1);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] covers [target]");
				BattleLogManager.Instance.QueueMessage("in sprinkles.");
				// DIALOGUE QUIRK: always prints the tier-1 HAPPY line regardless of the tier the target reaches
				QueueTierOneEmotionLine(target, "happy", sprinklesHappy);
				// DIALOGUE QUIRK: prints the "STATS rose!" line even when the UP states are already held at tier 1 or higher
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s STATS rose!");
				await AnimationManager.Instance.WaitForAnimation(218, target);
			}
		);

		// MILKSHAKE BUNNY //
		Skills["MSBAttack"] = new Skill(
			name: "MSBAttack",
			description: "MSBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] spills milkshake on [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["MSBDoNothing"] = new Skill(
			name: "MSBDoNothing",
			description: "MSBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] spins in a circle.");
				await Task.CompletedTask;
			}
		);

		Skills["MSBShake"] = new Skill(
			name: "MSBShake",
			description: "MSBShake",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] begins shaking!");
				BattleLogManager.Instance.QueueMessage("Milkshake flies everywhere!");
				await Task.WhenAll(targets.Select(member => AnimationManager.Instance.WaitForAnimation(123, member)));
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		// PANCAKE BUNNY //
		Skills["PCBAttack"] = new Skill(
			name: "PCBAttack",
			description: "PCBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] nibbles on [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["PCBDoNothing"] = new Skill(
			name: "PCBDoNothing",
			description: "PCBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] does a flip!");
				BattleLogManager.Instance.QueueMessage("So talented!");
				await Task.CompletedTask;
			}
		);

		// S.S. SNAKE //
		Skills["SSSAttack"] = new Skill(
			name: "SSSAttack",
			description: "SSSAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(157, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] sinks its fangs into [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["SSSDoNothing"] = new Skill(
			name: "SSSDoNothing",
			description: "SSSDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] hisses.");
				await Task.CompletedTask;
			}
		);

		bool slitherHappy = false;
		Skills["SSSSlither"] = new Skill(
			name: "SSSSlither",
			description: "SSSSlither",
			target: SkillTarget.Self,
			cost: 0,
			setup: (_, target) =>
			{
				slitherHappy = BattleManager.Instance.MakeHappy(target, silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(218, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] slithers around gleefully!");
				// DIALOGUE QUIRK: always prints the tier-1 HAPPY line regardless of the tier the target reaches
				QueueTierOneEmotionLine(target, "happy", slitherHappy);
			}
		);

		// DOROTHI (TRAIN) //
		Skills["DRTLiar"] = new Skill(
			name: "DRTLiar",
			description: "DRTLiar",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "Liar.");
				await Task.CompletedTask;
			}
		);

		// COPYPASTA //
		Skills["COPAttack"] = new Skill(
			name: "COPAttack",
			description: "COPAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] bops [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["COPDuplicate"] = new Skill(
			name: "COPDuplicate",
			description: "COPDuplicate",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(145, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] copies itself!");
				if (self is Enemy enemy)
					ReinforcementSlots.Summon(enemy, ReinforcementSlots.Copypasta);
			}
		);

		// CHICKEN? //
		Skills["CHKRunAway"] = new Skill(
			name: "CHKRunAway",
			description: "CHKRunAway",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] runs away.");
				// VANILLA BUG: RUN AWAY's "wait:120" has no space, so the base never waits after it
				if (target is Enemy enemy)
					await enemy.Escape();
			}
		);

		Skills["CHKDoNothing"] = new Skill(
			name: "CHKDoNothing",
			description: "CHKDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_INK", 0.7f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] clucks.");
				await Task.CompletedTask;
			}
		);
	}
}
