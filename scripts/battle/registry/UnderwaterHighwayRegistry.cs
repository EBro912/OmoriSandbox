using System;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Actors;

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
				if (damage > -1)
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
			  // VANILLA BUG: always prints the tier-1 HAPPY line, whatever tier the target reaches
			  if (canBeHappy)
				  BattleLogManager.Instance.QueueMessage(target, "[actor] feels HAPPY!");
			  else
				  BattleManager.Instance.ShowEmotionNoEffect(target, "happy");
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
			  // VANILLA BUG: always prints the tier-1 SAD line, even when the emotion didn't change
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
			  // VANILLA BUG: always prints the tier-1 ANGRY line, whatever tier the target reaches
			  if (canBeAngry)
				  BattleLogManager.Instance.QueueMessage(target, "[actor] feels ANGRY!");
			  else
				  BattleManager.Instance.ShowEmotionNoEffect(target, "angry");
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
				 if (BattleManager.Instance.MakeSad(self, silent: true))
					 BattleLogManager.Instance.QueueMessage(self, "[actor] feels SAD.");
				 else
					 BattleManager.Instance.ShowEmotionNoEffect(self, "sad");
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
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0.2f);
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
				await AnimationManager.Instance.WaitForScreenAnimation(270, targets[0] is Enemy);
				// shady mole's dynamite hits the enemies too
				foreach (Enemy ally in BattleManager.Instance.GetAllAliveEnemies().ToList())
					BattleManager.Instance.Damage(self, ally, () => 25, false, 0.25f, neverCrit: true);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => 25, false, 0.25f, neverCrit: true);
			}
		);
		
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
