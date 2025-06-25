using System.Linq;

public class AubreyEnemy : Enemy
{
    public override string Name => "AUBREY";
    public override string AnimationPath => "res://animations/aubrey_enemy.tres";
    protected override Stats Stats => new(240, 120, 24, 8, 12, 5, 95);

    protected override string[] EquippedSkills => ["AEAttack", "AEDoNothing", "AEHeadbutt"];

    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy"
              || state == "angry" || state == "hurt" || state == "toast";
    }

    public override bool FallsOffScreen => false;

    public override BattleCommand ProcessAI()
    {
        int roll = GameManager.Instance.Random.RandiRange(0, 100);
        Actor target = BattleManager.Instance.GetAlivePartyMembers().MaxBy(x => x.Actor.CurrentStats.SPD).Actor;
        if (roll < 46)
        {
            return new BattleCommand(this, target, Skills["AEAttack"]);
        }
        roll = GameManager.Instance.Random.RandiRange(0, 100);
        if (roll < 31)
        {
            return new BattleCommand(this, null, Skills["AEDoNothing"]);
        }
        return new BattleCommand(this, target, Skills["AEHeadbutt"]);
    }
}