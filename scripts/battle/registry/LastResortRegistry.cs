using Godot;
using System;
using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class LastResortRegistry
{
	private static Func<Actor, Actor, Task> ExpandFurther(Vector2? offset, float scale)
	{
		return async (_, target) =>
		{
			BattleLogManager.Instance.QueueMessage(target, "[actor] expands even further!");
			await Wait.Milliseconds(500);
			if (offset is Vector2 cameraOffset)
				BattleManager.Instance.SetCameraOffset(cameraOffset.X, cameraOffset.Y);
			BattleManager.Instance.ZoomCamera(scale, 90);
			await Wait.Frames(180);
			BattleManager.Instance.ResetCameraZoom(30);
			await Wait.Frames(30);
			await AnimationManager.Instance.WaitForAnimation(218, target);
			target.AddTierStatModifier("AttackUp");
			target.AddTierStatModifier("DefenseUp");
			if (target.StatModifiers.TryGetValue("SpeedDown", out StatModifier sd) && sd is TierStatModifier speedDown && speedDown.CurrentTier == 3)
				speedDown.ReduceTier(2);
			else if (target.GetStatModifierTier("SpeedDown") < 2)
				target.AddTierStatModifier("SpeedDown");
		};
	}

	internal static void Register()
	{
		Enemies.Add("GatorGuyJawsum", () => new GatorGuyJawsum());
		Enemies.Add("MrJawsum", () => new MrJawsum());
		Enemies.Add("PlutoExpanded", () => new PlutoExpanded());

		// Gator Guy //
		Skills["GGAttack"] = new Skill(
			name: "GGAttack",
			description: "GGAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(123, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] karate chomps [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);

		Skills["GGDoNothing"] = new Skill(
			name: "GGDoNothing",
			description: "GGDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] cracks his knuckles.");
				await Task.CompletedTask;
			}
		);

		Skills["GGRoughUp"] = new Skill(
			name: "GGRoughUp",
			description: "GGRoughUp",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] gets rough.");
				await Wait.Milliseconds(100);
				AnimationManager.Instance.PlayAnimation(123, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF, false, neverCrit: true);
				await Wait.Milliseconds(917);
				AnimationManager.Instance.PlayAnimation(123, target);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF, false, neverCrit: true);
			}
		);

		// Mr. Jawsum //
		Skills["MJAttackOrder"] = new Skill(
			name: "MJAttackOrder",
			description: "MJAttackOrder",
			target: SkillTarget.AllAllies,
			cost: 0,
			effect: (_, _) => Task.CompletedTask,
			setup: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] gives orders to attack!\nEveryone feels ANGRY!");
				AudioManager.Instance.PlaySFX("SE_dinosaur", 0.8f, 1f);
				await Wait.Milliseconds(250);
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.MakeEmotionTier(enemy, "angry", 1, true);
				}
			},
			hidden: true
		);

		Skills["MJSummonGator"] = new Skill(
			name: "MJSummonGator",
			description: "MJSummonGator",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				await AnimationManager.Instance.WaitForScreenAnimation(146, true);
				BattleLogManager.Instance.QueueMessage(target, "[actor] picks up the phone and\ncalls a GATOR GUY!");
				if (target is MrJawsum jawsum)
					jawsum.SpawnGatorGuy();
				if (target is MrJawsumAlt jawsumAlt)
					jawsumAlt.SpawnGatorGuy();
			},
			hidden: true
		);

		// Pluto (Expanded) //
		Skills["PEAttack"] = new Skill(
			name: "PEAttack",
			description: "PEAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(131, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws the Moon at\n[target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF, false);
			}
		);
		
		Skills["PESubmissionHold"] = new Skill(
			name: "PESubmissionHold",
			description: "PESubmissionHold",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{ 
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] puts [target] into a submission hold!");
				target.AddTierStatModifier("SpeedDown");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF, false, 0.3f);
				await AnimationManager.Instance.WaitForAnimation(164, target);
				AnimationManager.Instance.PlayAnimation(215, target);
			}
		);
		
		Skills["PEHeadbutt"] = new Skill(
			name: "PEHeadbutt",
			description: "PEHeadbutt",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(124, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] slams his head\ninto [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 3 - target.CurrentStats.DEF, false);
				self.CurrentHP = Math.Max(1, self.CurrentHP - (int)Math.Floor(self.CurrentStats.MaxHP * 0.01f));
			}
		);
		
		Skills["PEDoNothing"] = new Skill(
			name: "PEDoNothing",
			description: "PEDoNothing",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor]'s muscles\nintimidated you.");
				await Task.CompletedTask;
			}
		);
		
		Skills["PEExpandFurther"] = new Skill(
			name: "PEExpandFurther",
			description: "PEExpandFurther",
			target: SkillTarget.Self,
			cost: 0,
			effect: ExpandFurther(new Vector2(-10, -100), 3f)
		);

		Skills["PEExpandFurtherChest"] = new Skill(
			name: "PEExpandFurtherChest",
			description: "PEExpandFurtherChest",
			target: SkillTarget.Self,
			cost: 0,
			effect: ExpandFurther(null, 2f)
		);

		Skills["PEExpandFurtherLeft"] = new Skill(
			name: "PEExpandFurtherLeft",
			description: "PEExpandFurtherLeft",
			target: SkillTarget.Self,
			cost: 0,
			effect: ExpandFurther(new Vector2(150, 20), 2.5f)
		);

		Skills["PERandomExpand"] = new Skill(
			name: "PERandomExpand",
			description: "PERandomExpand",
			target: SkillTarget.Self,
			cost: 0,
			effect: (Actor self, Actor target) =>
			{
				// VANILLA BUG: the roll is 0-2, so EXPAND FURTHER RIGHT can never be picked
				int rr = GameManager.Instance.Random.RandiRange(0, 2);
				string key = rr == 1 ? "PEExpandFurther" : rr == 2 ? "PEExpandFurtherChest" : "PEExpandFurtherLeft";
				BattleManager.Instance.QueueForceCommand(self, target, Skills[key]);
				return Task.CompletedTask;
			},
			hidden: true
		);

		Skills["PEEarthsFinale"] = new Skill(
			name: "PEEarthsFinale",
			description: "PEEarthsFinale",
			target: SkillTarget.AllEnemies,
			cost: 0,
			// vanilla adds ACCURACY UP in the setup action and lets it expire at the end of the turn
			setup: (self, _) =>
			{
				self.AddStatModifier("AccuracyUp", silent: true);
				return Task.CompletedTask;
			},
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] picks up THE EARTH\nand slams it into everyone!");
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayScreenAnimation(198, targets[0] is Enemy);
				await Wait.Milliseconds(4000);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.Damage(self, member,
						() => self.CurrentStats.ATK * 2 - member.CurrentStats.DEF, false);
				}
				if (self is PlutoExpandedEarth pluto)
					pluto.KillEarth();
			},
			hidden: true
		);
		
		Skills["PEMeteor"] = new Skill(
			name: "PEMeteor",
			description: "PEMeteor",
			target: SkillTarget.AllEnemies,
			cost: 0,
			priority: 1000,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] summons a\nmeteor shower!");
				int skipped = -1;
				if (targets.Count > 3)
				{
					skipped = GameManager.Instance.Random.RandiRange(0, targets.Count - 1);
				}
				AnimationManager.Instance.PlayScreenAnimation(288, targets[0] is Enemy);
				await Wait.Milliseconds(2000);
				for (int i = 0; i < targets.Count; i++)
				{
					// meteor always skips one target
					if (i == skipped)
						continue;
					BattleManager.Instance.Damage(self, targets[i], () => 100, false, 0.1f, neverCrit: true);
					BattleManager.Instance.Damage(self, targets[i], () => 100, false, 0.1f, neverCrit: true);
				}
			}
		);
	}
}
