using System;

public class AttackCommand : BattleCommand
{
    public Actor Target;

    public AttackCommand(Actor actor, Actor target) : base(actor)
    {
        Target = target;
    }


    public override void Run()
    {
        // TODO: add emotion, crit, and damage calculations
        float baseDamage = (Actor.ATK * 2) - Target.DEF;
        float variance = GameManager.Instance.Random.RandfRange(0.8f, 1.2f);
        int finalDamage = (int)Math.Round(baseDamage * variance, MidpointRounding.AwayFromZero);
        Target.Damage(finalDamage);
        GameManager.Instance.ClearAndMessageBattleLog(Actor.Name.ToUpper() + " attacks " + Target.Name.ToUpper() + "!");
        GameManager.Instance.MessageBattleLog(Target.Name.ToUpper() + " takes " + finalDamage + " damage!");
    }
}