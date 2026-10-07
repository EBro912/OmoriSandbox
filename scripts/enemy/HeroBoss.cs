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
	protected override string[] EquippedSkills => ["HBossDazzle", "HBossCoffee", "SpicyFood", "HBossCook", "HAttack", "HBossSmile", "HBossCallAubreyAttack", "HBossCallAubrey", "HBossCallKelAttack", "HBossCallKel"];
	protected internal override bool ObserveHasMulti => true;

	public override bool IsEmotionValid(Emotion emotion)
	{
		return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
	}
	
	public override BattleCommand ProcessAI()
	{
		if (HasMultiTargetObserve())
			return new BattleCommand(this, SelectAllTargets(), Skills["HBossDazzle"]);
		
		if (HasObserveTarget(out PartyMember observe))
			return new BattleCommand(this, observe, Skills["SpicyFood"]);
		
		if (TrueTurnCount == 1)
			return new BattleCommand(this, SelectAllTargets(), Skills["HBossDazzle"]);
		if (TrueTurnCount == 3)
		{
			Enemy kel = SelectAllEnemies().MaxBy(x => x.CurrentStats.SPD);
			return new BattleCommand(this, kel, Skills["HBossCoffee"]);
		}

		if (Roll() < 46)
		{
			Enemy target = SelectAllEnemies().FirstOrDefault(x => x.CurrentHP < x.CurrentStats.MaxHP * 0.3);
			if (target != null)
				return new BattleCommand(this, target, Skills["HBossCook"]);
		}

		if (Roll() < 21)
			return new BattleCommand(this, SelectTarget(), Skills["HAttack"]);
		
		IReadOnlyList<Enemy> aliveEnemies = SelectAllEnemies();
		if (aliveEnemies.Count > 2 && Roll() < 26)
			return new BattleCommand(this, SelectTarget(), Skills["HBossCallAubreyAttack"]);
		
		if (aliveEnemies.Count > 2 && Roll() < 26)
			return new BattleCommand(this, SelectTarget(), Skills["HBossCallKelAttack"]);

		if (CurrentEmotion.Id is "neutral" or "sad")
		{
			if (Roll() < 31)
				return new BattleCommand(this, SelectTarget(), Skills["HBossSmile"]);
			if (Roll() < 36)
				return new BattleCommand(this, SelectTarget(), Skills["SpicyFood"]);
		}
		else if (CurrentEmotion.Id is "angry" or "happy")
		{
			if (Roll() < 36)
				return new BattleCommand(this, SelectTarget(), Skills["HBossSmile"]);
			if (Roll() < 46)
				return new BattleCommand(this, SelectTarget(), Skills["SpicyFood"]);
		}
		
		return new BattleCommand(this, SelectAllTargets(), Skills["HBossDazzle"]);
	}
}
