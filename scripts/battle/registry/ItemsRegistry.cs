using System;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;
using OmoriSandbox.Editor;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class ItemsRegistry
{
	internal static void Register()
	{
		#region SNACKS

		AddSnack("Tofu", "Soft cardboard, basically.\nHeals 5 HEART.", 5, 0);
		AddSnack("Candy", "A child's favorite food. Sweet!\nHeals 30 HEART.", 30, 17);
		AddSnack("Smores", "S'more smores, please!\nHeals 50 HEART.", 50, 34);
		AddSnack("Granola Bar", "A healthy stick of grain.\nHeals 60 HEART.", 60, 51);
		AddSnack("Bread", "A slice of life.\nHeals 60 HEART.", 60, 68);
		AddSnack("Nachos", "Suggested serving size: 6-8 nachos.\nHeals 75 HEART.", 75, 14);
		AddSnack("Chicken Wing", "Wing of chicken.\nHeals 80 HEART.", 80, 31);
		AddSnack("Hot Dog", "Better than a cold dog.\nHeals 100 HEART.", 100, 63);
		AddSnack("Waffle", "Designed to hold syrup!\nHeals 150 HEART.", 150, 71);
		AddSnack("Pancake", "Not designed to hold syrup...\nHeals 150 HEART.", 150, 8);
		AddSnack("Pizza Slice", "1/8th of a WHOLE PIZZA.\nHeals 175 HEART.", 175, 16);
		AddSnack("Fish Taco", "Aquatic taco.\nHeals 200 HEART.", 200, 24);
		AddSnack("Cheeseburger", "Contains all food groups, so it's healthy!\nHeals 250 HEART.", 250, 32);

		AddSnack("Chocolate", "Chocolate!? Oh, it's baking chocolate...\nHeals 40% of HEART.", 0.4, 40);
		AddSnack("Donut", "Circular bread with a hole in it.\nHeals 60% of HEART.", 0.6, 48);
		AddSnack("Ramen", "Now that is a lot of sodium!\nHeals 80% of HEART.", 0.8, 56);
		AddSnack("Spaghetti", "Wet noodles slathered with chunky\nsauce. Fully heals a friend's HEART.", 1.0, 64);
		AddSnack("Dino Pasta", "Pasta shaped like dinosaurs.\nFully restores a friend's HEART.", 1.0, 10);

		AddGroupSnack("Popcorn", "9/10 dentists hate it.\nHeals 35 HEART to all friends.", 35, 1);
		AddGroupSnack("Fries", "From France, wherever that is...\nHeals 60 HEART to all friends.", 60, 9);
		AddGroupSnack("Cheese Wheel", "Delicious, yet functional.\nHeals 100 HEART to all friends.", 100, 25);
		AddGroupSnack("Whole Chicken", "An entire chicken, wings and all.\nHeals 175 HEART to all friends.", 175, 33);
		AddGroupSnack("Whole Pizza", "8/8ths of a WHOLE PIZZA.\nHeals 250 HEART to all friends.", 250, 41);
		AddGroupSnack("Dino Clumps", "Chicken nuggets shaped like dinosaurs.\nHeals 250 HEART to all friends.", 250, 2);

		AddJuiceSnack("Plum Juice", "For seniors. Wait, that's PRUNE JUICE.\nHeals 15 JUICE.", 15, 26);
		AddJuiceSnack("Apple Juice", "Apparently better than ORANGE JUICE.\nHeals 25 JUICE.", 25, 42);
		AddJuiceSnack("Breadfruit Juice", "Does not taste like bread.\nHeals 50 JUICE.", 50, 66);
		AddJuiceSnack("Lemonade", "When life gives you lemons, make this!\nHeals 75 JUICE.", 75, 11);
		AddJuiceSnack("Orange Juice", "Apparently better than APPLE JUICE.\nHeals 100 JUICE.", 100, 35);
		AddJuiceSnack("Pineapple Juice", "Painful... Why do you drink it?\nHeals 150 JUICE.", 150, 43);
		AddJuiceSnack("Bottled Water", "Water in a bottle.\nHeals 100 JUICE.", 100, 44);
		AddJuiceSnack("Fruit Juice?", "You are not sure what fruit it is.\nHeals 75 JUICE.", 75, 29);


		AddJuiceSnack("Cherry Soda", "Carbonated hell sludge.\nHeals 25% of JUICE.", 0.25, 50);
		AddJuiceSnack("Star Fruit Soda", "To be shared with a friend.\nHeals 35% of JUICE.", 0.35, 58);
		AddJuiceSnack("Tasty Soda", "Tasty soda for thirsty people.\nHeals 50% of JUICE.", 0.5, 3);
		AddJuiceSnack("Peach Soda", "A regular PEACH SODA.\nHeals 60% of JUICE.", 0.6, 19);
		AddJuiceSnack("Butt Peach Soda", "An irregular PEACH SODA.\nHeals 61% of JUICE.", 0.61, 27);
		AddJuiceSnack("Watermelon Juice", "Heavenly nectar.\nFully heals a friend's JUICE.", 1.0, 36);
		AddJuiceSnack("Dino Melon Soda", "Melon soda in a dino-shaped bottle.\nFully heals a friend's JUICE.", 1.0, 5);

		AddGroupJuiceSnack("Banana Smoothie", "A little bland, but it does the job.\nHeals 20 JUICE to all friends.", 20, 67);
		AddGroupJuiceSnack("Mango Smoothie", "Makes you tango!\nHeals 40 JUICE to all friends.", 40, 52);
		AddGroupJuiceSnack("Berry Smoothie", "A healthy smoothie that tastes like\ndirt. Heals 60 JUICE to all friends.", 60, 12);
		AddGroupJuiceSnack("Melon Smoothie", "Chunky green melon goodness.\nHeals 80 JUICE to all friends.", 80, 20);
		AddGroupJuiceSnack("S.berry Smoothie", "The default smoothie.\nHeals 100 JUICE to all friends.", 100, 28);
		AddGroupJuiceSnack("Dino Smoothie", "Berry smoothie in a dino-shaped cup.\nHeals 150 JUICE to all friends.", 150, 13);

		AddComboSnack("Tomato", "You say tomato, I say tomato.\nHeals 100 HEART and 50 JUICE.", 100, 50, 57);
		AddComboSnack("Combo Meal", "What more could you ask for?\nHeals 250 HEART and 100 JUICE.", 250, 100, 65);

		Items["Grape Soda"] = new Item(
			name: "GRAPE SODA",
			description: "Objectively the best soda.\nHeals 80% of JUICE.",
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses GRAPE SODA!");
				AnimationManager.Instance.PlayAnimation(212, target);
				// grape soda uses emotion due to an oversight
				BattleManager.Instance.HealJuice(self, target, () => target.CurrentStats.MaxJuice * 0.8);
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 59
		);

		Items["Coffee"] = new Item(
			name: "COFFEE",
			description: "Bitter bean juice.\nIncreases a friend's SPEED.",
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses COFFEE!");
				AnimationManager.Instance.PlayAnimation(214, target);
				// coffee heals, uses emotion, and has a variance due to an oversight
				BattleManager.Instance.Heal(self, target, () => target.CurrentStats.MaxJuice * 0.1);
				// DIALOGUE QUIRK: prints the success line even when the stat is already at its limit
				target.AddTierStatModifier("SpeedUp", 3);
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 39
		);

		Items["☐☐☐"] = new Item(
		   name: "☐☐☐",
		   description: "☐☐☐☐☐☐☐☐☐ ☐☐☐ ☐☐☐",
		   target: SkillTarget.Ally,
		   effect: async (self, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses ☐☐☐!");
			   AnimationManager.Instance.PlayAnimation(215, target);
			   // ☐☐☐ uses emotion due to an oversight
			   BattleManager.Instance.Heal(self, target, () => 50, 0f);
			   await Task.CompletedTask;
		   },
		   spritesheetPath: "res://assets/system/itemConsumables.png",
		   spriteIndex: 0
	   );

		AddDamagingJuiceItem("Prune Juice", "This tastes horrible. Don't drink it.\nHeals 30 JUICE... probably.", 0.3, 30, 45, 26);
		AddDamagingJuiceItem("Rotten Milk", "This is bad. Don't drink it.\nHeals 10 JUICE + ???", 0.5, 10, 15, 79);

		Items["Milk"] = new Item(
			name: "MILK",
			description: "Good for your bones. Heals 10 JUICE\nand increases DEFENSE for the battle.",
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses MILK!");
				AnimationManager.Instance.PlayAnimation(213, target);
				await Wait.Milliseconds(2000);
				AnimationManager.Instance.PlayAnimation(214, target);
				int total = 10;
				if (BattleManager.Instance.PartyHasLivingWeapon("Blender", "Ol' Reliable"))
					total = 15;
				total = target.HealJuice(total);
				BattleManager.Instance.SpawnDamageNumber(total, target, DamageType.JuiceGain);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
				target.AddStatModifier("DefenseUp");
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 60
		);

		Items["Sno-Cone"] = new Item(
			name: "SNO-CONE",
			description: "Heals a friend's HEART and JUICE, and\nraises ALL STATS for the battle.",
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses SNO-CONE!");
				await AnimationManager.Instance.WaitForAnimation(212, target);
				target.Heal(target.CurrentStats.MaxHP);
				BattleManager.Instance.SpawnDamageNumber(target.CurrentStats.MaxHP, target, DamageType.Heal);
				target.HealJuice(target.CurrentStats.MaxJuice);
				BattleManager.Instance.SpawnDamageNumber(target.CurrentStats.MaxJuice, target, DamageType.JuiceGain);
				target.AddStatModifier("SnoCone");
				AnimationManager.Instance.PlayAnimation(214, target);
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s ATTACK rose!");
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s DEFENSE rose!");
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s SPEED rose!");
				BattleLogManager.Instance.QueueMessage(self, target, "[target]'s LUCK rose!");
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 49
		);

		Items["Life Jam"] = new Item(
			name: "LIFE JAM",
			description: "Infused with the spirit of life.\nRevives a friend that is TOAST.",
			target: SkillTarget.DeadAlly,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses LIFE JAM!");
				if (!target.IsToast)
				{
					target = BattleManager.Instance.GetRandomDeadPartyMember();
					if (target == null)
					{
						BattleLogManager.Instance.QueueMessage("It had no effect.");
						return;
					}
				}
				await AnimationManager.Instance.WaitForAnimation(269, target);
				if (BattleManager.Instance.PartyHasLivingCharm("Breadphones"))
					target.Revive(target.CurrentStats.MaxHP);
				else
					target.Revive(target.CurrentStats.MaxHP / 2);
				target.SetEmotion("neutral", true);
				BattleLogManager.Instance.QueueMessage(self, target, "[target] rose again!");
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 15
		);

		Items["Dino Jam"] = new Item(
		   name: "DINO JAM",
		   description: "Infused with the spirit of dino life.\nFully revives a friend that is TOAST.",
		   target: SkillTarget.DeadAlly,
		   effect: async (self, target) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses DINO JAM!");
			   if (!target.IsToast)
			   {
				   target = BattleManager.Instance.GetRandomDeadPartyMember();
				   if (target == null)
				   {
					   BattleLogManager.Instance.QueueMessage("It had no effect.");
					   return;
				   }
			   }
			   await AnimationManager.Instance.WaitForAnimation(269, target);
			   target.Revive(target.CurrentStats.MaxHP);
			   target.SetEmotion("neutral", true);
			   BattleLogManager.Instance.QueueMessage(self, target, "[target] rose again!");
		   },
		   spritesheetPath: "res://assets/system/itemConsumables.png",
		   spriteIndex: 47
		);

		Items["Jam Packets"] = new Item(
		   name: "JAM PACKETS",
		   description: "Infused with the spirit of life.\nRevives all friends that are TOAST.",
		   target: SkillTarget.AllDeadAllies,
		   effect: async (self, targets) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, "[actor] uses JAM PACKETS!");
			   if (targets.All(x => !x.IsToast))
			   {
				   BattleLogManager.Instance.QueueMessage("It had no effect.");
				   return;
			   }
			   foreach (Actor member in targets)
			   {
				   AnimationManager.Instance.PlayAnimation(269, member);
				   member.Revive(BattleManager.RoundHealing(member.CurrentStats.MaxHP * 0.25));
				   member.SetEmotion("neutral", true);
				   BattleLogManager.Instance.QueueMessage(self, member, "[target] rose again!");
			   }
			   await Task.CompletedTask;
		   },
		   spritesheetPath: "res://assets/system/itemConsumables.png",
		   spriteIndex: 82
		);

		// TODO: faraway town snacks

		#endregion

		#region TOYS
		Items["Rubber Band"] = new Item(
			name: "RUBBER BAND",
			description: "Deals damage to a foe and reduces\ntheir DEFENSE.",
			target: SkillTarget.Enemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses RUBBER BAND!");
				BattleManager.Instance.Damage(self, target, () => 50, true, 0, neverCrit: true, ignoreEmotion: !SettingsMenuManager.Instance.ToysUseEmotionDamage);
				await AnimationManager.Instance.WaitForAnimation(219, target);
				target.AddStatModifier("DefenseDown");
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 69
		);

		Items["Big Rubber Band"] = new Item(
			name: "BIG RUBBER BAND",
			description: "Deals big damage to a foe and\nreduces their DEFENSE.",
			target: SkillTarget.Enemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses BIG RUBBER BAND!");
				BattleManager.Instance.Damage(self, target, () => 150, true, 0, neverCrit: true, ignoreEmotion: !SettingsMenuManager.Instance.ToysUseEmotionDamage);
				await AnimationManager.Instance.WaitForAnimation(219, target);
				target.AddStatModifier("DefenseDown");
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 69
		);

		Items["Jacks"] = new Item(
			name: "JACKS",
			description: "Deals small damage to all foes\nand reduces their SPEED.",
			target: SkillTarget.AllEnemies,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses JACKS!");
				foreach (Actor enemy in targets)
				{
					AnimationManager.Instance.PlayAnimation(122, enemy);
				}
				await Wait.Milliseconds(1000);
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy, () => 25, true, 0, neverCrit: true, ignoreEmotion: !SettingsMenuManager.Instance.ToysUseEmotionDamage);
					AnimationManager.Instance.PlayAnimation(219, enemy);
					enemy.AddTierStatModifier("SpeedDown", silent: true);
				}
				BattleLogManager.Instance.QueueMessage("All foes' SPEED fell.");
				await Wait.Milliseconds(500);
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 61
		).WithTiersFromFirstTarget();

		Items["Dynamite"] = new Item(
			name: "DYNAMITE",
			description: "Actually dangerous...\nDeals heavy damage to all foes.",
			target: SkillTarget.AllEnemies,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses DYNAMITE!");
				await AnimationManager.Instance.WaitForScreenAnimation(172, true);
				foreach (Actor enemy in targets)
				{
					BattleManager.Instance.Damage(self, enemy, () => 150, false, 0, neverCrit: true, ignoreEmotion: !SettingsMenuManager.Instance.ToysUseEmotionDamage);
				}
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 6
		);

		Items["Air Horn"] = new Item(
			name: "AIR HORN",
			description: "Who would invent this!?\nInflicts ANGER on all friends.",
			target: SkillTarget.AllAllies,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses AIR HORN!");
				AudioManager.Instance.PlaySFX("SE_airhorn", 1, 0.9f);
				foreach (Actor member in targets)
					BattleManager.Instance.MakeAngry(member);
				await Task.CompletedTask;
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 62
		);

		Items["Rain Cloud"] = new Item(
			name: "RAIN CLOUD",
			description: "Angsty water droplets.\nInflicts SAD on all friends.",
			target: SkillTarget.AllAllies,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses RAIN CLOUD!");
				AudioManager.Instance.PlaySFX("BA_sad_level_2", 1, 0.9f);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.MakeSad(member);
				}
				await Task.CompletedTask;
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 46
		);

		Items["Confetti"] = new Item(
			name: "CONFETTI",
			description: "Small squares of colorful paper.\nInflicts HAPPY on all friends.",
			target: SkillTarget.AllAllies,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses CONFETTI!");
				AudioManager.Instance.PlaySFX("GEN_ta_da", 1, 0.9f);
				foreach (Actor member in targets)
				{
					BattleManager.Instance.MakeHappy(member);
				}
				await Task.CompletedTask;
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 30
		);

		Items["Sparkler"] = new Item(
			name: "SPARKLER",
			description: "Little fires.\nInflicts HAPPY on a friend or foe.",
			target: SkillTarget.AllyOrEnemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses SPARKLER!");
				AudioManager.Instance.PlaySFX("GEN_pahpuh", 1, 0.9f);
				BattleManager.Instance.MakeHappy(target);
				await Task.CompletedTask;
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 22
		);

		Items["Poetry Book"] = new Item(
			name: "POETRY BOOK",
			description: "Sad words strung together.\nInflicts SAD on a friend or foe.",
			target: SkillTarget.AllyOrEnemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses POETRY BOOK!");
				AudioManager.Instance.PlaySFX("BA_angsty_song", volume: 0.9f);
				await Wait.Milliseconds(360);
				AudioManager.Instance.PlaySFX("BA_sad_poem", volume: 0.9f);
				await Wait.Milliseconds(1000);
				BattleManager.Instance.MakeSad(target);
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 38
		);

		Items["Present"] = new Item(
			name: "PRESENT",
			description: "It's not what you wanted...\nInflicts ANGER on a friend or foe.",
			target: SkillTarget.AllyOrEnemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses PRESENT!");
				AudioManager.Instance.PlaySFX("SE_shuffle", 1, 0.9f);
				BattleManager.Instance.MakeAngry(target);
				await Task.CompletedTask;
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 54
		);

		Items["Dandelion"] = new Item(
			name: "DANDELION",
			description: "Has a calming effect.\nRemoves EMOTION from a friend or foe.",
			target: SkillTarget.AllyOrEnemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses DANDELION!");
				AudioManager.Instance.PlaySFX("BA_calm_down", 1, 0.9f);
				if (target.CurrentEmotion.Id == "neutral" || target.IsEmotionLocked)
				{
					BattleLogManager.Instance.QueueMessage("It had no effect.");
				}
				else
				{
					BattleLogManager.Instance.QueueMessage(self, target, "[target] feels NEUTRAL.");
					target.SetEmotion("neutral", true);
				}
				await Task.CompletedTask;
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 70
		).WithLastTarget(LastTargetRecord.None);
		Items["Pepper Spray"] = new Item(
			name: "PEPPER SPRAY",
			description: "For self-defense only.",
			target: SkillTarget.Enemy,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] uses PEPPER SPRAY!");
				await AnimationManager.Instance.WaitForScreenAnimation(271, target is Enemy);
				BattleManager.Instance.Damage(self, target, () => 500, false, 0f, neverCrit: true, ignoreEmotion: !SettingsMenuManager.Instance.ToysUseEmotionDamage);
			},
			isToy: true,
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: 83
		);
		#endregion
	}

	/// <summary>
	/// Adds a snack that provides flat healing
	/// </summary>
	private static void AddSnack(string name, string description, int healing, int iconIndex)
	{
		Items[name] = new Item(
			name: name.ToUpper(),
			description: description,
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
				AnimationManager.Instance.PlayAnimation(212, target);
				int heal = healing;
				if (BattleManager.Instance.PartyHasLivingWeapon("Frying Pan", "Ol' Reliable"))
					heal = BattleManager.RoundHealing(heal * 1.5);
				heal = target.Heal(heal);
				BattleManager.Instance.SpawnDamageNumber(heal, target, DamageType.Heal);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {heal} HEART!");
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: iconIndex
		);
	}

	/// <summary>
	/// Adds a snack that provides flat juice healing
	/// </summary>
	private static void AddJuiceSnack(string name, string description, int juice, int iconIndex)
	{
		Items[name] = new Item(
			name: name.ToUpper(),
			description: description,
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
				AnimationManager.Instance.PlayAnimation(213, target);
				int total = juice;
				if (BattleManager.Instance.PartyHasLivingWeapon("Blender", "Ol' Reliable"))
					total = BattleManager.RoundHealing(total * 1.5);
				total = target.HealJuice(total);
				BattleManager.Instance.SpawnDamageNumber(total, target, DamageType.JuiceGain);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: iconIndex
		);
	}

	private static void AddDamagingJuiceItem(string name, string description, double hpRate, int juice, int blenderJuice, int iconIndex)
	{
		Items[name] = new Item(
			name: name.ToUpper(),
			description: description,
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
				AnimationManager.Instance.PlayAnimation(213, target);
				bool blender = BattleManager.Instance.PartyHasLivingWeapon("Blender", "Ol' Reliable");
				int hpLoss = target.Damage((int)Math.Floor(target.CurrentHP * hpRate));
				// damaging items don't kill
				if (target.CurrentHP == 0)
					target.CurrentHP = 1;
				int total = target.HealJuice(blender ? blenderJuice : juice);
				if (hpLoss > 0)
				{
					if (blender)
						AudioManager.Instance.PlaySFX("se_impact_double", 1f, 1.5f);
					else
						AudioManager.Instance.PlaySFX("SE_dig", 0.7f, 0.9f);
					if (target is PartyMember)
					{
						BattleManager.Instance.AddEnergy(1);
						AnimationManager.Instance.InitShake(new Shake(5, 5, 10));
					}
				}
				BattleManager.Instance.SpawnDamageNumber(hpLoss, target);
				BattleManager.Instance.SpawnDamageNumber(total, target, DamageType.JuiceGain);
				if (blender && hpLoss > 0)
					BattleLogManager.Instance.QueueMessage("...It was a moving attack!");
				BattleLogManager.Instance.QueueMessage(self, target, hpLoss > 0 ? $"[target] took {hpLoss} damage!" : "[target] took no damage!");
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: iconIndex
		);
	}

	/// <summary>
	/// Adds a snack that provides percentage-based juice healing
	/// </summary>
	private static void AddJuiceSnack(string name, string description, double percentage, int iconIndex)
	{
		Items[name] = new Item(
			name: name.ToUpper(),
			description: description,
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
				AnimationManager.Instance.PlayAnimation(213, target);
				double juice = target.CurrentStats.MaxJuice * percentage;
				if (BattleManager.Instance.PartyHasLivingWeapon("Blender", "Ol' Reliable"))
					juice *= 1.5;
				int finalJuice = BattleManager.RoundHealing(juice);
				finalJuice = target.HealJuice(finalJuice);
				BattleManager.Instance.SpawnDamageNumber(finalJuice, target, DamageType.JuiceGain);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {finalJuice} JUICE!");
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: iconIndex
		);
	}

	/// <summary>
	/// Adds a snack that provides percentage-based healing
	/// </summary>
	private static void AddSnack(string name, string description, double percentage, int iconIndex)
	{
		Items[name] = new Item(
			name: name.ToUpper(),
			description: description,
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
				AnimationManager.Instance.PlayAnimation(212, target);
				double heal = target.CurrentStats.MaxHP * percentage;
				if (BattleManager.Instance.PartyHasLivingWeapon("Frying Pan", "Ol' Reliable"))
					heal *= 1.5;
				int finalHeal = BattleManager.RoundHealing(heal);
				finalHeal = target.Heal(finalHeal);
				BattleManager.Instance.SpawnDamageNumber(finalHeal, target, DamageType.Heal);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {finalHeal} HEART!");
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: iconIndex
		);
	}

	/// <summary>
	/// Adds a snack that provides flat healing to all allies
	/// </summary>
	private static void AddGroupSnack(string name, string description, int healing, int iconIndex)
	{
		Items[name] = new Item(
		   name: name.ToUpper(),
		   description: description,
		   target: SkillTarget.AllAllies,
		   effect: async (self, targets) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, $"[actor] uses {name.ToUpper()}!");
			   int heal = healing;
			   if (BattleManager.Instance.PartyHasLivingWeapon("Frying Pan", "Ol' Reliable"))
				   heal = BattleManager.RoundHealing(heal * 1.5);
			   foreach (Actor member in targets)
			   {
				   AnimationManager.Instance.PlayAnimation(212, member);
				   int healed = member.Heal(heal);
				   BattleManager.Instance.SpawnDamageNumber(healed, member, DamageType.Heal);
				   BattleLogManager.Instance.QueueMessage(self, member, $"[target] recovered {healed} HEART!");
			   }
			   await Task.CompletedTask;
		   },
		   spritesheetPath: "res://assets/system/itemConsumables.png",
		   spriteIndex: iconIndex
	   );
	}

	/// <summary>
	/// Adds a snack that provides flat juice healing to all allies
	/// </summary>
	private static void AddGroupJuiceSnack(string name, string description, int juice, int iconIndex)
	{
		Items[name] = new Item(
		   name: name.ToUpper(),
		   description: description,
		   target: SkillTarget.AllAllies,
		   effect: async (self, targets) =>
		   {
			   BattleLogManager.Instance.QueueMessage(self, $"[actor] uses {name.ToUpper()}!");
			   int total = juice;
			   if (BattleManager.Instance.PartyHasLivingWeapon("Blender", "Ol' Reliable"))
				   total = BattleManager.RoundHealing(total * 1.5);
			   foreach (Actor member in targets)
			   {
				   AnimationManager.Instance.PlayAnimation(213, member);
				   int restored = member.HealJuice(total);
				   BattleManager.Instance.SpawnDamageNumber(restored, member, DamageType.JuiceGain);
				   BattleLogManager.Instance.QueueMessage(self, member, $"[target] recovered {restored} JUICE!");
			   }
			   await Task.CompletedTask;
		   },
		   spritesheetPath: "res://assets/system/itemConsumables.png",
		   spriteIndex: iconIndex
	   );
	}

	/// <summary>
	/// A snack that provides flat healing and juice
	/// </summary>
	private static void AddComboSnack(string name, string description, int healing, int juice, int iconIndex)
	{
		Items[name] = new Item(
			name: name.ToUpper(),
			description: description,
			target: SkillTarget.Ally,
			effect: async (self, target) =>
			{
				BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
				AnimationManager.Instance.PlayAnimation(212, target);
				int heal = healing;
				int total = juice;
				if (BattleManager.Instance.PartyHasLivingWeapon("Frying Pan", "Ol' Reliable"))
					heal = BattleManager.RoundHealing(heal * 1.5);
				// donald compiler please come save us donald compiler please save us
				if (BattleManager.Instance.PartyHasLivingWeapon("Blender", "Ol' Reliable"))
					total = (int)Math.Round(total * 1.5f, MidpointRounding.AwayFromZero);
				heal = target.Heal(heal);
				total = target.HealJuice(total);
				BattleManager.Instance.SpawnDamageNumber(heal, target, DamageType.Heal);
				BattleManager.Instance.SpawnDamageNumber(total, target, DamageType.JuiceGain);
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {heal} HEART!");
				BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
				await Task.CompletedTask;
			},
			spritesheetPath: "res://assets/system/itemConsumables.png",
			spriteIndex: iconIndex
		);
	}
}
