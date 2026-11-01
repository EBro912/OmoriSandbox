using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Editor;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class BlackspaceRegistry
{
	internal static void Register()
	{
		Enemies.Add("OmoriEnemy", () => new OmoriEnemy());
		Enemies.Add("Merci", () => new Merci());
		Enemies.Add("OmoriSomething", () => new OmoriSomething());
		Enemies.Add("OmoriSomething (Heights)", () => new OmoriSomethingHeights());
		Enemies.Add("OmoriSomething (Spiders)", () => new OmoriSomethingSpiders());
		Enemies.Add("OmoriSomething (Drowning)", () => new OmoriSomethingDrowning());
		Enemies.Add("OmoriSomething (Blur)", () => new OmoriSomethingBlur());
		Enemies.Add("OmoriSomething (Hanging Body)", () => new OmoriSomethingHangingBody());
		Enemies.Add("Stranger", () => new Stranger());

		// FINAL OMORI //
		
		Skills["OMEAttack"] = new Skill(
			name: "OMEAttack",
			description: "OMEAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(3, target);
				await Wait.Frames(60);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF + 4,
					new DamageOptions { NeverMiss = false, Crit = CritMode.Never });
			}
		);
		
		Skills["OMEHackAway"] = new Skill(
			name: "OMEHackAway",
			description: "OMEHackAway",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] slashes wildly.");
				await Wait.Frames(10);
				await Task.WhenAll(targets.Select(member => AnimationManager.Instance.WaitForAnimation(294, member)));
				for (int i = 0; i < 3; i++)
				{
					if (i > 0)
						await Wait.Frames(8);
					foreach (Actor target in targets)
						BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF + 4,
							new DamageOptions { NeverMiss = false, Crit = CritMode.Never });
				}
			}
		);
		
		Skills["OMEStab"] = new Skill(
			name: "OMEStab",
			description: "OMEStab",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				AnimationManager.Instance.PlayAnimation(20, target);
				await Wait.Frames(60);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] stabs [target].");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					new DamageOptions { NeverMiss = false, Crit = CritMode.Always });
			}
		);
		
		Skills["OMERedHands"] = new Skill(
			name: "OMERedHands",
			description: "OMERedHands",
			target: SkillTarget.XRandomEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] erases the enemy.");
				foreach (Actor target in targets)
					AnimationManager.Instance.PlayAnimation(307, target);
				await Wait.Frames(20);
				foreach (Actor target in targets)
					BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
						DamageOptions.DefaultCanMiss);
				await Wait.Frames(20);
			}
		);
		
		Skills["OMERedHandAttack"] = new Skill(
			name: "OMERedHandAttack",
			description: "OMERedHandAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] erases the enemy.");
				Task animation = AnimationManager.Instance.WaitForAnimation(23, target);
				await Wait.Frames(20);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 4 - target.CurrentStats.DEF,
					new DamageOptions { Crit = CritMode.Never });
				await animation;
				Task statDown = AnimationManager.Instance.WaitForAnimation(215, target);
				// DIALOGUE QUIRK: prints the "fell." line even when DEFENSE is already at its limit
				target.AddTierStatModifier("DefenseDown", silent: true);
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s DEFENSE fell.");
				await statDown;
			}
		);
		
		Skills["OMEHairAttack"] = new Skill(
			name: "OMEHairAttack",
			description: "OMEHairAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] erases the enemy.");
				Task animation = AnimationManager.Instance.WaitForAnimation(24, target);
				await Wait.Frames(20);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 4 - target.CurrentStats.DEF,
					new DamageOptions { NeverMiss = false, Crit = CritMode.Never });
				await animation;
				Task statDown = AnimationManager.Instance.WaitForAnimation(215, target);
				// DIALOGUE QUIRK: prints the "STATS fell." line even when the stats are already at their limit
				target.AddTierStatModifier("SpeedDown", silent: true);
				target.AddTierStatModifier("AttackDown", silent: true);
				target.AddTierStatModifier("DefenseDown", silent: true);
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s STATS fell.");
				await statDown;
			}
		);
		
		Skills["OMEMariAttack"] = new Skill(
			name: "OMEMariAttack",
			description: "OMEMariAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				target.SetEmotion("afraid", true);
				await Wait.Frames(10);
				await AnimationManager.Instance.WaitForAnimation(25, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] erases the enemy.");
				// DIALOGUE QUIRK: prints the "feels AFRAID." line even when the target doesn't become AFRAID
				BattleLogManager.Instance.QueueMessage(self, target, "[target] feels AFRAID.");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 4 - target.CurrentStats.DEF,
					new DamageOptions { NeverMiss = false, Crit = CritMode.Never });
			}
		);
		
		Skills["OMEFinalAttack"] = new Skill(
			name: "OMEFinalAttack",
			description: "OMEFinalAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] erases the enemy.");
				await Wait.Frames(4);
				await AnimationManager.Instance.WaitForAnimation(26, target);
				await Wait.Frames(160);
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 5 - target.CurrentStats.DEF,
					new DamageOptions { Crit = CritMode.Never });
				await Wait.Frames(20);
				// DIALOGUE QUIRK: prints the "feels AFRAID." and "fell." lines even when the target doesn't become AFRAID or DEFENSE is already at its limit
				target.SetEmotion("afraid", true);
				BattleLogManager.Instance.QueueMessage(self, target, "[target] feels AFRAID.");
				await Wait.Frames(4);
				Task statDown = AnimationManager.Instance.WaitForAnimation(215, target);
				target.AddTierStatModifier("DefenseDown", silent: true);
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s DEFENSE fell.");
				await statDown;
			}
		);
		
		Skills["Play"] = new Skill(
			name: "PLAY",
			description: "Skill for final battle\n",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (Actor _, Actor _) =>
			{
				OmoriEnemy owner = BattleManager.Instance.GetAllEnemies().OfType<OmoriEnemy>()
					.FirstOrDefault(x => x is { IsToast: false, Phase: >= 5, CanChangeBGM: true });
				owner?.FadeOutBGS(300f);
				await Wait.Frames(60);
				owner?.FadeOutBGS(6f);
				owner?.FadeBGM(0.001f, 6f);
				owner?.StopBGS();
				await AnimationManager.Instance.WaitForTintOverlay(Colors.White, 90 / 60f, layer: 22);
				await Wait.Frames(360);
				if (SettingsMenuManager.Instance.PlayFinalDuet)
					await AnimationManager.Instance.WaitForVideoInternal("res://assets/movies/final_duet.ogv", layer: 100);
				BattleManager.Instance.PlayMode = false;
				foreach (Enemy enemy in BattleManager.Instance.GetAllAliveEnemies())
				{
					if (enemy is OmoriEnemy { Phase: >= 5 } omori)
						await omori.AbortBattle();
				}
				bool enemiesRemain = BattleManager.Instance.GetAllAliveEnemies().Any(x => x.CurrentHP > 0 || x.IsImmortal);
				if (owner != null && enemiesRemain && !BattleManager.Instance.AbortRequested)
				{
					BattleManager.Instance.PlayPresetBGM(0.05f);
					owner.FadeBGM(1f, 180 / 60f);
				}
				await AnimationManager.Instance.WaitForTintOverlay(new Color(1, 1, 1, 0), 180 / 60f, layer: 22);
			},
			hidden: true,
			priority: 2000
		).WithCustomRequirement(_ => true);

		// OMORI'S SOMETHING //
		Skills["OSMEngulfTrue"] = new Skill(
			name: "OSMEngulfTrue",
			description: "OSMEngulfTrue",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				foreach (Actor target in targets)
				{
					BattleLogManager.Instance.QueueMessage(self, target, "[actor] drags [target] into");
					BattleLogManager.Instance.QueueMessage("the shadows.");
					await Wait.Frames(45);
					await EngulfAnimation();
					await Wait.Frames(45);
					BattleManager.Instance.Damage(self, target, () => target.CurrentHP * 0.3f + 5, new DamageOptions { NeverMiss = true, Variance = 0.2f, Crit = CritMode.Never });
				}
			}
		);

		// SOMETHING (DROWNING) //
		Skills["OSDRReserved"] = new Skill(
			name: "OSDRReserved",
			description: "OSDRReserved",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: (Actor _, Actor _) => Task.CompletedTask
		);

		Skills["OSDROmorisSomething"] = new Skill(
			name: "OSDROmorisSomething",
			description: "OSDROmorisSomething",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: (Actor _, Actor _) => Task.CompletedTask
		);

		Skills["OSDRAttack"] = new Skill(
			name: "OSDRAttack",
			description: "OSDRAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			element: "afraid",
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(140, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] reaches through [target].");
				BattleManager.Instance.Damage(self, target, () => target.CurrentStats.MaxHP * 0.1f, new DamageOptions { NeverMiss = true, Variance = 0.2f, Crit = CritMode.Never });
				SomethingAfraid();
			}
		);

		Skills["OSDRDoNothing"] = new Skill(
			name: "OSDRDoNothing",
			description: "OSDRDoNothing",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(18, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] sees through [target].");
				BattleManager.Instance.AddEnergy(2);
				await Wait.Frames(90);
				SomethingAfraid();
			}
		);

		// SOMETHING (BLUR) //
		Skills["OSBBlurryNothing"] = new Skill(
			name: "OSBBlurryNothing",
			description: "OSBBlurryNothing",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: (Actor _, Actor _) =>
			{
				BattleLogManager.Instance.QueueMessage("SOMETHING sways in the wind.");
				return Task.CompletedTask;
			}
		);

		// HANGING BODY //
		Skills["OSHBNothing1"] = new Skill(
			name: "OSHBNothing1",
			description: "OSHBNothing1",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleManager.Instance.Damage(self, target, () => 143, new DamageOptions { NeverMiss = false, Variance = 0f, Crit = CritMode.Never });
				await Wait.Frames(15);
				await DialogueManager.Instance.ShowMessage("Your lungs tighten up.");
			},
			setup: (Actor _, Actor target) =>
			{
				HangingModifier.SetStage(target, 2);
				return Task.CompletedTask;
			}
		);

		Skills["OSHBNothing2"] = new Skill(
			name: "OSHBNothing2",
			description: "OSHBNothing2",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleManager.Instance.Damage(self, target, () => target.CurrentHP * 0.8f, new DamageOptions { NeverMiss = false, Variance = 0f, Crit = CritMode.Never });
				await Wait.Frames(15);
				await DialogueManager.Instance.ShowMessage("Your heart beats out of your chest.");
			},
			setup: (Actor _, Actor target) =>
			{
				HangingModifier.SetStage(target, 3);
				return Task.CompletedTask;
			}
		);

		Skills["OSHBNothing3"] = new Skill(
			name: "OSHBNothing3",
			description: "OSHBNothing3",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				BattleManager.Instance.Damage(self, target, () => target.CurrentHP * 0.8f, new DamageOptions { NeverMiss = false, Variance = 0f, Crit = CritMode.Never });
				await Wait.Frames(15);
				await DialogueManager.Instance.ShowMessage("Your fingers won't stop trembling.");
			},
			setup: (Actor _, Actor target) =>
			{
				// VANILLA BUG: NOTHING 3 adds HANGING 10 instead of HANGING 4, skipping NOTHING 4 to 9
				HangingModifier.SetStage(target, 10);
				return Task.CompletedTask;
			}
		);

		Skills["OSHBNothing10"] = new Skill(
			name: "OSHBNothing10",
			description: "OSHBNothing10",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (Actor _, Actor _) =>
			{
				await Wait.Frames(15);
				await DialogueManager.Instance.ShowMessage("You hear your breathing begin to steady.");
			},
			setup: (self, target) =>
			{
				if (self is Enemy enemy && OmoriSomethingTroop.Of(enemy) is { } troop)
					troop.BsSafe = true;
				HangingModifier.SetStage(target, 11);
				return Task.CompletedTask;
			}
		);

		Skills["OSHBNothing11"] = new Skill(
			name: "OSHBNothing11",
			description: "OSHBNothing11",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (Actor _, Actor _) =>
			{
				await Wait.Frames(15);
				await DialogueManager.Instance.ShowMessage("You hear your heart begin to steady.");
			},
			setup: (Actor _, Actor target) =>
			{
				HangingModifier.SetStage(target, 12);
				return Task.CompletedTask;
			}
		);

		Skills["OSHBNothing12"] = new Skill(
			name: "OSHBNothing12",
			description: "OSHBNothing12",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (Actor _, Actor _) =>
			{
				await Wait.Frames(15);
				await DialogueManager.Instance.ShowMessage(@"You gather all your courage...\![br]and learn to OVERCOME.");
			},
			setup: (Actor _, Actor target) =>
			{
				HangingModifier.SetStage(target, 13);
				return Task.CompletedTask;
			}
		);

		// STRANGER //
		Skills["STRShadowBasilAttack"] = new Skill(
			name: "STRShadowBasilAttack",
			description: "STRShadowBasilAttack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(199, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] strangles [target].");
				bool hit = false;
				BattleManager.Instance.Damage(self, target, () => target.CurrentHP - 1,
					new DamageOptions { NeverMiss = false, Variance = 0f, Crit = CritMode.Never, ShowJuiceLoss = false, OnHit = () => hit = true });
				if (hit)
				{
					target.CurrentJuice = 0;
					if (target.CurrentStats.MaxJuice > 0)
					{
						// DIALOGUE QUIRK: the base prints the target's max JUICE (result.mpDamage), not the JUICE actually lost
						BattleLogManager.Instance.QueueMessage(self, target, "[target] lost " + target.CurrentStats.MaxJuice + " JUICE...");
						BattleManager.Instance.SpawnDamageNumber(target.CurrentStats.MaxJuice, target, DamageType.JuiceLoss);
					}
				}
				// VANILLA BUG: CE 934 is reserved by this skill but never plays, because page 6 aborts before the troop interpreter picks it up
			}
		);
	}

	private static async Task EngulfAnimation()
	{
		AnimationManager animation = AnimationManager.Instance;
		AudioManager.Instance.PlaySFX("se_evil9", 1.1f, 0.9f);
		animation.ShowPicture(86, "something_engulf_1", Vector2.Zero, false, Vector2.One, 0);
		await animation.WaitForMovePicture(86, Vector2.Zero, Vector2.One, 255, 30);
		await Wait.Frames(10);
		animation.ShowPicture(85, "something_engulf_2", Vector2.Zero, false, Vector2.One, 255);
		await Wait.Frames(10);
		await animation.WaitForMovePicture(86, Vector2.Zero, Vector2.One, 0, 30);
		await Wait.Frames(50);
		animation.MovePicture(85, Vector2.Zero, Vector2.One, 0, 30);
	}

	private static void SomethingAfraid()
	{
		if (BattleManager.Instance.Energy < 6)
			return;
		PartyMember standIn = BattleManager.Instance.GetSunnyStandIn();
		if (!standIn.IsToast)
			standIn.SetEmotion("afraid", silent: true);
		standIn.ClearResult();
	}
}
