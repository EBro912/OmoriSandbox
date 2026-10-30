using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class FarawayRegistry
{
	internal static void Register()
	{
		Enemies.Add("AubreyEnemy", () => new AubreyEnemy());
		Enemies.Add("RecycultistLeft", () => new Recycultist(true));
		Enemies.Add("RecycultistRight", () => new Recycultist(false));
		Enemies.Add("Recyclepath", () => new Recyclepath());
		Enemies.Add("Angel", () => new Angel());
		Enemies.Add("Angel (Two Days Left)", () => new AngelAlt());
		Enemies.Add("Charlene", () => new Charlene());
		Enemies.Add("TheMaverick", () => new TheMaverick());
		Enemies.Add("Kim", () => new Kim());
		Enemies.Add("Vance", () => new Vance());
		Enemies.Add("TheHooligans", () => new TheHooligans());
		Enemies.Add("Jackson", () => new Jackson());
		Enemies.Add("Wasp", () => new Wasp());
		Enemies.Add("Housefly", () => new Housefly());
		Enemies.Add("SomethingInTheDark", () => new SomethingInTheDark());
		Enemies.Add("SomethingInTheWalls", () => new SomethingInTheWalls());
		Enemies.Add("SomethingInTheWater", () => new SomethingInTheWater());
		Enemies.Add("BasilEnemy", () => new BasilEnemy());

		// AUBREY (Enemy) //

		Skills["AEAttack"] = new Skill(
			name: "AEAttack",
			description: "AEAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(28, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		Skills["AEDoNothing"] = new Skill(
			name: "AEDoNothing",
			description: "AEDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] spits on your shoe.");
				await Task.CompletedTask;
			}
		);

		Skills["AEHeadbutt"] = new Skill(
			name: "AEHeadbutt",
			description: "AEHeadbutt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] headbutts [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		// Recycultist //
		Skills["RCultFlingTrash"] = new Skill(
			name: "RCultFlingTrash",
			description: "RCultFlingTrash",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(201, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws trash!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, 0f);
			}
		);
		
		Skills["RCultGatherTrash"] = new Skill(
			name: "RCultGatherTrash",
			description: "RCultGatherTrash",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				AudioManager.Instance.PlaySFX("SE_shuffle", 1f, 0.8f);
				BattleLogManager.Instance.QueueMessage(target, "[actor] gathers trash!");
				await Task.CompletedTask;
			}
		);

		// Recyclepath //
		Skills["RPathAttack"] = new Skill(
			name: "RPathAttack",
			description: "RPathAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(130, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] hits [target] with a bag!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);
		
		Skills["RPathGatherTrash"] = new Skill(
			name: "RPathGatherTrash",
			description: "RPathGatherTrash",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] picks up some TRASH!");
				target.AddTierStatModifier("Stockpile");
				await Task.CompletedTask;
			}
		);
		
		Skills["RPathFlingTrash"] = new Skill(
			name: "RPathFlingTrash",
			description: "RPathFlingTrash",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(201, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] flings all their TRASH\nat [target]!");
				if (!self.StatModifiers.TryGetValue("Stockpile", out StatModifier stockpile))
				{
					GD.PrintErr("Tried to use RPathFlingTrash with no stockpile stacks!");
					return;
				}
				BattleManager.Instance.Damage(self, target, () =>
				{
					return ((TierStatModifier)stockpile).CurrentTier switch
					{
						1 => 3 * self.CurrentStats.ATK - target.CurrentStats.DEF,
						2 => 4 * self.CurrentStats.ATK - target.CurrentStats.DEF,
						_ => 5 * self.CurrentStats.ATK - target.CurrentStats.DEF,
					};
				}, false, 0f);
				self.RemoveStatModifier("Stockpile");
			}
		);
		
		Skills["RPathSummon"] = new Skill(
			name: "RPathSummon",
			description: "RPathSummon",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(145, self);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] calls a follower!");
				BattleLogManager.Instance.QueueMessage("A RECYCULTIST appeared!");
				if (self is Recyclepath path)
					path.SummonRecycultist();
				await Task.CompletedTask;
			},
			hidden: true
		);

		// Angel //
		Skills["ANAttack"] = new Skill(
			name: "ANAttack",
			description: "ANAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] swiftly strikes [target]!");
				await AnimationManager.Instance.WaitForAnimation(136, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);
		
		Skills["ANDoNothing"] = new Skill(
			name: "ANDoNothing",
			description: "ANDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] does a flip and strikes a pose!");
				await Task.CompletedTask;
			}
		);
		
		Skills["ANQuickAttack"] = new Skill(
			name: "ANQuickAttack",
			description: "ANQuickAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] teleports behind [target]!");
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			},
			priority: 1000
		);
		
		Skills["ANTease"] = new Skill(
			name: "ANTease",
			description: "ANTease",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] says mean things about [target]!");
				if (target.CurrentEmotion.Group?.Id != "sad")
					target.SetEmotion("sad", true);
				await Task.CompletedTask;
			}
		);

		// Charlene //
		Skills["CHAttack"] = new Skill(
			name: "CHAttack",
			description: "CHAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "CHARLIE punches [target]!");
				await AnimationManager.Instance.WaitForAnimation(122, target);
				BattleManager.Instance.Damage(self, target, () => 1, false, 0f, neverCrit: true);
			}
		);
		
		Skills["CHDoNothing"] = new Skill(
			name: "CHDoNothing",
			description: "CHDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "CHARLIE is standing there.");
				await Task.CompletedTask;
			}
		);

		// The Maverick //
		Skills["TMAttack"] = new Skill(
			name: "TMAttack",
			description: "TMAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] hits [target]!");
				await AnimationManager.Instance.WaitForAnimation(137, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);
		
		Skills["TMDoNothing"] = new Skill(
			name: "TMDoNothing",
			description: "TMDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] starts bragging to\nhis adoring fans!");
				await Task.CompletedTask;
			}
		);
		
		Skills["TMSmile"] = new Skill(
			name: "TMSmile",
			description: "TMSmile",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] smiles seductively!");
				await AnimationManager.Instance.WaitForAnimation(148, self);
				await AnimationManager.Instance.WaitForAnimation(215, target);
				target.AddStatModifier("AttackDown");
			}
		);
		
		Skills["TMTaunt"] = new Skill(
			name: "TMTaunt",
			description: "TMTaunt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				// DIALOGUE QUIRK: prints the success text even when the emotion is blocked
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] starts making fun of\n[target]!\n[target] feels ANGRY!");
				if (target.CurrentEmotion.Group?.Id != "angry")
					target.SetEmotion("angry", true);
				await Task.CompletedTask;
			}
		);

		// Kim //
		Skills["KMAttack"] = new Skill(
			name: "KMAttack",
			description: "KMAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] punches [target]!");
				await AnimationManager.Instance.WaitForAnimation(138, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);
		
		Skills["KMDoNothing"] = new Skill(
			name: "KMDoNothing",
			description: "KMDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s phone rang...\nIt was a wrong number.");
				await Task.CompletedTask;
			}
		);
		
		Skills["KMSmash"] = new Skill(
			name: "KMSmash",
			description: "KMSmash",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] grabs [target]'s shirt and\npunches them in the face!");
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f, false, neverCrit: true);
			}
		);
		
		Skills["KMTaunt"] = new Skill(
			name: "KMTaunt",
			description: "KMTaunt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				// DIALOGUE QUIRK: prints the success text even when the emotion is blocked
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] starts making fun of [target]!\n[target] feels SAD.");
				if (target.CurrentEmotion.Group?.Id != "sad")
					target.SetEmotion("sad", true);
				await Task.CompletedTask;
			}
		);

		// Vance //
		Skills["VAAttack"] = new Skill(
			name: "VAAttack",
			description: "VAAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] punches [target]!");
				await AnimationManager.Instance.WaitForAnimation(139, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);
		
		Skills["VADoNothing"] = new Skill(
			name: "VADoNothing",
			description: "VADoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] scratches his belly.");
				await Task.CompletedTask;
			}
		);
		
		Skills["VACandy"] = new Skill(
			name: "VACandy",
			description: "VACandy",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] throws old candy!");
				foreach (Actor target in targets)
				{
					AnimationManager.Instance.PlayAnimation(123, target);
					BattleManager.Instance.Damage(self, target, () => 7, false, neverCrit: true);	
				}

				await Task.CompletedTask;
			}
		);
		
		Skills["VATease"] = new Skill(
			name: "VATease",
			description: "VATease",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				// DIALOGUE QUIRK: prints the success text even when the emotion is blocked
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] said mean things about [target]!\n[target] feels SAD.");
				if (target.CurrentEmotion.Group?.Id != "sad")
					target.SetEmotion("sad", true);
				await Task.CompletedTask;
			}
		);

		// The Hooligans //
		Skills["HOAngelAttack"] = new Skill(
			name: "HOAngelAttack",
			description: "HOAngelAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			priority: 999,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "ANGEL swiftly strikes [target]!");
				await AnimationManager.Instance.WaitForAnimation(136, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2f - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);
		
		Skills["HOMaverickCharm"] = new Skill(
			name: "HOMaverickCharm",
			description: "HOMaverickCharm",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "THE MAVERICK winks at [target]!");
				await AnimationManager.Instance.WaitForAnimation(148, self);
				await AnimationManager.Instance.WaitForAnimation(215, target);
				target.AddStatModifier("AttackDown");
			}
		);
		
		Skills["HOKimHeadbutt"] = new Skill(
			name: "HOKimHeadbutt",
			description: "HOKimHeadbutt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "KIM slams her head into [target]!");
				await AnimationManager.Instance.WaitForAnimation(138, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3f - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);
		
		Skills["HOVanceCandy"] = new Skill(
			name: "HOVanceCandy",
			description: "HOVanceCandy",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage("THE HOOLIGANS threw old candy!");
				foreach (Actor target in targets)
				{
					AnimationManager.Instance.PlayAnimation(123, target);
					BattleManager.Instance.Damage(self, target, () => 20, false, neverCrit: true);	
				}

				await Task.CompletedTask;
			}
		);
		
		Skills["HOGroupAttack"] = new Skill(
			name: "HOGroupAttack",
			description: "HOGroupAttack",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage("THE HOOLIGANS attack together!");
				await AnimationManager.Instance.WaitForScreenAnimation(202, targets[0] is Enemy);
				foreach (Actor target in targets)
				{
					BattleManager.Instance.Damage(self, target, () => 30, false, neverCrit: true);
					await Wait.Milliseconds(500);
				}
			},
			hidden: true
		);

		// Jackson //
		Skills["JKWalkSlowly"] = new Skill(
			name: "JKWalkSlowly",
			description: "JKWalkSlowly",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] slowly inches forward...\nYou feel like you can't escape!");
				await Task.CompletedTask;
			}
		);
		
		Skills["JKAutoKill"] = new Skill(
			name: "JKAutoKill",
			description: "JKAutoKill",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] catches you!");
				BattleManager.Instance.SetCameraOffset(0, -100);
				BattleManager.Instance.ZoomCamera(2f, 90);
				AudioManager.Instance.PlaySFX("SE_bs_ghost_moving", pitch: 0.5f, volume: 0.9f);
				await Wait.AnimationFrames(4);
				AnimationManager.Instance.InitShake(Shake.FromFlash(255, 100, 5));
				await Wait.AnimationFrames(5);
				AnimationManager.Instance.InitShake(Shake.FromFlash(255, 100, 5));
				await Wait.AnimationFrames(5);
				AnimationManager.Instance.InitShake(Shake.FromFlash(255, 100, 5));
				await Wait.Frames(34);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => 999, neverMiss: false, 0f, neverCrit: true);
			}
		);

		// WASP //
		Skills["WSPAttack"] = new Skill(
			name: "WSPAttack",
			description: "WSPAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] stings [target].");
				BattleManager.Instance.Damage(self, target, () => 15, false, 0.25f, neverCrit: true);
			}
		);

		Skills["WSPDoNothing"] = new Skill(
			name: "WSPDoNothing",
			description: "WSPDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] flies around quickly!");
				await Task.CompletedTask;
			}
		);

		// HOUSEFLY //
		Skills["HFAttack"] = new Skill(
			name: "HFAttack",
			description: "HFAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(122, target);
				await Wait.Frames(60);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] landed on [target]'s face.");
				BattleLogManager.Instance.QueueMessage(target, "[actor] slaps themself in the face!");
				BattleManager.Instance.Damage(self, target, () => 1, false, 0f, neverCrit: true);
			},
			setup: async (_, _) =>
			{
				AudioManager.Instance.PlaySFX("ba_bug_buzz", 1f, 0.8f);
				await Task.CompletedTask;
			}
		);

		Skills["HFDoNothing"] = new Skill(
			name: "HFDoNothing",
			description: "HFDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] buzzes around quickly!");
				await Task.CompletedTask;
			},
			setup: async (_, _) =>
			{
				AudioManager.Instance.PlaySFX("ba_bug_buzz", 1f, 0.8f);
				await Task.CompletedTask;
			}
		);

		Skills["HFAnnoy"] = new Skill(
			name: "HFAnnoy",
			description: "HFAnnoy",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] buzzes around [target]'s ear!");
				bool canBeAngry = target.IsEmotionValid("angry");
				BattleManager.Instance.MakeAngry(target, silent: true);
				// DIALOGUE QUIRK: always prints the tier-1 ANGRY line regardless of the tier the target reaches
				if (canBeAngry)
					BattleLogManager.Instance.QueueMessage(target, "[actor] feels ANGRY!");
				else
					BattleManager.Instance.ShowEmotionNoEffect(target, "angry");
				await Task.CompletedTask;
			},
			setup: async (_, _) =>
			{
				AudioManager.Instance.PlaySFX("ba_bug_buzz");
				await Task.CompletedTask;
			}
		);

		// SOMETHING IN THE DARK //
		Skills["DRKSilent"] = new Skill(
			name: "DRKSilent",
			description: "DRKSilent",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: (Actor _, Actor _) => Task.CompletedTask
		);

		Skills["DRKDoNothing1"] = new Skill(
			name: "DRKDoNothing1",
			description: "DRKDoNothing1",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("SE_evil5", 0.5f, 0.5f);
				await Wait.Frames(28);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] taunts [target]");
				BattleLogManager.Instance.QueueMessage("as they fall.");
			}
		);

		Skills["DRKDoNothing2"] = new Skill(
			name: "DRKDoNothing2",
			description: "DRKDoNothing2",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AudioManager.Instance.PlaySFX("SE_evil5", 0.5f, 0.5f);
				await Wait.Frames(28);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] taunts [target]");
				BattleLogManager.Instance.QueueMessage("as they fall.");
			}
		);

		Skills["DRKDarkAttack"] = new Skill(
			name: "DRKDarkAttack",
			description: "DRKDarkAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(140, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] shoves [target].");
				BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.15f, new DamageOptions { NeverMiss = true, Variance = 0.2f, Crit = CritMode.Never });
			}
		);

		Skills["DRKFinalAttack"] = new Skill(
			name: "DRKFinalAttack",
			description: "DRKFinalAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(140, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] shoves [target].");
				BattleManager.Instance.Damage(self, target, () => 143, new DamageOptions { NeverMiss = true, Variance = 0f, Crit = CritMode.Never });
			}
		);

		// SOMETHING IN THE WALLS //
		Skills["WALAttack"] = new Skill(
			name: "WALAttack",
			description: "WALAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(287, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] bites [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, new DamageOptions { NeverMiss = false, Variance = 0.2f, Crit = CritMode.Never });
			}
		);

		Skills["WALDoNothing"] = new Skill(
			name: "WALDoNothing",
			description: "WALDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] is trying to talk to you...");
				await Task.CompletedTask;
			}
		);

		Skills["WALSpiderWebs"] = new Skill(
			name: "WALSpiderWebs",
			description: "WALSpiderWebs",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(177, target);
				await Wait.Frames(90);
				await AnimationManager.Instance.WaitForAnimation(215, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] entangles [target]");
				BattleLogManager.Instance.QueueMessage("in sticky webs.");
				// DIALOGUE QUIRK: prints the SPEED line even when SPEED is already at its limit
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s SPEED fell!");
				await Wait.Frames(40);
			},
			setup: async (_, target) =>
			{
				target.AddTierStatModifier("SpeedDown", silent: true);
				await Task.CompletedTask;
			}
		);

		// SOMETHING IN THE WATER //
		Skills["WTRAttack"] = new Skill(
			name: "WTRAttack",
			description: "WTRAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(140, target);
				BattleLogManager.Instance.QueueMessage(self, target, "Water pulls [target] in different");
				BattleLogManager.Instance.QueueMessage("directions.");
				BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.1f, new DamageOptions { NeverMiss = false, Variance = 0f, Crit = CritMode.Never });
			}
		);

		Skills["WTRDoNothing"] = new Skill(
			name: "WTRDoNothing",
			description: "WTRDoNothing",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] listens to [target] struggle.");
				await Task.CompletedTask;
			}
		);

		Skills["WTRDragDown"] = new Skill(
			name: "WTRDragDown",
			description: "WTRDragDown",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(197, target);
				BattleManager.Instance.Damage(self, target, () => target.CurrentHP * 0.5f, new DamageOptions { NeverMiss = true, Variance = 0f, Crit = CritMode.Never });
				if (self is SomethingInTheWater water)
					await water.DragDownEvent();
			},
			setup: async (_, target) =>
			{
				if (target.HasStatModifier("Drowning2"))
				{
					target.AddStatModifier("Drowning3", silent: true);
					target.RemoveStatModifier("Drowning2");
				}
				else if (target.HasStatModifier("Drowning1"))
				{
					target.AddStatModifier("Drowning2", silent: true);
					target.RemoveStatModifier("Drowning1");
				}
				else
				{
					target.AddStatModifier("Drowning1", silent: true);
					target.RemoveStatModifier("DrowningFinish");
				}
				await Task.CompletedTask;
			}
		);

		// BASIL (Enemy) //
		Skills["BASAttack"] = new Skill(
			name: "BASAttack",
			description: "BASAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			setup: (self, _) =>
			{
				self.AddStatModifier("AccuracyUp", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, target) =>
			{
				Task animation = AnimationManager.Instance.WaitForAnimation(199, target);
				if (target.CurrentEmotion.Id is "stressed")
				{
					AnimationManager.Instance.TintScreen(-17, -255, -255, 0, 10 / 60f);
					AudioManager.Instance.PlaySFX("BA_stab", 1f, 0.9f);
					await Wait.Frames(40);
					AnimationManager.Instance.TintScreen(0, 0, 0, 255, 10 / 60f);
				}
				await animation;
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] reaches inside [target].");
				// the attack formula includes an energy decrement to cancel out the automatic energy gain until the end of the turn
				// however, as a result the a.atk * 5 - b.def in the original damage function gets discarded
				BattleManager.Instance.Damage(self, target, () =>
				{
					int old = BattleManager.Instance.Energy;
					BattleManager.Instance.Energy = Math.Max(old - 1, 0);
					return old - 1;
				}, new DamageOptions { NeverMiss = false, Crit = CritMode.Never });
			}
		);

		Skills["BASDoNothing"] = new Skill(
			name: "BASDoNothing",
			description: "BASDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s eyes are red from crying.");
				await Task.CompletedTask;
			}
		);

		Skills["BASSomethingSetUpPlayer"] = new Skill(
			name: "BASSomethingSetUpPlayer",
			description: "BASSomethingSetUpPlayer",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: (Actor self, IReadOnlyList<Actor> _) =>
			{
				BattleManager.Instance.ForceCommand(self, BattleManager.Instance.SmoothPartyTarget(self.LastTargetIndex), Skills["BASBasilAttackPlayer"]);
				return Task.CompletedTask;
			}
		);

		Skills["BASBasilAttackPlayer"] = new Skill(
			name: "BASBasilAttackPlayer",
			description: "BASBasilAttackPlayer",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(199, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] strangles [target].");
				BattleManager.Instance.Damage(self, target, () =>
				{
					BattleManager.Instance.Energy = Math.Max(BattleManager.Instance.Energy - 1, 0);
					return 10;
				}, new DamageOptions { NeverMiss = false, Variance = 0.5f, Crit = CritMode.Never });
				await BasilSomethingAttackBasil();
			}
		);
	}

	private static async Task BasilSomethingAttackBasil()
	{
		AnimationManager animation = AnimationManager.Instance;
		AudioManager.Instance.PlaySFX("SE_bs_deep_swell", 1f, 0.9f);
		animation.InitShake(new Shake(5, 9, 60));
		await Wait.Frames(60);
		animation.ShowPicture(6, "something_attack[5x2]", Vector2.Zero, false, Vector2.One, 255);
		await animation.PlayPictureAnimation(6, "attack");
		animation.ErasePicture(6);
	}
}
