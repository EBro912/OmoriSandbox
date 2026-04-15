using System.Collections.Generic;
using System.Linq;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal sealed class AubreyBoss : Enemy
{
    public override string Name => "AUBREY";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/aubrey_boss.tres");
    protected override Stats Stats => new(12000, 4000, 120, 55, 75, 15, 95);
    protected override string[] EquippedSkills => ["AAttack", "ABossBeatdown", "ABossLookAtKel", "ABossLookAtHero", "PowerHit", "WindUpThrow", "MoodWrecker", "PepTalk", "ABossTwirl"];

    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "toast" or "happy" or "sad" or "angry";
    }

    private int TurnCount = 0;
    public override BattleCommand ProcessAI()
    {
        TurnCount++;
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["ABossBeatdown"]);

        if (TurnCount == 1)
            return new BattleCommand(this, SelectTarget(), Skills["ABossBeatdown"]);
        
        if (TurnCount == 3)
        {
            Enemy kel = SelectAllEnemies().MaxBy(x => x.CurrentStats.SPD);
            if (kel != null && kel.CurrentState is not "happy")
                return new BattleCommand(this, kel, Skills["PepTalk"]);
        }

        IReadOnlyList<PartyMember> targets = SelectAllTargets();
        PartyMember target = targets.FirstOrDefault(x => x.HasStatModifier("Tickle"));
        if (target != null)
            return new BattleCommand(this, target, Skills["ABossBeatdown"]);

        target = targets.FirstOrDefault(x => x.CurrentState is "happy" or "ecstatic" or "manic");
        if (target != null)
            return new BattleCommand(this, target, Skills["MoodWrecker"]);

        if (Roll() < 21)
            return new BattleCommand(this, SelectTarget(), Skills["AAttack"]);

        IReadOnlyList<Enemy> aliveEnemies = SelectAllEnemies();
        if (aliveEnemies.Count > 2 && Roll() < 26)
        {
            Enemy kel = aliveEnemies.FirstOrDefault(x => x is KelBoss);
            // check if kel is alive, if not just choose a random other enemy
            BattleManager.Instance.ForceCommand(this, kel ?? aliveEnemies.FirstOrDefault(x => x != this), Skills["ABossLookAtKel"]);
            return new BattleCommand(this, SelectTarget(), Skills["AAttack"]);
        }
        
        if (aliveEnemies.Count > 2 && Roll() < 26)
        {
            Enemy hero = aliveEnemies.FirstOrDefault(x => x is HeroBoss);
            // check if hero is alive, if not just choose a random other enemy
            BattleManager.Instance.ForceCommand(this, hero ?? aliveEnemies.FirstOrDefault(x => x != this), Skills["ABossLookAtHero"]);
            return new BattleCommand(this, SelectTarget(), Skills["AAttack"]);
        }

        if (Roll() < 41)
        {
            target = targets.MaxBy(x => x.CurrentStats.DEF);
            return new BattleCommand(this, target, Skills["PowerHit"]);
        }

        if (Roll() < 36)
        {
            return new BattleCommand(this, targets, Skills["WindUpThrow"]);
        }

        target = targets.FirstOrDefault(x => x.CurrentState is "angry" or "enraged" or "furious");
        if (target != null)
            return new BattleCommand(this, target, Skills["ABossTwirl"]);
        
        return new BattleCommand(this, SelectTarget(), Skills["ABossBeatdown"]);
    }
}