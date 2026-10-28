using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class SunnyRegistry
{
	internal static void Register()
	{
		PartyMembers.Add("Sunny", () => new Sunny());
		PartyMembers.Add("Sunny (Alt)", () => new SunnyAlt());

		// SUNNY

		Skills["SRWAttack"] = new Skill(
			name: "SRWAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForAnimation(108, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement(_ => true);

		Skills["SRWAltAttack"] = new Skill(
			name: "SRWAltAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Wait.Milliseconds(1000);
				await AnimationManager.Instance.WaitForAnimation(21, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement(_ => true);

		Skills["SRWFearAttack"] = new Skill(
			name: "SRWFearAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Wait.Milliseconds(1000);
				AnimationManager.Instance.PlayAnimation(3, target);
				await Wait.Frames(30);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement(_ => true);

		Skills["SRWBasilAttack"] = new Skill(
			name: "SRWBasilAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Wait.Milliseconds(1000);
				AudioManager.Instance.PlaySFX("SE_bs_deep_swell", 1f, 0.9f);
				AnimationManager.Instance.ShowPicture(6, "something_attack[5x2]", Vector2.Zero, false, Vector2.One, 255);
				if (BattleManager.Instance.GetSunnyStandIn().CurrentEmotion.Id is "stressed")
				{
					await AnimationManager.Instance.PlayPictureAnimation(6, "stab_windup");
					AnimationManager.Instance.TintScreen(-17, -255, -255, 0, 10 / 60f);
					AnimationManager.Instance.InitShake(new Shake(5, 8, 15));
					AudioManager.Instance.PlaySFX("BA_stab", 1f, 0.9f);
					await AnimationManager.Instance.PlayPictureAnimation(6, "stab");
					AnimationManager.Instance.TintScreen(0, 0, 0, 255, 10 / 60f);
				}
				else
					await AnimationManager.Instance.PlayPictureAnimation(6, "attack");
				AudioManager.Instance.PlaySFX("SE_dig", 0.7f, 0.9f);
				await Wait.Frames(1);
				AnimationManager.Instance.ErasePicture(6);
				await Wait.Frames(30);
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement(_ => true);

		Skills["SRWBlackSpaceAttack"] = new Skill(
			name: "SRWBlackSpaceAttack",
			description: "Basic Attack",
			target: SkillTarget.Enemy,
			cost: 0,
			effect: async (self, target) =>
			{
				await Wait.Milliseconds(1000);
				await Wait.Frames(30);
				AudioManager.Instance.PlaySFX("sys_blackletter1", 1.5f, 0.9f);
				await Wait.Frames(10);
				AnimationManager.Instance.InitShake(new Shake(1, 5, 20));
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
				BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 2 - target.CurrentStats.DEF,
					false, neverCrit: true);
			},
			hidden: true,
			showFollowups: true
		).WithCustomRequirement(_ => true);

		Skills["CalmDown"] = new Skill(
			name: "CALM DOWN",
			description: "Removes EMOTIONS and heals some HEART.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (self, target) =>
			{
				self.AddStatModifier("SpecialImmortal", silent: true);
				await CalmDownEvent(self as PartyMember);
				FearFadeOut();
				AnimationManager.Instance.PlayScreenAnimation(104, false);
				self.AddStatModifier("CalmDownText", silent: true);
				int heal = target.Heal(BattleManager.RoundHealing(target.CurrentStats.MaxHP * 0.5));
				target.SetEmotion("neutral", true);
				// VANILLA BUG: CALM DOWN's EMOTION-category removal also strips DROWNING 2 and 3
				target.RemoveStatModifiersInCategory("EMOTION");
				BattleManager.Instance.SpawnDamageNumber(heal, target, DamageType.Heal);
				BattleLogManager.Instance.QueueMessage(target, "[actor] calms down.");
				if (heal > 0)
					BattleLogManager.Instance.QueueMessage(target, $"[actor] recovers {heal} HEART!");
				await Wait.Frames(250);
				await FearFadeIn();
				self.RemoveStatModifier("SpecialImmortal");
			},
			priority: 2000
			// calm down can be used while afraid
		).WithCustomRequirement(actor => actor.CurrentEmotion.Id is not "stressed");

		Skills["BrokenCalmDown"] = new Skill(
			name: "CALM DOWN",
			description: "Removes EMOTIONS and heals some HEART.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor self, Actor _) =>
			{
				await BrokenFearSequence(self, 110, "CalmDownText");
				await CalmDownEvent(self as PartyMember);
			},
			priority: 1999
		).WithCustomRequirement(actor => actor.CurrentEmotion.Id is not "stressed");

		Skills["Focus"] = new Skill(
			name: "FOCUS",
			description: "[actor]'s next attack deals more damage.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (self, target) =>
			{
				self.AddStatModifier("SpecialImmortal", silent: true);
				await FocusEvent(self as PartyMember);
				FearFadeOut();
				AnimationManager.Instance.PlayScreenAnimation(105, false);
				target.AddStatModifier("Flex");
				self.AddStatModifier("FocusText", silent: true);
				await Wait.Frames(10);
				BattleLogManager.Instance.QueueMessage(target, "[actor] focuses.");
				await Wait.Frames(250);
				await FearFadeIn();
				self.RemoveStatModifier("SpecialImmortal");
			},
			priority: 2000
		);

		Skills["Persist"] = new Skill(
			name: "PERSIST",
			description: "HEART cannot reach 0 for 1 turn.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			setup: async (Actor self, Actor _) =>
			{
				self.AddStatModifier("PersistCheck", silent: true);
				self.AddStatModifier("SpecialImmortal", silent: true);
				await PersistEvent(self as PartyMember);
				FearFadeOut();
				AnimationManager.Instance.PlayScreenAnimation(106, false);
			},
			effect: async (Actor self, Actor target) =>
			{
				target.Heal(20);
				target.AddStatModifier("SecondChance");
				target.AddStatModifier("PersistText", silent: true);
				if (target.HasStatModifier("Drowning1"))
				{
					target.AddStatModifier("DrowningFinish", silent: true);
					target.RemoveStatModifier("Drowning1");
				}
				else if (target.HasStatModifier("Drowning2"))
				{
					target.AddStatModifier("Drowning1", silent: true);
					target.RemoveStatModifier("Drowning2");
				}
				else
				{
					target.RemoveStatModifier("Drowning3");
					target.AddStatModifier("Drowning2", silent: true);
				}
				await Wait.Frames(10);
				BattleLogManager.Instance.QueueMessage(target, "[actor] persists.");
				await Wait.Frames(280);
				await FearFadeIn();
				self.RemoveStatModifier("SpecialImmortal");
			},
			priority: 2000
		);

		Skills["Overcome"] = new Skill(
			name: "OVERCOME",
			description: "Gather all your courage.",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (Actor self, IReadOnlyList<Actor> targets) =>
			{
				FearFadeOut();
				if (FightMusicAllowed())
				{
					AudioManager.Instance.FadeBGMTo(0.001f, 3f);
					AudioManager.Instance.ForceFadeOutBGS(3f);
				}
				StunAllEnemies();
				AnimationManager.Instance.ErasePicture(2);
				Task overcome = AnimationManager.Instance.WaitForScreenAnimation(107, true);
				await Wait.Frames(400);
				AnimationManager.Instance.TintScreen(255, 255, 255, 1, 1f);
				foreach (Actor target in targets)
				{
					await Wait.Frames(10);
					await overcome;
					target.AddStatModifier("OvercomeText", silent: true);
					BattleLogManager.Instance.QueueMessage(self, "[actor] overcomes.");
				}
				self.AddStatModifier("HangingEndFight", silent: true);
			},
			priority: 2000
		);

		Skills["BrokenFocus"] = new Skill(
			name: "FOCUS",
			description: "The next attack is a critical hit.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: async (Actor self, Actor _) =>
			{
				await BrokenFearSequence(self, 111, "FocusText");
				await FocusEvent(self as PartyMember);
			},
			priority: 2000
		).WithCustomRequirement(actor => actor.CurrentEmotion.Id is not "stressed");

		Skills["BrokenPersist"] = new Skill(
			name: "PERSIST",
			description: "HEART cannot reach 0 for 1 turn.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			setup: (Actor _, Actor _) => BrokenPersistSetup(),
			effect: async (Actor self, Actor target) =>
			{
				await BrokenPersistSequence(target);
				await PersistEvent(self as PartyMember);
			},
			priority: 2000
		).WithCustomRequirement(actor => actor.CurrentEmotion.Id is not "stressed");

		Skills["BrokenCalmDownBS"] = new Skill(
			name: "CALM DOWN",
			description: "Removes EMOTIONS and heals some HEART.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: (Actor self, Actor _) => BrokenFearSequence(self, 110, "CalmDownText"),
			priority: 1999
		).WithCustomRequirement(actor => actor.CurrentEmotion.Id is not "stressed").WithAfterUse(CalmDownEvent);

		Skills["BrokenFocusBS"] = new Skill(
			name: "FOCUS",
			description: "The next attack is a critical hit.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			effect: (Actor self, Actor _) => BrokenFearSequence(self, 111, "FocusText"),
			priority: 2000
		).WithCustomRequirement(actor => actor.CurrentEmotion.Id is not "stressed").WithAfterUse(FocusEvent);

		Skills["BrokenPersistBS"] = new Skill(
			name: "PERSIST",
			description: "HEART cannot reach 0 for 1 turn.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			setup: (Actor _, Actor _) => BrokenPersistSetup(),
			effect: (Actor _, Actor target) => BrokenPersistSequence(target),
			priority: 2000
		).WithCustomRequirement(actor => actor.CurrentEmotion.Id is not "stressed").WithAfterUse(PersistEvent);

		Skills["Allegro"] = new Skill(
			name: "ALLEGRO",
			description: "Attacks 3 times. \nCost: 19",
			target: SkillTarget.Enemy,
			cost: 19,
			effect: async (self, target) =>
			{
				await AnimationManager.Instance.WaitForAnimation(103, target);
				BattleLogManager.Instance.QueueMessage(self, "[actor] strikes three times.");
				for (int i = 0; i < 3; i++)
				{
					if (i > 0)
						await Wait.Frames(8);
					BattleManager.Instance.Damage(self, target,
						() => target.CurrentStats.MaxHP * 0.15f + self.CurrentStats.ATK - target.CurrentStats.DEF,
						false);
				}
			}
		);

		Skills["Encore"] = new Skill(
			name: "ENCORE",
			description: "Your JUICE will not fall for 3 turns.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			priority: 1000,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] gathered themself...");
				target.AddStatModifier("Encore");
				await AnimationManager.Instance.WaitForEncore();
			}
		).WithCustomRequirement(actor =>
			!actor.HasStatModifier("Encore") && !actor.CurrentEmotion.BlocksActions);

		Skills["Cherish"] = new Skill(
			name: "CHERISH",
			description: "Heal your wounds and come back stronger.\nCost: 0",
			target: SkillTarget.Self,
			cost: 0,
			priority: 1000,
			effect: async (_, target) =>
			{
				BattleLogManager.Instance.QueueMessage(target, "[actor] steadies their breathing.");
				target.AddTierStatModifier("AttackUp", silent: true);
				target.AddTierStatModifier("DefenseUp", silent: true);
				target.AddTierStatModifier("SpeedUp", silent: true);
				int index = target.GetStatModifierTier("CherishDialogue");
				if (index < 4)
					target.AddTierStatModifier("CherishDialogue");
				await AnimationManager.Instance.PlayCherish(index);
				if (index == 4)
					target.Skills.Remove("Cherish");
				if (target.HasStatModifier("Encore"))
				{
					// VANILLA BUG: CHERISH removes MARI POISON instead of ENCORE, so ENCORE stays
					foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
						member.Heal(300);
					foreach (PartyMember member in BattleManager.Instance.GetAllPartyMembers())
						member.HealJuice(30);
				}
				else
				{
					target.RecoverAll();
				}

				AddFixedTierStatModifier(target, "AttackUp", 3);
				AddFixedTierStatModifier(target, "DefenseUp", 1);
				AddFixedTierStatModifier(target, "SpeedUp", 2);
				if (target.GetStatModifierTier("CherishDialogue") == 4)
					target.AddStatModifier("Cherished");
				if (OmoriEnemy.SealsCherish())
				{
					foreach (PartyMember member in BattleManager.Instance.GetAlivePartyMembers())
						member.AddStatModifier("CherishSeal", silent: true);
				}
				await AnimationManager.Instance.HideCherish();
				foreach (Enemy enemy in BattleManager.Instance.GetAllAliveEnemies())
					enemy.AddStatModifier("CallForFriendDelay", silent: true);
			}
		).WithCustomRequirement(actor => !actor.CurrentEmotion.BlocksActions && !actor.HasStatModifier("CherishSeal"));
	}

	internal static async Task CalmDownEvent(PartyMember user)
	{
		if (!HasBasilSomething(user))
			BattleManager.Instance.AddEnergy(-10);
		if (!StorySwitches.HeightsDone)
		{
			if (FightMusicAllowed())
			{
				AudioManager.Instance.FadeBGMTo(0.001f, 5f);
				AudioManager.Instance.ForceFadeOutBGS(5f);
			}
			StunAllEnemies();
		}
		if (StorySwitches.FearPhase == 1)
			StunAllEnemies();
		if (!HasBasilSomething(user))
			return;
		user.UnequipSkill(0);
		await Wait.Frames(2);
		PartyForgets("BrokenCalmDown", "BrokenCalmDownBS");
		await Wait.Frames(2);
		DialogueManager.Instance.QueueMessage(@"You tried to CALM DOWN...\|[br]but your lungs fought to breathe.");
		await DialogueManager.Instance.WaitForDialogue();
		if (!StorySwitches.BasilBattle)
			StorySwitches.BrokenSkillUses += 1;
	}

	internal static async Task FocusEvent(PartyMember user)
	{
		if (!StorySwitches.SpidersDone)
		{
			if (FightMusicAllowed())
			{
				AudioManager.Instance.FadeBGMTo(0.001f, 5f);
				AudioManager.Instance.ForceFadeOutBGS(5f);
			}
			StunAllEnemies();
		}
		if (StorySwitches.FearPhase == 2)
			StunAllEnemies();
		if (!HasBasilSomething(user))
			return;
		await Wait.Frames(2);
		user.UnequipSkill(1);
		await Wait.Frames(2);
		PartyForgets("BrokenFocus", "BrokenFocusBS");
		await Wait.Frames(2);
		DialogueManager.Instance.QueueMessage(@"You tried to FOCUS...\|[br]but nausea overwhelmed you.");
		await DialogueManager.Instance.WaitForDialogue();
		if (!StorySwitches.BasilBattle)
			StorySwitches.BrokenSkillUses += 1;
	}

	internal static async Task PersistEvent(PartyMember user)
	{
		if (!StorySwitches.DrowningDone)
			StunAllEnemies();
		if (StorySwitches.FearPhase == 3)
			StunAllEnemies();
		if (!HasBasilSomething(user))
			return;
		user.UnequipSkill(2);
		await Wait.Frames(2);
		PartyForgets("BrokenPersist", "BrokenPersistBS");
		await Wait.Frames(2);
		DialogueManager.Instance.QueueMessage(@"You tried to PERSIST...\|[br]but you couldn't hold steady.");
		await DialogueManager.Instance.WaitForDialogue();
		if (!StorySwitches.BasilBattle)
			StorySwitches.BrokenSkillUses += 1;
	}

	internal static async Task ReleaseStress(PartyMember sunny)
	{
		if (sunny.CurrentEmotion.Id is "stressed" || !StorySwitches.BasilBattle)
			return;
		AudioStreamPlayer bgm = AudioManager.Instance.BGM;
		string savedBGM = bgm.Playing ? AudioManager.Instance.CurrentBGM : "";
		float savedPosition = AudioManager.Instance.GetBGMPosition();
		float savedVolume = Mathf.DbToLinear(bgm.VolumeDb);
		float savedPitch = AudioManager.Instance.CurrentBGMPitch;
		double savedLoopOffset = (bgm.Stream as AudioStreamOggVorbis)?.LoopOffset ?? 0;
		await Wait.Frames(60);
		if (FightMusicAllowed())
		{
			AudioManager.Instance.FadeBGMTo(0.001f, 5f);
			AudioManager.Instance.ForceFadeOutBGS(5f);
		}
		AnimationManager.Instance.ShowPicture(5, "black_screen", Vector2.Zero, false, Vector2.One, 0);
		await AnimationManager.Instance.WaitForMovePicture(5, Vector2.Zero, Vector2.One, 255, 300);
		await Wait.Frames(90);
		if (!sunny.IsToast)
			sunny.SetEmotion("stressed", true);
		AnimationManager.Instance.ShowPicture(3, "black_screen", Vector2.Zero, false, Vector2.One, 255);
		await Wait.Frames(1);
		AnimationManager.Instance.ErasePicture(5);
		AnimationManager.Instance.ShowPicture(4, "release_stress[5x5]", Vector2.Zero, false, Vector2.One, 255);
		_ = AnimationManager.Instance.PlayPictureAnimation(4, "loop");
		await Wait.Frames(7);
		if (FightMusicAllowed())
			AudioManager.Instance.ForcePlayBGS("boss_basil_2", 0.9f, 1f);
		AudioManager.Instance.PlaySFX("cutscene_stress_out", 1f, 0.9f);
		AnimationManager.Instance.MovePicture(3, Vector2.Zero, Vector2.One, 0, 200);
		await AnimationManager.Instance.PlayPictureAnimation(4, "stress");
		await AnimationManager.Instance.WaitForMovePicture(3, Vector2.Zero, Vector2.One, 255, 1);
		AnimationManager.Instance.ErasePicture(4);
		AnimationManager.Instance.TintScreen(0, 0, 0, 255, 1 / 60f);
		AnimationManager.Instance.ShowPicture(5, "black_screen", Vector2.Zero, false, Vector2.One, 255);
		AnimationManager.Instance.ErasePicture(3);
		await Wait.Frames(90);
		await AnimationManager.Instance.WaitForMovePicture(5, Vector2.Zero, Vector2.One, 0, 360);
		AnimationManager.Instance.ErasePicture(5);
		if (savedBGM == "" || !FightMusicAllowed())
			return;
		AudioManager.Instance.ForcePlayBGM(savedBGM, 0.001f, savedPitch);
		AudioManager.Instance.SeekBGM(savedPosition);
		AudioManager.Instance.ForceSetBGMLoopOffset(savedLoopOffset);
		AudioManager.Instance.FadeBGMTo(savedVolume, 0.5f);
	}

	private static bool HasBasilSomething(PartyMember user) => user != null && user.HasStatModifier("BasilSomething");

	private static void PartyForgets(params string[] skills)
	{
		foreach (PartyMember member in BattleManager.Instance.GetAllPartyMembers())
		{
			foreach (string skill in skills)
				member.ForgetSkill(skill);
		}
	}

	private static async Task BrokenFearSequence(Actor self, int animation, string marker)
	{
		FearFadeOut();
		AnimationManager.Instance.PlayScreenAnimation(animation, false);
		self.AddStatModifier(marker, silent: true);
		await Wait.Frames(250);
		await FearFadeIn();
	}

	private static Task BrokenPersistSetup()
	{
		FearFadeOut();
		AnimationManager.Instance.PlayScreenAnimation(112, false);
		return Task.CompletedTask;
	}

	private static async Task BrokenPersistSequence(Actor target)
	{
		target.AddStatModifier("PersistText", silent: true);
		target.AddStatModifier("PersistText", silent: true);
		await Wait.Frames(280);
		await FearFadeIn();
	}

	private static void FearFadeOut()
	{
		if (!StorySwitches.HeightsDone)
			AnimationManager.Instance.TintScreen(-255, -255, -255, 0, 1f);
	}

	private static async Task FearFadeIn()
	{
		if (StorySwitches.HeightsDone)
			return;
		await AnimationManager.Instance.WaitForTintOverlay(Colors.Black, 24 / 60f, layer: 22);
		AnimationManager.Instance.TintScreen(0, 0, 0, 0, 1f);
	}

	private static bool FightMusicAllowed() =>
		!AudioManager.Instance.BGMLocked && BattleManager.Instance.GetAllAliveEnemies().All(enemy => enemy.CanChangeBGM);

	private static void StunAllEnemies()
	{
		foreach (Enemy enemy in BattleManager.Instance.GetAllAliveEnemies())
			enemy.Stun(1);
	}
}
