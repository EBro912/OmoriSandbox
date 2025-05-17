public class Sweetheart : Enemy
{
    public override string Name => "SWEETHEART";
    public override string AnimationPath => "res://animations/sweetheart.tres";
    protected override Stats Stats => new(3300, 1650, 30, 25, 40, 10, 90);

    protected override string[] EquippedSkills => ["SHAttack", "SharpInsult", "SwingMace", "Brag"];

    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy" 
            || state == "angry" || state == "hurt" || state == "toast" 
            || state == "ecstatic" || state == "manic";
    }

    // TODO: emotion locks
    public override BattleCommand ProcessAI()
    {
        int roll;
        Actor target = GameManager.Instance.BattleManager.GetRandomAlivePartyMember();
        switch (CurrentState)
        {
            case "manic":
            case "ecstatic":
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 46)
                    goto attack;
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 46)
                    goto insult;
                goto mace;
            case "happy":
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 36)
                    goto attack;
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 46)
                    goto insult;
                goto mace;
            case "sad":
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 36)
                    goto attack;
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 21)
                    goto insult;
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 31)
                    goto mace;
                goto brag;
            case "angry":
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 51)
                    goto attack;
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 31)
                    goto insult;
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 71)
                    goto mace;
                goto brag;
            default:
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 41)
                    goto attack;
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 31)
                    goto insult;
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 36)
                    goto mace;
                goto brag;

        }
    attack:
        return new BattleCommand(this, target, Skills["SHAttack"]);
    insult:
        return new BattleCommand(this, target, Skills["SharpInsult"]);
    mace:
        return new BattleCommand(this, target, Skills["SwingMace"]);
    brag:
        return new BattleCommand(this, target, Skills["Brag"]);
    }
}