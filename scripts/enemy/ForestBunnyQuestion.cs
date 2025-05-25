public class ForestBunnyQuestion : Enemy
{
    public override string Name => "FOREST BUNNY?";

    public override string AnimationPath => "res://animations/forest_bunny_alt.tres";

    protected override Stats Stats => new(110, 55, 13, 6, 11, 10, 95);

    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy" || state == "angry" || state == "hurt" || state == "toast";
    }

    protected override string[] EquippedSkills => ["FBQAttack", "FBQDoNothing", "FBQBeCute", "FBQSadEyes"];

    public override bool FallsOffScreen => true;

    public override BattleCommand ProcessAI()
    {
        int roll;
        Actor target = GameManager.Instance.BattleManager.GetRandomAlivePartyMember();
        switch (CurrentState)
        {
            case "happy":
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 46)
                    goto attack;
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 31)
                    goto nothing;
                goto cute;
            case "sad":
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 31)
                    goto attack;
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 31)
                    goto nothing;
                goto sad;
            case "angry":
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 71)
                    goto attack;
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 21)
                    goto nothing;
                goto cute;
            default:
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 61)
                    goto attack;
                roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 41)
                    goto nothing;
                goto cute;

        }
    attack:
        return new BattleCommand(this, target, Skills["FBQAttack"]);
    nothing:
        return new BattleCommand(this, target, Skills["FBQDoNothing"]);
    cute:
        return new BattleCommand(this, target, Skills["FBQBeCute"]);
    sad:
        return new BattleCommand(this, target, Skills["FBQSadEyes"]);
    }
}