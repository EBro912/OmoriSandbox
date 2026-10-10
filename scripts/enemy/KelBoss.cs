using System.Collections.Generic;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class KelBoss : Enemy
{
    public override string Name => "KEL";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/kel_boss.tres");
    public override Vector2 InfoBoxOffset => new(-15, -360);
    public override Vector2 DamageNumberOffset => new(0, -150);
    protected override Stats Stats => new(9000, 9000, 100, 70, 230, 20, 100);
    protected override string[] EquippedSkills => ["KBossPassToAubreyAttack", "KBossPassToAubrey", "KBossPassToHeroAttack", "KBossPassToHero", "KBossFlex", "KBossRainCloud", "KAttack", "RunNGun", "Annoy", "Tickle", "Rebound", "KBossCurveball", "Ricochet"];
    protected internal override bool ObserveHasMulti => false;
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }
    
    public override BattleCommand ProcessAI()
    {
        if (CanUse("Rebound") && HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["Rebound"]);
        if (CanUse("RunNGun") && HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["RunNGun"]);

        if (CanUse("KBossRainCloud") && TrueTurnCount == 1)
            return new BattleCommand(this, SelectAllEnemies(), Skills["KBossRainCloud"]);
        if (CanUse("KBossFlex") && TrueTurnCount == 3)
            return new BattleCommand(this, this, Skills["KBossFlex"]);
        if (CanUse("RunNGun") && TrueTurnCount == 4)
            return new BattleCommand(this, SelectTarget(), Skills["RunNGun"]);

        return CurrentEmotion.Id switch
        {
            "happy" => ProcessHappy(),
            "sad" => ProcessSad(),
            "angry" => ProcessAngry(),
            _ => ProcessNeutral()
        };
    }

    // ts is so ahh but at least it helps keep things organized...
    private BattleCommand ProcessNeutral()
    {
        if (CanUse("Annoy") && Roll() < 51)
            return new BattleCommand(this, SelectTarget(), Skills["Annoy"]);
        if (CanUse("KAttack") && Roll() < 21)
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        IReadOnlyList<Enemy> aliveEnemies = SelectAllEnemies();
        if (CanUse("KBossPassToAubreyAttack") && aliveEnemies.Count > 2 && Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["KBossPassToAubreyAttack"]);
        if (CanUse("KBossPassToHeroAttack") && aliveEnemies.Count > 2 && Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["KBossPassToHeroAttack"]);
        if (CanUse("Tickle") && Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["Tickle"]);
        if (CanUse("Rebound") && Roll() < 31)
            return new BattleCommand(this, SelectAllTargets(), Skills["Rebound"]);
        if (CanUse("KBossCurveball") && Roll() < 31)
            return new BattleCommand(this, SelectTarget(), Skills["KBossCurveball"]);
        return CanUse("Ricochet") ? new BattleCommand(this, SelectTarget(), Skills["Ricochet"]) : null;
    }

    private BattleCommand ProcessHappy()
    {
        // VANILLA BUG: HAPPY KEL's ANNOY line checks "State !== 14" without the "State" prefix, which YEP can't resolve, so he never uses ANNOY
        if (CanUse("KAttack") && Roll() < 21)
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        IReadOnlyList<Enemy> aliveEnemies = SelectAllEnemies();
        if (CanUse("KBossPassToAubreyAttack") && aliveEnemies.Count > 2 && Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["KBossPassToAubreyAttack"]);
        if (CanUse("KBossPassToHeroAttack") && aliveEnemies.Count > 2 && Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["KBossPassToHeroAttack"]);
        if (CanUse("Rebound") && Roll() < 41)
            return new BattleCommand(this, SelectAllTargets(), Skills["Rebound"]);
        if (CanUse("Tickle") && Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["Tickle"]);
        if (CanUse("KBossCurveball") && Roll() < 51)
            return new BattleCommand(this, SelectTarget(), Skills["KBossCurveball"]);
        return CanUse("Ricochet") ? new BattleCommand(this, SelectTarget(), Skills["Ricochet"]) : null;
    }

    private BattleCommand ProcessSad()
    {
        if (CanUse("KAttack") && Roll() < 21)
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        IReadOnlyList<Enemy> aliveEnemies = SelectAllEnemies();
        if (CanUse("KBossPassToAubreyAttack") && aliveEnemies.Count > 2 && Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["KBossPassToAubreyAttack"]);
        if (CanUse("KBossPassToHeroAttack") && aliveEnemies.Count > 2 && Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["KBossPassToHeroAttack"]);
        if (CanUse("Tickle") && Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["Tickle"]);
        if (CanUse("Rebound") && Roll() < 31)
            return new BattleCommand(this, SelectAllTargets(), Skills["Rebound"]);
        if (CanUse("KBossCurveball") && Roll() < 31)
            return new BattleCommand(this, SelectTarget(), Skills["KBossCurveball"]);
        return CanUse("Ricochet") ? new BattleCommand(this, SelectTarget(), Skills["Ricochet"]) : null;
    }

    private BattleCommand ProcessAngry()
    {
        if (CanUse("Annoy") && Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["Annoy"]);
        if (CanUse("KAttack") && Roll() < 21)
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        IReadOnlyList<Enemy> aliveEnemies = SelectAllEnemies();
        if (CanUse("KBossPassToAubreyAttack") && aliveEnemies.Count > 2 && Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["KBossPassToAubreyAttack"]);
        if (CanUse("KBossPassToHeroAttack") && aliveEnemies.Count > 2 && Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["KBossPassToHeroAttack"]);
        if (CanUse("Tickle") && Roll() < 16)
            return new BattleCommand(this, SelectTarget(), Skills["Tickle"]);
        if (CanUse("Rebound") && Roll() < 36)
            return new BattleCommand(this, SelectAllTargets(), Skills["Rebound"]);
        if (CanUse("KBossCurveball") && Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["KBossCurveball"]);
        return CanUse("Ricochet") ? new BattleCommand(this, SelectTarget(), Skills["Ricochet"]) : null;
    }
}