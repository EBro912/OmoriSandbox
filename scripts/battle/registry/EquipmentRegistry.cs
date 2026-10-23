using System;
using System.Threading.Tasks;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Actors;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class EquipmentRegistry
{
	internal static void Register()
	{
		#region WEAPONS
		Equipment["Shiny Knife"] = new Equipment("Shiny Knife", [new StatBonus(StatType.ATK, 5), new StatBonus(StatType.HIT, 100)]);
		Equipment["Knife"] = new Equipment("Knife", [new StatBonus(StatType.ATK, 7), new StatBonus(StatType.SPD, 2), new StatBonus(StatType.HIT, 100)]);
		Equipment["Dull Knife"] = new Equipment("Dull Knife", [new StatBonus(StatType.ATK, 9), new StatBonus(StatType.SPD, 4), new StatBonus(StatType.LCK, 2), new StatBonus(StatType.HIT, 100)]);
		Equipment["Rusty Knife"] = new Equipment("Rusty Knife", [new StatBonus(StatType.ATK, 11), new StatBonus(StatType.DEF, 2), new StatBonus(StatType.SPD, 6), new StatBonus(StatType.LCK, 4), new StatBonus(StatType.HIT, 100)]);
		Equipment["Red Knife"] = new Equipment("Red Knife", [new StatBonus(StatType.ATK, 13), new StatBonus(StatType.DEF, 6), new StatBonus(StatType.SPD, 6), new StatBonus(StatType.LCK, 6), new StatBonus(StatType.HIT, 100)]);

		Equipment["Fly Swatter"] = new Equipment("Fly Swatter", [new StatBonus(StatType.ATK, 1), new StatBonus(StatType.HIT, 1000)]);
		Equipment["Steak Knife"] = new Equipment("Steak Knife", [new StatBonus(StatType.ATK, 30), new StatBonus(StatType.HIT, 25)]);
		Equipment["Hands"] = new Equipment("Hands", [new StatBonus(StatType.ATK, 2), new StatBonus(StatType.HIT, 95)]);
		// potential todo: other violin variants?
		Equipment["Violin"] = new Equipment("Violin", [new StatBonus(StatType.ATK, 14), new StatBonus(StatType.HIT, 1000)]);

		Equipment["Stuffed Toy"] = new Equipment("Stuffed Toy", [new StatBonus(StatType.ATK, 4), new StatBonus(StatType.HIT, 100)]);
		Equipment["Comet Hammer"] = new Equipment("Comet Hammer", [new StatBonus(StatType.ATK, 6), new StatBonus(StatType.LCK, 2), new StatBonus(StatType.HIT, 100)]);
		Equipment["Body Pillow"] = new Equipment("Body Pillow", [new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.ATK, 8), new StatBonus(StatType.HIT, 100)]);
		Equipment["Pool Noodle"] = new Equipment("Pool Noodle", [new StatBonus(StatType.ATK, -5), new StatBonus(StatType.DEF, -5), new StatBonus(StatType.SPD, -5), new StatBonus(StatType.LCK, -5), new StatBonus(StatType.HIT, 100)]);
		Equipment["Cool Noodle"] = new Equipment("Cool Noodle", [new StatBonus(StatType.ATK, 15), new StatBonus(StatType.HIT, 100)]);
		Equipment["Hero's Trophy"] = new Equipment("Hero's Trophy", [new StatBonus(StatType.ATK, 10), new StatBonus(StatType.DEF, 5), new StatBonus(StatType.HIT, 100)]);
		Equipment["Mailbox"] = new Equipment("Mailbox", [new StatBonus(StatType.ATK, 12), new StatBonus(StatType.HIT, 100)]);
		Equipment["Baguette"] = new Equipment("Baguette", [new StatBonus(StatType.ATK, 10), new StatBonus(StatType.DEF, 10), new StatBonus(StatType.HIT, 100)]);
		Equipment["Sweetheart Bust"] = new Equipment("Sweetheart Bust", [new StatBonus(StatType.ATK, 20), new StatBonus(StatType.SPD, -30), new StatBonus(StatType.HIT, 75)]);
		Equipment["Baseball Bat"] = new Equipment("Baseball Bat", [new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.ATK, 20), new StatBonus(StatType.SPD, 10), new StatBonus(StatType.LCK, 10), new StatBonus(StatType.HIT, 100)]);

		Equipment["Nail Bat"] = new Equipment("Nail Bat", [new StatBonus(StatType.ATK, 3), new StatBonus(StatType.HIT, 95)]);

		Equipment["Rubber Ball"] = new Equipment("Rubber Ball", [new StatBonus(StatType.ATK, 3), new StatBonus(StatType.HIT, 100)]);
		Equipment["Meteor Ball"] = new Equipment("Meteor Ball", [new StatBonus(StatType.ATK, 4), new StatBonus(StatType.LCK, 2), new StatBonus(StatType.HIT, 100)]);
		Equipment["Blood Orange"] = new Equipment("Blood Orange", [new StatBonus(StatType.MaxJuice, 30), new StatBonus(StatType.ATK, 6), new StatBonus(StatType.HIT, 100)]);
		Equipment["Jack"] = new Equipment("Jack", [new StatBonus(StatType.ATK, 12), new StatBonus(StatType.DEF, -6), new StatBonus(StatType.LCK, -6), new StatBonus(StatType.HIT, 100)]);
		Equipment["Beach Ball"] = new Equipment("Beach Ball", [new StatBonus(StatType.ATK, 10), new StatBonus(StatType.SPD, 25), new StatBonus(StatType.HIT, 100)]);
		Equipment["Coconut"] = new Equipment("Coconut", [new StatBonus(StatType.MaxJuice, 50), new StatBonus(StatType.ATK, 8), new StatBonus(StatType.HIT, 100)]);
		Equipment["Globe"] = new Equipment("Globe", [new StatBonus(StatType.ATK, 10), new StatBonus(StatType.HIT, 1000)]);
		Equipment["Chicken Ball"] = new Equipment("Chicken Ball", [new StatBonus(StatType.SPD, 200), new StatBonus(StatType.HIT, 100)]);
		Equipment["Snowball"] = new Equipment("Snowball", [new StatBonus(StatType.ATK, 13), new StatBonus(StatType.HIT, 100)]);
		Equipment["Basketball"] = new Equipment("Basketball", [new StatBonus(StatType.MaxJuice, 50), new StatBonus(StatType.ATK, 15), new StatBonus(StatType.SPD, 100), new StatBonus(StatType.LCK, 15), new StatBonus(StatType.HIT, 100)]);

		Equipment["Basketball (Real World)"] = new Equipment("Basketball", [new StatBonus(StatType.ATK, 2), new StatBonus(StatType.HIT, 95)]);

		Equipment["Spatula"] = new Equipment("Spatula", [new StatBonus(StatType.ATK, 4), new StatBonus(StatType.HIT, 100)]);
		Equipment["Rolling Pin"] = new Equipment("Rolling Pin", [new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.ATK, 12), new StatBonus(StatType.DEF, 12), new StatBonus(StatType.HIT, 100)]);
		Equipment["Teapot"] = new Equipment("Teapot", [new StatBonus(StatType.MaxJuice, 30), new StatBonus(StatType.ATK, 6), new StatBonus(StatType.HIT, 100)]);
		Equipment["Frying Pan"] = new Equipment("Frying Pan", [new StatBonus(StatType.MaxHP, 30), new StatBonus(StatType.ATK, 7), new StatBonus(StatType.HIT, 100)]);
		Equipment["Blender"] = new Equipment("Blender", [new StatBonus(StatType.MaxJuice, 30), new StatBonus(StatType.ATK, 7), new StatBonus(StatType.HIT, 100)]);
		Equipment["Baking Pan"] = new Equipment("Baking Pan", [new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.ATK, 6), new StatBonus(StatType.HIT, 100)]);
		Equipment["Tenderizer"] = new Equipment("Tenderizer",[new StatBonus(StatType.ATK, 30), new StatBonus(StatType.HIT, 100)]);
		Equipment["LOL Sword"] = new Equipment("LOL Sword", [new StatBonus(StatType.MaxJuice, 10), new StatBonus(StatType.ATK, 14), new StatBonus(StatType.HIT, 100)]).WithStartOfBattleEffect((actor) =>
		{
			BattleManager.Instance.MakeEmotionTier(actor, "happy", 1, true);
			return Task.CompletedTask;
		});
		Equipment["Ol' Reliable"] = new Equipment("Ol' Reliable", [new StatBonus(StatType.MaxHP, 20), new StatBonus(StatType.MaxJuice, 20), new StatBonus(StatType.ATK, 20), new StatBonus(StatType.HIT, 100)]);
		Equipment["Shucker"] = new Equipment("Shucker", [new StatBonus(StatType.ATK, 10), new StatBonus(StatType.HIT, 100)]);

		Equipment["Fist"] = new Equipment("Fist", [new StatBonus(StatType.ATK, 1), new StatBonus(StatType.HIT, 95)]);

		Equipment["Garden Shears"] = new Equipment("Garden Shears", [new StatBonus(StatType.ATK, 13), new StatBonus(StatType.DEF, 6), new StatBonus(StatType.SPD, 6), new StatBonus(StatType.LCK, 6), new StatBonus(StatType.HIT, 100)]);
		#endregion

		#region CHARMS
		// TODO: missing charms (special behavior/unused): unused charms
		Equipment["3-leaf Clover"] = new Equipment("3-leaf Clover", new StatBonus(StatType.LCK, 3), true);
		Equipment["4-leaf Clover"] = new Equipment("4-leaf Clover", [new StatBonus(StatType.MaxHP, 4), new StatBonus(StatType.LCK, 4)], true);
		Equipment["5-leaf Clover"] = new Equipment("5-leaf Clover", true).WithApplyEffect(() =>
		{
			return [new StatBonus(StatType.LCK, 2 + BattleManager.Instance.Energy)];
		});
		Equipment["Backpack"] = new Equipment("Backpack", new StatBonus(StatType.DEF, 2), true);
		Equipment["Baseball Cap"] = new Equipment("Baseball Cap", new StatBonus(StatType.DEF, 10), true);
		Equipment["Binoculars"] = new Equipment("Binoculars", [new StatBonus(StatType.DEF, 2), new StatBonus(StatType.HIT, 200)], true);
		Equipment["Blanket"] = new Equipment("Blanket", [new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.DEF, 1)], true);
		Equipment["Bow Tie"] = new Equipment("Bow Tie", new StatBonus(StatType.DEF, 4), true);
		Equipment["Bracelet"] = new Equipment("Bracelet", new StatBonus(StatType.DEF, 1), true);
		Equipment["Breadphones"] = new Equipment("Breadphones", [new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.DEF, 5)], true);
		Equipment["Bubble Wrap"] = new Equipment("Bubble Wrap", new StatBonus(StatType.DEF, 3), true);
		Equipment["Bunny Ears"] = new Equipment("Bunny Ears", [new StatBonus(StatType.DEF, 3), new StatBonus(StatType.SPD, 12)], true);
		Equipment["Cat Ears"] = new Equipment("Cat Ears", [new StatBonus(StatType.DEF, 1), new StatBonus(StatType.SPD, 10)], true);
		Equipment["Cellphone"] = new Equipment("Cellphone", new StatBonus(StatType.DEF, 10), true);
		Equipment["Cool Glasses"] = new Equipment("Cool Glasses", [new StatBonus(StatType.ATK, 5), new StatBonus(StatType.DEF, 5)], true);
		Equipment["Cough Mask"] = new Equipment("Cough Mask", [new StatBonus(StatType.MaxHP, 25), new StatBonus(StatType.MaxJuice, 25), 
			new StatBonus(StatType.ATK, 10), new StatBonus(StatType.DEF, 10), 
			new StatBonus(StatType.SPD, 10), new StatBonus(StatType.LCK, 10)], true);
		Equipment["Daisy"] = new Equipment("Daisy", [new StatBonus(StatType.MaxHP, 10)], true).WithStartOfBattleEffect((actor) =>
		{
			BattleManager.Instance.MakeEmotionTier(actor, "happy", 1, true);
			return Task.CompletedTask;
		});
		Equipment["Eye Patch"] = new Equipment("Eye Patch", [new StatBonus(StatType.ATK, 7), new StatBonus(StatType.HIT, -25)], true);
		Equipment["Faux Tail"] = new Equipment("Faux Tail", new StatBonus(StatType.SPD, 15), true);
		Equipment["Fedora"] = new Equipment("Fedora", new StatBonus(StatType.DEF, 5), true);
		Equipment["Finger"] = new Equipment("Finger", [new StatBonus(StatType.ATK, 10), new StatBonus(StatType.DEF, -5)], true).WithStartOfBattleEffect((actor) =>
		{
			BattleManager.Instance.MakeEmotionTier(actor, "angry", 1, true);
			return Task.CompletedTask;
		});
		Equipment["Fox Tail"] = new Equipment("Fox Tail", true).WithApplyEffect(() =>
		{
			return [new StatBonus(StatType.SPD, 5 + (3 * BattleManager.Instance.Energy))];
		});
		Equipment["Friendship Bracelet"] = new Equipment("Friendship Bracelet",[new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.MaxJuice, 10)], true);
		Equipment["Nerdy Glasses"] = new Equipment("Nerdy Glasses", [new StatBonus(StatType.DEF, 5), new StatBonus(StatType.HIT, 200)], true);
		Equipment["Gold Watch"] = new Equipment("Gold Watch", new StatBonus(StatType.SPD, -10), true);
		Equipment["Hard Hat"] = new Equipment("Hard Hat", new StatBonus(StatType.DEF, 6), true);
		Equipment["Headband"] = new Equipment("Headband", [new StatBonus(StatType.MaxJuice, 20), new StatBonus(StatType.ATK, 10), 
			new StatBonus(StatType.DEF, 3), new StatBonus(StatType.SPD, 15)], true);
		Equipment["Heart String"] = new Equipment("Heart String", [new StatBonus(StatType.MaxHP, 30)], true).WithStartOfBattleEffect((actor) =>
		{
			BattleManager.Instance.MakeEmotionTier(actor, "happy", 1, true);
			return Task.CompletedTask;
		});
		Equipment["High Heels"] = new Equipment("High Heels", [new StatBonus(StatType.ATK, 10), new StatBonus(StatType.SPD, 10)], true);
		Equipment["Homework"] = new Equipment("Homework", true).WithStartOfBattleEffect((actor) =>
		{
			BattleManager.Instance.MakeEmotionTier(actor, "sad", 1, true);
			return Task.CompletedTask;
		});
		Equipment["Inner Tube"] = new Equipment("Inner Tube", true).WithApplyEffect(() =>
		{
			return [new StatBonus(StatType.DEF, 2 + BattleManager.Instance.Energy)];
		});
		Equipment["Magical Bean"] = new Equipment("Magical Bean", true).WithStartOfBattleEffect((actor) =>
		{
			EmotionGroup rolled = RollRandomEmotionGroup();
			if (rolled != null)
				BattleManager.Instance.MakeEmotionTier(actor, rolled.Id, 1, true);
			return Task.CompletedTask;
		});
		Equipment["Onion Ring"] = new Equipment("Onion Ring", [new StatBonus(StatType.MaxHP, 20), new StatBonus(StatType.MaxJuice, 20)], true);
		Equipment["Paper Bag"] = new Equipment("Paper Bag", [new StatBonus(StatType.MaxHP, 40), new StatBonus(StatType.DEF, 13)], true).WithImmunityToAllEmotions("stressed");
		Equipment["Hector"] = new Equipment("Hector", true);
		Equipment["Pretty Bow"] = new Equipment("Pretty Bow", [new StatBonus(StatType.MaxHP, 50), new StatBonus(StatType.ATK, 10), new StatBonus(StatType.DEF, 3)], true);
		Equipment["Punching Bag"] = new Equipment("Punching Bag", true).WithStartOfBattleEffect((actor) =>
		{
			BattleManager.Instance.MakeEmotionTier(actor, "angry", 1, true);
			return Task.CompletedTask;
		});
		Equipment["Rabbit Foot"] = new Equipment("Rabbit Foot", [new StatBonus(StatType.SPD, 15), new StatBonus(StatType.LCK, 10)], true);
		Equipment["Red Ribbon"] = new Equipment("Red Ribbon", true).WithApplyEffect(() =>
		{
			return [new StatBonus(StatType.ATK, 1 + (2 * BattleManager.Instance.Energy)), new StatBonus(StatType.DEF, 5)];
		});
		Equipment["Deep Poetry Book"] = new Equipment("Deep Poetry Book", true).WithStartOfBattleEffect((actor) =>
		{
			BattleManager.Instance.MakeEmotionTier(actor, "sad", 1, true);
			return Task.CompletedTask;
		});
		Equipment["Rubber Duck"] = new Equipment("Rubber Duck", new StatBonus(StatType.DEF, 7), true);
		Equipment["Seer Goggles"] = new Equipment("Seer Goggles", [new StatBonus(StatType.DEF, 1), new StatBonus(StatType.LCK, 3), new StatBonus(StatType.HIT, 200)], true);
		Equipment["Top Hat"] = new Equipment("Top Hat", [new StatBonus(StatType.MaxHP, 13), new StatBonus(StatType.DEF, 13), new StatBonus(StatType.LCK, 13)], true);
		Equipment["Hector Jr."] = new Equipment("Hector Jr.", true).WithApplyEffect(() =>
		{
			int energy = BattleManager.Instance.Energy;
			return
			[
				new StatBonus(StatType.ATK, 1 + energy), new StatBonus(StatType.DEF, 1 + energy),
				new StatBonus(StatType.SPD, 1 + energy), new StatBonus(StatType.LCK, energy)
			];
		});
		Equipment["Wedding Ring"] = new Equipment("Wedding Ring", [new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.MaxJuice, 10), 
			new StatBonus(StatType.ATK, 3), new StatBonus(StatType.DEF, 3), new StatBonus(StatType.SPD, 3), new StatBonus(StatType.LCK, 3)], true)
			.WithStartOfBattleEffect((actor) =>
		{
			BattleManager.Instance.MakeEmotionTier(actor, "happy", 1, true);
			return Task.CompletedTask;
		});
		Equipment["Wishbone"] = new Equipment("Wishbone", new StatBonus(StatType.LCK, 7), true);
		Equipment["Veggie Kid"] = new Equipment("Veggie Kid", [new StatBonus(StatType.MaxHP, 15), new StatBonus(StatType.MaxJuice, 15)], true);
		Equipment["Watering Pail"] = new Equipment("Watering Pail", new StatBonus(StatType.MaxJuice, 10), true);
		Equipment["Sunscreen"] = new Equipment("Sunscreen", new StatBonus(StatType.MaxHP, 15), true);
		Equipment["Rake"] = new Equipment("Rake", new StatBonus(StatType.ATK, 3), true);
		Equipment["Scarf"] = new Equipment("Scarf", new StatBonus(StatType.DEF, 3), true);
		Equipment["Cotton Ball"] = new Equipment("Cotton Ball", [new StatBonus(StatType.DEF, 1), new StatBonus(StatType.SPD, 3)], true);
		Equipment["Flashlight"] = new Equipment("Flashlight", new StatBonus(StatType.DEF, 4), true);
		Equipment["Universal Remote"] = new Equipment("Universal Remote", [new StatBonus(StatType.MaxHP, 10), new StatBonus(StatType.MaxJuice, 10), 
			new StatBonus(StatType.ATK, 5), new StatBonus(StatType.DEF, 5), new StatBonus(StatType.SPD, 5), new StatBonus(StatType.LCK, 5)], true);
		Equipment["TV Remote"] = new Equipment("TV Remote", [new StatBonus(StatType.MaxHP, 5), new StatBonus(StatType.DEF, 2)], true);
		Equipment["Flower Crown"] = new Equipment("Flower Crown", [new StatBonus(StatType.MaxHP, 100), new StatBonus(StatType.MaxJuice, 25)], true);
		Equipment["Tulip Hairstick"] = new Equipment("Tulip Hairstick", new StatBonus(StatType.MaxHP, 50), true);
		Equipment["Gladiolus Hairband"] = new Equipment("Gladiolus Hairband", [new StatBonus(StatType.ATK, 10), new StatBonus(StatType.LCK, 10), new StatBonus(StatType.HIT, 100)], true);
		Equipment["Cactus Hairclip"] = new Equipment("Cactus Hairclip", [new StatBonus(StatType.DEF, 15), new StatBonus(StatType.MaxHP, 15)], true);
		Equipment["Rose Hairclip"] = new Equipment("Rose Hairclip", [new StatBonus(StatType.MaxHP, 15), new StatBonus(StatType.MaxJuice, 15), 
			new StatBonus(StatType.ATK, 5), new StatBonus(StatType.DEF, 5), new StatBonus(StatType.SPD, 5), new StatBonus(StatType.LCK, 5), new StatBonus(StatType.HIT, 100)], true);
		Equipment["Seashell Necklace"] = new Equipment("Seashell Necklace", [new StatBonus(StatType.MaxHP, 25), new StatBonus(StatType.MaxJuice, 25), new StatBonus(StatType.DEF, 5)], true);
		Equipment["Contract"] = new Equipment("Contract", [
			new StatBonus(StatType.MaxHP, 0.2f), new StatBonus(StatType.MaxJuice, 0.2f),
			new StatBonus(StatType.ATK, 20), new StatBonus(StatType.DEF, 20), new StatBonus(StatType.SPD, 20),
			new StatBonus(StatType.LCK, 20)
		], true);
		Equipment["Chef's Hat"] = new Equipment("Chef's Hat", new StatBonus(StatType.DEF, 15), true).WithStartOfTurnEffect((actor) =>
		{
			int juice = (int)Math.Floor(actor.CurrentStats.MaxJuice * 0.05);
			juice = actor.HealJuice(juice);
			BattleManager.Instance.SpawnDamageNumber(juice, actor, DamageType.JuiceGain);
			return Task.CompletedTask;
		});
		Equipment["Sales Tag"] = new Equipment("Sales Tag", true).WithStartOfBattleEffect(actor =>
		{
			actor.AddStatModifier("SalesTag");
			return Task.CompletedTask;
		});
		Equipment["Abbi's Eye"] = new Equipment("Abbi's Eye", [new StatBonus(StatType.MaxHP, 0.1f),
			new StatBonus(StatType.MaxJuice, 0.01f),
			new StatBonus(StatType.ATK, 40), new StatBonus(StatType.SPD, 40), new StatBonus(StatType.LCK, 40),
			new StatBonus(StatType.HIT, 100)], true).WithStartOfBattleEffect(async actor =>
		{
			// vanilla force-casts OBSERVE on a random enemy for every wearer at battle start
			Enemy target = BattleManager.Instance.GetRandomAliveEnemy();
			await Wait.Milliseconds(1000);
			BattleManager.Instance.RecordLastTarget(actor, target);
			await Skills["Observe"].Effect(actor, [target]);
		});

		#endregion
	}
}
