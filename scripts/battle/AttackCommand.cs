using Godot;
using System;

public class AttackCommand : BattleCommand
{
    public AttackCommand(Actor actor, Actor target, string message) : base(actor, target, message) { }

    public override CommandResult Run()
    {
        if (Target.CurrentHP == 0)
        {
            if (Target is Enemy)
                Target = GameManager.Instance.BattleManager.GetRandomAliveEnemy();
            else
                Target = GameManager.Instance.BattleManager.GetRandomAlivePartyMember();
            if (Target == null)
            {
                GD.PrintErr("Running AttackCommand when all enemies are dead!");
                return new CommandResult(false);
            }
        }

        GameManager.Instance.ClearAndMessageBattleLog(ParseMessage(Message));
        bool miss = Actor.CurrentStats.HIT < GameManager.Instance.Random.RandiRange(0, 100);
        if (miss)
        {
            GameManager.Instance.MessageBattleLog(ParseMessage("[actor]'s attack missed..."));
            return new CommandResult(false);
        }
        float baseDamage = (Actor.CurrentStats.ATK * 2) - Target.CurrentStats.DEF;
        float variance = GameManager.Instance.Random.RandfRange(0.8f, 1.2f);
        bool critical = Actor.CurrentStats.LCK * .01f >= GameManager.Instance.Random.Randf();
        float finalDamage = baseDamage * variance;
        if (critical)
        {
            finalDamage = (finalDamage * 1.5f) + 2;
            GameManager.Instance.MessageBattleLog("IT HIT RIGHT IN THE HEART!");
        }
        int rounded = (int)Math.Round(finalDamage, MidpointRounding.AwayFromZero);
        Target.Damage(rounded);
        GameManager.Instance.MessageBattleLog(ParseMessage("[target] takes " + rounded + " damage!"));
        return new CommandResult(true, critical, rounded);
    }
}