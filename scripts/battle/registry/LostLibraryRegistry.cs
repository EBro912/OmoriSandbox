using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class LostLibraryRegistry
{
	internal static void Register()
	{
		Enemies.Add("FearOfSpiders", () => new FearOfSpiders());
		Enemies.Add("FearOfHeights", () => new FearOfHeights());
		Enemies.Add("FearOfDrowning", () => new FearOfDrowning());
		Enemies.Add("Salli", () => new Salli());
		Enemies.Add("Cindi", () => new Cindi());
		Enemies.Add("Lili", () => new Lili());
		Enemies.Add("Dorothi", () => new Dorothi());

		// Fear of Spiders //
		Skills["FOSAttack"] = new Skill(
		   name: "FOSAttack",
		   description: "FOSAttack",
		   target: SkillTarget.Enemy,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   await AnimationManager.Instance.WaitForAnimation(287, target);
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] sinks its fangs into [target].");
			   BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
		   }
		);

		Skills["FOSDoNothing"] = new Skill(
		   name: "FOSDoNothing",
		   description: "FOSDoNothing",
		   target: SkillTarget.Self,
		   cost: 0,
		   effect: async (_, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(target, "[actor] is trying to talk to you...");
			   await Task.CompletedTask;
		   }
		);

		Skills["FOSSpinWeb"] = new Skill(
		   name: "FOSSpinWeb",
		   description: "FOSSpinWeb",
		   target: SkillTarget.Enemy,
		   cost: 0,
		   effect: async (self, target) =>
		   {
			   AnimationManager.Instance.PlayScreenAnimation(176, false);
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] shoots webs at [target]!\n[target]'s SPEED fell.");
			   target.AddStatModifier("SpeedDown", silent: true);
			   await Task.CompletedTask;
		   }
		);

		Skills["FOSAttackAll"] = new Skill(
		   name: "FOSAttackAll",
		   description: "FOSAttackAll",
		   target: SkillTarget.AllEnemies,
		   cost: 0,
		   effect: async (self, targets) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, "[actor] catches everyone!");
			   AnimationManager.Instance.PlayScreenAnimation(176, false);
			   await Wait.Milliseconds(1000);
			   foreach (Actor member in targets)
			   {
				   BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK * 2f - member.CurrentStats.DEF, false);
				   AnimationManager.Instance.PlayAnimation(287, member);
			   }
		   }
		);

		// Fear of Heights //
		Skills["FOHAttack"] = new Skill(
		  name: "FOHAttack",
		  description: "FOHAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(140, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] strikes [target].");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
		  }
		);

		Skills["FOHDoNothing"] = new Skill(
		  name: "FOHDoNothing",
		  description: "FOHDoNothing",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] taunts [target]\nas they fall.");
			  await Task.CompletedTask;
		  }
		);

		Skills["FOHGrab"] = new Skill(
		  name: "FOHGrab",
		  description: "FOHGrab",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (_, targets) =>
		  {
			  BattleLogManager.Instance.QueueMessage("Hands appear and grab everyone!");
			  foreach (Actor member in targets)
				  AnimationManager.Instance.PlayAnimation(164, member);
			  await Wait.Milliseconds(2000);
			  BattleLogManager.Instance.QueueMessage("Everyone's ATTACK fell...");
			  foreach (Actor member in targets)
			  {
				  AnimationManager.Instance.PlayAnimation(215, member);
				  member.AddTierStatModifier("AttackDown", silent: true);
			  }
			  await Wait.Milliseconds(1000);
		  }
		).WithTiersFromFirstTarget();

		Skills["FOHHands"] = new Skill(
		  name: "FOHHands",
		  description: "FOHHands",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, target, "More hands appear and surround\n[actor].");
			  AnimationManager.Instance.PlayAnimation(11, self);
			  await Wait.Milliseconds(2000);
			  AnimationManager.Instance.PlayAnimation(218, self);
			  self.AddStatModifier("DefenseUp");
			  await Wait.Milliseconds(1000);
		  }
		);

		Skills["FOHShove"] = new Skill(
		  name: "FOHShove",
		  description: "FOHShove",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(209, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] shoves [target].");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK, neverCrit: true);
			  // the skill applies afraid before removing emotion, so if an afraid-immune actor is targeted they still lose their emotion
			  if (!target.SetEmotion("afraid") && target.CurrentEmotion.Group != null)
				  target.SetEmotion("neutral", true);
		  }
		);

		// Fear of Drowning //
		
		Skills["FODAttack"] = new Skill(
			name: "FODAttack",
			description: "FODAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(140, target);
				BattleLogManager.Instance.QueueMessage(self, target, "Water pulls [target] in different\ndirections.");
				BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.15f, false, 0f, neverCrit: true);
			}
		);
		
		Skills["FODDoNothing"] = new Skill(
			name: "FODDoNothing",
			description: "FODDoNothing",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] listens to [target] struggle.");
				await Task.CompletedTask;
			}
		);
		
		Skills["FODDragDown"] = new Skill(
			name: "FODDragDown",
			description: "FODDragDown",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(197, target);
				// unused in the base game, Drag Down displays no battle text
				//BattleLogManager.Instance.QueueMessage(self, target, "[actor] grabs [target]'s leg and drags them down!");
				BattleManager.Instance.Damage(self, target, () => target.CurrentHP * 0.5f, false, 0f, neverCrit: true);
			}
		);
		
		Skills["FODWhirlpool"] = new Skill(
			name: "FODWhirlpool",
			description: "FODWhirlpool",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] creates a whirlpool.");
				BattleLogManager.Instance.QueueMessage("Everyone's SPEED fell...");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(140, member);
					member.AddTierStatModifier("SpeedDown", silent: true);
				}
				await Wait.Milliseconds(1000);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(215, member);
					BattleManager.Instance.Damage(self, member, () => member.CurrentStats.MaxHP * 0.1f, false,  neverCrit: true);
				}
			}
		).WithTiersFromFirstTarget();
		
		Skills["FODDrowning1"] = new Skill(
			name: "FODDrowning1",
			description: "FODDrowning1",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				DialogueManager.Instance.QueueMessage("You feel like you can't breathe.");
				await DialogueManager.Instance.WaitForDialogue();
				await Wait.Milliseconds(500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(140, member);
					BattleManager.Instance.Damage(self, member, () => 50, true, 0f, neverCrit: true);
				}
			},
			hidden: true
		);
		
		Skills["FODDrowning2"] = new Skill(
			name: "FODDrowning2",
			description: "FODDrowning2",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				DialogueManager.Instance.QueueMessage("You feel like you can't breathe.");
				await DialogueManager.Instance.WaitForDialogue();
				await Wait.Milliseconds(500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(140, member);
					BattleManager.Instance.Damage(self, member, () => 100, true, 0f, neverCrit: true);
				}
			},
			hidden: true
		);
		
		Skills["FODDrowning3"] = new Skill(
			name: "FODDrowning3",
			description: "FODDrowning3",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				DialogueManager.Instance.QueueMessage("You feel like you can't breathe.");
				await DialogueManager.Instance.WaitForDialogue();
				await Wait.Milliseconds(500);
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(140, member);
					BattleManager.Instance.Damage(self, member, () => 150, true, 0f, neverCrit: true);
				}
			},
			hidden: true
		);

		// SALLI //
		Skills["SALAttack"] = new Skill(
			name: "SALAttack",
			description: "SALAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(144, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] runs into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		bool salliSpeedChanged = false;
		Skills["SALSpeedUp"] = new Skill(
			name: "SALSpeedUp",
			description: "SALSpeedUp",
			target: SkillTarget.Self,
			cost: 0,
			setup: (_, target) =>
			{
				int before = target.GetStatModifierTier("SpeedUp") + target.GetStatModifierTier("SpeedDown");
				target.AddTierStatModifier("SpeedUp", silent: true);
				salliSpeedChanged = target.GetStatModifierTier("SpeedUp") + target.GetStatModifierTier("SpeedDown") != before;
				return Task.CompletedTask;
			},
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(218, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] starts speeding around the room!");
				if (salliSpeedChanged)
				{
					BattleLogManager.Instance.QueueMessage(target, "[actor]'s SPEED rose!");
				}
				else
				{
					BattleLogManager.Instance.QueueMessage(target, "[actor]'s SPEED can't go");
					BattleLogManager.Instance.QueueMessage(target, "any higher!");
				}
			}
		);

		Skills["SALLiar"] = new Skill(
			name: "SALLiar",
			description: "SALLiar",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "Liar.");
				await Task.CompletedTask;
			}
		);

		// CINDI //
		Skills["CINAttack"] = new Skill(
			name: "CINAttack",
			description: "CINAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(144, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] punches [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["CINSlam"] = new Skill(
			name: "CINSlam",
			description: "CINSlam",
			target: SkillTarget.Enemy,
			cost: 0,
			element: "afraid",
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams her arm into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
			}
		).WithHPCost(0, 0.1f);

		Skills["CINLiar"] = new Skill(
			name: "CINLiar",
			description: "CINLiar",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "Liar.");
				await Task.CompletedTask;
			}
		);

		// LILI //
		Skills["LILAttack"] = new Skill(
			name: "LILAttack",
			description: "LILAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(144, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] stares into [target]'s soul!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["LILSadEyes"] = new Skill(
			name: "LILSadEyes",
			description: "LILSadEyes",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: (_, target) =>
			{
				target.AddTierStatModifier("AttackDown", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(149, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[target] saw sadness in [actor]'s eyes.");
				// DIALOGUE QUIRK: the base drops this line's period (its "+ '.'" is a separate statement)
				BattleLogManager.Instance.QueueMessage(self, target, "[target] became reluctant to attack [actor]");
			}
		);

		Skills["LILLiar"] = new Skill(
			name: "LILLiar",
			description: "LILLiar",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "Liar.");
				await Task.CompletedTask;
			}
		);

		// DOROTHI //
		Skills["DORAttack"] = new Skill(
			name: "DORAttack",
			description: "DORAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(129, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] stomps on [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["DORAttackTwice"] = new Skill(
			name: "DORAttackTwice",
			description: "DORAttackTwice",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] kicks with its hind legs.");
				foreach (Actor target in targets)
				{
					await AnimationManager.Instance.WaitForAnimation(123, target);
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
				}
			}
		).WithRandomTargets(2);

		Skills["DORSelfHappy"] = new Skill(
			name: "DORSelfHappy",
			description: "DORSelfHappy",
			target: SkillTarget.Self,
			cost: 0,
			setup: (_, target) =>
			{
				BattleManager.Instance.MakeHappy(target, silent: true);
				return Task.CompletedTask;
			},
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_Horse", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] prances around!");
				await Task.CompletedTask;
			}
		);
	}
}
