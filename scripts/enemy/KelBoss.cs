using System.Collections.Generic;
using System.Linq;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal sealed class KelBoss : Enemy
{
    public override string Name => "KEL";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/kel_boss.tres");
    protected override Stats Stats => new(9000, 9000, 100, 70, 230, 20, 100);
    protected override string[] EquippedSkills => ["KBossPassToAubrey", "KBossPassToHero", "KBossFlex", "KBossRainCloud", "KAttack", "RunNGun", "Annoy", "Tickle", "Rebound", "Curveball", "Ricochet"];

    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "toast" or "happy" or "sad" or "angry";
    }
    
    private int TurnCount = 0;
    public override BattleCommand ProcessAI()
    {
        TurnCount++;
        
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["RunNGun"]);
        
        if (TurnCount == 1)
            return new BattleCommand(this, SelectAllEnemies(), Skills["KBossRainCloud"]);
        if (TurnCount == 3)
            return new BattleCommand(this, this, Skills["KBossFlex"]);
        if (TurnCount == 4)
            return new BattleCommand(this, SelectTarget(), Skills["RunNGun"]);

        return CurrentState switch
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
        if (Roll() < 51)
        {
            PartyMember target = SelectAllTargets()
                .FirstOrDefault(x => x.CurrentState is not "angry" and not "enraged" and not "furious");
            if (target != null)
                return new BattleCommand(this, target, Skills["Annoy"]);
        }
        
        if (Roll() < 21)
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        
        IReadOnlyList<Enemy> aliveEnemies = SelectAllEnemies();
        if (aliveEnemies.Count > 2 && Roll() < 26)
        {
            Enemy aubrey = aliveEnemies.FirstOrDefault(x => x is AubreyBoss);
            // check if aubrey is alive, if not just choose a random other enemy
            BattleManager.Instance.ForceCommand(this, aubrey ?? aliveEnemies.FirstOrDefault(x => x != this), Skills["KBossPassToAubrey"]);
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        }
        
        if (aliveEnemies.Count > 2 && Roll() < 26)
        {
            Enemy hero = aliveEnemies.FirstOrDefault(x => x is HeroBoss);
            // check if hero is alive, if not just choose a random other enemy
            BattleManager.Instance.ForceCommand(this, hero ?? aliveEnemies.FirstOrDefault(x => x != this), Skills["KBossPassToHero"]);
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        }
        
        if (Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["Tickle"]);
        
        if (Roll() < 31)
            return new BattleCommand(this, SelectAllTargets(), Skills["Rebound"]);
        
        if (Roll() < 31)
            return new BattleCommand(this, SelectTarget(), Skills["Curveball"]);
        
        return new BattleCommand(this, SelectTarget(), Skills["Ricochet"]);
    }

    private BattleCommand ProcessHappy()
    {
        if (Roll() < 51)
        {
            PartyMember target = SelectAllTargets()
                .FirstOrDefault(x => x.CurrentState is not "angry" and not "enraged" and not "furious");
            if (target != null)
                return new BattleCommand(this, target, Skills["Annoy"]);
        }
        
        if (Roll() < 21)
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        
        IReadOnlyList<Enemy> aliveEnemies = SelectAllEnemies();
        if (aliveEnemies.Count > 2 && Roll() < 26)
        {
            Enemy aubrey = aliveEnemies.FirstOrDefault(x => x is AubreyBoss);
            // check if aubrey is alive, if not just choose a random other enemy
            BattleManager.Instance.ForceCommand(this, aubrey ?? aliveEnemies.FirstOrDefault(x => x != this), Skills["KBossPassToAubrey"]);
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        }
        
        if (aliveEnemies.Count > 2 && Roll() < 26)
        {
            Enemy hero = aliveEnemies.FirstOrDefault(x => x is HeroBoss);
            // check if hero is alive, if not just choose a random other enemy
            BattleManager.Instance.ForceCommand(this, hero ?? aliveEnemies.FirstOrDefault(x => x != this), Skills["KBossPassToHero"]);
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        }
        
        if (Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["Tickle"]);
        
        if (Roll() < 41)
            return new BattleCommand(this, SelectAllTargets(), Skills["Rebound"]);
        
        if (Roll() < 51)
            return new BattleCommand(this, SelectTarget(), Skills["Curveball"]);
        
        return new BattleCommand(this, SelectTarget(), Skills["Ricochet"]);
    }
    
    private BattleCommand ProcessSad()
    {
        if (Roll() < 21)
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        
        IReadOnlyList<Enemy> aliveEnemies = SelectAllEnemies();
        if (aliveEnemies.Count > 2 && Roll() < 26)
        {
            Enemy aubrey = aliveEnemies.FirstOrDefault(x => x is AubreyBoss);
            // check if aubrey is alive, if not just choose a random other enemy
            BattleManager.Instance.ForceCommand(this, aubrey ?? aliveEnemies.FirstOrDefault(x => x != this), Skills["KBossPassToAubrey"]);
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        }
        
        if (aliveEnemies.Count > 2 && Roll() < 26)
        {
            Enemy hero = aliveEnemies.FirstOrDefault(x => x is HeroBoss);
            // check if hero is alive, if not just choose a random other enemy
            BattleManager.Instance.ForceCommand(this, hero ?? aliveEnemies.FirstOrDefault(x => x != this), Skills["KBossPassToHero"]);
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        }
        
        if (Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["Tickle"]);
        
        if (Roll() < 31)
            return new BattleCommand(this, SelectAllTargets(), Skills["Rebound"]);
        
        if (Roll() < 31)
            return new BattleCommand(this, SelectTarget(), Skills["Curveball"]);
        
        return new BattleCommand(this, SelectTarget(), Skills["Ricochet"]);
    }
    
    private BattleCommand ProcessAngry()
    {
        if (Roll() < 36)
        {
            PartyMember target = SelectAllTargets()
                .FirstOrDefault(x => x.CurrentState is not "angry" and not "enraged" and not "furious");
            if (target != null)
                return new BattleCommand(this, target, Skills["Annoy"]);
        }
        
        if (Roll() < 21)
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        
        IReadOnlyList<Enemy> aliveEnemies = SelectAllEnemies();
        if (aliveEnemies.Count > 2 && Roll() < 26)
        {
            Enemy aubrey = aliveEnemies.FirstOrDefault(x => x is AubreyBoss);
            // check if aubrey is alive, if not just choose a random other enemy
            BattleManager.Instance.ForceCommand(this, aubrey ?? aliveEnemies.FirstOrDefault(x => x != this), Skills["KBossPassToAubrey"]);
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        }
        
        if (aliveEnemies.Count > 2 && Roll() < 26)
        {
            Enemy hero = aliveEnemies.FirstOrDefault(x => x is HeroBoss);
            // check if hero is alive, if not just choose a random other enemy
            BattleManager.Instance.ForceCommand(this, hero ?? aliveEnemies.FirstOrDefault(x => x != this), Skills["KBossPassToHero"]);
            return new BattleCommand(this, SelectTarget(), Skills["KAttack"]);
        }
        
        if (Roll() < 16)
            return new BattleCommand(this, SelectTarget(), Skills["Tickle"]);
        
        if (Roll() < 36)
            return new BattleCommand(this, SelectAllTargets(), Skills["Rebound"]);
        
        if (Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["Curveball"]);
        
        return new BattleCommand(this, SelectTarget(), Skills["Ricochet"]);
    }
}