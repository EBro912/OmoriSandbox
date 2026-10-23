using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class EmotionsRegistry
{
	internal static void Register()
	{
		#region EMOTIONS
		AddEmotionGroup(new EmotionGroup("happy").WithBeatsGroup("angry").WithMaxTierMessage("[target] can't get HAPPIER!").WithRandomEmotion());
		AddEmotionGroup(new EmotionGroup("angry").WithBeatsGroup("sad").WithMaxTierMessage("[target] can't get ANGRIER!").WithRandomEmotion());
		AddEmotionGroup(new EmotionGroup("sad").WithBeatsGroup("happy").WithMaxTierMessage("[target] can't get SADDER!").WithRandomEmotion());

		AddEmotion(new Emotion("neutral")
			.WithAsset(EmotionAsset.Vanilla(0, 0, 0)));
		AddEmotion(new Emotion("happy").WithGroup("happy", 1)
			.WithStatBonuses(new StatBonus(StatType.LCK, 2f), new StatBonus(StatType.SPD, 1.25f), new StatBonus(StatType.HIT, -10))
			.WithAsset(EmotionAsset.Vanilla(3, 2, 0)));
		AddEmotion(new Emotion("sad").WithGroup("sad", 1)
			.WithStatBonuses(new StatBonus(StatType.DEF, 1.25f), new StatBonus(StatType.SPD, 0.8f))
			.WithJuiceBleed(0.3f)
			.WithAsset(EmotionAsset.Vanilla(6, 1, 1)));
		AddEmotion(new Emotion("angry").WithGroup("angry", 1)
			.WithStatBonuses(new StatBonus(StatType.ATK, 1.3f), new StatBonus(StatType.DEF, 0.5f))
			.WithAsset(EmotionAsset.Vanilla(9, 0, 2)));
		AddEmotion(new Emotion("ecstatic").WithGroup("happy", 2)
			.WithStatBonuses(new StatBonus(StatType.LCK, 3f), new StatBonus(StatType.SPD, 1.5f), new StatBonus(StatType.HIT, -20))
			.WithAsset(EmotionAsset.Vanilla(4, 3, 0)));
		AddEmotion(new Emotion("depressed").WithGroup("sad", 2)
			.WithStatBonuses(new StatBonus(StatType.DEF, 1.35f), new StatBonus(StatType.SPD, 0.65f))
			.WithJuiceBleed(0.5f)
			.WithAsset(EmotionAsset.Vanilla(7, 2, 1)));
		AddEmotion(new Emotion("enraged").WithGroup("angry", 2)
			.WithStatBonuses(new StatBonus(StatType.ATK, 1.5f), new StatBonus(StatType.DEF, 0.3f))
			.WithAsset(EmotionAsset.Vanilla(10, 1, 2)));
		AddEmotion(new Emotion("manic").WithGroup("happy", 3)
			.WithStatBonuses(new StatBonus(StatType.LCK, 4f), new StatBonus(StatType.SPD, 2f), new StatBonus(StatType.HIT, -30))
			.WithAsset(EmotionAsset.Vanilla(5, 0, 1)));
		AddEmotion(new Emotion("miserable").WithGroup("sad", 3)
			.WithStatBonuses(new StatBonus(StatType.DEF, 1.5f), new StatBonus(StatType.SPD, 0.5f))
			.WithJuiceBleed(1f)
			.WithAsset(EmotionAsset.Vanilla(8, 3, 1)));
		AddEmotion(new Emotion("furious").WithGroup("angry", 3)
			.WithStatBonuses(new StatBonus(StatType.ATK, 2f), new StatBonus(StatType.DEF, 0.15f))
			.WithAsset(EmotionAsset.Vanilla(11, 2, 2)));
		AddEmotion(new Emotion("afraid")
			.WithBlocksActions()
			.WithDefensiveRate("emotion", 1.5f)
			.WithDefensiveRate("exploit", 1.5f)
			.WithDefensiveRate("afraid", 1.5f)
			.WithAsset(EmotionAsset.Vanilla(12, 3, 2)));
		AddEmotion(new Emotion("stressed")
			.WithBlocksActions()
			.WithStatBonuses(new StatBonus(StatType.ATK, 1.2f), new StatBonus(StatType.DEF, 0.9f))
			.WithAsset(EmotionAsset.Vanilla(2, 1, 0)));
		#endregion
	}
}
