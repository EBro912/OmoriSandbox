using System;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class UnderwaterHighwayRegistry
{
	internal static void Register()
	{
		Enemies.Add("SlimeGirls", () => new SlimeGirls());
		Enemies.Add("Mutantheart", () => new Mutantheart());
		Enemies.Add("Perfectheart", () => new Perfectheart());
		Enemies.Add("Roboheart", () => new Roboheart());
		Enemies.Add("Snaley 1", () => new SnaleyOne());
		Enemies.Add("Snaley 2", () => new SnaleyTwo());
		Enemies.Add("Snaley 3", () => new SnaleyThree());
		Enemies.Add("ShadyMole", () => new ShadyMole());
		Enemies.Add("Mussel", () => new Mussel());
		Enemies.Add("ReverseMermaid", () => new ReverseMermaid());
		Enemies.Add("SharkFin", () => new SharkFin());
		Enemies.Add("Squizzard", () => new Squizzard());
		Enemies.Add("LabRat", () => new LabRat());
		Enemies.Add("SlimeBunny", () => new SlimeBunny());
		Enemies.Add("Watermimic", () => new Watermimic());
		Enemies.Add("WormBot", () => new WormBot());
		Enemies.Add("FishBunny", () => new FishBunny());
		Enemies.Add("SnotBubble", () => new SnotBubble());
		Enemies.Add("SproutMole??", () => new SproutMoleDoubleQuestion());

		// SLIME GIRLS //
		Skills["ComboAttack"] = new Skill(
			name: "ComboAttack",
			description: "ComboAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "The [actor] attack all at once!");
				AnimationManager.Instance.PlayAnimation(133, target);
				await Wait.Milliseconds(580);
				AnimationManager.Instance.PlayAnimation(134, target);
				await Wait.Milliseconds(580);
				AnimationManager.Instance.PlayAnimation(135, target);
				await Wait.Milliseconds(580);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF,
					false, neverCrit: true);
			}
		);

		Skills["StrangeGas"] = new Skill(
			name: "StrangeGas",
			description: "StrangeGas",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (_, targets) =>
			{
				BattleLogManager.Instance.QueueMessage("MEDUSA threw a bottle...");
				AnimationManager.Instance.PlayScreenAnimation(194, false);
				await Wait.Milliseconds(1500);
				AnimationManager.Instance.PlayScreenAnimation(181, false);
				BattleLogManager.Instance.QueueMessage("A strange gas fills the room.");
				await Wait.Milliseconds(2000);

				foreach (Actor member in targets)
					BattleManager.Instance.RandomEmotion(member);
			}
		);

		Skills["Dynamite"] = new Skill(
			name: "Dynamite",
			description: "Dynamite",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage("MEDUSA threw a bottle...");
				AnimationManager.Instance.PlayScreenAnimation(194, false);
				await Wait.Milliseconds(1500);
				AnimationManager.Instance.PlayScreenAnimation(172, false);
				BattleLogManager.Instance.QueueMessage("And it explodes!");
				await Wait.Milliseconds(2000);

				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => 75, false, 0f, false, true);
				}
			}
		);

		Skills["StingRay"] = new Skill(
			name: "StingRay",
			description: "StingRay",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target,
					"MOLLY fires her stingers!\n[target] gets struck!");
				await AnimationManager.Instance.WaitForAnimation(193, target);
				int damage = BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2, false, neverCrit: true);
				if (damage > -1 && RollLuckStateChance(self, target, 1))
				{
					target.AddTierStatModifier("SpeedDown", 3, silent: true);
				}
			}
		);

		Skills["Chainsaw"] = new Skill(
			name: "Chainsaw",
			description: "Chainsaw",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] pulls out a chainsaw!");
				await AnimationManager.Instance.WaitForAnimation(208, target);
				for (int i = 0; i < 3; i++)
				{
					BattleManager.Instance.Damage(self, target, () => 40, false, 0.75f, false, true);
					await Wait.Milliseconds(500);
				}
			}
		);

		Skills["ChainsawAlt"] = new Skill(
			name: "ChainsawAlt",
			description: "ChainsawAlt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] pulls out a chainsaw!");
				await AnimationManager.Instance.WaitForAnimation(208, target);
				for (int i = 0; i < 3; i++)
				{
					BattleManager.Instance.Damage(self, target, () => 100, false, 0.75f, false, true);
					await Wait.Milliseconds(500);
				}
			}
		);

		Skills["Swap"] = new Skill(
			name: "Swap",
			description: "Swap",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] did their thing!\nHEART and JUICE were swapped!");
				await AnimationManager.Instance.WaitForScreenAnimation(191, false);
				foreach (Actor member in targets)
				{
					int hp = member.CurrentHP;
					int juice = member.CurrentJuice;
					member.CurrentHP = Math.Min(member.CurrentStats.MaxHP, juice + 1);
					member.CurrentJuice = Math.Min(member.CurrentStats.MaxJuice, Math.Max(0, hp));
				}
			}
		);

		Skills["SGSelfAngry"] = new Skill(
			name: "SGSelfAngry",
			description: "SGSelfAngry",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				target.SetEmotionForced("angry");
				BattleLogManager.Instance.ClearAndShowMessage(target, target, "[target] becomes ANGRIER!");
				await Task.CompletedTask;
			},
			hidden: true
		);

		Skills["SlimeUltimateAttack"] = new Skill(
			name: "SlimeUltimateAttack",
			description: "SlimeUltimateAttack",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				bool wasBelowUI = AnimationManager.Instance.ToneBelowUI;
				AnimationManager.Instance.ToneBelowUI = true;
				BattleLogManager.Instance.QueueMessage(self, "[actor] throw everything they have!");
				AnimationManager.Instance.PlayScreenAnimation(293, false);
				await Wait.Milliseconds(1162);
				AnimationManager.Instance.TintScreen(0, 0, 255, 155, 1f);
				AnimationManager.Instance.PlayScreenAnimation(181, false);
				await Wait.Milliseconds(332);
				// VANILLA BUG: shows and logs max JUICE, not the JUICE actually lost
				foreach (Actor member in targets)
				{
					BattleManager.Instance.SpawnDamageNumber(member.CurrentStats.MaxJuice, member, DamageType.JuiceLoss);
					member.CurrentJuice = 0;
				}
				foreach (Actor member in targets)
					BattleLogManager.Instance.QueueMessage(member, $"[actor] lost {member.CurrentStats.MaxJuice} JUICE!");
				await Wait.Milliseconds(1660);
				AnimationManager.Instance.TintScreen(160, 160, 0, 155, 1f);
				await Wait.Milliseconds(332);
				foreach (Actor member in targets)
					AnimationManager.Instance.PlayAnimation(193, member);
				await Wait.Milliseconds(664);
				foreach (Actor member in targets)
				{
					member.AddTierStatModifier("AttackDown", 3, silent: true);
					member.AddTierStatModifier("DefenseDown", 3, silent: true);
					member.AddTierStatModifier("SpeedDown", 3, silent: true);
					AnimationManager.Instance.PlayAnimation(215, member);
				}
				BattleLogManager.Instance.QueueMessage("Everyone's ATTACK fell.");
				await Wait.Milliseconds(166);
				BattleLogManager.Instance.QueueMessage("Everyone's DEFENSE fell.");
				await Wait.Milliseconds(166);
				BattleLogManager.Instance.QueueMessage("Everyone's SPEED fell.");
				await Wait.Milliseconds(1330);
				AnimationManager.Instance.TintScreen(255, 0, 0, 155, 1f);
				await Wait.Milliseconds(332);
				AnimationManager.Instance.PlayScreenAnimation(172, false);
				await Wait.Milliseconds(332);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member, () => member.CurrentStats.MaxHP * 0.4f, false, 0f, neverCrit: true);
				}
				await Wait.Milliseconds(400);
				AnimationManager.Instance.TintScreen(0, 0, 0, 0, 1f);
				// VANILLA BUG: adds the RANDOM EMOTION marker but never rolls the random emotion
				foreach (PartyMember member in targets.OfType<PartyMember>().Where(x => x.CurrentHP > 0))
					member.AddStatModifier("RandomEmotion", silent: true);
				await Wait.Milliseconds(664);
				AnimationManager.Instance.ToneBelowUI = wasBelowUI;
			}
		);

		// Mutantheart //
		Skills["MHWink"] = new Skill(
		  name: "MHWink",
		  description: "MHWink",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: (_, _) => Task.CompletedTask,
		  setup: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(298, self);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] winks at [target]!\nIt was kind of cute...");
			  bool canBeHappy = target.IsEmotionValid("happy");
			  BattleManager.Instance.MakeHappy(target, silent: true);
			  // DIALOGUE QUIRK: always prints the tier-1 HAPPY line, whatever tier the target reaches
			  QueueTierOneEmotionLine(target, "happy", canBeHappy);
		  }
		);

		Skills["MHCry"] = new Skill(
		  name: "MHCry",
		  description: "MHCry",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: (_, _) => Task.CompletedTask,
		  setup: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(297, self);
			  // DIALOGUE QUIRK: always prints the tier-1 SAD line, even when the emotion didn't change
			  BattleLogManager.Instance.QueueMessage(self, target, "Tears well up in [actor]'s eyes.\n[target] feels SAD.");
			  BattleManager.Instance.MakeSad(target, silent: true);
		  }
		);

		Skills["MHInsult"] = new Skill(
		  name: "MHInsult",
		  description: "MHInsult",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: (_, _) => Task.CompletedTask,
		  setup: async (self, target) =>
		  {
			  AudioManager.Instance.PlaySFX("BA_INK", volume: 0.9f);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] accidently says something\nmean.");
			  bool canBeAngry = target.IsEmotionValid("angry");
			  BattleManager.Instance.MakeAngry(target, silent: true);
			  // DIALOGUE QUIRK: always prints the tier-1 ANGRY line regardless of the tier the target reaches
			  QueueTierOneEmotionLine(target, "angry", canBeAngry);
			  await Task.CompletedTask;
		  }
		);

		Skills["MHInstakill"] = new Skill(
		  name: "MHInstakill",
		  description: "MHInstakill",
		  target: SkillTarget.Self,
		  cost: 0,
		  // vanilla INSTAKILL is a self-target skill used by the party member, so ACCURACY UP goes on them in the setup action
		  setup: (_, target) =>
		  {
			  target.AddStatModifier("AccuracyUp", silent: true);
			  return Task.CompletedTask;
		  },
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(122, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "MUTANTHEART slaps [target]!");
			  BattleManager.Instance.Damage(target, target, () => 999, false, 0f, neverCrit: true);
			  // vanilla's follow action then removes it
			  target.RemoveStatModifier("AccuracyUp");
		  }
		);

		// Perfectheart //
		Skills["PHStealHeart"] = new Skill(
		 name: "PHStealHeart",
		 description: "PHStealHeart",
		 target: SkillTarget.Enemy,
		 cost: 0,
		 effect: async (self, target) =>
		 {
			 await AnimationManager.Instance.WaitForAnimation(122, target);
			 BattleLogManager.Instance.QueueMessage(self, target, "[actor] steals [target]'s\nHEART.");
			 int damage = BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			 if (damage > 0)
			 {
				 // damage was already scaled earlier
				 self.Heal(damage, applyDebugScale: false);
				 BattleManager.Instance.SpawnDamageNumber(damage, self, DamageType.Heal);
				 BattleLogManager.Instance.QueueMessage(self, $"[actor] recovers {damage} HEART!");
			 }
		 }
		);

		Skills["PHStealBreath"] = new Skill(
		 name: "PHStealBreath",
		 description: "PHStealBreath",
		 target: SkillTarget.Enemy,
		 cost: 0,
		 effect: async (self, target) =>
		 {
			 await AnimationManager.Instance.WaitForAnimation(122, target);
			 BattleLogManager.Instance.QueueMessage(self, target, "[actor] steals [target]'s\nbreath away.");
			 int stolen = BattleManager.Instance.DamageJuice(self, target, () => target.CurrentStats.MaxJuice, false, 0f, neverCrit: true, silent: true);
			 if (stolen > 0)
			 {
				 BattleLogManager.Instance.QueueMessage(self, target, "[target] lost " + stolen + " JUICE...");
				 // a transfer of what the target actually had, so it isn't scaled
				 self.HealJuice(stolen, applyDebugScale: false);
				 BattleManager.Instance.SpawnDamageNumber(stolen, self, DamageType.JuiceGain);
				 BattleLogManager.Instance.QueueMessage(self, "[actor] recovers " + stolen + " JUICE...");
			 }
		 }
		);

		Skills["PHWrath"] = new Skill(
			 name: "PHWrath",
			 description: "PHWrath",
			 target: SkillTarget.AllEnemies,
			 cost: 0,
			 setup: async (self, targets) =>
			 {
				 BattleLogManager.Instance.QueueMessage(self, "[actor] unleashes her wrath.");
				 foreach (Actor member in targets)
					 AnimationManager.Instance.PlayAnimation(210, member);
				 await Wait.Milliseconds(1500);
				 foreach (Actor member in targets)
					 BattleManager.Instance.RandomEmotion(member, silent: true);
			 },
			 effect: async (self, targets) =>
			 {
				 foreach (Actor member in targets)
				 {
					 // VANILLA BUG: the ANGRY branch re-checks the SAD states, so an angry target gets no line
					 if (member.CurrentEmotion.Group?.Id is "happy" or "sad")
						 BattleLogManager.Instance.QueueMessage(member.CurrentEmotion.FeelsMessage(member.Name));
					 BattleManager.Instance.Damage(self, member, () => member.CurrentStats.MaxHP * 0.75f, false, 0.15f, neverCrit: true);
				 }
				 await Task.CompletedTask;
			 }
		 );

		Skills["PHExploitEmotion"] = new Skill(
			 name: "PHExploitEmotion",
			 description: "PHExploitEmotion",
			 target: SkillTarget.Enemy,
			 cost: 0,
			 element: "emotion",
			 effect: async (self, target) =>
			 {
				 await AnimationManager.Instance.WaitForAnimation(124, target);
				 BattleLogManager.Instance.QueueMessage(self, target, "[actor] exploits [target]'s\nEMOTIONS!");
				 // vanilla implements EXPLOIT as an attack with the EMOTION element; the attacker keeps their own emotion stats
				 BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0f, neverCrit: true);
			 }
		);

		Skills["PHSpare"] = new Skill(
			 name: "PHSpare",
			 description: "PHSpare",
			 target: SkillTarget.Enemy,
			 cost: 0,
			 effect: async (self, target) =>
			 {
				 await AnimationManager.Instance.WaitForAnimation(122, target);
				 BattleLogManager.Instance.QueueMessage(self, target, "[actor] decides to let\n[target] live.");
				 int damage = 1;
				 if (target.CurrentHP > 1)
					 damage = target.CurrentHP - 1;
				 BattleManager.Instance.Damage(self, target, () => damage, neverMiss: false, variance: 0f, neverCrit: true, ignoreEmotion: true);
			 }
		);

		Skills["PHAngelicVoice"] = new Skill(
			 name: "PHAngelicVoice",
			 description: "PHAngelicVoice",
			 target: SkillTarget.AllEnemies,
			 cost: 0,
			 setup: async (self, _) =>
			 {
				 AnimationManager.Instance.PlayAnimation(154, self);
				 await Wait.Milliseconds(166);
				 AnimationManager.Instance.PlayAnimation(155, self);
				 BattleLogManager.Instance.QueueMessage(self, "[actor] sings a soulful song...");
				 QueueTierOneEmotionLine(self, "sad", BattleManager.Instance.MakeSad(self, silent: true));
			 },
			 effect: async (self, targets) =>
			 {
				 BattleLogManager.Instance.QueueMessage("Everyone feels HAPPY!");
				 foreach (Actor member in targets)
				 {
					 BattleManager.Instance.Damage(self, member, () => 175, false, 0f, neverCrit: true, silent: true);
					 BattleManager.Instance.MakeHappy(member, silent: true);
				 }
				 await Task.CompletedTask;
			 }
		);

		// Roboheart //
		Skills["RHAttack"] = new Skill(
		  name: "RHAttack",
		  description: "RHAttack",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(125, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] fires rocket hands!");
			  BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
		  }
		);

		Skills["RHDoNothing"] = new Skill(
		  name: "RHDoNothing",
		  description: "RHDoNothing",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (_, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(target, "[actor] is buffering...");
			  await Task.CompletedTask;
		  }
		);

		Skills["RHLaser"] = new Skill(
		  name: "RHLaser",
		  description: "RHLaser",
		  target: SkillTarget.Enemy,
		  cost: 0,
		  effect: async (self, target) =>
		  {
			  await AnimationManager.Instance.WaitForAnimation(160, target);
			  BattleLogManager.Instance.QueueMessage(self, target, "[actor] opens her mouth and\nfires a laser!");
			  BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
		  }
		);

		Skills["RHSnack"] = new Skill(
		  name: "RHSnack",
		  description: "RHSnack",
		  target: SkillTarget.Self,
		  cost: 0,
		  effect: async (_, target) =>
		  {
			  BattleLogManager.Instance.QueueMessage(target, "[actor] opens her mouth!\nA nutritious SNACK appears!");
			  await AnimationManager.Instance.WaitForAnimation(216, target);
			  BattleManager.Instance.Heal(target, target, () => 200);
		  }
		);

		Skills["RHExplode"] = new Skill(
		  name: "RHExplode",
		  description: "RHExplode",
		  target: SkillTarget.AllEnemies,
		  cost: 0,
		  effect: async (self, targets) =>
		  {
			  BattleLogManager.Instance.QueueMessage(self, "[actor] sheds a single tear...\nand bids everyone farewell!");
			  AnimationManager.Instance.PlayScreenAnimation(172, true);
			  bool landed = false;
			  foreach (Enemy enemy in BattleManager.Instance.GetAllAliveEnemies())
			  {
				  if (enemy != self && BattleManager.Instance.Damage(self, enemy, () => enemy.CurrentStats.MaxHP * 0.1f, false, 0f, neverCrit: true) > -1)
					  landed = true;
			  }
			  foreach (Actor member in targets)
			  {
				  if (BattleManager.Instance.Damage(self, member, () => member.CurrentStats.MaxHP * 0.1f, false, 0f, neverCrit: true) > -1)
					  landed = true;
			  }
			  if (landed)
				  self.Damage(self.CurrentStats.MaxHP);
			  await Task.CompletedTask;
		  }
		);

		// Snaley //
		Skills["SNAttack"] = new Skill(
			name: "SNAttack",
			description: "SNAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0.1f);
			}
		);

		Skills["RabbitAttack"] = new Skill(
			name: "RabbitAttack",
			description: "RabbitAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] nibbles at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["SNAttackFollowup"] = new Skill(
			name: "SNAttackFollowup",
			description: "SNAttackFollowup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0.1f);
				BattleManager.Instance.QueueForceCommand(self, BattleManager.Instance.SmoothPartyTarget(self.LastTargetIndex), Skills["SNFollowup"]);
				await Wait.Milliseconds(1000);
			},
			hidden: true
		);
		
		Skills["SNFollowup"] = new Skill(
			name: "SNFollowup",
			description: "SNFollowup",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("Skill2");
				await Wait.Milliseconds(500);
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] attacks again!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF, false, 0.1f);
			},
			hidden: true
		);
		
		Skills["SNDoNothing"] = new Skill(
			name: "SNDoNothing",
			description: "SNDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] falls over.");
				await Task.CompletedTask;
			}
		);
		
		Skills["SNBeatdown"] = new Skill(
			name: "SNBeatdown",
			description: "SNBeatdown",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] furiously attacks!");
				await AnimationManager.Instance.WaitForAnimation(326, target);
				for (int i = 0; i < 3; i++)
				{
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
					await Wait.Milliseconds(500);
				}
			}
		);
		
		Skills["SNMegaphone"] = new Skill(
			name: "SNMegaphone",
			description: "SNMegaphone",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			priority: -500,
			setup: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses an AIRHORN!");
				AudioManager.Instance.PlaySFX("SE_airhorn");
				await Wait.Milliseconds(500);
				foreach (Actor target in targets)
				{
					BattleManager.Instance.MakeAngry(target);
				}
			}
		);
		
		Skills["SNReleaseEnergy"] = new Skill(
			name: "SNReleaseEnergy",
			description: "SNReleaseEnergy",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] releases energy!");
				AudioManager.Instance.PlaySFX("Skill2");
				if (self is Enemy enemy)
					enemy.SetOpacity(0f, 0.5f);
				await Wait.Milliseconds(500);
				await AnimationManager.Instance.WaitForSnaley();
				await Wait.Milliseconds(1000);
				foreach (Actor target in targets)
				{
					AnimationManager.Instance.PlayAnimation(327, target);
					BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.5f, false,
						neverCrit: true, ignoreEmotion: true);
				}
				if (self is Enemy stillAnEnemy)
					stillAnEnemy.SetOpacity(1f, 0.5f);
				await Wait.Milliseconds(500);
			}
		);

		// Shady Mole //
		Skills["SMAttack"] = new Skill(
			name: "SMAttack",
			description: "SMAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(3, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] cuts [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["SMB.E.D."] = new Skill(
			name: "SMB.E.D.",
			description: "SMB.E.D.",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(182, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] pulls out the B.E.D.!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["SMDynamite"] = new Skill(
			name: "SMDynamite",
			description: "SMDynamite",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] lobs DYNAMITE!");
				await AnimationManager.Instance.WaitForScreenAnimation(270, self is Enemy);
				// shady mole's dynamite hits the enemies too
				foreach (Enemy ally in BattleManager.Instance.GetAllAliveEnemies().ToList())
					BattleManager.Instance.Damage(self, ally, () => 25, false, 0.25f, neverCrit: true);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => 25, false, 0.25f, neverCrit: true);
			}
		);

		// MUSSEL //
		Skills["MUSAttack"] = new Skill(
			name: "MUSAttack",
			description: "MUSAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] punches [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["MUSFlex"] = new Skill(
			name: "MUSFlex",
			description: "MUSFlex",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(218, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] flexes and feels its best!");
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s HIT RATE rose!");
				target.AddStatModifier("Flex");
			}
		);

		Skills["MUSHide"] = new Skill(
			name: "MUSHide",
			description: "MUSHide",
			target: SkillTarget.Self,
			cost: 0,
			priority: 1000,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(178, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] hides in its shell.");
				target.AddStatModifier("Guard");
			}
		);

		// REVERSE MERMAID //
		Skills["RVMAttack"] = new Skill(
			name: "RVMAttack",
			description: "RVMAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[target] bumps into [actor]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["RVMDoNothing"] = new Skill(
			name: "RVMDoNothing",
			description: "RVMDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_weird", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] does a backflip!");
				BattleLogManager.Instance.QueueMessage("WOW!");
				await Task.CompletedTask;
			}
		);

		Skills["RVMRunAround"] = new Skill(
			name: "RVMRunAround",
			description: "RVMRunAround",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] runs amok!");
				foreach (Actor target in targets)
				{
					await AnimationManager.Instance.WaitForAnimation(123, target);
					BattleManager.Instance.Damage(self, target, () => self.CurrentEmotion.Id switch
					{
						"happy" => self.CurrentStats.ATK * 2 + 5 - target.CurrentStats.DEF,
						"ecstatic" => self.CurrentStats.ATK * 2 + 7 - target.CurrentStats.DEF,
						"manic" => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF,
						_ => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF
					}, false);
				}
			}
		).WithRandomTargets(2);

		// SHARK FIN //
		Skills["SHFAttack"] = new Skill(
			name: "SHFAttack",
			description: "SHFAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] charges at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["SHFDoNothing"] = new Skill(
			name: "SHFDoNothing",
			description: "SHFDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_dance", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] swims in a circle.");
				await Task.CompletedTask;
			}
		);

		Skills["SHFBite"] = new Skill(
			name: "SHFBite",
			description: "SHFBite",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(157, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] chomps [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentEmotion.Id switch
				{
					"angry" => self.CurrentStats.ATK * 3,
					"enraged" => self.CurrentStats.ATK * 3 + 5,
					"furious" => self.CurrentStats.ATK * 4,
					_ => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF
				}, false);
			}
		);

		Skills["SHFWorkUp"] = new Skill(
			name: "SHFWorkUp",
			description: "SHFWorkUp",
			target: SkillTarget.Self,
			cost: 0,
			setup: async (self, target) =>
			{
				target.AddTierStatModifier("SpeedUp", silent: true);
				bool applied = BattleManager.Instance.MakeAngry(target, silent: true);
				await AnimationManager.Instance.WaitForAnimation(218, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] works itself up!");
				// DIALOGUE QUIRK: prints the SPEED line even when the stat is already at its limit
				BattleLogManager.Instance.QueueMessage(self, "[actor]'s SPEED increased!");
				// DIALOGUE QUIRK: always prints the tier-1 ANGRY line regardless of the tier the target reaches
				QueueTierOneEmotionLine(target, "angry", applied);
			},
			effect: (_, _) => Task.CompletedTask
		);

		// SQUIZZARD //
		Skills["SQZAttack"] = new Skill(
			name: "SQZAttack",
			description: "SQZAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses magic on [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["SQZDoNothing"] = new Skill(
			name: "SQZDoNothing",
			description: "SQZDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_weird", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] mutters nonsense.");
				await Task.CompletedTask;
			}
		);

		Skills["SQZSquidWard"] = new Skill(
			name: "SQZSquidWard",
			description: "SQZSquidWard",
			target: SkillTarget.Ally,
			cost: 0,
			priority: 1000,
			setup: async (_, target) =>
			{
				Task playing = AnimationManager.Instance.WaitForAnimation(178, target);
				target.AddTierStatModifier("DefenseUp", silent: true);
				await playing;
			},
			effect: (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] creates a squid ward.");
				// DIALOGUE QUIRK: prints the DEFENSE line even when the stat is already at its limit
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s DEFENSE increased.");
				return Task.CompletedTask;
			}
		);

		Skills["SQZSquidMagic"] = new Skill(
			name: "SQZSquidMagic",
			description: "SQZSquidMagic",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses squid magic!");
				BattleLogManager.Instance.QueueMessage("Everyone feels weird...");
				await Task.WhenAll(targets.Select(member => AnimationManager.Instance.WaitForAnimation(161, member)));
				foreach (PartyMember member in targets.OfType<PartyMember>().Where(x => x.CurrentHP > 0))
					member.AddStatModifier("RandomEmotion", silent: true);
				RandomPlayerEmotion();
			}
		);

		// LAB RAT //
		Skills["LRAttack"] = new Skill(
			name: "LRAttack",
			description: "LRAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(125, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] fires a tiny mouse laser!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentEmotion.Id switch
				{
					"happy" => self.CurrentStats.ATK * 2 + 5 - target.CurrentStats.DEF,
					"ecstatic" => self.CurrentStats.ATK * 2 + 7 - target.CurrentStats.DEF,
					"manic" => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF,
					_ => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF
				}, false);
			}
		);

		Skills["LRDoNothing"] = new Skill(
			name: "LRDoNothing",
			description: "LRDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_INK", 0.7f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] lets out a little steam.");
				await Task.CompletedTask;
			}
		);

		Skills["LRHappyGas"] = new Skill(
			name: "LRHappyGas",
			description: "LRHappyGas",
			target: SkillTarget.AllEnemies,
			cost: 0,
			setup: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] releases HAPPY gas!");
				await AnimationManager.Instance.WaitForScreenAnimation(181, targets[0] is Enemy);
				foreach (Actor member in BattleManager.Instance.GetAllAliveEnemies().Concat(targets))
					BattleManager.Instance.MakeHappy(member, silent: true);
			},
			effect: (_, _) => Task.CompletedTask
		);

		// SLIME BUNNY //
		Skills["SLBAttack"] = new Skill(
			name: "SLBAttack",
			description: "SLBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(128, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] nuzzles up against [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["SLBDoNothing"] = new Skill(
			name: "SLBDoNothing",
			description: "SLBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_Drink", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] smiles at everyone.");
				await Task.CompletedTask;
			}
		);

		bool stickySpeedChanged = false;
		Skills["SLBSticky"] = new Skill(
			name: "SLBSticky",
			description: "SLBSticky",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: (_, target) =>
			{
				BattleManager.Instance.MakeSad(target, silent: true);
				int before = target.GetStatModifierTier("SpeedDown") + target.GetStatModifierTier("SpeedUp");
				target.AddTierStatModifier("SpeedDown", silent: true);
				stickySpeedChanged = target.GetStatModifierTier("SpeedDown") + target.GetStatModifierTier("SpeedUp") != before;
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(219, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] feels lonely and cries.");
				if (stickySpeedChanged)
				{
					BattleLogManager.Instance.QueueMessage(self, target, "[target]'s SPEED fell!");
				}
				else
				{
					BattleLogManager.Instance.QueueMessage(self, target, "[target]'s SPEED can't go");
					BattleLogManager.Instance.QueueMessage(self, target, "any lower!");
				}
				// DIALOGUE QUIRK: always prints the tier-1 SAD line regardless of the tier the target reaches
				BattleLogManager.Instance.QueueMessage(target, "[actor] feels SAD.");
			}
		);

		// WATERMIMIC //
		Skills["WMMRubberBand"] = new Skill(
			name: "WMMRubberBand",
			description: "WMMRubberBand",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_rubberband", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] flings a RUBBER BAND!");
				BattleManager.Instance.Damage(self, target, () => 60, false, 0f, neverCrit: true);
				await Task.CompletedTask;
			}
		);

		Skills["WMMJacks"] = new Skill(
			name: "WMMJacks",
			description: "WMMJacks",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] throws JACKS ");
				BattleLogManager.Instance.QueueMessage("everywhere!");
				await Task.WhenAll(targets.Select(member => AnimationManager.Instance.WaitForAnimation(122, member)));
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => 40, false, 0f, neverCrit: true);
			}
		);

		Skills["WMMDynamite"] = new Skill(
			name: "WMMDynamite",
			description: "WMMDynamite",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] lobs a stick of DYNAMITE!");
				BattleLogManager.Instance.QueueMessage("OH NO!");
				await AnimationManager.Instance.WaitForScreenAnimation(172, targets[0] is Enemy);
				foreach (Enemy ally in BattleManager.Instance.GetAllAliveEnemies().ToList())
					BattleManager.Instance.Damage(self, ally, () => 100, false, 0f, neverCrit: true);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => 100, false, 0f, neverCrit: true);
			}
		);

		Skills["WMMTomato"] = new Skill(
			name: "WMMTomato",
			description: "WMMTomato",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] throws a TOMATO!");
				await AnimationManager.Instance.WaitForAnimation(216, target);
				BattleManager.Instance.Heal(self, target, () => 150, 0f);
			}
		);

		Skills["WMMGrapes"] = new Skill(
			name: "WMMGrapes",
			description: "WMMGrapes",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] throws a GRAPE!");
				await AnimationManager.Instance.WaitForAnimation(217, target);
				if (BattleManager.Instance.RollHit(self, target))
					BattleManager.Instance.HealJuice(self, target, () => 30);
			}
		);

		Skills["WMMFrenchFries"] = new Skill(
			name: "WMMFrenchFries",
			description: "WMMFrenchFries",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] throws FRENCH FRIES!");
				await Task.WhenAll(targets.Select(member => AnimationManager.Instance.WaitForAnimation(216, member)));
				foreach (Actor target in targets)
					BattleManager.Instance.Heal(self, target, () => 50, 0f);
			}
		);

		Skills["WMMConfetti"] = new Skill(
			name: "WMMConfetti",
			description: "WMMConfetti",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] throws CONFETTI!");
				// DIALOGUE QUIRK: prints "Everyone feels HAPPY!" even when a target's emotion can't move
				BattleLogManager.Instance.QueueMessage("Everyone feels HAPPY!");
				AudioManager.Instance.PlaySFX("GEN_ta_da", 1f, 0.9f);
				foreach (Actor member in BattleManager.Instance.GetAllAliveEnemies().Concat(targets))
					BattleManager.Instance.MakeHappy(member, silent: true);
				return Task.CompletedTask;
			}
		);

		Skills["WMMRainCloud"] = new Skill(
			name: "WMMRainCloud",
			description: "WMMRainCloud",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] summons a RAIN CLOUD!");
				// DIALOGUE QUIRK: prints "Everyone feels SAD." even when a target's emotion can't move
				BattleLogManager.Instance.QueueMessage("Everyone feels SAD.");
				for (int i = 0; i < 3; i++)
					AudioManager.Instance.PlaySFX("BA_sad_level_2", 1f, 0.9f);
				foreach (Actor member in BattleManager.Instance.GetAllAliveEnemies().Concat(targets))
					BattleManager.Instance.MakeSad(member, silent: true);
				return Task.CompletedTask;
			}
		);

		Skills["WMMAirHorn"] = new Skill(
			name: "WMMAirHorn",
			description: "WMMAirHorn",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses a GIANT AIRHORN!");
				// DIALOGUE QUIRK: prints "Everyone feels ANGRY!" even when a target's emotion can't move
				BattleLogManager.Instance.QueueMessage("Everyone feels ANGRY!");
				AudioManager.Instance.PlaySFX("SE_airhorn", 1f, 0.9f);
				foreach (Actor member in BattleManager.Instance.GetAllAliveEnemies().Concat(targets))
					BattleManager.Instance.MakeAngry(member, silent: true);
				return Task.CompletedTask;
			}
		);

		// WORM-BOT //
		Skills["WBTAttack"] = new Skill(
			name: "WBTAttack",
			description: "WBTAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(125, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["WBTDoNothing"] = new Skill(
			name: "WBTDoNothing",
			description: "WBTDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_roar", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] crunches loudly!");
				await Task.CompletedTask;
			}
		);

		Skills["WBTLaser"] = new Skill(
			name: "WBTLaser",
			description: "WBTLaser",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(158, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] fires a laser at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
			}
		);

		// FISH BUNNY //
		Skills["FSBAttack"] = new Skill(
			name: "FSBAttack",
			description: "FSBAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] swims into [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["FSBDoNothing"] = new Skill(
			name: "FSBDoNothing",
			description: "FSBDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_dance", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] swims in a circle. ");
				await Task.CompletedTask;
			}
		);

		Skills["FSBSchooling"] = new Skill(
			name: "FSBSchooling",
			description: "FSBSchooling",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(145, target);
				BattleLogManager.Instance.QueueMessage(target, "[actor] calls for friends!");
				if (self is Enemy enemy)
					ReinforcementSlots.Summon(enemy, ReinforcementSlots.FishBunny);
			}
		);

		// SNOT BUBBLE //
		Task inflateAnimation = Task.CompletedTask;
		Skills["SNTInflate"] = new Skill(
			name: "SNTInflate",
			description: "SNTInflate",
			target: SkillTarget.Self,
			cost: 0,
			setup: async (_, target) =>
			{
				BattleManager.Instance.ZoomCamera(2f, 60);
				await Wait.Frames(90);
				inflateAnimation = AnimationManager.Instance.WaitForAnimation(218, target);
				target.AddTierStatModifier("AttackUp", silent: true);
			},
			effect: async (self, _) =>
			{
				await inflateAnimation;
				BattleLogManager.Instance.QueueMessage(self, "[actor]'s snot inflated!");
				// DIALOGUE QUIRK: prints the ATTACK line even when ATTACK is already at its limit
				BattleLogManager.Instance.QueueMessage(self, "[actor]'s ATTACK rose!");
			}
		);

		Skills["SNTPop"] = PopSkill("SNTPop");

		Skills["SNTPopOnDeath"] = PopSkill("SNTPopOnDeath");

		// SPROUT MOLE?? //
		Skills["SMQQAttack"] = new Skill(
			name: "SMQQAttack",
			description: "SMQQAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(125, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] fires a laser at [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["SMQQDoNothing"] = new Skill(
			name: "SMQQDoNothing",
			description: "SMQQDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("BA_do_nothing_space_out", 1f, 0.9f);
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s eye glows a little.");
				await Task.CompletedTask;
			}
		);

		Skills["SMQQExplode"] = new Skill(
			name: "SMQQExplode",
			description: "SMQQExplode",
			target: SkillTarget.AllEnemies,
			cost: 0,
			setup: (self, _) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] sheds a single tear...");
				BattleLogManager.Instance.QueueMessage("and gloriously explodes!");
				return Task.CompletedTask;
			},
			effect: (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(172, self is Enemy);
				foreach (Enemy ally in BattleManager.Instance.GetAllAliveEnemies().ToList())
					Explode(self, ally, 50f);
				foreach (Actor target in targets)
					Explode(self, target, 50f);
				return Task.CompletedTask;
			}
		);

		Skills["SMQQStrangeLaser"] = new Skill(
			name: "SMQQStrangeLaser",
			description: "SMQQStrangeLaser",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(161, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor]'s eyes emits a strange");
				BattleLogManager.Instance.QueueMessage(self, target, "light. [target] felt strange.");
				target.AddStatModifier("RandomEmotion", silent: true);
				RandomPlayerEmotion();
			}
		);
	}

	private static Skill PopSkill(string name)
	{
		return new Skill(
			name: name,
			description: name,
			target: SkillTarget.AllEnemies,
			cost: 0,
			setup: (self, _) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] explodes!");
				BattleLogManager.Instance.QueueMessage("Snot flies everywhere!");
				return Task.CompletedTask;
			},
			effect: async (self, targets) =>
			{
				AnimationManager.Instance.PlayScreenAnimation(189, targets[0] is Enemy);
				await Wait.Frames(40);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK + target.CurrentStats.MaxHP * 0.25f,
						new DamageOptions
						{
							NeverMiss = false,
							Variance = 0f,
							Crit = CritMode.Never,
							OnHit = () =>
							{
								self.CurrentHP = 0;
								self.ClearResult();
							}
						});
				if (self is Enemy enemy)
				{
					if (!enemy.IsImmortal)
						enemy.CurrentHP = 0;
					enemy.RemoveImmortal();
				}
			}
		);
	}

	private static void Explode(Actor self, Actor target, float damage)
	{
		BattleManager.Instance.Damage(self, target, () => damage,
			new DamageOptions
			{
				NeverMiss = false,
				Variance = 0f,
				Crit = CritMode.Never,
				OnHit = () =>
				{
					self.CurrentHP = 0;
					self.ClearResult();
				}
			});
	}

	private static void RandomPlayerEmotion()
	{
		for (int slot = 0; slot < 4; slot++)
		{
			int roll = GameManager.Instance.Random.RandiRange(0, 2);
			PartyMember member = BattleManager.Instance.GetAlivePartyMembers()
				.FirstOrDefault(x => x.Component.Position == slot && x.HasStatModifier("RandomEmotion"));
			if (member != null)
				SquidMagicEmotion(member, slot, roll);
		}
	}

	private static readonly string[][] SquidMagicGroups = [["sad", "angry", "happy"], ["happy", "angry", "sad"], ["angry", "happy", "sad"], ["sad", "happy", "angry"]];

	private static void SquidMagicEmotion(PartyMember member, int slot, int roll)
	{
		string group = SquidMagicGroups[slot][roll];
		string[] tiers = group switch
		{
			"sad" => ["sad", "depressed", "miserable"],
			"angry" => ["angry", "enraged", "furious"],
			_ => ["happy", "ecstatic", "manic"]
		};
		string[] others = group switch
		{
			"sad" => ["happy", "ecstatic", "manic", "angry", "enraged", "furious", "afraid"],
			"angry" => ["happy", "ecstatic", "manic", "sad", "depressed", "miserable", "afraid"],
			_ => ["angry", "enraged", "furious", "sad", "depressed", "miserable", "afraid"]
		};
		RemoveEmotions(member, others);
		string held = member.CurrentEmotion.Id;
		if (held == tiers[2])
			return;
		if (held == tiers[1])
		{
			RemoveEmotions(member, tiers[1], tiers[0]);
			// VANILLA BUG: SQUID MAGIC's HAPPY branch stops at ECSTATIC
			AddEmotion(member, group == "happy" ? tiers[1] : tiers[2]);
		}
		else if (held == tiers[0])
		{
			RemoveEmotions(member, tiers[0]);
			AddEmotion(member, tiers[1]);
		}
		else
			AddEmotion(member, tiers[0]);
	}

	private static void RemoveEmotions(PartyMember member, params string[] emotions)
	{
		foreach (string emotion in emotions)
		{
			if (member.CurrentEmotion.Id == emotion)
				member.SetEmotion("neutral", silent: true);
			member.ClearResult();
		}
	}

	private static void AddEmotion(PartyMember member, string emotion)
	{
		member.SetEmotion(emotion, silent: true);
		member.ClearResult();
	}
}
