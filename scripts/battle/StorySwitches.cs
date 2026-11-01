namespace OmoriSandbox.Battle;

// various story related switches that boss logic uses, mainly the fears
internal static class StorySwitches
{
	internal static bool HeightsDone { get; set; } = true;
	internal static bool SpidersDone { get; set; } = true;
	internal static bool DrowningDone { get; set; } = true;
	internal static bool BasilBattle { get; set; }
	internal static bool BasilTroop { get; set; }
	internal static bool BlackSpaceWorld { get; set; }
	internal static int FearPhase { get; set; }

	internal static void Reset()
	{
		HeightsDone = true;
		SpidersDone = true;
		DrowningDone = true;
		BasilBattle = false;
		BasilTroop = false;
		BlackSpaceWorld = false;
		FearPhase = 0;
	}
}
