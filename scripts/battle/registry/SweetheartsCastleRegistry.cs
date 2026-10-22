using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class SweetheartsCastleRegistry
{
	internal static void Register()
	{
		Enemies.Add("Sweetheart", () => new Sweetheart());
		Enemies.Add("FearOfSpiders", () => new FearOfSpiders());
		Enemies.Add("FearOfHeights", () => new FearOfHeights());
		Enemies.Add("SirMaximusI", () => new SirMaximusI());
		Enemies.Add("SirMaximusII", () => new SirMaximusII());
		Enemies.Add("SirMaximusIII", () => new SirMaximusIII());
		Enemies.Add("FearOfDrowning", () => new FearOfDrowning());

		// SWEETHEART //
		Skills["SHAttack"] = new Skill(
			name: "Attack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(132, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slaps [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false);
			}
		);

		Skills["SharpInsult"] = new Skill(
			name: "Sharp Insult",
			description: "Sharp Insult",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] insults everyone!\nEveryone feels ANGRY!");
				await AnimationManager.Instance.WaitForScreenAnimation(183, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => self.CurrentStats.ATK, false, 0.1f,
						neverCrit: true);
					BattleManager.Instance.MakeAngry(member, silent: true);
				}
			}
		);

		Skills["SwingMace"] = new Skill(
			name: "Swing Mace",
			description: "Swing Mace",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] swings her mace!");
				await AnimationManager.Instance.WaitForScreenAnimation(206, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member,
						() => self.CurrentStats.ATK * 2.5f - member.CurrentStats.DEF, false);
				}
			}
		);

		Skills["Brag"] = new Skill(
			name: "Brag",
			description: "Brag",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(162, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] boasts about\none of her many, many talents!");
				BattleManager.Instance.MakeHappy(target);
			}
		);

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

		// Sir Maximus I //
		
		Skills["SMIAttack"] = new Skill(
			name: "SMIAttack",
			description: "SMIAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(3, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] swings his sword!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["SMIDoNothing"] = new Skill(
			name: "SMIDoNothing",
			description: "SMIDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] pulled his back...");
				BattleManager.Instance.MakeSad(target);
				await Task.CompletedTask;
			}
		);
		
		Skills["SMIStrikeTwice"] = new Skill(
			name: "SMIStrikeTwice",
			description: "SMIStrikeTwice",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self,  "[actor] strikes twice!");
				await AnimationManager.Instance.WaitForAnimation(3, targets[0]);
				BattleManager.Instance.Damage(self, targets[0], () => self.CurrentStats.ATK * 2 - targets[0].CurrentStats.DEF, false);
				await Wait.Milliseconds(500);
				await AnimationManager.Instance.WaitForAnimation(3, targets[1]);
				BattleManager.Instance.Damage(self, targets[1],
					() => self.CurrentStats.ATK * 2 - targets[1].CurrentStats.DEF, false);
			},
			hidden: true
		);
		
		Skills["SMIUltimateAttack"] = new Skill(
			name: "SMIUltimateAttack",
			description: "SMIUltimateAttack",
			target: SkillTarget.AllEnemies,
			cost: 0,
			priority: 100,
			// vanilla adds ACCURACY UP in the setup action and lets it expire at the end of the turn
			setup: (self, _) =>
			{
				self.AddStatModifier("AccuracyUp", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his\nultimate attack!");
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayScreenAnimation(186, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 50, false, 0.25f, neverCrit: true);
				}
			}
		);
		
		Skills["SMUltimateAttack"] = new Skill(
			name: "SMUltimateAttack",
			description: "SMUltimateAttack",
			target: SkillTarget.AllEnemies,
			cost: 0,
			priority: 100,
			// vanilla adds ACCURACY UP in the setup action and lets it expire at the end of the turn
			setup: (self, _) =>
			{
				self.AddStatModifier("AccuracyUp", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his\nultimate attack!");
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayScreenAnimation(186, false);
				int damage = BattleManager.Instance.GetAllAliveEnemies().Count switch
				{
					2 => 75,
					1 => 50,
					_ => 100
				};
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => damage, false, 0f, neverCrit: true);
				}

				await AnimationManager.Instance.ToSignal(AnimationManager.Instance,
					AnimationManager.SignalName.AnimationFinished);
			}
		);

		// Sir Maximus II //
		
		Skills["SMIIDoNothing"] = new Skill(
			name: "SMIIDoNothing",
			description: "SMIIDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] remembers his\nfather's dying words.");
				BattleManager.Instance.MakeSad(target);
				await Task.CompletedTask;
			}
		);
		
		Skills["SMIISpin"] = new Skill(
			name: "SMIISpin",
			description: "SMIISpin",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] spins quickly!");
				await Wait.Milliseconds(500);
				float damage = targets.Count switch
				{
					1 => self.CurrentStats.ATK * 4,
					2 => self.CurrentStats.ATK * 3,
					_ => self.CurrentStats.ATK * 2
				};
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(123, member);
					BattleManager.Instance.Damage(self, member, () => damage, false);
				}
			}
		);
		
		Skills["SMIIUltimateAttack"] = new Skill(
			name: "SMIIUltimateAttack",
			description: "SMIIUltimateAttack",
			target: SkillTarget.AllEnemies,
			cost: 0,
			priority: 100,
			// vanilla adds ACCURACY UP in the setup action and lets it expire at the end of the turn
			setup: (self, _) =>
			{
				self.AddStatModifier("AccuracyUp", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his father's\nultimate attack!");
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayScreenAnimation(186, targets[0] is Enemy);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 50, false, 0.25f, neverCrit: true);
				}
			}
		);

		// Sir Maximus III //
		Skills["SMIIIDoNothing"] = new Skill(
			name: "SMIIIDoNothing",
			description: "SMIIIDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (_,target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] remembers his\ngrandfather's dying words.\n[actor] feels SAD.");
				BattleManager.Instance.MakeSad(target, silent: true);
				await Task.CompletedTask;
			}
		);
		
		Skills["SMIIIFlex"] = new Skill(
			name: "SMIIIFlex",
			description: "SMIIIFlex",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] flexes and feels\nhis best!");
				await AnimationManager.Instance.WaitForAnimation(218, target);
				target.AddStatModifier("Flex");
				if (target.CurrentEmotion.Id != "happy")
					BattleManager.Instance.MakeHappy(target);
			}
		);
		
		Skills["SMIIIUltimateAttack"] = new Skill(
			name: "SMIIIUltimateAttack",
			description: "SMIIIUltimateAttack",
			target: SkillTarget.AllEnemies,
			cost: 0,
			priority: 100,
			// vanilla adds ACCURACY UP in the setup action and lets it expire at the end of the turn
			setup: (self, _) =>
			{
				self.AddStatModifier("AccuracyUp", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses his grandfather's\nultimate attack!");
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayScreenAnimation(186, false);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 50, false, 0.25f, neverCrit: true);
				}
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
	}
}
