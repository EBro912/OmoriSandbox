using System.Collections.Generic;
using System.Linq;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal sealed class HeroBoss : Enemy
{
	public override string Name => "HERO";
	public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/hero_boss.tres");
	protected override Stats Stats => new(10000, 7000, 90, 100, 45, 10, 95);
	protected override string[] EquippedSkills => ["HBossDazzle", "HBossCoffee", "SpicyFood", "HBossCook", "HAttack", "HBossSmile", "HBossCallAubrey", "HBossCallKel"];

	public override bool IsStateValid(string state)
	{
		return state is "neutral" or "toast" or "happy" or "sad" or "angry";
	}
	
	private int TurnCount = 0;
	public override BattleCommand ProcessAI()
	{
		TurnCount++;
		
		if (HasMultiTargetObserve())
			return new BattleCommand(this, SelectAllTargets(), Skills["HBossDazzle"]);
		
		if (HasObserveTarget(out PartyMember observe))
			return new BattleCommand(this, observe, Skills["SpicyFood"]);
		
		if (TurnCount == 1)
			return new BattleCommand(this, SelectAllTargets(), Skills["HBossDazzle"]);
		if (TurnCount == 3)
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
		{
			Enemy aubrey = aliveEnemies.FirstOrDefault(x => x is AubreyBoss);
			// check if aubrey is alive, if not just choose a random other enemy
			BattleManager.Instance.ForceCommand(this, aubrey ?? aliveEnemies.FirstOrDefault(x => x != this), Skills["HBossCallAubrey"]);
			return new BattleCommand(this, SelectTarget(), Skills["HAttack"]);
		}
		
		if (aliveEnemies.Count > 2 && Roll() < 26)
		{
			Enemy kel = aliveEnemies.FirstOrDefault(x => x is KelBoss);
			// check if kel is alive, if not just choose a random other enemy
			BattleManager.Instance.ForceCommand(this, kel ?? aliveEnemies.FirstOrDefault(x => x != this), Skills["HBossCallKel"]);
			return new BattleCommand(this, SelectTarget(), Skills["HAttack"]);
		}

		if (CurrentState is "neutral" or "sad")
		{
			if (Roll() < 31)
				return new BattleCommand(this, SelectTarget(), Skills["HBossSmile"]);
			if (Roll() < 36)
				return new BattleCommand(this, SelectTarget(), Skills["SpicyFood"]);
		}
		else if (CurrentState is "angry" or "happy")
		{
			if (Roll() < 36)
				return new BattleCommand(this, SelectTarget(), Skills["HBossSmile"]);
			if (Roll() < 46)
				return new BattleCommand(this, SelectTarget(), Skills["SpicyFood"]);
		}
		
		return new BattleCommand(this, SelectAllTargets(), Skills["HBossDazzle"]);
	}
}
