using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Editor;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class ModifiersRegistry
{
	internal static void Register()
	{
		#region MODIFIERS
		Modifiers.Add("AttackUp", () => CreateBuffDebuff(new StatBonus(StatType.ATK, 1.1f), new StatBonus(StatType.ATK, 1.25f), new StatBonus(StatType.ATK, 1.5f))
			.WithMessages("ATTACK rose!", "ATTACK can't go\nany higher!")
			.WithCounterpart("AttackDown")
			.WithStateIcons(new StateIcon("bnw_+1att", "Attack Up 1: x1.1 ATK"), new StateIcon("bnw_+2att", "Attack Up 2: x1.25 ATK"), new StateIcon("bnw_+3att", "Attack Up 3: x1.5 ATK"))
			.WithCategories(StatModifier.BuffCategory));
		Modifiers.Add("AttackDown", () => CreateBuffDebuff(new StatBonus(StatType.ATK, 0.9f), new StatBonus(StatType.ATK, 0.8f), new StatBonus(StatType.ATK, 0.7f))
			.WithMessages("ATTACK fell.", "ATTACK can't go\nany lower!")
			.WithCounterpart("AttackUp")
			.WithStateIcons(new StateIcon("bnw_-1att", "Attack Down 1: x0.9 ATK"), new StateIcon("bnw_-2att", "Attack Down 2: x0.8 ATK"), new StateIcon("bnw_-3att", "Attack Down 3: x0.7 ATK"))
			.WithCategories(StatModifier.DebuffCategory));
		Modifiers.Add("DefenseUp", () =>
		{
			float[] values = SettingsMenuManager.Instance.UseConsoleDefense ? [1.1f, 1.2f, 1.3f] : [1.15f, 1.3f, 1.5f];
			return CreateBuffDebuff(new StatBonus(StatType.DEF, values[0]), new StatBonus(StatType.DEF, values[1]),
					new StatBonus(StatType.DEF, values[2]))
				.WithMessages("DEFENSE rose!", "DEFENSE can't go\nany higher!")
				.WithCounterpart("DefenseDown")
				.WithStateIcons(new StateIcon("bnw_+1def", $"Defense Up 1: x{values[0]} DEF"), new StateIcon("bnw_+2def", $"Defense Up 2: x{values[1]} DEF"), new StateIcon("bnw_+3def", $"Defense Up 3: x{values[2]} DEF"))
				.WithCategories(StatModifier.BuffCategory);

		});
		Modifiers.Add("DefenseDown", () => CreateBuffDebuff(new StatBonus(StatType.DEF, 0.75f), new StatBonus(StatType.DEF, 0.5f), new StatBonus(StatType.DEF, 0.25f))
			.WithMessages("DEFENSE fell.", "DEFENSE can't go\nany lower!")
			.WithCounterpart("DefenseUp")
			.WithStateIcons(new StateIcon("bnw_-1def", "Defense Down 1: x0.75 DEF"), new StateIcon("bnw_-2def", "Defense Down 2: x0.5 DEF"), new StateIcon("bnw_-3def", "Defense Down 3: x0.25 DEF"))
			.WithCategories(StatModifier.DebuffCategory));
		Modifiers.Add("SpeedUp", () =>
		{
			float speedUp3 = SettingsMenuManager.Instance.UseConsoleSpeed ? 3f : 5f;
			return CreateBuffDebuff(new StatBonus(StatType.SPD, 1.5f), new StatBonus(StatType.SPD, 2f),
					new StatBonus(StatType.SPD, speedUp3))
				.WithMessages("SPEED rose!", "SPEED can't go\nany higher!")
				.WithCounterpart("SpeedDown")
				.WithStateIcons(new StateIcon("bnw_+1spd", "Speed Up 1: x1.5 SPD"), new StateIcon("bnw_+2spd", "Speed Up 2: x2 SPD"), new StateIcon("bnw_+3spd", $"Speed Up 3: x{speedUp3} SPD"))
				.WithCategories(StatModifier.BuffCategory);

		});
		Modifiers.Add("SpeedDown", () => CreateBuffDebuff(new StatBonus(StatType.SPD, 0.8f), new StatBonus(StatType.SPD, 0.5f), new StatBonus(StatType.SPD, 0.25f))
			.WithMessages("SPEED fell.", "SPEED can't go\nany lower!")
			.WithCounterpart("SpeedUp")
			.WithStateIcons(new StateIcon("bnw_-1spd", "Speed Down 1: x0.8 SPD"), new StateIcon("bnw_-2spd", "Speed Down 2: x0.5 SPD"), new StateIcon("bnw_-3spd", "Speed Down 3: x0.25 SPD"))
			.WithCategories(StatModifier.DebuffCategory));

		Modifiers.Add("ReleaseEnergy", () => new StatModifier(new StatBonus(StatType.SPD, 1.25f), new StatBonus(StatType.ATK, 1.25f), new StatBonus(StatType.DEF, 1.25f), new StatBonus(StatType.LCK, 1.25f))
			.WithStateIcons(new StateIcon("bnw_energy", "Release Energy: ATK/DEF/LCK/SPD x1.25")));
		Modifiers.Add("ReleaseEnergyBasil", () => new ReleaseEnergyBasilStatModifier(new StatBonus(StatType.SPD, 1.25f), new StatBonus(StatType.ATK, 1.25f), new StatBonus(StatType.DEF, 1.25f), new StatBonus(StatType.LCK, 1.25f))
			.WithStateIcons(new StateIcon("bnw_energy_basil", "Release Energy (Basil): ATK/DEF/LCK/SPD x1.25, 10% HEART + 5% JUICE regen/turn")));
		Modifiers.Add("ReleaseEnergyBasilBonus",
			() => new StatModifier(4, new StatBonus(StatType.SPD, 1.2f), new StatBonus(StatType.ATK, 1.2f),
				new StatBonus(StatType.DEF, 1.2f), new StatBonus(StatType.LCK, 1.2f)));
		Modifiers.Add("SnoCone", () => new StatModifier(new StatBonus(StatType.SPD, 1.2f), new StatBonus(StatType.ATK, 1.2f), new StatBonus(StatType.DEF, 1.2f), new StatBonus(StatType.LCK, 1.2f))
			.WithStateIcons(new StateIcon("bnw_snocone", "Sno-Cone: ATK/DEF/SPD/LCK x1.2")));
		Modifiers.Add("Flex", () => new FlexStatModifier(new StatBonus(StatType.HIT, 1000))
			.WithStateIcons(new StateIcon("bnw_flex", "Flex: Next Physical hit x2.5 damage, HIT x1000")));
		Modifiers.Add("AccuracyUp", () => new StatModifier(1, new StatBonus(StatType.HIT, 1000)));
		// see if these even need to be their own classes
		Modifiers.Add("Guard", () => new GuardStatModifier(1)
			.WithStateIcons(new StateIcon("bnw_guard", "Guard: x0.5 incoming damage")));
		Modifiers.Add("SecondChance", () => new SecondChanceStatModifier(1));
		Modifiers.Add("PlotArmor", () => new PlotArmorStatModifier());
		Modifiers.Add("Immortal", () => new ImmortalStatModifier());
		Modifiers.Add("CallForFriendDelay", () => new CallForFriendDelayModifier());
		Modifiers.Add("Stunned", () => new StunnedModifier().WithReapplyAllowed());
		Modifiers.Add("Tickle", () => new StatModifier(1));
		Modifiers.Add("RandomEmotion", () => new StatModifier(1));
		Modifiers.Add("MinionBarrier", () => new MinionBarrierModifier());
		Modifiers.Add("Taunt", () => new StatModifier(1));
		Modifiers.Add("AubreyCounter", () => new AubreyCounterModifier(1));
		Modifiers.Add("HitRateDown", () => new StatModifier(2, new StatBonus(StatType.HIT, -55)));
		Modifiers.Add("PhotographHitRateDown", () => new StatModifier(1, new StatBonus(StatType.HIT, -25)));
		Modifiers.Add("Charm", () => new CharmStatModifier(1).WithActionEndTicking());
		Modifiers.Add("ObserveIgnoreTaunt", () => new ObserveStatModifier(2).WithActionEndTicking());
		Modifiers.Add("ObserveSingle", () => new StatModifier(2));
		Modifiers.Add("ObserveMulti", () => new StatModifier(2));
		Modifiers.Add("CharmCheck", () => new StatModifier(1));
		Modifiers.Add("SpaceExHusbandBlock", () => new SpaceExHusbandStatModifier());
		Modifiers.Add("Stockpile", () => new TierStatModifier().WithMaxTier(3));
		Modifiers.Add("PlutoCharging", () => new StatModifier(2, new StatBonus(StatType.DEF, 3f)));
		Modifiers.Add("PlutoBuff", () => new StatModifier(new StatBonus(StatType.ATK, 1.5f), new StatBonus(StatType.DEF, 1.5f), new StatBonus(StatType.LCK, 1.5f), new StatBonus(StatType.SPD, 10f)));
		Modifiers.Add("Encore", () => new EncoreStatModifier(3).WithActionEndTicking());
		Modifiers.Add("SalesTag", () => new SalesTagStatModifier());
		Modifiers.Add("Immune", () => new ImmuneStatModifier());
		Modifiers.Add("ResistPhysical", () => new ResistPhysicalModifier());
		Modifiers.Add("CherishDialogue", () => new TierStatModifier().WithMaxTier(5).WithBypassRecoverAllRemoval().WithBypassDeathRemoval());
		Modifiers.Add("Cherished", () => new StatModifier(new StatBonus(StatType.ATK, 1.3f), new StatBonus(StatType.DEF, 1.15f), new StatBonus(StatType.SPD, 2f)).WithResists("AttackUp", "DefenseUp", "SpeedUp"));
		Modifiers.Add("CherishSeal", () => new StatModifier(-1));
		#endregion
	}

	// Helper method for creating buffs and debuffs affected by the InfiniteBuffsDebuffs setting
	private static TierStatModifier CreateBuffDebuff(params StatBonus[] bonuses)
	{
		return SettingsMenuManager.Instance.InfiniteBuffsDebuffs
			? new TierStatModifier(bonuses)
			: new TierStatModifier(6, bonuses);
	}
}
