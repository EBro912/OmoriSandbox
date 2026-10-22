using Godot;
using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Actors;

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
				// VANILLA BUG: prints the success text even when the emotion is blocked
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
				// VANILLA BUG: prints the success text even when the emotion is blocked
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
				// VANILLA BUG: prints the success text even when the emotion is blocked
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
	}
}
