using System;

public class AttackCommand : BattleCommand
{
    public AttackCommand(Actor actor, Actor target) : base(actor, target) { }

    public override void Run()
    {
        // TODO: add emotion, crit, and damage calculations
        float baseDamage = (Actor.CurrentStats.ATK * 2) - Target.CurrentStats.DEF;
        float variance = GameManager.Instance.Random.RandfRange(0.8f, 1.2f);
        int finalDamage = (int)Math.Round(baseDamage * variance, MidpointRounding.AwayFromZero);
        Target.Damage(finalDamage);
        GameManager.Instance.ClearAndMessageBattleLog(Actor.Name.ToUpper() + " attacks " + Target.Name.ToUpper() + "!");
        GameManager.Instance.MessageBattleLog(Target.Name.ToUpper() + " takes " + finalDamage + " damage!");
        Target.SetHurt(true);
    }
}