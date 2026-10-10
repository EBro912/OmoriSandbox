using System.Collections.Generic;
using System.Linq;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class HeroBoss : Enemy
{
	public override string Name => "HERO";
	public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/hero_boss.tres");
	public override Vector2 InfoBoxOffset => new(0, -200);
	public override Vector2 DamageNumberOffset => new(0, -150);
	public override bool InfoBoxCursorAboveBox => true;
	protected override Stats Stats => new(10000, 7000, 90, 100, 45, 10, 95);
	protected override string[] EquippedSkills => ["HBossDazzle", "HBossCoffee", "SpicyFood", "HBossCook", "HAttack", "HBossSmile", "HBossCallAubreyAttack", "HBossCallAubrey", "HBossCallKelAttack", "HBossCallKel", "Massage"];
	protected internal override bool ObserveHasMulti => false;

	public override bool IsEmotionValid(Emotion emotion)
	{
		return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
	}
	
	public override BattleCommand ProcessAI()
	{
		if (CanUse("HBossDazzle") && HasMultiTargetObserve())
			return new BattleCommand(this, SelectAllTargets(), Skills["HBossDazzle"]);
		if (CanUse("SpicyFood") && HasObserveTarget(out PartyMember observe))
			return new BattleCommand(this, observe, Skills["SpicyFood"]);

		if (CanUse("HBossDazzle") && TrueTurnCount == 1)
			return new BattleCommand(this, SelectAllTargets(), Skills["HBossDazzle"]);
		if (CanUse("HBossCoffee") && TrueTurnCount == 3)
			return new BattleCommand(this, SelectEnemyWithHighest(x => x.CurrentStats.SPD), Skills["HBossCoffee"]);

		if (CanUse("SpicyFood"))
		{
			List<PartyMember> tickled = SelectAllTargets().Where(x => x.HasStatModifier("Tickle")).ToList();
			if (tickled.Count > 0)
				return new BattleCommand(this, tickled[GameManager.Instance.Random.RandiRange(0, tickled.Count - 1)], Skills["SpicyFood"]);
		}

		foreach (string tier in (string[])["manic", "miserable", "furious"])
		{
			if (!CanUse("Massage"))
				break;
			List<Enemy> holders = SelectAllEnemies().Where(x => x.CurrentEmotion.Id == tier).ToList();
			if (holders.Count > 0 && Roll() < 76)
				return new BattleCommand(this, holders[GameManager.Instance.Random.RandiRange(0, holders.Count - 1)], Skills["Massage"]);
		}

		if (CanUse("HBossCook"))
		{
			Enemy hurt = SelectEnemyWithLowest(x => (float)x.CurrentHP / x.CurrentStats.MaxHP,
				x => (double)x.CurrentHP / x.CurrentStats.MaxHP <= 0.3);
			if (hurt != null && Roll() < 46)
				return new BattleCommand(this, hurt, Skills["HBossCook"]);
		}

		if (CanUse("HAttack") && Roll() < 21)
			return new BattleCommand(this, SelectTarget(), Skills["HAttack"]);
		IReadOnlyList<Enemy> aliveEnemies = SelectAllEnemies();
		if (CanUse("HBossCallAubreyAttack") && aliveEnemies.Count > 2 && Roll() < 26)
			return new BattleCommand(this, SelectTarget(), Skills["HBossCallAubreyAttack"]);
		if (CanUse("HBossCallKelAttack") && aliveEnemies.Count > 2 && Roll() < 26)
			return new BattleCommand(this, SelectTarget(), Skills["HBossCallKelAttack"]);

		if (CurrentEmotion.Id == "happy")
		{
			if (CanUse("SpicyFood") && Roll() < 46)
				return new BattleCommand(this, SelectTarget(), Skills["SpicyFood"]);
			if (CanUse("HBossSmile") && Roll() < 36)
				return new BattleCommand(this, SelectTargetWithHighest(StatType.ATK), Skills["HBossSmile"]);
		}
		else if (CurrentEmotion.Id == "angry")
		{
			if (CanUse("HBossSmile") && Roll() < 31)
				return new BattleCommand(this, SelectTargetWithHighest(StatType.ATK), Skills["HBossSmile"]);
			if (CanUse("SpicyFood") && Roll() < 46)
				return new BattleCommand(this, SelectTarget(), Skills["SpicyFood"]);
		}
		else
		{
			if (CanUse("HBossSmile") && Roll() < 31)
				return new BattleCommand(this, SelectTargetWithHighest(StatType.ATK), Skills["HBossSmile"]);
			if (CanUse("SpicyFood") && Roll() < 36)
				return new BattleCommand(this, SelectTarget(), Skills["SpicyFood"]);
		}

		return CanUse("HBossDazzle") ? new BattleCommand(this, SelectAllTargets(), Skills["HBossDazzle"]) : null;
	}
}
